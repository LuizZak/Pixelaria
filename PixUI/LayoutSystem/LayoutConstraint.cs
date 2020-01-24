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
using System.Linq.Expressions;
using Cassowary;
using JetBrains.Annotations;
using PixCore.Geometry;

namespace PixUI.LayoutSystem
{
    /// <summary>
    /// Produces variables for constraint resolution for left/top/right/bottom and width/height for a specific view
    /// </summary>
    internal class ViewLayoutConstraintVariables
    {
        private readonly BaseView _view;

        internal readonly ClVariable Left;
        internal readonly ClVariable Top;
        internal readonly ClVariable Width;
        internal readonly ClVariable Height;
        internal readonly ClVariable Right;
        internal readonly ClVariable Bottom;
        internal readonly ClVariable IntrinsicWidth;
        internal readonly ClVariable IntrinsicHeight;

        internal int HorizontalCompressResistance = 100;
        internal int VerticalCompressResistance = 100;
        internal int HorizontalHuggingPriority = 50;
        internal int VerticalHuggingPriority = 50;

        public ViewLayoutConstraintVariables([NotNull] BaseView view)
        {
            var name = GetUniqueName(view);

            _view = view;
            Left = new ClVariable($"{name}_left", view.X);
            Top = new ClVariable($"{name}_top", view.Y);
            Width = new ClVariable($"{name}_width", view.Width);
            Height = new ClVariable($"{name}_height", view.Height);
            Right = new ClVariable($"{name}_right", view.Bounds.Right);
            Bottom = new ClVariable($"{name}_right", view.Bounds.Bottom);
            IntrinsicWidth = new ClVariable($"{name}_intrinsicWidth", view.IntrinsicSize.X);
            IntrinsicHeight = new ClVariable($"{name}_intrinsicHeight", view.IntrinsicSize.Y);
        }

        public void AddVariables([NotNull] ClSimplexSolver solver, LayoutAnchorOrientationFlags orientation)
        {
            var hasIntrinsicSize = _view.IntrinsicSize != Vector.Zero;

            var location = _view.ConvertTo(Vector.Zero, null);

            if (orientation.HasFlag(LayoutAnchorOrientationFlags.Horizontal))
            {
                if (_view.TranslateBoundsIntoConstraints)
                {
                    solver.AddStay(Left, ClStrength.Medium);
                    solver.AddStay(Width, ClStrength.Medium);
                }
                if (hasIntrinsicSize)
                {
                    solver.AddStay(IntrinsicWidth, ClStrength.Medium);
                }

                solver.BeginEdit(Left, Width, IntrinsicWidth)
                    .SuggestValue(Left, location.X)
                    .SuggestValue(Width, _view.Width)
                    .SuggestValue(IntrinsicWidth, _view.IntrinsicSize.X)
                    .EndEdit();
            }
            if (orientation.HasFlag(LayoutAnchorOrientationFlags.Vertical))
            {
                if (_view.TranslateBoundsIntoConstraints)
                {
                    solver.AddStay(Top, ClStrength.Medium);
                    solver.AddStay(Height, ClStrength.Medium);
                }
                if (hasIntrinsicSize)
                {
                    solver.AddStay(IntrinsicHeight, ClStrength.Medium);
                }

                solver.BeginEdit(Top, Height, IntrinsicHeight)
                    .SuggestValue(Top, location.Y)
                    .SuggestValue(Height, _view.Height)
                    .SuggestValue(IntrinsicHeight, _view.IntrinsicSize.Y)
                    .EndEdit();
            }
        }

        public void BuildConstraints([NotNull] ClSimplexSolver solver, LayoutAnchorOrientationFlags orientation)
        {
            var hasIntrinsicSize = _view.IntrinsicSize != Vector.Zero;
            
            if (orientation.HasFlag(LayoutAnchorOrientationFlags.Horizontal))
            {
                solver.AddConstraint(new ClLinearEquation(Cl.Plus(new ClLinearExpression(Width), Left), new ClLinearExpression(Right), ClStrength.Required));
                if (hasIntrinsicSize)
                {
                    // Compression resistance
                    solver.AddConstraint(Width, IntrinsicWidth, (d, d1) => d >= d1, LayoutConstraintHelpers.StrengthFromPriority(HorizontalCompressResistance));
                    // Content hugging priority
                    solver.AddConstraint(Width, IntrinsicWidth, (d, d1) => d <= d1, LayoutConstraintHelpers.StrengthFromPriority(HorizontalHuggingPriority));
                }
            }
            if (orientation.HasFlag(LayoutAnchorOrientationFlags.Vertical))
            {
                solver.AddConstraint(new ClLinearEquation(Cl.Plus(new ClLinearExpression(Height), Top), new ClLinearExpression(Bottom), ClStrength.Required));
                if (hasIntrinsicSize)
                {
                    // Compression resistance
                    solver.AddConstraint(Height, IntrinsicHeight, (d, d1) => d >= d1, LayoutConstraintHelpers.StrengthFromPriority(VerticalCompressResistance));
                    // Content hugging priority
                    solver.AddConstraint(Height, IntrinsicHeight, (d, d1) => d <= d1, LayoutConstraintHelpers.StrengthFromPriority(VerticalHuggingPriority));
                }
            }
        }

        public void ApplyVariables()
        {
            if (_view.TranslateBoundsIntoConstraints)
                return;

            var location = new Vector((float)Left.Value, (float)Top.Value);
            if (_view.Parent != null)
            {
                location = _view.Parent.ConvertFrom(location, null);
            }

            _view.X = location.X;
            _view.Y = location.Y;
            _view.Width = (float) Width.Value;
            _view.Height = (float) Height.Value;
        }

        private static string GetUniqueName([NotNull] BaseView view)
        {
            return $"{view}_{view.GetHashCode()}";
        }
    }

    /// <summary>
    /// A layout constraint
    /// </summary>
    public class LayoutConstraint
    {
        /// <summary>
        /// In case this constraint is active on a view hierarchy, this property
        /// points to the view that contains this <see cref="LayoutConstraint"/>
        /// within its layout constraints list.
        /// </summary>
        [CanBeNull]
        internal BaseView Container;

        /// <summary>
        /// Gets or sets the constant target value for this constraint
        /// </summary>
        public float Constant { get; set; }

        /// <summary>
        /// Gets or sets the multiplier for this constraint
        /// </summary>
        public float Multiplier { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value specifying whether this layout constraint is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// The priority for this constraint
        /// </summary>
        public ClStrength Priority { get; }

        /// <summary>
        /// The first anchor involved in this constraint
        /// </summary>
        public LayoutAnchor FirstAnchor { get; }

        /// <summary>
        /// The second anchor involved in this constraint.
        ///
        /// If <c>null</c>, indicates that this is an absolute constraint
        /// </summary>
        [CanBeNull]
        public LayoutAnchor SecondAnchor { get; }

        /// <summary>
        /// The relationship for this constraint
        /// </summary>
        public LayoutRelationship Relationship { get; }

        internal LayoutConstraint([NotNull] LayoutAnchor firstAnchor, [CanBeNull] LayoutAnchor secondAnchor, LayoutRelationship relationship, [NotNull] ClStrength priority)
        {
            if (secondAnchor != null)
            {
                if (ReferenceEquals(firstAnchor.Target, secondAnchor.Target))
                {
                    throw new ArgumentException($"Cannot create constraints that relate a view to itself: ${firstAnchor.Variable().Name} and ${secondAnchor.Variable().Name}");
                }

                if (firstAnchor.Orientation != secondAnchor.Orientation)
                {
                    throw new ArgumentException($"Cannot relate constraint anchors of different orientations: ${firstAnchor.Variable().Name} and ${secondAnchor.Variable().Name}");
                }
            }

            FirstAnchor = firstAnchor;
            SecondAnchor = secondAnchor;
            Relationship = relationship;
            Priority = priority;
        }

        /// <summary>
        /// Removes this constraint from the view hierarchy such that it no longer affects any view.
        /// </summary>
        public void RemoveConstraint()
        {
            IsEnabled = false;

            Container?.LayoutConstraints.Remove(this);
            FirstAnchor.Target.AffectingConstraints.Remove(this);
            SecondAnchor?.Target.AffectingConstraints.Remove(this);

            FirstAnchor.Target.SetNeedsLayout();
            SecondAnchor?.Target.SetNeedsLayout();
        }

        internal void BuildConstraints([NotNull] ClSimplexSolver solver)
        {
            if (!IsEnabled)
                return;

            var firstVar = FirstAnchor.Variable();

            if (SecondAnchor != null)
            {
                // Create an expression of the form:
                //
                // first [==|<=|>=] (second - containerLocation) * multiplier + containerLocation + offset
                //
                // The container is a reference to the direct parent of the second anchor's view,
                // or the second anchor's view itself in case it has no parents, and is used to
                // apply proper multiplication of the constraints.
                // For width/height constraints, containerLocation is zero, for left/right
                // containerLocation is containerView.layout.left, and for top/bottom
                // containerLocation is containerView.layout.top.
                //
                // Without this relative container offset, multiplicative constraints
                // would multiply the absolute view locations, resulting in views
                // that potentially break their parent view's bounds.
                //
                //
                // For non multiplicative constraints (where multiplier == 1), a simpler solution
                // is used:
                //
                // first [==|<=|>=] second + offset
                //

                var container = SecondAnchor.Target.Parent ?? SecondAnchor.Target;

                ClLinearExpression secondExpression;

                if (Math.Abs(Multiplier - 1) < float.Epsilon)
                {
                    secondExpression = new ClLinearExpression(SecondAnchor.Variable())
                        .Plus(new ClLinearExpression(Constant));
                }
                else
                {
                    secondExpression = new ClLinearExpression(SecondAnchor.Variable())
                        .Minus(SecondAnchor.RelativeExpression(container))
                        .Times(Multiplier)
                        .Plus(SecondAnchor.RelativeExpression(container))
                        .Plus(new ClLinearExpression(Constant));
                }

                ClConstraint constraint;

                if (Relationship == LayoutRelationship.Equal)
                {
                    constraint = new ClLinearEquation(new ClLinearExpression(firstVar), secondExpression);
                }
                else
                {
                    constraint = new ClLinearInequality(new ClLinearExpression(firstVar), OperatorForRelationship(Relationship), secondExpression);
                }

                solver.AddConstraint(constraint);
            }
            else
            {
                solver.AddConstraint(firstVar, GetSingleExpression(Relationship, Constant), Priority);
            }
        }

        private Expression<Func<double, bool>> GetSingleExpression(LayoutRelationship constraintRelatedBy, double constant)
        {
            switch (constraintRelatedBy)
            {
                case LayoutRelationship.Equal:
                    return source => source == constant;
                case LayoutRelationship.GreaterThanOrEqual:
                    return source => source >= constant;
                case LayoutRelationship.LessThanOrEqual:
                    return source => source <= constant;
                default:
                    throw new ArgumentOutOfRangeException(nameof(constraintRelatedBy), constraintRelatedBy, null);
            }
        }

        private static Cl.Operator OperatorForRelationship(LayoutRelationship relationship)
        {
            switch (relationship)
            {
                case LayoutRelationship.GreaterThanOrEqual:
                    return Cl.Operator.GreaterThanOrEqualTo;
                case LayoutRelationship.LessThanOrEqual:
                    return Cl.Operator.LessThanOrEqualTo;
                default:
                    throw new ArgumentOutOfRangeException(nameof(relationship), relationship, null);
            }
        }

        public static LayoutConstraint Create([NotNull] LayoutAnchor firstAnchor, LayoutRelationship relationship = LayoutRelationship.Equal, ClStrength priority = null, float constant = 0, float multiplier = 1)
        {
            return Create(firstAnchor, null, relationship, priority, constant, multiplier);
        }

        public static LayoutConstraint Create([NotNull] LayoutAnchor firstAnchor, [CanBeNull] LayoutAnchor secondAnchor, LayoutRelationship relationship = LayoutRelationship.Equal, ClStrength priority = null, float constant = 0, float multiplier = 1)
        {
            var constraint = new LayoutConstraint(firstAnchor, secondAnchor, relationship, priority ?? ClStrength.Strong)
            {
                Constant = constant, Multiplier = multiplier
            };

            firstAnchor.Target.AffectingConstraints.Add(constraint);
            secondAnchor?.Target.AffectingConstraints.Add(constraint);

            if (secondAnchor != null)
            {
                var ancestor = firstAnchor.Target.CommonAncestor(secondAnchor.Target);

                if (ancestor == null)
                    throw new ArgumentException("Cannot create constraints between views in different hierarchies");

                ancestor.LayoutConstraints.Add(constraint);
                constraint.Container = ancestor;
            }
            else
            {
                firstAnchor.Target.LayoutConstraints.Add(constraint);
                constraint.Container = firstAnchor.Target;
            }
            
            firstAnchor.Target.SetNeedsLayout();
            secondAnchor?.Target.SetNeedsLayout();

            return constraint;
        }
    }

    public enum LayoutRelationship
    {
        Equal,
        GreaterThanOrEqual,
        LessThanOrEqual
    }

    public enum LayoutAnchorKind
    {
        Left,
        Top,
        Right,
        Bottom,
        Width,
        Height
    }

    [Flags]
    internal enum LayoutAnchorOrientationFlags
    {
        Horizontal = 0b1,
        Vertical = 0b10
    }
}
