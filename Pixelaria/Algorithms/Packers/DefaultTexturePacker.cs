/*
    Pixelaria
    Copyright (C) 2013 Luiz Fernando Silva

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    The full license may be found on the License.txt file attached to the
    base directory of this project.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pixelaria.Data;
using Pixelaria.Data.Exports;
using Pixelaria.Utils;
using Pixelaria.Views.Controls.PaintTools;

namespace Pixelaria.Algorithms.Packers
{
    /// <summary>
    /// Defines the default texture packer for the program
    /// </summary>
    public class DefaultTexturePacker : ITexturePacker
    {
        /// <summary>
        /// Packs a given atlas with a specified progress event handler
        /// </summary>
        /// <param name="atlas">The texture atlas to pack</param>
        /// <param name="handler">The event handler for the packing process</param>
        public void Pack(TextureAtlas atlas, BundleExportProgressEventHandler handler = null)
        {
            Pack(atlas, new CancellationToken(), handler).Wait();
        }

        /// <summary>
        /// Packs a given atlas with a specified progress event handler
        /// </summary>
        /// <param name="atlas">The texture atlas to pack</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort this task</param>
        /// <param name="handler">The event handler for the packing process</param>
        public async Task Pack(TextureAtlas atlas, CancellationToken cancellationToken, BundleExportProgressEventHandler handler = null)
        {
            if (atlas.FrameCount == 0)
            {
                atlas.AtlasRectangle = new Rectangle(0, 0, 1, 1);
                return;
            }

            // Cache some fields as locals
            List<IFrame> frameList = atlas.FrameList;

            _progressHandler = handler;
            _frameComparision = new FrameComparision(atlas.ExportSettings.ForceMinimumDimensions);

            // 1. (Optional) Sort the frames from largest to smallest before packing
            if (atlas.ExportSettings.AllowUnorderedFrames)
            {
                // Use a stable sort
                frameList.AddRange(frameList.OrderBy(frame => frame, _frameComparision).ToList());
                frameList.RemoveRange(0, frameList.Count / 2);
            }

            // 2. (Optional) Find identical frames and pack them to use the same sheet area
            if (atlas.ExportSettings.ReuseIdenticalFramesArea)
            {
                MarkIdenticalFramesFromList(frameList);
            }

            // 3. Find the maximum possible horizontal sheet size
            CalculateMaximumSizes(frameList);

            // 4. Iterate through possible widths and match the smallest area to use as a maxWidth
            uint atlasWidth;
            uint atlasHeight;

            var frameBoundsMap = PrepareAtlas(atlas, atlas.ExportSettings.UseUniformGrid ? _maxFrameWidth : -1, atlas.ExportSettings.UseUniformGrid ? _maxFrameHeight : -1);
            Rectangle[] frameBounds = frameBoundsMap.SheetBounds;

            var minAreaTask = new Task<int>(() => IterateAtlasSize(atlas, frameBounds, _maxWidthCapped, out atlasWidth, out atlasHeight, cancellationToken), cancellationToken);
            minAreaTask.Start();

            int minAreaWidth = await minAreaTask;

            atlasWidth = 0;
            atlasHeight = 0;

            // 5. Pack the texture atlas
            Rectangle[] finalFrameRegions = InternalPack(atlas.ExportSettings, frameBounds, ref atlasWidth, ref atlasHeight, minAreaWidth);

            // Replace bounds now
            frameBoundsMap.ReplaceSheetBounds(finalFrameRegions);

            // Unwrap on atlas now
            //atlas.BoundsList
            for (int i = 0; i < atlas.FrameList.Count; i++)
            {
                var frame = atlas.FrameList[i];
                var sheetBounds = frameBoundsMap.GetSheetBoundsForFrame(frame);

                if (sheetBounds != null)
                    atlas.BoundsList[i] = sheetBounds.Value;

                var localBounds = frameBoundsMap.GetLocalBoundsForFrame(frame);

                if(localBounds != null)
                    atlas.OriginsList[i] = localBounds.Value;
            }

            // Round up to the closest power of two
            if (atlas.ExportSettings.ForcePowerOfTwoDimensions)
            {
                atlasWidth = Utilities.SnapToNextPowerOfTwo(atlasWidth);
                atlasHeight = Utilities.SnapToNextPowerOfTwo(atlasHeight);
            }

            if (atlasWidth == 0)
                atlasWidth = 1;
            if (atlasHeight == 0)
                atlasHeight = 1;

            atlas.AtlasRectangle = new Rectangle(0, 0, (int)atlasWidth, (int)atlasHeight);
            
            // Assign the information on the texture atlas
            atlas.Information.ReusedFrameOriginsCount = _frameComparision.CachedSimilarCount;

            // Register reusal in atlas
            atlas.ReuseCount.Clear();

            var simMatrix = _frameComparision.SimilarMatrixIndexDictionary;

            foreach (IFrame frame in atlas.FrameList)
            {
                if (atlas.ExportSettings.ReuseIdenticalFramesArea)
                {
                    int repCount = 0;

                    int index;
                    if (simMatrix.TryGetValue(frame.ID, out index))
                    {
                        repCount = _frameComparision.SimilarFramesMatrix[index].Count - 1;
                    }

                    atlas.ReuseCount.Add(repCount);
                }
                else
                {
                    atlas.ReuseCount.Add(0);
                }
            }
        }
        
        private FrameBoundsMap PrepareAtlas(TextureAtlas atlas, int frameWidth = -1, int frameHeight = -1)
        {
            var boundsMap = new FrameBoundsMap();

            // Register all frames in sequence, first
            for (int i = 0; i < atlas.FrameList.Count; i++)
            {
                var frame = atlas.FrameList[i];

                ////
                //// 2. Calculate frame origin
                ////
                Rectangle local = new Rectangle(0, 0, (frameWidth == -1 ? frame.Width : frameWidth), (frameHeight == -1 ? frame.Height : frameHeight));

                if (atlas.ExportSettings.ForceMinimumDimensions && !atlas.ExportSettings.UseUniformGrid)
                {
                    local = _frameComparision.GetFrameArea(frame);
                }

                atlas.OriginsList[i] = local;

                boundsMap.RegisterFrames(new [] { frame }, local);
            }

            // Match identical frames prior to executing tasks

            ////
            //// 1. Identical frame matching
            ////
            if (atlas.ExportSettings.ReuseIdenticalFramesArea)
            {
                foreach (var frame in atlas.FrameList)
                {
                    IFrame original = _frameComparision.GetOriginalSimilarFrame(frame);

                    if (original != null)
                    {
                        boundsMap.ShareSheetBoundsForFrames(original, frame);
                    }
                }
            }

            return boundsMap;
        }

        /// <summary>
        /// Iterates the given texture atlas using the given properties in order to find the proper atlas width and height configuration that will match the desired settings more
        /// </summary>
        /// <param name="atlas">The atlas to iterate</param>
        /// <param name="frameBounds">The list of frame size rectangles to fit into the atlas</param>
        /// <param name="minAreaWidth">Helper value for calculatingthe minimum area width</param>
        /// <param name="atlasWidth">The out atlas width</param>
        /// <param name="atlasHeight">The out atlas height</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the export process</param>
        /// <returns>The minimum area width that was calculated</returns>
        private int IterateAtlasSize(TextureAtlas atlas, Rectangle[] frameBounds, int minAreaWidth, out uint atlasWidth, out uint atlasHeight, CancellationToken cancellationToken)
        {
            float minRatio = 0;
            int minArea = int.MaxValue;
            uint curWidth = (uint)_maxFrameWidth;

            if (atlas.ExportSettings.ForcePowerOfTwoDimensions)
            {
                curWidth = Utilities.SnapToNextPowerOfTwo(curWidth);
            }

            atlasWidth = 0;
            atlasHeight = 0;

            for (; curWidth < _maxWidthCapped;)
            {
                cancellationToken.ThrowIfCancellationRequested();

                atlasWidth = 0;
                atlasHeight = 0;

                InternalPack(atlas.ExportSettings, frameBounds, ref atlasWidth, ref atlasHeight, (int)curWidth);

                float ratio = (float)atlasWidth / atlasHeight;

                // Round up to the closest power of two
                if (atlas.ExportSettings.ForcePowerOfTwoDimensions)
                {
                    atlasWidth = Utilities.SnapToNextPowerOfTwo(atlasWidth);
                    atlasHeight = Utilities.SnapToNextPowerOfTwo(atlasHeight);
                }

                // Calculate the area now
                int area = (int)(atlasWidth * atlasHeight);

                // Decide whether to swap the best sheet target width with the current one
                if ((atlas.ExportSettings.FavorRatioOverArea && (Math.Abs(ratio - 1) < Math.Abs(minRatio - 1))) ||
                    (!atlas.ExportSettings.FavorRatioOverArea && area < minArea))
                {
                    minArea = area;
                    minRatio = ratio;
                    minAreaWidth = (int)curWidth;
                }

                // Iterate the width now
                if (atlas.ExportSettings.HighPrecisionAreaMatching)
                {
                    curWidth++;
                }
                else
                {
                    curWidth += (uint)_minFrameWidth / 2;
                }

                // Report progress
                if (_progressHandler != null)
                {
                    int progress = (int)((float)curWidth / _maxWidthCapped * 100);

                    if (atlas.ExportSettings.FavorRatioOverArea)
                    {
                        progress = (int)((float)atlasWidth / atlasHeight * 100);
                    }

                    progress = Math.Min(100, progress);

                    _progressHandler.Invoke(new BundleExportProgressEventArgs(BundleExportStage.TextureAtlasGeneration, progress, progress, atlas.Name));
                }

                // Exit the loop if favoring ratio and no better ratio can be achieved
                if (atlas.ExportSettings.FavorRatioOverArea && atlasWidth > atlasHeight)
                {
                    break;
                }
            }

            return minAreaWidth;
        }

        /// <summary>
        /// Calculates the information of the maximum sheet and frame size from a set of frames
        /// </summary>
        /// <param name="frameList">The list of frames to iterate over to calculate the maximum sheet and frame size</param>
        private void CalculateMaximumSizes(List<IFrame> frameList)
        {
            _maxWidthReal = 0;

            _maxFrameWidth = 0;
            _maxFrameHeight = 0;

            _minFrameWidth = int.MaxValue;

            foreach (IFrame frame in frameList)
            {
                int frameWidth = frame.Width;
                int frameHeight = frame.Height;

                if (frameWidth > _maxFrameWidth)
                {
                    _maxFrameWidth = frameWidth;
                }

                if (frameHeight > _maxFrameHeight)
                {
                    _maxFrameHeight = frameHeight;
                }
                if (frameWidth < _minFrameWidth)
                {
                    _minFrameWidth = frameWidth;
                }

                _maxWidthReal += frameWidth;
            }

            _maxWidthCapped = Math.Min(4096, _maxWidthReal);
        }

        /// <summary>
        /// Internal atlas packer method, tailored to work with individual rectangle frames
        /// </summary>
        /// <param name="exportSettings">The export settings to use when packing the rectangles</param>
        /// <param name="rectangles">The list of frame rectangles to try to pack</param>
        /// <param name="atlasWidth">An output atlas width uint</param>
        /// <param name="atlasHeight">At output atlas height uint</param>
        /// <param name="maxWidth">The maximum width the generated sheet can have</param>
        /// <returns>An array of rectangles, where each index matches the original passed Rectangle array, and marks the final computed bounds of the rectangle frames calculated</returns>
        private Rectangle[] InternalPack(AnimationExportSettings exportSettings, Rectangle[] rectangles, ref uint atlasWidth, ref uint atlasHeight, int maxWidth)
        {
            // Cache some fields as locals
            int x = exportSettings.XPadding;

            var boundsFinal = new Rectangle[rectangles.Length];

            for (int i = 0; i < rectangles.Length; i++)
            {
                var width = rectangles[i].Width;
                var height = rectangles[i].Height;

                // X coordinate wrapping
                if (x + width > maxWidth)
                {
                    x = exportSettings.XPadding;
                }

                var y = exportSettings.YPadding;

                // Do a little trickery to find the minimum Y for this frame
                if (x - exportSettings.XPadding < atlasWidth)
                {
                    // Intersect the current frame rectangle with all rectangles above it, and find the maximum bottom Y coordinate between all the intersections
                    int contactRectX = x - exportSettings.XPadding;
                    int contactRectWidth = width + exportSettings.XPadding * 2;

                    for (int j = 0; j < i; j++)
                    {
                        Rectangle rect = boundsFinal[j];

                        if (rect.X < (contactRectX + contactRectWidth) && contactRectX < (rect.X + rect.Width))
                        {
                            y = Math.Max(y, rect.Y + rect.Height + exportSettings.YPadding);
                        }
                    }
                }

                ////
                //// 3. Calculate frame area on sheet
                ////
                boundsFinal[i] = new Rectangle(x, y, width, height);

                atlasWidth = (uint)Math.Max(atlasWidth, boundsFinal[i].X + boundsFinal[i].Width + exportSettings.XPadding);
                atlasHeight = (uint)Math.Max(atlasHeight, boundsFinal[i].Y + boundsFinal[i].Height + exportSettings.YPadding);


                // X coordinate update
                x += boundsFinal[i].Width + exportSettings.XPadding;

                if (x > maxWidth) // Jump to next line, at left-most corner
                {
                    x = exportSettings.XPadding;
                }
            }

            return boundsFinal;
        }
        
        /// <summary>
        /// From a list of frames, marks all the identical frames on the instance frame comparision object
        /// </summary>
        /// <param name="frameList">The list of frames to identify the identical copies</param>
        private void MarkIdenticalFramesFromList(List<IFrame> frameList)
        {
            // Pre-update the frame hashes
            foreach (var frame in frameList)
            {
                frame.UpdateHash();
            }

            for (int i = 0; i < frameList.Count; i++)
            {
                for (int j = i + 1; j < frameList.Count; j++)
                {
                    if (frameList[i].Equals(frameList[j]))
                    {
                        _frameComparision.RegisterSimilarFrames(frameList[i], frameList[j]);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// The default FrameComparision object that will sort frames by size and cache information about the frames' areas
        /// </summary>
        private FrameComparision _frameComparision;

        /// <summary>
        /// The current event handler to report progress to
        /// </summary>
        private BundleExportProgressEventHandler _progressHandler;

        /// <summary>
        /// The maximum possible width when tiling one frame next to another, capped at 4096 pixels
        /// </summary>
        private int _maxWidthCapped;

        /// <summary>
        /// The maximum possible width when tiling one frame next to another, uncapped
        /// </summary>
        private int _maxWidthReal;

        /// <summary>
        /// The maximum width between all the frames being computed with this texture packer
        /// </summary>
        private int _maxFrameWidth;

        /// <summary>
        /// The maximum height between all the frames being computed with this texture packer
        /// </summary>
        private int _maxFrameHeight;

        /// <summary>
        /// The smallest width between all the frames being computed with this texture packer
        /// </summary>
        private int _minFrameWidth;

        /// <summary>
        /// Default IComparer used to sort and store information about frames
        /// </summary>
        public class FrameComparision : IComparer<IFrame>
        {
            /// <summary>
            /// Dictionary of internal compare fragments
            /// </summary>
            readonly Dictionary<int, CompareFrag> _fragDictionary;

            /// <summary>
            /// Matrix of similar frames stored in a multi-dimensional list
            /// </summary>
            private readonly List<List<IFrame>> _similarFramesMatrix;

            /// <summary>
            /// Dictionary of indexes assigned to similar frames used to store the index of the corresponding frame in the similarMatrix field
            /// </summary>
            private readonly Dictionary<int, int> _similarMatrixIndexDictionary;

            /// <summary>
            /// Gets the matrix of similar frames stored in a multi-dimensional list
            /// </summary>
            public List<List<IFrame>> SimilarFramesMatrix => _similarFramesMatrix;

            /// <summary>
            /// Gets the ictionary of indexes assigned to similar frames used to store the index of the corresponding frame in the similarMatrix field
            /// </summary>
            public Dictionary<int, int> SimilarMatrixIndexDictionary => _similarMatrixIndexDictionary;

            /// <summary>
            /// Whether to compute the minimum areas of the frames before comparing them
            /// </summary>
            readonly bool _useMinimumTextureArea;

            /// <summary>
            /// Gets the number of cached compare fragments currently stores
            /// </summary>
            public int CachedCompareCount => _fragDictionary.Count;

            /// <summary>
            /// Gets the number of cached similar fragments currently stores
            /// </summary>
            public int CachedSimilarCount { get { return _similarFramesMatrix.Sum(list => list.Count) - _similarFramesMatrix.Count; } }

            /// <summary>
            /// Creates a new instance of the FrameComparision class
            /// </summary>
            /// <param name="useMinimumTextureArea">Whether to compute the minimum areas of the frames before comparing them</param>
            public FrameComparision(bool useMinimumTextureArea)
            {
                _fragDictionary = new Dictionary<int, CompareFrag>();
                _similarFramesMatrix = new List<List<IFrame>>();
                _similarMatrixIndexDictionary = new Dictionary<int, int>();

                _useMinimumTextureArea = useMinimumTextureArea;
            }

            /// <summary>
            /// Resets this comparer
            /// </summary>
            public void Reset()
            {
                // Clear the references for the GC's sake
                _fragDictionary.Clear();

                _similarFramesMatrix.Clear();
                _similarMatrixIndexDictionary.Clear();
            }

            // Summary:
            //     Compares two objects and returns a value indicating whether one is less than,
            //     equal to, or greater than the other.
            // 
            // Parameters:
            //   x:
            //     The first object to compare.
            // 
            //   y:
            //     The second object to compare.
            // 
            // Returns:
            //     A signed integer that indicates the relative values of x and y, as shown
            //     in the following table.
            //     Value                Meaning
            //     Less than zero       x is less than y.
            //     Zero                 x equals y.
            //     Greater than zero    x is greater than y.
            public int Compare(IFrame x, IFrame y)
            {
                // Get the frame areas
                Rectangle minFrameX = GetFrameArea(x);
                Rectangle minFrameY = GetFrameArea(y);

                int xArea = minFrameX.Width * minFrameX.Height;
                int yArea = minFrameY.Width * minFrameY.Height;

                return (xArea == yArea) ? 0 : ((xArea > yArea) ? -1 : 1);
            }

            /// <summary>
            /// Gets the area for the given Frame object
            /// </summary>
            /// <param name="frame">The Frame object to get the area of</param>
            /// <returns>A Rectangle representing the Frame's area</returns>
            public Rectangle GetFrameArea(IFrame frame)
            {
                // Try to find the already-computed frame area first
                CompareFrag frag;

                _fragDictionary.TryGetValue(frame.ID, out frag);

                if (frag != null)
                    return frag.FrameRectangle;
                
                using (var frameBitmap = frame.GetComposedBitmap())
                {
                    var newFrag = new CompareFrag
                    {
                        FrameRectangle = _useMinimumTextureArea
                            ? ImageUtilities.FindMinimumImageArea(frameBitmap)
                            : new Rectangle(0, 0, frame.Width, frame.Height)
                    };

                    _fragDictionary[frame.ID] = newFrag;

                    return newFrag.FrameRectangle;
                }
            }

            /// <summary>
            /// Registers two frames as being similar to the pixel-level
            /// </summary>
            /// <param name="frame1">The first frame to register</param>
            /// <param name="frame2">The second frame to register</param>
            public void RegisterSimilarFrames(IFrame frame1, IFrame frame2)
            {
                // Check existence of either frames in the matrix index dictionary
                int index;
                if (!_similarMatrixIndexDictionary.TryGetValue(frame1.ID, out index) && !_similarMatrixIndexDictionary.TryGetValue(frame2.ID, out index))
                {
                    _similarFramesMatrix.Add(new List<IFrame>());
                    index = _similarFramesMatrix.Count - 1;
                }

                _similarMatrixIndexDictionary[frame1.ID] = index;
                _similarMatrixIndexDictionary[frame2.ID] = index;

                if(!_similarFramesMatrix[index].ContainsReference(frame2))
                    _similarFramesMatrix[index].Add(frame2);
                if (!_similarFramesMatrix[index].ContainsReference(frame1))
                    _similarFramesMatrix[index].Add(frame1);
            }

            /// <summary>
            /// Gets the original similar frame based on the given frame.
            /// The returned frame is the first frame inserted that is similar to the given frame. If the given frame is the original similar
            /// frame, null is returned. If no similar frames were stored, null is returned.
            /// </summary>
            /// <param name="frame">The frame to seek the original similar frame from</param>
            /// <returns>The first frame inserted that is similar to the given frame. If the given frame is the original similar frame, null is returned. If no similar frames were stored, null is returned.</returns>
            public IFrame GetOriginalSimilarFrame(IFrame frame)
            {
                int index;
                if (_similarMatrixIndexDictionary.TryGetValue(frame.ID, out index))
                {
                    return _similarFramesMatrix[index][0];
                }

                return null;
            }

            /// <summary>
            /// Internal compare speed-up fragment.
            /// The fragment is composed of a Frame and a Rectangle
            /// that represents the Frame's area
            /// </summary>
            class CompareFrag
            {
                /// <summary>
                /// The frame's rectangle
                /// </summary>
                public Rectangle FrameRectangle;
            }
        }

        /// <summary>
        /// Provides a simple interface for managing sharing of frame bounds so that changes to one frame final sheet rectangle reflects on all other shared frames.
        /// </summary>
        public class FrameBoundsMap
        {
            /// <summary>
            /// Maps Frame.ID -> _sheetBounds Rectangle list index
            /// </summary>
            private readonly Dictionary<int, int> _frameSheetBoundsMap = new Dictionary<int, int>();

            /// <summary>
            /// Maps Frame.ID -> local frame image Rectangle
            /// </summary>
            private readonly Dictionary<int, Rectangle> _frameLocalBoundsMap = new Dictionary<int, Rectangle>();

            /// <summary>
            /// List of available frame bounds in global sheet coordinates
            /// </summary>
            private readonly List<Rectangle> _sheetBounds = new List<Rectangle>();

            /// <summary>
            /// Array of available frame bounds in global sheet coordinates
            /// </summary>
            public Rectangle[] SheetBounds => _sheetBounds.ToArray();

            /// <summary>
            /// Register shared bounds for a given set of frames
            /// This method throws an exception if the frame passed in does not have an id set (-1).
            /// </summary>
            /// <param name="frames">Shared frames with the same bounds</param>
            /// <param name="localBounds">Local bounds for the frames</param>
            /// <exception cref="ArgumentException">The IFrame instance passed in has an id of -1, or is not initialized</exception>
            public void RegisterFrames(IEnumerable<IFrame> frames, Rectangle localBounds)
            {
                int sheetIndex = _sheetBounds.Count;

                var localZero = localBounds;
                localZero.X = 0;
                localZero.Y = 0;

                _sheetBounds.Add(localZero);
                
                // Map values
                foreach (var frame in frames)
                {
                    // Check invalid id
                    if (frame.ID == -1)
                        throw new ArgumentException(@"Frame appears to have no valid ID set (negative number).", nameof(frames));
                    if(!frame.Initialized)
                        throw new ArgumentException(@"Frame is uninitialized.", nameof(frames));

                    _frameSheetBoundsMap[frame.ID] = sheetIndex;
                    _frameLocalBoundsMap[frame.ID] = localBounds;
                }
            }

            /// <summary>
            /// Replaces the current list of rectangles with a given rectangle collection.
            /// The rectangle collection must have the same count of rectangles as the currently registered SheetBounds array.
            /// An exception is thrown, if the count of new bounds do not match the current count of sheets
            /// </summary>
            /// <exception cref="ArgumentException">The newBounds enumerable has a count different than SheetBounds.Count</exception>
            public void ReplaceSheetBounds(IEnumerable<Rectangle> newBounds)
            {
                var newList = new List<Rectangle>(newBounds);
                if (newList.Count != _sheetBounds.Count)
                    throw new ArgumentException($"The count of items in the passed enumerable is mismatched: {newList.Count} new bounds vs {_sheetBounds.Count} local bounds", nameof(newBounds));
                
                // Straight replace the values
                _sheetBounds.Clear();
                _sheetBounds.AddRange(newList);
            }

            /// <summary>
            /// Gets the rectangle for a given frame.
            /// Returns null, if no rectangle was found
            /// </summary>
            public Rectangle? GetSheetBoundsForFrame(IFrame frame)
            {
                int index;
                if (_frameSheetBoundsMap.TryGetValue(frame.ID, out index))
                {
                    return _sheetBounds[index];
                }

                return null;
            }

            /// <summary>
            /// Gets the local image bounds for a given frame
            /// </summary>
            public Rectangle? GetLocalBoundsForFrame(IFrame frame)
            {
                Rectangle bounds;
                if (_frameLocalBoundsMap.TryGetValue(frame.ID, out bounds))
                {
                    return bounds;
                }

                return null;
            }

            /// <summary>
            /// Sets the sheet bounds for a specified frame, and all shared frames, in this frame bounds map.
            /// Does nothing if this frame is not registered on this frame bounds map.
            /// This method throws an exception if the frame passed in does not have an id set (-1).
            /// </summary>
            /// <exception cref="ArgumentException">The IFrame instance passed in has an id of -1, or is not initialized</exception>
            public void SetSheetBoundsForFrame(IFrame frame, Rectangle sheetBounds)
            {
                // Check invalid id
                if (frame.ID == -1)
                    throw new ArgumentException(@"Frame appears to have no valid ID set (negative number).", nameof(frame));
                if (!frame.Initialized)
                    throw new ArgumentException(@"Frame is uninitialized.", nameof(frame));

                int index;
                if (_frameSheetBoundsMap.TryGetValue(frame.ID, out index))
                {
                    _sheetBounds[index] = sheetBounds;
                }
            }

            /// <summary>
            /// Marks two frames as sharing the same sheet bounds.
            /// Any changes to a sheet bound of one frame will reflect when fetching the sheet bounds of the other.
            /// Both frames must be registered on this frame bounds map previously, otherwise nothing is done and false is retured.
            /// In case the operation succeeded, true is returned.
            /// This effectively associates frame2 -> frame1, so any sheet bounds associated with frame2 are no longer associated to it.
            /// This method throws an exception if the frame passed in does not have an id set (-1).
            /// </summary>
            /// <exception cref="ArgumentException">The IFrame instance passed in has an id of -1, or is not initialized</exception>
            public bool ShareSheetBoundsForFrames(IFrame frame1, IFrame frame2)
            {
                // Check invalid id
                if (frame1.ID == -1)
                    throw new ArgumentException(@"Frame appears to have no valid ID set (negative number).", nameof(frame1));
                if (!frame1.Initialized)
                    throw new ArgumentException(@"Frame is uninitialized.", nameof(frame1));
                if (frame1.ID == -2)
                    throw new ArgumentException(@"Frame appears to have no valid ID set (negative number).", nameof(frame2));
                if (!frame2.Initialized)
                    throw new ArgumentException(@"Frame is uninitialized.", nameof(frame2));

                // Missing registration
                if (!ContainsFrame(frame1) || !ContainsFrame(frame2))
                {
                    return false;
                }

                // Find current indexes on map dictionary
                int frame1Index = _frameSheetBoundsMap[frame1.ID];
                int frame2Index = _frameSheetBoundsMap[frame2.ID];

                // Frames already share the same bounds
                if (frame1Index == frame2Index)
                    return true;

                // Share from frame1 -> frame2
                _frameSheetBoundsMap[frame2.ID] = frame1Index;

                CompactMapArray();

                return true;
            }

            /// <summary>
            /// Returns whether this frame bounds map contains information pertaininig to a given frame
            /// </summary>
            public bool ContainsFrame(IFrame frame)
            {
                return _frameSheetBoundsMap.ContainsKey(frame.ID) && _frameLocalBoundsMap.ContainsKey(frame.ID);
            }

            /// <summary>
            /// Moves all internal references from one frame sheet map array index to another
            /// </summary>
            private void MoveIndex(int oldIndex, int newIndex)
            {
                foreach (var key in _frameSheetBoundsMap.Keys.ToArray())
                {
                    if (_frameSheetBoundsMap[key] == oldIndex)
                    {
                        _frameSheetBoundsMap[key] = newIndex;
                    }
                }
            }

            /// <summary>
            /// Compacts the data so the sheet array doesn't points to no-longer referenced indexes
            /// </summary>
            private void CompactMapArray()
            {
                // Ref-counting array
                var refs = _sheetBounds.Select(v => false).ToList();

                foreach (var value in _frameSheetBoundsMap.Values)
                {
                    refs[value] = true;
                }

                // Find 'false' indexes and remove them, moving indexes back as we go
                for (int i = 0; i < refs.Count; i++)
                {
                    if (refs[i])
                        continue;

                    // Move all next indexes back
                    for (int j = i; j < _sheetBounds.Count - 1; j++)
                    {
                        MoveIndex(j + 1, j);
                    }

                    // Remove index, and continue searching
                    _sheetBounds.RemoveAt(i);

                    refs.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}