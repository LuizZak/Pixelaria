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
using PixCore.Geometry;

namespace PixUI.Animation
{
    /// <summary>
    /// Provides support for a closure-based animation that is delegated to
    /// an external closure, which receives the animation period as an argument
    /// to apply arbitrary changes.
    /// </summary>
    public class ClosureAnimation : IAnimation
    {
        private readonly Action<double> _closure;
        private double _duration;
        private readonly TimingFunction _timingFunction;
        public double Duration { get; }
        public bool IsFinished { private set; get; }

        public ClosureAnimation(double duration, Action<double> closure)
            : this(duration, TimingFunctions.Linear, closure)
        {

        }

        public ClosureAnimation(double duration, TimingFunction timingFunction, Action<double> closure)
        {
            if (duration <= 0)
                throw new ArgumentException($@"duration must be a positive non-zero value, received {duration:F1}",
                    nameof(duration));

            Duration = duration;
            _closure = closure;
            _duration = duration;
            _timingFunction = timingFunction;
        }

        public void Update(TimeSpan interval)
        {
            if (IsFinished)
                return;
            
            _duration -= interval.TotalSeconds;
            if (_duration <= 0)
            {
                IsFinished = true;
                return;
            }

            _closure(_timingFunction(1 - _duration / Duration));
        }
    }

    /// <summary>
    /// Collection of default timing functions
    /// </summary>
    public static class TimingFunctions
    {
        /// <summary>
        /// A linear timing function from 0 to 1.
        /// </summary>
        public static TimingFunction Linear = t => t;

        /// <summary>
        /// A timing function that has the start changing slower than the ending.
        /// </summary>
        public static TimingFunction EaseIn = p => p * p * p;

        /// <summary>
        /// A timing function that has the start changing faster than the ending.
        /// </summary>
        public static TimingFunction EaseOut = p =>
        {
            double f = p - 1;
            return f * f * f + 1;
        };

        /// <summary>
        /// A timing function that has the middle changing faster than the starting and ending.
        /// </summary>
        public static TimingFunction EaseInOut = p =>
        {
            if (p < 0.5f)
                return 4 * p * p * p;

            double f = 2 * p - 2;
            return 0.5f * f * f * f + 1;
        };

        // TODO: Create a proper bezier easing function.
        // ref: https://github.com/gre/bezier-easing
    }

    /// <summary>
    /// A timing function that takes in an input time value that is normalized
    /// between [0-1], and outputs a point on a curve for that time period.
    /// </summary>
    public delegate double TimingFunction(double time);
}
