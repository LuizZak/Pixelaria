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
using JetBrains.Annotations;
using Pixelaria.Data;
using Pixelaria.Data.Exports;
using Pixelaria.Utils;

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
            var frameList = atlas.FrameList;

            _progressHandler = handler;
            _frameComparision = new FrameComparision(atlas.SheetExportSettings.ForceMinimumDimensions);

            // 1. (Optional) Sort the frames from largest to smallest before packing
            if (atlas.SheetExportSettings.AllowUnorderedFrames)
            {
                // Use a stable sort
                frameList.AddRange(frameList.OrderBy(frame => frame, _frameComparision).ToList());
                frameList.RemoveRange(0, frameList.Count / 2);
            }

            // 2. (Optional) Find identical frames and pack them to use the same sheet area
            if (atlas.SheetExportSettings.ReuseIdenticalFramesArea)
            {
                MarkIdenticalFramesFromList(frameList);
            }

            // 3. Find the maximum possible horizontal sheet size
            CalculateMaximumSizes(frameList);

            // 4. Iterate through possible widths and match the smallest area to use as a maxWidth
            uint atlasWidth;
            uint atlasHeight;

            var frameBoundsMap = PrepareAtlas(atlas, atlas.SheetExportSettings.UseUniformGrid ? _maxFrameWidth : -1, atlas.SheetExportSettings.UseUniformGrid ? _maxFrameHeight : -1);
            var frameBounds = frameBoundsMap.SheetBounds;

            var minAreaTask = new Task<int>(() =>
                    IterateAtlasSize(atlas, frameBounds, _maxWidthCapped, out atlasWidth, out atlasHeight,
                        cancellationToken),
                cancellationToken);
            minAreaTask.Start();

            int minAreaWidth = await minAreaTask;
            if (cancellationToken.IsCancellationRequested)
                return;

            atlasWidth = 0;
            atlasHeight = 0;

            // 5. Pack the texture atlas
            var finalFrameRegions = InternalPack(atlas.SheetExportSettings, frameBounds, ref atlasWidth, ref atlasHeight, minAreaWidth, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                return;

            // Replace bounds now
            frameBoundsMap.ReplaceSheetBounds(finalFrameRegions);

            atlas.SetFrameBoundsMap(frameBoundsMap);

            // Round up to the closest power of two
            if (atlas.SheetExportSettings.ForcePowerOfTwoDimensions)
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
            atlas.Information.ReusedFrameOriginsCount = atlas.FrameCount - frameBoundsMap.SheetBounds.Length;
        }
        
        private FrameBoundsMap PrepareAtlas([NotNull] TextureAtlas atlas, int frameWidth = -1, int frameHeight = -1)
        {
            var boundsMap = new FrameBoundsMap();

            // Register all frames in sequence, first
            foreach (var frame in atlas.FrameList)
            {
                ////
                //// 2. Calculate frame origin
                ////
                var local = new Rectangle(0, 0, (frameWidth == -1 ? frame.Width : frameWidth), (frameHeight == -1 ? frame.Height : frameHeight));

                if (atlas.SheetExportSettings.ForceMinimumDimensions && !atlas.SheetExportSettings.UseUniformGrid)
                {
                    local = _frameComparision.GetFrameArea(frame);
                }

                boundsMap.RegisterFrames(new [] { frame }, local);
            }

            // Match identical frames prior to executing tasks

            ////
            //// 1. Identical frame matching
            ////
            if (atlas.SheetExportSettings.ReuseIdenticalFramesArea)
            {
                foreach (var frame in atlas.FrameList)
                {
                    var original = _frameComparision.GetOriginalSimilarFrame(frame);

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
        private int IterateAtlasSize([NotNull] TextureAtlas atlas, Rectangle[] frameBounds, int minAreaWidth, out uint atlasWidth, out uint atlasHeight, CancellationToken cancellationToken)
        {
            float minRatio = 0;
            int minArea = int.MaxValue;
            uint curWidth = (uint)_maxFrameWidth;

            if (atlas.SheetExportSettings.ForcePowerOfTwoDimensions)
            {
                curWidth = Utilities.SnapToNextPowerOfTwo(curWidth);
            }

            atlasWidth = 0;
            atlasHeight = 0;

            for (; curWidth < _maxWidthCapped;)
            {
                if (cancellationToken.IsCancellationRequested)
                    return 0;

                atlasWidth = 0;
                atlasHeight = 0;

                InternalPack(atlas.SheetExportSettings, frameBounds, ref atlasWidth, ref atlasHeight, (int)curWidth, cancellationToken);

                float ratio = (float)atlasWidth / atlasHeight;

                // Round up to the closest power of two
                if (atlas.SheetExportSettings.ForcePowerOfTwoDimensions)
                {
                    atlasWidth = Utilities.SnapToNextPowerOfTwo(atlasWidth);
                    atlasHeight = Utilities.SnapToNextPowerOfTwo(atlasHeight);
                }

                // Calculate the area now
                int area = (int)(atlasWidth * atlasHeight);

                // Decide whether to swap the best sheet target width with the current one
                if (atlas.SheetExportSettings.FavorRatioOverArea && Math.Abs(ratio - 1) < Math.Abs(minRatio - 1) ||
                    !atlas.SheetExportSettings.FavorRatioOverArea && area < minArea)
                {
                    minArea = area;
                    minRatio = ratio;
                    minAreaWidth = (int)curWidth;
                }

                // Iterate the width now
                if (atlas.SheetExportSettings.HighPrecisionAreaMatching)
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

                    if (atlas.SheetExportSettings.FavorRatioOverArea)
                    {
                        progress = (int)((float)atlasWidth / atlasHeight * 100);
                    }

                    progress = Math.Min(100, progress);

                    _progressHandler.Invoke(new BundleExportProgressEventArgs(BundleExportStage.TextureAtlasGeneration, progress, progress, atlas.Name));
                }

                // Exit the loop if favoring ratio and no better ratio can be achieved
                if (atlas.SheetExportSettings.FavorRatioOverArea && atlasWidth > atlasHeight)
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
        private void CalculateMaximumSizes([NotNull] List<IFrame> frameList)
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
        /// <param name="sheetExportSettings">The export settings to use when packing the rectangles</param>
        /// <param name="rectangles">The list of frame rectangles to try to pack</param>
        /// <param name="atlasWidth">An output atlas width uint</param>
        /// <param name="atlasHeight">At output atlas height uint</param>
        /// <param name="maxWidth">The maximum width the generated sheet can have</param>
        /// <param name="cancellationToken">Cancelation token for operation. When canceled, eventually the method stops attempting to pack the texture rectangles.</param>
        /// <returns>An array of rectangles, where each index matches the original passed Rectangle array, and marks the final computed bounds of the rectangle frames calculated</returns>
        private static Rectangle[] InternalPack(AnimationSheetExportSettings sheetExportSettings, [NotNull] IReadOnlyList<Rectangle> rectangles, ref uint atlasWidth, ref uint atlasHeight, int maxWidth, CancellationToken cancellationToken)
        {
            // Cache some fields as locals
            int x = sheetExportSettings.XPadding;

            var boundsFinal = new Rectangle[rectangles.Count];

            for (int i = 0; i < rectangles.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return new Rectangle[0];

                int width = rectangles[i].Width;
                int height = rectangles[i].Height;

                // X coordinate wrapping
                if (x + width > maxWidth)
                {
                    x = sheetExportSettings.XPadding;
                }

                int y = sheetExportSettings.YPadding;

                // Do a little trickery to find the minimum Y for this frame
                if (x - sheetExportSettings.XPadding < atlasWidth)
                {
                    // Intersect the current frame rectangle with all rectangles above it, and find the maximum bottom Y coordinate between all the intersections
                    int contactRectX = x - sheetExportSettings.XPadding;
                    int contactRectWidth = width + sheetExportSettings.XPadding * 2;

                    for (int j = 0; j < i; j++)
                    {
                        Rectangle rect = boundsFinal[j];

                        if (rect.X < (contactRectX + contactRectWidth) && contactRectX < (rect.X + rect.Width))
                        {
                            y = Math.Max(y, rect.Y + rect.Height + sheetExportSettings.YPadding);
                        }
                    }
                }

                ////
                //// 3. Calculate frame area on sheet
                ////
                boundsFinal[i] = new Rectangle(x, y, width, height);

                atlasWidth = (uint)Math.Max(atlasWidth, boundsFinal[i].X + boundsFinal[i].Width + sheetExportSettings.XPadding);
                atlasHeight = (uint)Math.Max(atlasHeight, boundsFinal[i].Y + boundsFinal[i].Height + sheetExportSettings.YPadding);


                // X coordinate update
                x += boundsFinal[i].Width + sheetExportSettings.XPadding;

                if (x > maxWidth) // Jump to next line, at left-most corner
                {
                    x = sheetExportSettings.XPadding;
                }
            }

            return boundsFinal;
        }
        
        /// <summary>
        /// From a list of frames, marks all the identical frames on the instance frame comparision object
        /// </summary>
        /// <param name="frameList">The list of frames to identify the identical copies</param>
        private void MarkIdenticalFramesFromList([NotNull] List<IFrame> frameList)
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
                if (y == null)
                    return -1;
                if (x == null)
                    return 1;

                // Get the frame areas
                var minFrameX = GetFrameArea(x);
                var minFrameY = GetFrameArea(y);

                int xArea = minFrameX.Width * minFrameX.Height;
                int yArea = minFrameY.Width * minFrameY.Height;

                return (xArea == yArea) ? 0 : ((xArea > yArea) ? -1 : 1);
            }

            /// <summary>
            /// Gets the area for the given Frame object
            /// </summary>
            /// <param name="frame">The Frame object to get the area of</param>
            /// <returns>A Rectangle representing the Frame's area</returns>
            public Rectangle GetFrameArea([NotNull] IFrame frame)
            {
                // Try to find the already-computed frame area first

                _fragDictionary.TryGetValue(frame.ID, out CompareFrag frag);

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
            public void RegisterSimilarFrames([NotNull] IFrame frame1, [NotNull] IFrame frame2)
            {
                // Check existence of either frames in the matrix index dictionary
                if (!_similarMatrixIndexDictionary.TryGetValue(frame1.ID, out int index) && !_similarMatrixIndexDictionary.TryGetValue(frame2.ID, out index))
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
            public IFrame GetOriginalSimilarFrame([NotNull] IFrame frame)
            {
                if (_similarMatrixIndexDictionary.TryGetValue(frame.ID, out int index))
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
    }
}