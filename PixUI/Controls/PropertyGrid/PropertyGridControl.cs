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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using PixCore.Colors;
using PixCore.Geometry;
using PixRendering;
using Color = System.Drawing.Color;

namespace PixUI.Controls.PropertyGrid
{
    /// <summary>
    /// A property grid-style control.
    /// </summary>
    public class PropertyGridControl : ControlView
    {
        private object[] _selectedObjects = new object[0];

        private readonly ScrollViewControl _scrollView = ScrollViewControl.Create();
        private readonly List<PropertyField> _propertyFields = new List<PropertyField>();
        private readonly LabelViewControl _titleLabel = LabelViewControl.Create("Properties Panel - No selection");

        private PropertyInspector _propertyInspector;

        #region Events

        /// <summary>
        /// Event raised whenever the user updates a property on this properties grid control.
        /// </summary>
        public event InspectablePropertyChangedEventHandler InspectablePropertyChanged;

        #endregion

        /// <summary>
        /// Gets or sets the object to display the properties of on this property grid
        /// </summary>
        [CanBeNull]
        public object SelectedObject
        {
            get => _selectedObjects.FirstOrDefault();
            set
            {
                _selectedObjects = value == null ? new object[0] : new[] {value};

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
        public static PropertyGridControl Create()
        {
            var grid = new PropertyGridControl();
            grid.Initialize();

            return grid;
        }

        private PropertyGridControl()
        {

        }

        protected virtual void Initialize()
        {
            BackColor = Color.Transparent;
            _scrollView.BackColor = Color.Transparent;
            _scrollView.ScrollBarsMode = ScrollViewControl.VisibleScrollBars.Vertical;

            _titleLabel.ForeColor = Color.White;
            _titleLabel.BackColor = Color.Transparent;
            _titleLabel.VerticalTextAlignment = VerticalTextAlignment.Center;
            _titleLabel.TextFont = new Font(FontFamily.GenericSansSerif.Name, 12);
            _titleLabel.StrokeColor = Color.Transparent;
            _titleLabel.AutoResize = false;

            AddChild(_titleLabel);
            AddChild(_scrollView);

            Layout();
        }

        private void ReloadFields()
        {
            Clear();

            _titleLabel.Text = "Properties Panel";

            if (SelectedObject == null)
            {
                _titleLabel.Text += " - No selection";
                return;
            }

            if (SelectedObjects.Length == 1)
            {
                _titleLabel.Text += $" - {SelectedObject?.GetType().Name}";
            }
            else
            {
                _titleLabel.Text += $" - {SelectedObjects.Length} objects";
            }

            LoadFields(SelectedObjects);
        }

        private void Clear()
        {
            _propertyInspector = null;
            _scrollView.SetContentOffset(Vector.Zero);

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
                var field = PropertyField.Create(this, property);
                field.Y = itemY;
                field.Size = new Vector(_scrollView.VisibleContentBounds.Width, itemHeight);
                _propertyFields.Add(field);

                _scrollView.AddChild(field);

                itemY += itemHeight;
            }

            _scrollView.ContentSize = new Vector(0, itemY);
        }

        public override void Layout()
        {
            base.Layout();

            _titleLabel.SetFrame(new AABB(0, 0, 30, Width).Inset(new InsetBounds(5, 5, 5, 5)));
            _scrollView.SetFrame(Bounds.Inset(new InsetBounds(0, _titleLabel.Height, 0, 0)));

            foreach (var field in _propertyFields)
            {
                field.Width = _scrollView.VisibleContentBounds.Width;
            }
        }
        
        protected virtual void OnInspectablePropertyChanged(InspectablePropertyChangedEventArgs e)
        {
            InspectablePropertyChanged?.Invoke(this, e);
        }

        internal sealed class PropertyField : ControlView
        {
            private readonly LabelViewControl _label = LabelViewControl.Create();
            private readonly TextField _textField = TextField.Create();
            private readonly ButtonControl _editButton = ButtonControl.Create();
            private InspectableProperty _inspect;
            private TypeConverter _typeConverter;
            private PropertyGridControl _propertyGrid;

            public string Value => _textField.Text;

            internal static PropertyField Create([NotNull] PropertyGridControl propertyGrid, [NotNull] InspectableProperty inspect)
            { 
                var view = new PropertyField
                {
                    BackColor = Color.Transparent, 
                    StrokeColor = Color.FromArgb(255, 50, 50, 50), 
                    _inspect = inspect,
                    _propertyGrid = propertyGrid
                };

                // Load initial value
                var converterAttr = view._inspect.PropertyType.GetCustomAttribute<TypeConverterAttribute>();
                if (converterAttr != null)
                {
                    string converterName = converterAttr.ConverterTypeName;

                    var converterType = Type.GetType(converterName);
                    if (converterType != null)
                    {
                        view._typeConverter =
                            converterType.GetConstructor(Type.EmptyTypes)?.Invoke(new object[0]) as TypeConverter;
                    }
                }
                
                view.Initialize();

                return view;
            }

            private PropertyField()
            {
                
            }
            
            private void Initialize()
            {
                AddChild(_label);
                AddChild(_textField);
                AddChild(_editButton);
                
                _label.Text = _inspect.DisplayName;
                _label.BackColor = Color.Transparent;
                _label.ForeColor = Color.White;
                _label.TextFont = new Font(FontFamily.GenericSansSerif.Name, 11);
                _label.VerticalTextAlignment = VerticalTextAlignment.Center;

                _textField.Text = "";
                _textField.Editable = _inspect.CanSet;
                _textField.AcceptsEnterKey = true;
                _textField.EnterKey += TextFieldOnEnterKey;
                _textField.ResignedFirstResponder += TextFieldOnResignedFirstResponder;

                _editButton.Text = "...";
                _editButton.NormalColor = Color.Black.WithTransparency(0.3f);
                _editButton.HighlightColor = Color.White.WithTransparency(1).BlendedOver(Color.Black).WithTransparency(0.3f);
                _editButton.StrokeColor = Color.White;
                _editButton.TextColor = Color.White;
                
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

                // Editing button misc configurations
                _editButton.Visible = _inspect.HasTypeEditor();
                _editButton.Clicked += (sender, args) =>
                {
                    _inspect.InvokeEditorUi();
                    ReloadValue();
                };

                ReloadValue();
                Layout();
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

                    object value = _typeConverter.ConvertFromString(str);
                    _inspect.SetValue(value);
                    _propertyGrid.OnInspectablePropertyChanged(new InspectablePropertyChangedEventArgs(_inspect, value));
                }
                else if (_inspect.PropertyType == typeof(string))
                {
                    _inspect.SetValue(str);
                    _propertyGrid.OnInspectablePropertyChanged(new InspectablePropertyChangedEventArgs(_inspect, str));
                }
                else
                {
                    // When empty, fill with the default value of the type (in case it has one)
                    if (string.IsNullOrEmpty(str) && _inspect.PropertyType.IsValueType)
                    {
                        object value = Activator.CreateInstance(_inspect.PropertyType);
                        _inspect.SetValue(value);
                        _propertyGrid.OnInspectablePropertyChanged(new InspectablePropertyChangedEventArgs(_inspect, value));
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
                                {
                                    _inspect.SetValue(newValue);
                                    _propertyGrid.OnInspectablePropertyChanged(new InspectablePropertyChangedEventArgs(_inspect, newValue));
                                }
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

                if (typeof(ICollection).IsAssignableFrom(_inspect.PropertyType))
                {
                    var values = _inspect.GetValues();

                    representations = new HashSet<string>(values.Select(o =>
                    {
                        if (!(o is ICollection list))
                            return "<null>";

                        if (list.Count == 1)
                        {
                            return "[1 value]";
                        }

                        return $"[{list.Count} values]";
                    }));
                }
                else if (_typeConverter != null && _typeConverter.CanConvertTo(typeof(string)))
                {
                    var values = _inspect.GetValues();

                    // See if all values reduce to the same value representation.
                    representations = new HashSet<string>(values.Select(_typeConverter.ConvertToString));
                }
                else
                {
                    representations = new HashSet<string>(_inspect.GetValues().Select(v => v?.ToString() ?? "<null>"));
                }

                if (representations.Count > 1)
                {
                    _textField.Text = "<multiple values>";

                    _editButton.Visible = false;
                    Layout();
                }
                else if (representations.Count == 1)
                {
                    _textField.Text = representations.FirstOrDefault() ?? "";

                    _editButton.Visible = _inspect.HasTypeEditor();
                    Layout();
                }
                else
                {
                    _textField.Text = "";

                    _editButton.Visible = false;
                    Layout();
                }
            }

            public override void Layout()
            {
                base.Layout();

                var labelFrame = new AABB(0, 0, Height, Width / 2);
                labelFrame = labelFrame.Inset(new InsetBounds(8, 2, 2, 8));
                var textFieldFrame = new AABB(Width / 2, 0, Height, Width);

                var buttonArea = Bounds.Inset(new InsetBounds(4));
                float buttonSize = buttonArea.Height;
                var buttonFrame = 
                    new AABB(buttonArea.Right - buttonSize,
                        buttonArea.Top,
                        buttonArea.Bottom,
                        buttonArea.Right);

                if (_editButton.Visible)
                {
                    textFieldFrame = textFieldFrame.Setting(right: buttonFrame.Left - 4);
                }

                _label.SetFrame(labelFrame);
                _textField.SetFrame(textFieldFrame);
                _editButton.SetFrame(buttonFrame);
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
                        // Cannot inspect subscriptions
                        if (property.GetIndexParameters().Length > 0)
                        {
                            valid = false;
                            break;
                        }
                        // Cannot inspect set-only properties
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
        /// An inspectable property for a property grid.
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
            /// Gets a pre-formatted display name based on the member name of the inspected property
            /// </summary>
            public string DisplayName { private set; get; }

            /// <summary>
            /// Gets the declaring type for the property being inspected
            /// </summary>
            public Type TargetType { get; }

            /// <summary>
            /// Gets the type for the property
            /// </summary>
            public Type PropertyType { get; }

            /// <summary>
            /// Whether this property can be set
            /// </summary>
            public bool CanSet { get; }

            internal InspectableProperty([NotNull, ItemNotNull] object[] targets, [NotNull, ItemNotNull] PropertyInfo[] properties)
            {
                Properties = properties;
                Targets = targets;

                Name = properties[0].Name;
                TargetType = properties[0].DeclaringType;
                PropertyType = properties[0].PropertyType;
                CanSet = properties[0].CanWrite && properties[0].GetSetMethod(false) != null;

                ComputeDisplayName();
            }

            private void ComputeDisplayName()
            {
                string res = Regex.Replace(Name, "(.)([A-Z][a-z]+)", "$1 $2");
                DisplayName = Regex.Replace(res, "([a-z0-9])([A-Z])", "$1 $2");
            }

            /// <summary>
            /// Gets the targets for which this inspectable property is currently acting upon.
            /// </summary>
            [NotNull, ItemCanBeNull]
            public object[] GetTargets()
            {
                return Targets;
            }

            /// <summary>
            /// Gets the values of this inspectable property across all associated target objects.
            /// </summary>
            [NotNull, ItemCanBeNull]
            public object[] GetValues()
            {
                return Targets.Zip(Properties, (obj, prop) => prop.GetValue(obj)).ToArray();
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

            /// <summary>
            /// Gets the <see cref="System.Drawing.Design.UITypeEditor"/> for this inspectable property, either by inspecting
            /// its defining attributes, or its type's defining attributes.
            ///
            /// Returns null in case no editor type is found, or if multiple distinct <see cref="PropertyInfo"/> from many different
            /// types been provided during construction.
            /// </summary>
            [CanBeNull]
            public Type TypeEditorType()
            {
                // Check if all properties point to the same property object
                if (!CheckPropertiesAreSame())
                    return null;

                var property = Properties[0];

                var attribute =
                    property.GetCustomAttribute<EditorAttribute>() ??
                    PropertyType.GetCustomAttribute<EditorAttribute>();

                return attribute == null ? null : Type.GetType(attribute.EditorTypeName);
            }

            /// <summary>
            /// Returns true iff the property being inspected has a compatible editor type associated.
            /// </summary>
            public bool HasTypeEditor()
            {
                var type = TypeEditorType();
                if (type == null)
                    return false;

                var editor = (UITypeEditor)Activator.CreateInstance(type);

                return editor.GetEditStyle() == UITypeEditorEditStyle.Modal;
            }

            /// <summary>
            /// If this property has an editor type set (see <see cref="TypeEditorType"/>), invokes it to allow the user to change the
            /// current value.
            ///
            /// Only modal editor style is currently supported; other editor styles will result in an <see cref="InvalidOperationException"/>
            /// being raised.
            /// </summary>
            /// <exception cref="InvalidOperationException">No editor type is available.</exception>
            /// <exception cref="InvalidOperationException">Editor style is not <see cref="UITypeEditorEditStyle.Modal"/>.</exception>
            public void InvokeEditorUi()
            {
                var typeEditor = TypeEditorType() ??
                                 throw new InvalidOperationException(
                                     $"Cannot invoke {nameof(InvokeEditorUi)} with no editor type available.");

                var editor = (UITypeEditor)Activator.CreateInstance(typeEditor);

                if (editor.GetEditStyle() != UITypeEditorEditStyle.Modal)
                    throw new InvalidOperationException($"Only modal editor style is currently supported, but received {editor.GetEditStyle()}.");

                var prop = TypeDescriptor.GetProperties(TargetType)[Properties[0].Name];

                var context = new TypeDescriptionContext(Targets[0], prop);

                var values = GetValues();

                SetValue(editor.EditValue(context, values[0]));
            }

            private bool CheckPropertiesAreSame()
            {
                var first = Properties[0];
;
                return Properties.All(info => info.Equals(first));
            }

            private class TypeDescriptionContext : ITypeDescriptorContext
            {
                public IContainer Container => null;

                public object Instance { get; }

                public TypeDescriptionContext(object obj, PropertyDescriptor property)
                {
                    Instance = obj;
                    PropertyDescriptor = property;
                }

                public void OnComponentChanged()
                {
                }

                public bool OnComponentChanging()
                {
                    return true;
                }

                public PropertyDescriptor PropertyDescriptor { get; }

                public object GetService(Type serviceType)
                {
                    return serviceType == typeof(ITypeDescriptorContext) ? this : null;
                }
            }
        }
    }

    /// <summary>
    /// Event raised when the user updates an inspectable property on a properties grid control.
    /// </summary>
    public class InspectablePropertyChangedEventArgs : EventArgs
    {
        [NotNull]
        public PropertyGridControl.InspectableProperty InspectableProperty { get; }
        [CanBeNull]
        public object NewValue { get; }

        public InspectablePropertyChangedEventArgs([NotNull] PropertyGridControl.InspectableProperty inspectableProperty, [CanBeNull] object newValue)
        {
            InspectableProperty = inspectableProperty;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// Event handler for a <see cref="PropertyGridControl.InspectablePropertyChanged"/> event.
    /// </summary>
    public delegate void InspectablePropertyChangedEventHandler([NotNull] object sender, [NotNull] InspectablePropertyChangedEventArgs e);
}
