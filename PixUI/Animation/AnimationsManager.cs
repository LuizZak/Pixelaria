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
using System.Linq;

namespace PixUI.Animation
{
    /// <summary>
    /// Class that manages Animation objects running on a render loop.
    /// </summary>
    public class AnimationsManager
    {
        private readonly List<IAnimation> _animations = new List<IAnimation>();

        /// <summary>
        /// Gets an array of all currently executing animations
        /// </summary>
        public IAnimation[] Animations => _animations.ToArray();

        public void AddAnimation(IAnimation animation)
        {
            _animations.Add(animation);
        }

        /// <summary>
        /// Updates the animations running on this manager using a given time step.
        /// </summary>
        /// <param name="interval">Interval since the last update frame</param>
        public void Update(TimeSpan interval)
        {
            if (_animations.Count == 0)
                return;

            foreach (var animation in _animations.Where(a => !a.IsFinished))
            {
                animation.Update(interval);
            }

            _animations.RemoveAll(a => a.IsFinished);
        }
    }
}
