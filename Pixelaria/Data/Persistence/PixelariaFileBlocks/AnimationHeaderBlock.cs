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

using System.IO;
using JetBrains.Annotations;

namespace Pixelaria.Data.Persistence.PixelariaFileBlocks
{
    /// <summary>
    /// Represents an animation block to save/load from a file
    /// </summary>
    public class AnimationHeaderBlock : FileBlock
    {
        /// <summary>
        /// The animation being manipulated by this animation header block
        /// </summary>
        public Animation Animation { get; private set; }

        /// <summary>
        /// Initializes a new instance of the AnimationHeaderBlock class
        /// </summary>
        public AnimationHeaderBlock()
        {
            blockID = BLOCKID_ANIMATION_HEADER;
            removeOnPrepare = true;
        }

        /// <summary>
        /// Initializes a new instance of the AnimationHeaderBlock class
        /// </summary>
        public AnimationHeaderBlock(Animation animation)
            : this()
        {
            Animation = animation;
        }

        /// <summary>
        /// Loads the content portion of this block from the given stream
        /// </summary>
        /// <param name="stream">The stream to load the content portion from</param>
        protected override void LoadContentFromStream(Stream stream)
        {
            Animation = LoadAnimationFromStream(stream);
        }

        /// <summary>
        /// Saves the content portion of this block to the given stream
        /// </summary>
        /// <param name="stream">The stream to save the content portion to</param>
        protected override void SaveContentToStream(Stream stream)
        {
            SaveAnimationToStream(Animation, stream);
        }

        /// <summary>
        /// Prepares the contents of this animation to be saved based on the contents of the given Bundle
        /// </summary>
        /// <param name="bundle">The bundle to prepare this block from</param>
        public override void PrepareFromBundle(Bundle bundle)
        {
            base.PrepareFromBundle(bundle);

            // Add file blocks for each of the frames for this animation
            foreach (var frame in Animation.Frames)
            {
                owningFile?.AddBlock(new FrameBlock(frame));
            }
        }

        /// <summary>
        /// Loads an Animation from the given stream, using the specified version
        /// number when reading properties
        /// </summary>
        /// <param name="stream">The stream to load the animation from</param>
        /// <returns>The Animation object loaded</returns>
        protected Animation LoadAnimationFromStream([NotNull] Stream stream)
        {
            var reader = new BinaryReader(stream);

            int id = reader.ReadInt32();
            string name = reader.ReadString();
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            int fps = reader.ReadInt32();
            bool frameskip = reader.ReadBoolean();

            var anim = new Animation(name, width, height)
            {
                ID = id,
                PlaybackSettings = new AnimationPlaybackSettings
                {
                    FPS = fps,
                    FrameSkip = frameskip
                }
            };

            return anim;
        }

        /// <summary>
        /// Saves the given Animation object to the given stream
        /// </summary>
        /// <param name="animation">The animation to save</param>
        /// <param name="stream">The stream to save the animation to</param>
        protected void SaveAnimationToStream([NotNull] Animation animation, [NotNull] Stream stream)
        {
            var writer = new BinaryWriter(stream);

            writer.Write(animation.ID);
            writer.Write(animation.Name);
            writer.Write(animation.Width);
            writer.Write(animation.Height);
            writer.Write(animation.PlaybackSettings.FPS);
            writer.Write(animation.PlaybackSettings.FrameSkip);
        }
    }
}