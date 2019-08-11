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
using System.ComponentModel;
using System.Drawing.Design;
using System.Reflection;
using JetBrains.Annotations;
using PixPipelineGraph;

namespace Pixelaria.ExportPipeline
{
    /// <summary>
    /// Wraps a <see cref="PropertyInfo"/> into an <see cref="IEditableProperty"/> interface.
    ///
    /// Wrapped properties must be of types that provide both a <see cref="System.Drawing.Design.UITypeEditor"/>
    /// and a <see cref="System.ComponentModel.TypeConverter"/>.
    /// </summary>
    sealed class EditableTypePropertyProxy : IEditableProperty
    {
        [NotNull]
        private readonly object _target;
        [NotNull]
        private readonly PropertyInfo _propertyInfo;

        [NotNull]
        public string PropertyName { get; }
        [NotNull]
        public Type PropertyType { get; }
        [NotNull]
        public Type TypeEditor { get; }
        [NotNull]
        public Type TypeConverter { get; }

        public EditableTypePropertyProxy([NotNull] object target, [NotNull] PropertyInfo propertyInfo)
        {
            _target = target;
            _propertyInfo = propertyInfo;

            PropertyName = _propertyInfo.Name;
            PropertyType = _propertyInfo.PropertyType;

            var editorAttribute =
                _propertyInfo.GetCustomAttribute<EditorAttribute>()
                ?? PropertyType.GetCustomAttribute<EditorAttribute>();

            var typeConverterAttribute =
                _propertyInfo.GetCustomAttribute<TypeConverterAttribute>()
                ?? PropertyType.GetCustomAttribute<TypeConverterAttribute>();

            if (editorAttribute == null || typeConverterAttribute == null)
                throw new InvalidOperationException(
                    $"Can only create {nameof(EditableTypePropertyProxy)} for properties with types implementing {nameof(EditorAttribute)} and {nameof(TypeConverterAttribute)}");

            if(Type.GetType(editorAttribute.EditorBaseTypeName) != typeof(UITypeEditor))
                throw new InvalidOperationException(
                    $"Expected {nameof(EditorAttribute)} to provide editor of base type {nameof(UITypeEditor)}, got {editorAttribute.EditorBaseTypeName}, instead.");

            TypeEditor = Type.GetType(editorAttribute.EditorTypeName)
                         ?? throw new InvalidOperationException($"Could not resolve type editor of type {editorAttribute.EditorTypeName}.");
            TypeConverter = Type.GetType(typeConverterAttribute.ConverterTypeName)
                            ?? throw new InvalidOperationException(
                                $"Could not resolve type converter of type {typeConverterAttribute.ConverterTypeName}.");
        }
        
        public void SetValue(object value)
        {
            _propertyInfo.SetValue(_target, value);
        }

        public object GetValue()
        {
            return _propertyInfo.GetValue(_target);
        }
    }
}
