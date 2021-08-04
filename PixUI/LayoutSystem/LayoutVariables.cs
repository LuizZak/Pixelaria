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
using Cassowary;
using JetBrains.Annotations;
using PixCore.Geometry;

namespace PixUI.LayoutSystem
{
    /// <summary>
    /// Produces variables for constraint resolution for left/top/right/bottom and width/height for a specific view
    /// </summary>
    internal class LayoutVariables
    {
        private readonly ILayoutVariablesContainer _container;

        internal readonly ClVariable Left;
        internal readonly ClVariable Top;
        internal readonly ClVariable Width;
        internal readonly ClVariable Height;
        internal readonly ClVariable Right;
        internal readonly ClVariable Bottom;
        internal readonly ClVariable IntrinsicWidth;
        internal readonly ClVariable IntrinsicHeight;

        internal int HorizontalCompressResistance = 750;
        internal int VerticalCompressResistance = 750;
        internal int HorizontalHuggingPriority = 150;
        internal int VerticalHuggingPriority = 150;

        public LayoutVariables([NotNull] BaseView view)
        {
            var name = GetUniqueName(view);

            _container = view;
            Left = new ClVariable($"{name}_left", view.X);
            Top = new ClVariable($"{name}_top", view.Y);
            Width = new ClVariable($"{name}_width", view.Width);
            Height = new ClVariable($"{name}_height", view.Height);
            Right = new ClVariable($"{name}_right", view.Bounds.Right);
            Bottom = new ClVariable($"{name}_right", view.Bounds.Bottom);
            IntrinsicWidth = new ClVariable($"{name}_intrinsicWidth", view.IntrinsicSize.X);
            IntrinsicHeight = new ClVariable($"{name}_intrinsicHeight", view.IntrinsicSize.Y);
        }

        public LayoutVariables([NotNull] LayoutGuide layoutGuide)
        {
            var name = GetUniqueName(layoutGuide);

            _container = layoutGuide;
            Left = new ClVariable($"{name}_left", layoutGuide.FrameOnParent.Left);
            Top = new ClVariable($"{name}_top", layoutGuide.FrameOnParent.Top);
            Width = new ClVariable($"{name}_width", layoutGuide.FrameOnParent.Width);
            Height = new ClVariable($"{name}_height", layoutGuide.FrameOnParent.Height);
            Right = new ClVariable($"{name}_right", layoutGuide.FrameOnParent.Right);
            Bottom = new ClVariable($"{name}_right", layoutGuide.FrameOnParent.Bottom);
            IntrinsicWidth = new ClVariable($"{name}_intrinsicWidth", 0);
            IntrinsicHeight = new ClVariable($"{name}_intrinsicHeight", 0);
        }
        
        public void AddVariables([NotNull] ClSimplexSolver solver)
        {
            if (_container is BaseView view)
            {
                AddVariablesForView(view, solver);
            }
            else
            {
                AddVariablesForGenericContainer(_container, solver);
            }
        }

        private void AddVariablesForView([NotNull] BaseView view, [NotNull] ClSimplexSolver solver)
        {
            var hasIntrinsicSize = view.IntrinsicSize != Vector.Zero;

            if (view.TranslateBoundsIntoConstraints)
            {
                solver.AddStay(Left, ClStrength.Medium);
                solver.AddStay(Width, ClStrength.Medium);
                solver.AddStay(Top, ClStrength.Medium);
                solver.AddStay(Height, ClStrength.Medium);
            }
            if (hasIntrinsicSize)
            {
                solver.AddStay(IntrinsicWidth, ClStrength.Medium);
                solver.AddStay(IntrinsicHeight, ClStrength.Medium);
            }

            var location = view.ConvertTo(Vector.Zero, null);

            solver.BeginEdit(Left, Width, IntrinsicWidth)
                .SuggestValue(Left, location.X)
                .SuggestValue(Width, view.Width)
                .SuggestValue(IntrinsicWidth, view.IntrinsicSize.X)
                .EndEdit();

            solver.BeginEdit(Top, Height, IntrinsicHeight)
                .SuggestValue(Top, location.Y)
                .SuggestValue(Height, view.Height)
                .SuggestValue(IntrinsicHeight, view.IntrinsicSize.Y)
                .EndEdit();
        }

        private void AddVariablesForGenericContainer([NotNull] ILayoutVariablesContainer container, [NotNull] ClSimplexSolver solver)
        {
            if (container.ParentSpatialReference == null) 
                return;

            var location = container.ParentSpatialReference.ConvertTo(Vector.Zero, null);

            solver.BeginEdit(Left, Width, IntrinsicWidth)
                .SuggestValue(Left, location.X)
                .SuggestValue(Width, container.FrameOnParent.Width)
                .EndEdit();

            solver.BeginEdit(Top, Height, IntrinsicHeight)
                .SuggestValue(Top, location.Y)
                .SuggestValue(Height, container.FrameOnParent.Height)
                .EndEdit();
        }

        public void BuildConstraints([NotNull] ClSimplexSolver solver)
        {
            solver.AddConstraint(new ClLinearEquation(new ClLinearExpression(Width).Plus(Left), new ClLinearExpression(Right), ClStrength.Required));
            solver.AddConstraint(new ClLinearEquation(new ClLinearExpression(Height).Plus(Top), new ClLinearExpression(Bottom), ClStrength.Required));
            
            if(_container is BaseView view)
                BuildConstraints(view, solver);
        }

        private void BuildConstraints([NotNull] BaseView view, [NotNull] ClSimplexSolver solver)
        {
            var hasIntrinsicSize = view.IntrinsicSize != Vector.Zero;
            if (hasIntrinsicSize)
            {
                // Compression resistance
                solver.AddConstraint(Width, IntrinsicWidth, (d, d1) => d >= d1, LayoutConstraintHelpers.StrengthFromPriority(HorizontalCompressResistance));
                // Content hugging priority
                solver.AddConstraint(Width, IntrinsicWidth, (d, d1) => d <= d1, LayoutConstraintHelpers.StrengthFromPriority(HorizontalHuggingPriority));
                // Compression resistance
                solver.AddConstraint(Height, IntrinsicHeight, (d, d1) => d >= d1, LayoutConstraintHelpers.StrengthFromPriority(VerticalCompressResistance));
                // Content hugging priority
                solver.AddConstraint(Height, IntrinsicHeight, (d, d1) => d <= d1, LayoutConstraintHelpers.StrengthFromPriority(VerticalHuggingPriority));
            }
        }

        public void ApplyVariables()
        {
            if (_container is BaseView view)
            {
                ApplyVariables(view);
            }
            else if (_container is LayoutGuide guide)
            {
                ApplyVariables(guide);
            }
            else
            {
                throw new InvalidOperationException("Currently ApplyVariables only supports BaseView and LayoutGuide containers");
            }
        }

        private void ApplyVariables([NotNull] BaseView view)
        {
            if (view.TranslateBoundsIntoConstraints)
                return;

            var location = new Vector((float)Left.Value, (float)Top.Value);
            if (view.Parent != null)
            {
                location = view.Parent.ConvertFrom(location, null);
            }

            view.X = location.X;
            view.Y = location.Y;
            view.Width = (float)Width.Value;
            view.Height = (float)Height.Value;
        }

        private void ApplyVariables([NotNull] LayoutGuide guide)
        {
            var location = new Vector((float)Left.Value, (float)Top.Value);
            if (guide.ParentSpatialReference != null)
            {
                location = guide.ParentSpatialReference.ConvertFrom(location, null);
            }

            guide.FrameOnParent = AABB.FromRectangle(location.X, location.Y, (float)Width.Value, (float)Height.Value);
        }

        private static string GetUniqueName([NotNull] ILayoutVariablesContainer container)
        {
            return $"{container}_{container.GetHashCode()}";
        }
    }
}