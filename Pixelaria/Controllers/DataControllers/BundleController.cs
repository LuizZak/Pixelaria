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

using System.Linq;
using Pixelaria.Data;

namespace Pixelaria.Controllers.DataControllers
{
    /// <summary>
    /// Class that controls the manipulation of bundles
    /// </summary>
    public class BundleController
    {
        private readonly Bundle _source;

        public BundleController(Bundle source)
        {
            _source = source;
        }

        /// <summary>
        /// Returns a copy of all animations stored on this animation controller
        /// </summary>
        public IAnimationId[] GetAnimations()
        {
            return _source.Animations.Select(AnimationIdFor).ToArray();
        }

        /// <summary>
        /// Gets the metadata information for a given animation ID.
        /// If no animation with a given ID was found, null is returned
        /// </summary>
        public AnimationMetadata? GetAnimationMetadata(IAnimationId animationId)
        {
            var anim = _source.Animations.FirstOrDefault(a => a.ID == animationId.Id);
            if (anim == null)
                return null;

            return new AnimationMetadata(anim.Name, anim.FrameCount);
        }

        /// <summary>
        /// Gets the frame controller for a given animation.
        /// Returns null, if no animation was found with that 
        /// </summary>
        /// <param name="animation">The ID of the animation to get the frame controller of</param>
        public AnimationController GetAnimationController(IAnimationId animation)
        {
            var animId = (AnimId)animation;
            var anim = _source.GetAnimationByID(animId.Id);

            return new AnimationController(anim);
        }

        /// <summary>
        /// Gets an animation ID by display name
        /// </summary>
        /// <returns>The animation ID for a given animation name - or null, if none was found</returns>
        public IAnimationId GetAnimationByName(string name)
        {
            return _source.Animations.Where(a => a.Name == name).Select(AnimationIdFor).FirstOrDefault();
        }

        private static IAnimationId AnimationIdFor(Animation animation)
        {
            return new AnimId(animation.ID);
        }

        private struct AnimId : IAnimationId
        {
            public int Id { get; }

            public AnimId(int id)
            {
                Id = id;
            }

            public override string ToString()
            {
                return Id.ToString();
            }
        }
    }
}
