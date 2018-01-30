﻿/*
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
using System.Drawing;
using System.Reflection;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixDirectX.Rendering;

namespace PixUI.Controls.PropertyGrid
{
    /// <summary>
    /// A property grid-style control.
    /// </summary>
    public class PropertyGridControl : ScrollViewControl
    {
        [CanBeNull] 
        private object _selectedObject;

        private readonly List<PropertyField> _propertyFields = new List<PropertyField>();

        private PropertyInspector _propertyInspector;

        /// <summary>
        /// Gets or sets the object to display the propreties of on this property grid
        /// </summary>
        [CanBeNull]
        public object SelectedObject
        {
            get => _selectedObject;
            set
            {
                _selectedObject = value;
                ReloadFields();
            }
        }
        
        /// <summary>
        /// Creates a new instance of <see cref="PropertyGridControl"/>
        /// </summary>
        public new static PropertyGridControl Create()
        {
            var grid = new PropertyGridControl();
            grid.Initialize();

            return grid;
        }

        private void ReloadFields()
        {
            Clear();

            if (_selectedObject == null)
                return;

            LoadFields(_selectedObject);
        }

        private void Clear()
        {
            _propertyInspector = null;
            contentOffset = Vector.Zero;

            foreach (var field in _propertyFields)
            {
                field.RemoveFromParent();
            }

            _propertyFields.Clear();
        }

        private void LoadFields([NotNull] object source)
        {
            _propertyInspector = new PropertyInspector(source);

            var properties = _propertyInspector.GetProperties();

            const float itemHeight = 30;
            float itemY = 0;

            foreach (var property in properties)
            {
                var field = new PropertyField(property)
                {
                    Y = itemY,
                    Size = new Vector(VisibleContentBounds.Width, itemHeight)
                };
                _propertyFields.Add(field);

                AddChild(field);

                itemY += itemHeight;
            }

            ContentSize = new Vector(0, itemY);
        }

        public override void Layout()
        {
            base.Layout();

            foreach (var field in _propertyFields)
            {
                field.Width = VisibleContentBounds.Width;
            }
        }

        private sealed class PropertyField : ControlView
        {
            private readonly LabelViewControl _label;
            private readonly TextField _textField;
            private readonly InspectableProperty _inspect;

            internal PropertyField([NotNull] InspectableProperty inspect)
            {
                BackColor = Color.Transparent;

                _inspect = inspect;
                _label = LabelViewControl.Create(inspect.Name);
                _label.BackColor = Color.Transparent;
                _label.ForeColor = Color.White;
                _label.TextFont = new Font(FontFamily.GenericSansSerif, 14);
                _label.VerticalTextAlignment = VerticalTextAlignment.Center;

                _textField = TextField.Create();
                _textField.Text = inspect.GetValue()?.ToString() ?? "<null>";
                _textField.Editable = inspect.CanSet;

                Initialize();
            }

            private void Initialize()
            {
                AddChild(_label);
                AddChild(_textField);

                Layout();
            }

            public override void Layout()
            {
                base.Layout();

                var labelFrame = new AABB(0, 0, Height, Width / 2);
                labelFrame = labelFrame.Inset(new InsetBounds(2, 2, 2, 2));
                var textFieldFrame = new AABB(Width / 2, 0, Height, Width);

                _label.SetFrame(labelFrame);
                _textField.SetFrame(textFieldFrame);
            }
        }

        /// <summary>
        /// Class that extracts inspectable properties out of objects to display on a property grid.
        /// </summary>
        public class PropertyInspector
        {
            [NotNull]
            private readonly object _target;

            /// <summary>
            /// Initializes this property inspector with a target object to inspect.
            /// </summary>
            public PropertyInspector([NotNull] object target)
            {
                _target = target;
            }

            /// <summary>
            /// Gets all inspectable properties from the inspected object.
            /// </summary>
            public InspectableProperty[] GetProperties()
            {
                var result = new List<InspectableProperty>();
                var properties = _target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var property in properties)
                {
                    if (property.GetIndexParameters().Length > 0)
                        continue;
                    // Cannot inspect set-only methods
                    if (property.GetGetMethod(false) == null)
                        continue;

                    var prop = new InspectableProperty(_target, property);
                    result.Add(prop);
                }

                return result.ToArray();
            }
        }

        /// <summary>
        /// An inspectable property for a property grid
        /// </summary>
        public class InspectableProperty
        {
            internal readonly object Target;
            internal readonly PropertyInfo Property;

            /// <summary>
            /// The name of this property
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Gets the type for the property
            /// </summary>
            public Type PropertyType { get; }

            /// <summary>
            /// Whether this property can be set
            /// </summary>
            public bool CanSet { get; }

            internal InspectableProperty([NotNull] object target, [NotNull] PropertyInfo property)
            {
                Property = property;
                Target = target;

                Name = property.Name;
                PropertyType = property.PropertyType;
                CanSet = property.CanWrite && property.GetSetMethod(false) != null;
            }

            /// <summary>
            /// Gets the value of this inspectable property.
            /// </summary>
            [CanBeNull]
            public object GetValue()
            {
                return Property.GetValue(Target);
            }

            /// <summary>
            /// Sets the value of this inspectable property.
            /// 
            /// Throws an exception, if the value is not settable, or its type does not
            /// match <see cref="PropertyType"/>.
            /// </summary>
            public void SetValue([CanBeNull] object value)
            {
                if (!CanSet)
                    throw new InvalidOperationException("Property cannot be set: it is read-only.");

                if (value == null && PropertyType.IsValueType)
                    throw new ArgumentException($@"Cannot set property of value type {PropertyType} to null.", nameof(value));

                if (value != null && value.GetType() != PropertyType)
                    throw new ArgumentException($@"Expected value of type {PropertyType}, but received {value.GetType()} instead.", nameof(value));

                Property.SetValue(Target, value);
            }
        }
    }
}