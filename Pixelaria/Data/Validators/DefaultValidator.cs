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
using System.Linq;

using Pixelaria.Controllers;

namespace Pixelaria.Data.Validators
{
    /// <summary>
    /// Default validator
    /// </summary>
    public class DefaultValidator : IAnimationValidator, IAnimationSheetValidator
    {
        /// <summary>
        /// The controller that owns this validator
        /// </summary>
        readonly Controller _controller;

        /// <summary>
        /// Default constructor for the IAnimationValidator interface
        /// </summary>
        /// <param name="controller">The controller that controls the application flow</param>
        public DefaultValidator(Controller controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Validates the given Animation field, and returns a string based on the validation results.
        /// If the validation fails, an error string is returned, if it succeeds, an empty string is returned.
        /// </summary>
        /// <param name="name">The Animation name to validate</param>
        /// <param name="anim">The Animation that the field is comming from</param>
        /// <returns>The result of the validation. An empty string if the field is valid, or an error message if it's invalid.</returns>
        public string ValidateAnimationName(string name, Animation anim = null)
        {
            if (name.Trim() == "")
                return "The name of the animation cannot be empty";

            // Check any case of repeated animation name
            if (_controller.CurrentBundle != null)
            {
                if (_controller.CurrentBundle.Animations.Any(canim => canim.Name == name && !ReferenceEquals(canim, anim)))
                {
                    return "The name '" + name + "' conflicts with another animation in the project";
                }
            }

            return IsStringValidFileName(name);
        }

        /// <summary>
        /// Validates the given Animation field, and returns a string based on the validation results.
        /// If the validation fails, an error string is returned, if it succeeds, an empty string is returned.
        /// </summary>
        /// <param name="width">The Animation width to validate</param>
        /// <param name="anim">The Animation that the field is comming from</param>
        /// <returns>The result of the validation. An empty string if the field is valid, or an error message if it's invalid. </returns>
        public string ValidateAnimationWidth(int width, Animation anim = null)
        {
            if (width < 1 || width > 4096)
                return "The width of the animation (" + width + ") is invalid. It must be between 1 and 4096";

            return "";
        }

        /// <summary>
        /// Validates the given Animation field, and returns a string based on the validation results.
        /// If the validation fails, an error string is returned, if it succeeds, an empty string is returned.
        /// </summary>
        /// <param name="height">The Animation height to validate</param>
        /// <param name="anim">The Animation that the field is comming from</param>
        /// <returns>The result of the validation. An empty string if the field is valid, or an error message if it's invalid. </returns>
        public string ValidateAnimationHeight(int height, Animation anim = null)
        {
            if (height < 1 || height > 4096)
                return "The height of the animation (" + height + ") is invalid. It must be between 1 and 4096";

            return "";
        }

        /// <summary>
        /// Validates the given Animation field, and returns a string based on the validation results.
        /// If the validation fails, an error string is returned, if it succeeds, an empty string is returned.
        /// </summary>
        /// <param name="fps">The Animation fps to validate</param>
        /// <param name="anim">The Animation that the field is comming from</param>
        /// <returns>The result of the validation. An empty string if the field is valid, or an error message if it's invalid. </returns>
        public string ValidateAnimationFPS(int fps, Animation anim = null)
        {
            if (fps < -1 || fps > 420)
                return "The fps of the animation (" + fps + ") is invalid. It must be between -1 and 420";

            return "";
        }

        /// <summary>
        /// Validates the given AnimationSheet field, and returns a string based on the validation results.
        /// If the validation fails, an error string is returned, if it succeeds, an empty string is returned.
        /// </summary>
        /// <param name="name">The AnimationSheet name to validate</param>
        /// <param name="sheet">The AnimationSheet the field is coming from</param>
        /// <returns>The result of the validation. An empty string if the field is valid, or an error message if it's invalid.</returns>
        public string ValidateAnimationSheetName(string name, AnimationSheet sheet = null)
        {
            if (name.Trim() == "")
                return "The name of the animation sheet cannot be empty";

            string validate = IsStringValidFileName(name);

            if (validate != "")
                return validate;

            // Check any case of repeated animation sheet name
            if (_controller.CurrentBundle != null)
            {
                if (_controller.CurrentBundle.AnimationSheets.Any(csheet => csheet.Name == name && csheet != sheet))
                {
                    return "The name '" + name + "' conflicts with another animation sheet in the project";
                }
            }

            return "";
        }

        /// <summary>
        /// Validates whether the given string can be used as a file name, and returns a string based on the validation results.
        /// If the validation fails, an error string is returned, if it succeeds, an empty string is returned.
        /// </summary>
        /// <param name="text">The string to validate</param>
        /// <returns>The result of the validation. An empty string if the field is valid, or an error message if it's invalid.</returns>
        private string IsStringValidFileName(string text)
        {
            if (text.Trim() == "")
                return "The file name cannot be empty.";

            char[] invalid = Path.GetInvalidFileNameChars();

            foreach (char inv in invalid)
            {
                if (text.Contains(inv))
                {
                    return "The character '" + inv + "' is invalid for use as a file name.";
                }
            }

            return "";
        }
    }
}