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

namespace PixLib.Data
{
    /// <summary>
    /// Provides an abstraction for objects that provide animations and related export settings for exporters
    /// </summary>
    public interface IAnimationProvider
    {
        /// <summary>
        /// Gets the export settings for this animation sheet
        /// </summary>
        AnimationSheetExportSettings SheetExportSettings { get; }

        /// <summary>
        /// Gets the name of this animation provider.
        /// This is used to identify namely the provider in places where it needs to be.
        /// Ideally should be unique among providers in the same algorithm
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Animations on this provider
        /// </summary>
        IAnimation[] GetAnimations();
    }

    /// <summary>
    /// A basic wrapper for an animation provider, which can be used in places where animations simply have to be associated with an export settings object
    /// </summary>
    public struct BasicAnimationProvider : IAnimationProvider
    {
        private readonly IAnimation[] _animations;

        /// <summary>
        /// Gets the export settings associated with this basic provider
        /// </summary>
        public AnimationSheetExportSettings SheetExportSettings { get; }

        /// <summary>
        /// Gets the display name for this basic provider
        /// </summary>
        public string Name { get; }

        public BasicAnimationProvider(IAnimation[] animations, AnimationSheetExportSettings sheetExportSettings, string name)
        {
            _animations = animations;
            SheetExportSettings = sheetExportSettings;
            Name = name;
        }

        /// <summary>
        /// Gets the animations listed on this basic provider
        /// </summary>
        public IAnimation[] GetAnimations()
        {
            return _animations;
        }
    }
}