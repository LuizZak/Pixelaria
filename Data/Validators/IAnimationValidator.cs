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
using System.Text;

using Pixelaria.Controllers;

namespace Pixelaria.Data.Validators
{
    /// <summary>
    /// Interface to be implemented by objects that validate Animation objects and fields
    /// </summary>
    public interface IAnimationValidator
    {
        /// <summary>
        /// Validates the given Animation field, and returns a string based on the validation results.
        /// If the validation fails, an error string is returned, if it succeeds, an empty string is returned.
        /// </summary>
        /// <param name="name">The Animation name to validate</param>
        /// <param name="anim">The Animation that the field is comming from</param>
        /// <returns>The result of the validation. An empty string if the field is valid, or an error message if it's invalid.</returns>
        string ValidateAnimationName(string name, Animation anim = null);

        /// <summary>
        /// Validates the given Animation field, and returns a string based on the validation results.
        /// If the validation fails, an error string is returned, if it succeeds, an empty string is returned.
        /// </summary>
        /// <param name="width">The Animation width to validate</param>
        /// <param name="anim">The Animation that the field is comming from</param>
        /// <returns>The result of the validation. An empty string if the field is valid, or an error message if it's invalid. </returns>
        string ValidateAnimationWidth(int width, Animation anim = null);

        /// <summary>
        /// Validates the given Animation field, and returns a string based on the validation results.
        /// If the validation fails, an error string is returned, if it succeeds, an empty string is returned.
        /// </summary>
        /// <param name="height">The Animation height to validate</param>
        /// <param name="anim">The Animation that the field is comming from</param>
        /// <returns>The result of the validation. An empty string if the field is valid, or an error message if it's invalid. </returns>
        string ValidateAnimationHeight(int height, Animation anim = null);

        /// <summary>
        /// Validates the given Animation field, and returns a string based on the validation results.
        /// If the validation fails, an error string is returned, if it succeeds, an empty string is returned.
        /// </summary>
        /// <param name="fps">The Animation fps to validate</param>
        /// <param name="anim">The Animation that the field is comming from</param>
        /// <returns>The result of the validation. An empty string if the field is valid, or an error message if it's invalid. </returns>
        string ValidateAnimationFPS(int fps, Animation anim = null);
    }
}