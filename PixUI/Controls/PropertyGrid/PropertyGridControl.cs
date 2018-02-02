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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixDirectX.Rendering;
using Color = System.Drawing.Color;

namespace PixUI.Controls.PropertyGrid
{
    /// <summary>
    /// A property grid-style control.
    /// </summary>
    public class PropertyGridControl : ScrollViewControl
    {
        private object[] _selectedObjects = new object[0];

        private readonly List<PropertyField> _propertyFields = new List<PropertyField>();

        private PropertyInspector _propertyInspector;

        /// <summary>
        /// Gets or sets the object to display the propreties of on this property grid
        /// </summary>
        [CanBeNull]
        public object SelectedObject
        {
            get => _selectedObjects.FirstOrDefault();
            set
            {
                if (value == null)
                {
                    _selectedObjects = new object[0];
                }
                else
                {
                    _selectedObjects = new []{ value };
                }
                
                ReloadFields();
            }
        }

        /// <summary>
        /// Gets or sets the array of selected objects currently being displayed.
        /// 
        /// The property grid displays all properties in common across the objects in the array.
        /// 
        /// Setting this value override any value that may be set at <see cref="SelectedObject"/>.
        /// </summary>
        public object[] SelectedObjects
        {
            get => _selectedObjects;
            set
            {
                _selectedObjects = value;
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

            if (SelectedObject == null)
                return;

            LoadFields(SelectedObjects);
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

        private void LoadFields([NotNull] object[] sources)
        {
            _propertyInspector = new PropertyInspector(sources);

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
            private readonly TypeConverter _typeConverter;

            internal PropertyField([NotNull] InspectableProperty inspect)
            {
                BackColor = Color.Transparent;
                StrokeColor = Color.FromArgb(50, 50, 50);

                _inspect = inspect;
                _label = LabelViewControl.Create(inspect.Name);
                _label.BackColor = Color.Transparent;
                _label.ForeColor = Color.White;
                _label.TextFont = new Font(FontFamily.GenericSansSerif.Name, 11);
                _label.VerticalTextAlignment = VerticalTextAlignment.Center;

                _textField = TextField.Create();
                _textField.Text = "";
                _textField.Editable = inspect.CanSet;
                _textField.AcceptsEnterKey = true;
                _textField.EnterKey += TextFieldOnEnterKey;
                _textField.ResignedFirstResponder += TextFieldOnResignedFirstResponder;
                
                // Textfield style
                var stylePlain = TextFieldVisualStyleParameters.DefaultDarkStyle();
                stylePlain.TextColor = Color.LightGray;

                var styleEditing = TextFieldVisualStyleParameters.DefaultDarkStyle();
                styleEditing.StrokeColor = Color.CornflowerBlue;
                styleEditing.StrokeWidth = 1.5f;

                var styleHighlighted = TextFieldVisualStyleParameters.DefaultDarkStyle();
                styleHighlighted.StrokeColor = Color.CornflowerBlue;
                
                _textField.SetStyleForState(stylePlain, ControlViewState.Normal);
                _textField.SetStyleForState(styleHighlighted, ControlViewState.Highlighted);
                _textField.SetStyleForState(styleEditing, ControlViewState.Focused);

                // Load initial value
                var converterAttr = _inspect.PropertyType.GetCustomAttribute<TypeConverterAttribute>();
                if (converterAttr != null)
                {
                    string converterName = converterAttr.ConverterTypeName;

                    var converterType = Type.GetType(converterName);
                    if (converterType != null)
                    {
                        _typeConverter =
                            converterType.GetConstructor(Type.EmptyTypes)?.Invoke(new object[0]) as TypeConverter;
                    }
                }
                
                ReloadValue();

                Initialize();
            }

            private void TextFieldOnEnterKey(object sender, EventArgs eventArgs)
            {
                if (!_inspect.CanSet)
                    return;

                TrySetValueFromString(_textField.Text);

                // Re-select as a visual feedback to the user the input was accepted
                _textField.SelectAll();
            }

            private void TextFieldOnResignedFirstResponder(object sender, EventArgs eventArgs)
            {
                if (!_inspect.CanSet)
                    return;

                TrySetValueFromString(_textField.Text);
            }
            
            // TODO: Consider moving this method/responsibility to InspectableProperty
            private void TrySetValueFromString(string str)
            {
                if (_typeConverter != null)
                {
                    if (!_typeConverter.CanConvertFrom(typeof(string)))
                        return;

                    _inspect.SetValue(_typeConverter.ConvertFromString(str));
                }
                else if (_inspect.PropertyType == typeof(string))
                {
                    _inspect.SetValue(str);
                }
                else
                {
                    // When empty, fill with the default value of the type (in case it has one)
                    if (string.IsNullOrEmpty(str) && _inspect.PropertyType.IsValueType)
                    {
                        _inspect.SetValue(Activator.CreateInstance(_inspect.PropertyType));
                    }
                    else
                    {
                        var typeCode = Type.GetTypeCode(_inspect.PropertyType);
                    
                        try
                        {
                            if (typeCode != TypeCode.Object)
                            {
                                object newValue = Convert.ChangeType(str, typeCode);
                                if (newValue != null)
                                    _inspect.SetValue(newValue);
                            }
                        }
                        catch
                        {
                            // Empty
                        }
                    }
                }

                ReloadValue();
            }

            private void ReloadValue()
            {
                HashSet<string> representations;

                if (_typeConverter != null && _typeConverter.CanConvertTo(typeof(string)))
                {
                    var values = _inspect.GetValues();

                    // See if all values reduce to the same value representation.
                    representations = new HashSet<string>(values.Select(_typeConverter.ConvertToString));
                }
                else
                {
                    representations = new HashSet<string>(_inspect.GetValues().Select(v => v.ToString()));
                }

                if (representations.Count > 1)
                {
                    _textField.Text = "<multiple values>";
                }
                else if (representations.Count == 1)
                {
                    _textField.Text = representations.FirstOrDefault() ?? "";
                }
                else
                {
                    _textField.Text = "";
                }
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
                labelFrame = labelFrame.Inset(new InsetBounds(8, 2, 2, 8));
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
            private readonly object[] _targets;

            /// <summary>
            /// Initializes this property inspector with a target object to inspect.
            /// </summary>
            public PropertyInspector([NotNull] object target)
            {
                _targets = new[] {target};
            }
            
            /// <summary>
            /// Initializes this property inspector with multiple target objects to inspect.
            /// </summary>
            public PropertyInspector([NotNull] object[] targets)
            {
                _targets = targets;
            }

            /// <summary>
            /// Gets all inspectable properties from the inspected object.
            /// </summary>
            public InspectableProperty[] GetProperties()
            {
                var result = new List<InspectableProperty>();

                var allProperties =
                    _targets.SelectMany(type =>
                        type.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance));

                var reducedProperties =
                    allProperties.GroupBy(prop => (prop.PropertyType, prop.Name));

                var propertyGroups =
                    reducedProperties
                        .Where(g => g.Count() == _targets.Length)
                        .Select(g => g.ToArray());
                
                foreach (var properties in propertyGroups)
                {
                    bool valid = true;
                    foreach (var property in properties)
                    {
                        if (property.GetIndexParameters().Length > 0)
                        {
                            valid = false;
                            break;
                        }
                        // Cannot inspect set-only methods
                        if (property.GetGetMethod(false) == null)
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (!valid)
                        continue;
                    
                    var prop = new InspectableProperty(_targets, properties);
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
            internal readonly object[] Targets;
            internal readonly PropertyInfo[] Properties;

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

            internal InspectableProperty([NotNull] object[] targets, [NotNull] PropertyInfo[] properties)
            {
                Properties = properties;
                Targets = targets;

                Name = properties[0].Name;
                PropertyType = properties[0].PropertyType;
                CanSet = properties[0].CanWrite && properties[0].GetSetMethod(false) != null;
            }

            /// <summary>
            /// Gets the values of this inspectable property across all associated target objects.
            /// </summary>
            [NotNull, ItemCanBeNull]
            public object[] GetValues()
            {
                return Targets.Zip(Properties, (obj, prop) => (obj, prop)).Select(pair => pair.prop.GetValue(pair.obj)).ToArray();
            }

            /// <summary>
            /// Sets the value of this inspectable property across all associated objects.
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

                foreach (var (obj, prop) in Targets.Zip(Properties, (obj, prop) => (obj, prop)))
                {
                    prop.SetValue(obj, value);
                }
            }
        }
    }
}
