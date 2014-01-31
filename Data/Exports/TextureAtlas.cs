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

using Pixelaria.Utils;

namespace Pixelaria.Data.Exports
{
    /// <summary>
    /// Describes a texture atlas that is used to pack frames of an animation together
    /// </summary>
    public class TextureAtlas : IDisposable
    {
        /// <summary>
        /// Creates a new TextureAtlas, preparing the atlas to export using the given export settings
        /// </summary>
        /// <param name="settings">The export settings to use when packing the frames</param>
        /// <param name="name">An optional name for the TextureAtlas to be used on progress report</param>
        public TextureAtlas(AnimationExportSettings settings, string name = "")
        {
            this.frameList = new List<Frame>();
            this.boundsList = new List<Rectangle>();
            this.originsList = new List<Rectangle>();
            this.frameComparision = new FrameComparision(settings.ForceMinimumDimensions);
            this.exportSettings = settings;

            this.name = name;
        }

        /// <summary>
        /// Disposes of the data stored by this TextureAtlas
        /// </summary>
        public void Dispose()
        {
            // Clear the lists
            frameList.Clear();
            boundsList.Clear();
            originsList.Clear();

            frameComparision.Reset();
        }

        /// <summary>
        /// Inserts a frame into this TextureAtlas
        /// </summary>
        /// <param name="frame">The frame to pack</param>
        public void InsertFrame(Frame frame)
        {
            frameList.Add(frame);
            boundsList.Add(new Rectangle());
            originsList.Add(new Rectangle());
        }

        /// <summary>
        /// Pack all the inserted frames
        /// </summary>
        /// <param name="progressHandler">Optional event handler for reporting the atlas packing progress</param>
        public void Pack(BundleExportProgressEventHandler progressHandler = null)
        {
            this.progressHandler = progressHandler;

            if (frameList.Count == 0)
            {
                atlasRectangle = new Rectangle(0, 0, 1, 1);
                return;
            }

            uint atlasWidth = 0;
            uint atlasHeight = 0;
            int maxWidthCapped = 0;
            int maxWidthReal = 0;
            
            int maxFrameWidth = 0;
            int minFrameWidth = int.MaxValue;

            frameComparision.Reset();

            // Sort the frames from largest to smallest before packing
            if (exportSettings.AllowUnorderedFrames)
            {
                frameList.Sort(frameComparision);
            }

            // Find identical frames and pack them to use the same sheet area
            if (exportSettings.ReuseIdenticalFramesArea)
            {
                for (int i = 0; i < frameList.Count; i++)
                {
                    for (int j = i + 1; j < frameList.Count; j++)
                    {
                        if(frameList[i].Equals(frameList[j]))
                        {
                            frameComparision.RegisterSimilarFrames(frameList[i], frameList[j]);
                            break;
                        }
                    }
                }
            }

            // Find the maximum possible horizontal sheet size
            foreach (Frame frame in frameList)
            {
                if (frame.Width > maxFrameWidth)
                {
                    maxFrameWidth = frame.Width;
                }
                if (frame.Width < minFrameWidth)
                {
                    minFrameWidth = frame.Width;
                }

                maxWidthCapped += frame.Width;
            }

            maxWidthReal = maxWidthCapped;
            maxWidthCapped = Math.Min(4096, maxWidthCapped);

            int minAreaWidth = maxWidthCapped;
            float minRatio = 0;
            int minArea = int.MaxValue;

            // Iterate through possible widths and match the smallest area to use as a maxWidth
            uint curWidth = (uint)maxFrameWidth;

            if (exportSettings.ForcePowerOfTwoDimensions)
            {
                curWidth = SnapToNextPowerOfTwo(curWidth);
            }

            for (; curWidth < maxWidthCapped; )
            {
                atlasWidth = 0;
                atlasHeight = 0;
                InternalPack(ref atlasWidth, ref atlasHeight, (int)curWidth);

                float ratio = (float)atlasWidth / atlasHeight;

                // Round up to the closest power of two
                if (exportSettings.ForcePowerOfTwoDimensions)
                {
                    atlasWidth = SnapToNextPowerOfTwo(atlasWidth);
                    atlasHeight = SnapToNextPowerOfTwo(atlasHeight);
                }

                // Calculate the area now
                int area = (int)(atlasWidth * atlasHeight);

                float areaDiff = (float)area / minArea;
                float ratioDiff = (float)Math.Abs(ratio - 1) / Math.Abs(minRatio - 1);

                bool swap = false;

                // Decide whether to swap the best sheet target width with tue current one
                if (exportSettings.FavorRatioOverArea && (Math.Abs(ratio - 1) < Math.Abs(minRatio - 1)))
                {
                    swap = true;
                }
                else if (!exportSettings.FavorRatioOverArea && (area < minArea || (Math.Abs(ratio - 1) < Math.Abs(minRatio - 1))))
                {
                    swap = true;
                }

                if(swap)
                {
                    minArea = area;
                    minRatio = ratio;
                    minAreaWidth = (int)curWidth;
                }

                // Iterate the width now
                if (exportSettings.HighPrecisionAreaMatching)
                {
                    curWidth++;
                }
                else
                {
                    curWidth += (uint)minFrameWidth / 2;
                }

                // Report progress
                if(progressHandler != null)
                {
                    int progress = (int)((float)curWidth / maxWidthCapped * 100);

                    if (exportSettings.FavorRatioOverArea)
                    {
                        progress = (int)((float)atlasWidth / atlasHeight * 100);
                    }

                    progress = Math.Min(100, progress);

                    progressHandler.Invoke(new BundleExportProgressEventArgs(BundleExportStage.TextureAtlasGeneration, progress, progress, name));
                }

                // Exit the loop if favoring ratio and no better ratio can be achieved
                if (exportSettings.FavorRatioOverArea && atlasWidth > atlasHeight)
                {
                    break;
                }
            }

            atlasWidth = 0;
            atlasHeight = 0;

            InternalPack(ref atlasWidth, ref atlasHeight, minAreaWidth);

            // Round up to the closest power of two
            if (exportSettings.ForcePowerOfTwoDimensions)
            {
                atlasWidth = SnapToNextPowerOfTwo(atlasWidth);
                atlasHeight = SnapToNextPowerOfTwo(atlasHeight);
            }

            if (atlasWidth == 0)
                atlasWidth = 1;
            if (atlasHeight == 0)
                atlasHeight = 1;

            atlasRectangle = new Rectangle(0, 0, (int)atlasWidth, (int)atlasHeight);
        }

        /// <summary>
        /// Internal atlas packer method
        /// </summary>
        /// <param name="atlasWidth">An output atlas width uint</param>
        /// <param name="atlasHeight">At output atlas height uint</param>
        private void InternalPack(ref uint atlasWidth, ref uint atlasHeight, int maxWidth)
        {
            int x = exportSettings.XPadding;
            int y = exportSettings.YPadding;
            int width = 0;
            int height = 0;

            for (int i = 0; i < frameList.Count; i++)
            {
                Frame frame = frameList[i];

                // Identical frame matching
                if (exportSettings.ReuseIdenticalFramesArea)
                {
                    Frame original = frameComparision.GetOriginalSimilarFrame(frame);

                    if (original != null)
                    {
                        originsList[i] = originsList[frameList.IndexOf(original)];
                        boundsList[i] = boundsList[frameList.IndexOf(original)];

                        continue;
                    }
                }


                // Calculate frame origin
                originsList[i] = new Rectangle(0, 0, frame.Width, frame.Height);

                if (exportSettings.ForceMinimumDimensions)
                {
                    originsList[i] = frameComparision.GetFrameArea(frame);
                }

                width = originsList[i].Width;
                height = originsList[i].Height;


                // X coordinate wrapping
                if (x + width > maxWidth)
                {
                    x = exportSettings.XPadding;
                }

                y = 0;

                // Do a little trickery to find the minimum Y for this frame
                if (x - exportSettings.XPadding < atlasWidth)
                {
                    // Intersect the current frame rectangle with all rectangles above it, and find the maximum bottom Y coordinate between all the intersections
                    int contactRectX = x - exportSettings.XPadding;
                    int contactRectWidth = width + exportSettings.XPadding * 2;
                    
                    for (int j = 0; j < i; j++)
                    {
                        Rectangle rect = boundsList[j];

                        if (rect.X < (contactRectX + contactRectWidth) && contactRectX < (rect.X + rect.Width))
                        {
                            y = Math.Max(y, rect.Y + rect.Height + exportSettings.YPadding);
                        }
                    }
                }


                // Calculate frame area on sheet
                boundsList[i] = new Rectangle(x, y, width, height);

                atlasWidth = (uint)Math.Max(atlasWidth, boundsList[i].X + boundsList[i].Width + exportSettings.XPadding);
                atlasHeight = (uint)Math.Max(atlasHeight, boundsList[i].Y + boundsList[i].Height + exportSettings.YPadding);


                // X coordinate update
                x += boundsList[i].Width + exportSettings.XPadding;

                if (x > maxWidth)
                {
                    x = exportSettings.XPadding;
                }
            }
        }

        /// <summary>
        /// Generates the Sheet for this TextureAtlas
        /// </summary>
        /// <returns>A composed texture atlas of all the frames imported on this TextureAtlas</returns>
        public Image GenerateSheet()
        {
            Bitmap image = new Bitmap(AtlasWidth, AtlasHeight);

            Graphics graphics = Graphics.FromImage(image);

            graphics.Clear(Color.Transparent);

            for (int i = 0; i < FrameCount; i++)
            {
                Frame frame = GetFrame(i);

                Rectangle frameBounds = GetFrameBoundsRectangle(i);
                Rectangle originBounds = GetFrameOriginsRectangle(i);

                graphics.DrawImage(frame.GetComposedBitmap(), frameBounds, originBounds, GraphicsUnit.Pixel);
            }

            graphics.Flush();
            graphics.Dispose();

            return image;
        }

        /// <summary>
        /// Gets a list of animations whose frames are on this atlas
        /// </summary>
        /// <returns>The array of animations on this TextureAtlas</returns>
        public Animation[] GetAnimationsOnAtlas()
        {
            List<Animation> animations = new List<Animation>();

            foreach (Frame frame in frameList)
            {
                if (!animations.Contains(frame.Animation))
                {
                    animations.Add(frame.Animation);
                }
            }

            return animations.ToArray();
        }

        /// <summary>
        /// Returns the frame that relies on the given index
        /// </summary>
        /// <param name="frameIndex">The index for frame</param>
        /// <returns>The frame that relies on that index</returns>
        public Frame GetFrame(int frameIndex)
        {
            return frameList[frameIndex];
        }

        /// <summary>
        /// Gets the bounding rectangle for the given frame index. The bounds rectangle represents the
        /// area on the atlas sheet the frame occupies. It is relative to the atlas sheet
        /// </summary>
        /// <param name="frameIndex">The index of the frame to get the bounding rectangle from</param>
        /// <returns>The bounding rectangle for the given frame</returns>
        public Rectangle GetFrameBoundsRectangle(int frameIndex)
        {
            return boundsList[frameIndex];
        }

        /// <summary>
        /// Gets the origin rectangle for the given frame index. The origin rectangle represents the area
        /// of the Frame image that is used on the drawn atlas sheet. It is relative to the Frame area
        /// </summary>
        /// <param name="frameIndex">The index of the frame to get the origin rectangle from</param>
        /// <returns>The origin rectangle for the given frame</returns>
        public Rectangle GetFrameOriginsRectangle(int frameIndex)
        {
            return originsList[frameIndex];
        }

        /// <summary>
        /// Returns a Rectangle that specifies the minimum image area, clipping out all the alpha pixels
        /// </summary>
        /// <param name="image">The image to find the mimimum texture area</param>
        /// <returns>A Rectangle that specifies the minimum image area, clipping out all the alpha pixels</returns>
        private static Rectangle FindMinimumTextureArea(Bitmap image)
        {
            int x = 0;
            int y = 0;
            int widthRange = image.Width - 1;
            int heightRange = image.Height - 1;

            int width = image.Width;
            int height = image.Height;

            FastBitmap fastBitmap = new FastBitmap(image);

            fastBitmap.Lock();

            // Scan horizontally until the first non-0 alpha pixel is found
            for (x = 0; x < width; x++)
            {
                for (int _y = 0; _y < height; _y++)
                {
                    if (fastBitmap.GetPixelInt(x, _y) >> 24 != 0)
                    {
                        goto skipx;
                    }
                }
            }

        skipx:

            widthRange -= x;

            // Scan vertically until the first non-0 alpha pixel is found
            for (y = 0; y < height; y++)
            {
                for (int _x = 0; _x < width; _x++)
                {
                    if (fastBitmap.GetPixelInt(_x, y) >> 24 != 0)
                    {
                        goto skipy;
                    }
                }
            }

        skipy:

            heightRange -= y;

            // Scan the width now and skip the empty pixels
            for (; widthRange > x; widthRange--)
            {
                for (int _y = y; _y < height; _y++)
                {
                    if (fastBitmap.GetPixelInt(x + widthRange, _y) >> 24 != 0)
                    {
                        goto skipwidth;
                    }
                }
            }

        skipwidth:

            // Scan the height now and skip the empty pixels
            for (; heightRange > y; heightRange--)
            {
                for (int _x = x; _x < x + widthRange + 1; _x++)
                {
                    if (fastBitmap.GetPixelInt(_x, heightRange + y) >> 24 != 0)
                    {
                        goto skipheight;
                    }
                }
            }

    skipheight:

            fastBitmap.Unlock();

            return new Rectangle(x, y, widthRange + 1, heightRange + 1);
        }

        /// <summary>
        /// Returns the given uint value snapped to the next highest power of two value
        /// </summary>
        /// <param name="value">The value to snap to the closest power of two value</param>
        /// <returns>The given uint value snapped to the next highest power of two value</returns>
        private static uint SnapToNextPowerOfTwo(uint value)
        {
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value++;

            return value;
        }

        /// <summary>
        /// The name of this TextureAtlas. Used on progress reports
        /// </summary>
        private string name;
        
        /// <summary>
        /// List of frames to pack
        /// </summary>
        private List<Frame> frameList;

        /// <summary>
        /// List of frame bounds
        /// </summary>
        private List<Rectangle> boundsList;

        /// <summary>
        /// List of frame origins
        /// </summary>
        private List<Rectangle> originsList;

        /// <summary>
        /// Total area of this TextureAtlas
        /// </summary>
        private Rectangle atlasRectangle;

        /// <summary>
        /// Export settings used by this TextureAtlas
        /// </summary>
        private AnimationExportSettings exportSettings;

        /// <summary>
        /// The default FrameComparision object that will sort frames by size and cache information about the frames' areas
        /// </summary>
        private FrameComparision frameComparision;

        /// <summary>
        /// The current event handler to report progress to
        /// </summary>
        private BundleExportProgressEventHandler progressHandler;

        /// <summary>
        /// Gets the number of frames in this TextureAtlas
        /// </summary>
        public int FrameCount { get { return frameList.Count; } }

        /// <summary>
        /// Gets this atlas' width
        /// </summary>
        public int AtlasWidth { get { return atlasRectangle.Width; } }

        /// <summary>
        /// Gets this atlas' height
        /// </summary>
        public int AtlasHeight { get { return atlasRectangle.Height; } }

        /// <summary>
        /// Gets this atlas' export settings
        /// </summary>
        public AnimationExportSettings ExportSettings { get { return exportSettings; } }

        /// <summary>
        /// Gets the FrameComparision object used during the generation of the texture atlas with information
        /// about frames positioning, frames minimum area, and repeated frames
        /// </summary>
        public FrameComparision GeneratedFrameComparision { get { return frameComparision; } }

        /// <summary>
        /// Default IComparer used to sort and store information about frames
        /// </summary>
        public class FrameComparision : IComparer<Frame>
        {
            /// <summary>
            /// List of internal compare fragments
            /// </summary>
            List<CompareFrag> fragList;

            /// <summary>
            /// List of internal similar fragments
            /// </summary>
            List<SimilarFrag> similarList;

            /// <summary>
            /// Whether to compute the minimum areas of the frames before comparing them
            /// </summary>
            bool useMinimumTextureArea;

            /// <summary>
            /// Gets the number of cached compare fragments currently stores
            /// </summary>
            public int CachedCompareCount { get { return fragList.Count; } }

            /// <summary>
            /// Gets the number of cached similar fragments currently stores
            /// </summary>
            public int CachedSimilarCount { get { return similarList.Count; } }

            /// <summary>
            /// Creates a new instance of the FrameComparision class
            /// </summary>
            /// <param name="useMinimumTextureArea">Whether to compute the minimum areas of the frames before comparing them</param>
            public FrameComparision(bool useMinimumTextureArea)
            {
                this.fragList = new List<CompareFrag>();
                this.similarList = new List<SimilarFrag>();

                this.useMinimumTextureArea = useMinimumTextureArea;
            }

            /// <summary>
            /// Resets this comparer
            /// </summary>
            public void Reset()
            {
                // Clear the references for the GC's sake
                fragList.Clear();
                similarList.Clear();
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
            public int Compare(Frame x, Frame y)
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
            public Rectangle GetFrameArea(Frame frame)
            {
                // Try to find the already-computed frame area first
                foreach (CompareFrag frag in fragList)
                {
                    if (frag.Frame == frame)
                    {
                        return frag.FrameRectangle;
                    }
                }

                CompareFrag newFrag = new CompareFrag();

                newFrag.Frame = frame;

                if (useMinimumTextureArea)
                    newFrag.FrameRectangle = TextureAtlas.FindMinimumTextureArea(frame.GetComposedBitmap());
                else
                    newFrag.FrameRectangle = new Rectangle(0, 0, frame.Width, frame.Height);

                fragList.Add(newFrag);

                return newFrag.FrameRectangle;
            }

            /// <summary>
            /// Registers two frames as being similar to the pixel-level
            /// </summary>
            /// <param name="frame1">The first frame to register</param>
            /// <param name="frame2">The second frame to register</param>
            public void RegisterSimilarFrames(Frame frame1, Frame frame2)
            {
                // Seek similar repeated frames and discard this one if it's repeated
                foreach (SimilarFrag frag in similarList)
                {
                    if ((frag.Frame1 == frame1 && frag.Frame2 == frame2) ||
                        (frag.Frame2 == frame1 && frag.Frame1 == frame2))
                        return;
                }

                similarList.Add(new SimilarFrag() { Frame1 = frame1, Frame2 = frame2 });
            }

            /// <summary>
            /// Gets the original similar frame based on the given frame.
            /// The returned frame is the first frame inserted that is similar
            /// to the given frame. If the given frame is the original similar
            /// frame, null is returned. If no similar frames were stored, null
            /// is returned.
            /// </summary>
            /// <param name="frame">The frame to seek the original similar frame from</param>
            /// <returns>The first frame inserted that is similar to the given frame. If the given frame is the original similar frame, null is returned. If no similar frames were stored, null is returned.</returns>
            public Frame GetOriginalSimilarFrame(Frame frame)
            {
                foreach (SimilarFrag frag in similarList)
                {
                    if (frag.Frame2 == frame)
                    {
                        return frag.Frame1;
                    }
                }

                return null;
            }

            /// <summary>
            /// Internal compare speed-up fragment.
            /// The fragment is composed of a Frame and a Rectangle
            /// that represents the Frame's area
            /// </summary>
            struct CompareFrag
            {
                /// <summary>
                /// The frame of this fragment
                /// </summary>
                public Frame Frame;

                /// <summary>
                /// The frame's rectangle
                /// </summary>
                public Rectangle FrameRectangle;
            }

            /// <summary>
            /// Internal similar speed-up fragment.
            /// The fragment is composed of a pair of frames that
            /// are identical in the pixel level
            /// </summary>
            struct SimilarFrag
            {
                /// <summary>
                /// The first similar frame
                /// </summary>
                public Frame Frame1;

                /// <summary>
                /// The second similar frame
                /// </summary>
                public Frame Frame2;
            }
        }
    }
}