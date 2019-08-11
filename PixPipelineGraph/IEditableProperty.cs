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

namespace PixPipelineGraph
{
    /// <summary>
    /// Describes a property from a pipeline metadata object that can be directly edited by the user.
    /// </summary>
    public interface IEditableProperty
    {
        /// <summary>
        /// Gets the display name of this property.
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// Gets the associated type for the property.
        /// </summary>
        Type PropertyType { get; }

        /// <summary>
        /// Gets the associated type editor for this property.
        ///
        /// Must be a subtype of <see cref="System.Drawing.Design.UITypeEditor"/>.
        /// </summary>
        Type TypeEditor { get; }

        /// <summary>
        /// Gets type of the associated type converter for the property's type.
        ///
        /// Must be a subtype of <see cref="System.ComponentModel.TypeConverter"/>.
        /// </summary>
        Type TypeConverter { get; }

        /// <summary>
        /// Sets the value for this property.
        /// </summary>
        void SetValue(object value);

        /// <summary>
        /// Gets the currently designated value for this property.
        /// </summary>
        object GetValue();
    }
}
