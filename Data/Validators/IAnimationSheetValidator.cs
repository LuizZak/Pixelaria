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

namespace Pixelaria.Data.Validators
{
    /// <summary>
    /// Interface to be implemented by objects that validate AnimationSheet objects and fields
    /// </summary>
    public interface IAnimationSheetValidator
    {
        /// <summary>
        /// Validates the given AnimationSheet field, and returns a string based on the validation results.
        /// If the validation fails, an error string is returned, if it succeeds, an empty string is returned.
        /// </summary>
        /// <param name="name">The AnimationSheet name to validate</param>
        /// <param name="sheet">The AnimationSheet the field is coming from</param>
        /// <returns>The result of the validation. An empty string if the field is valid, or an error message if it's invalid.</returns>
        string ValidateAnimationSheetName(string name, AnimationSheet sheet = null);
    }
}