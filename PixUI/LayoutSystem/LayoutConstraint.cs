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
                    .SuggestValue(Left, _view.X)
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
                    .SuggestValue(Top, _view.Y)
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
                    solver.AddConstraint(Width, IntrinsicWidth, (d, d1) => d == d1, ClStrength.Weak);
                }
            }
            if (orientation.HasFlag(LayoutAnchorOrientationFlags.Vertical))
            {
                solver.AddConstraint(new ClLinearEquation(Cl.Plus(new ClLinearExpression(Height), Top), new ClLinearExpression(Bottom), ClStrength.Required));
                if (hasIntrinsicSize)
                {
                    solver.AddConstraint(Height, IntrinsicHeight, (d, d1) => d == d1, ClStrength.Weak);
                }
            }
        }

        public void ApplyVariables()
        {
            if (_view.TranslateBoundsIntoConstraints)
                return;

            _view.X = (float) Left.Value;
            _view.Y = (float) Top.Value;
            _view.Width = (float) Width.Value;
            _view.Height = (float) Height.Value;
        }

        private static string GetUniqueName([NotNull] BaseView view)
        {
            return $"{view}_{view.GetHashCode()}";
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

        internal LayoutConstraint([NotNull] LayoutAnchor anchor, LayoutRelationship relationship, [NotNull] ClStrength priority) : this(anchor, null, relationship, priority)
        {
            
        }

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
        }

        internal void BuildConstraints([NotNull] ClSimplexSolver solver)
        {
            if (!IsEnabled)
                return;

            var firstVar = FirstAnchor.Variable();

            if (SecondAnchor != null)
            {
                var secondVar = SecondAnchor.Variable();

                solver.AddConstraint(firstVar, secondVar, GetExpression(Relationship, Multiplier, Constant), Priority);
            }
            else
            {
                solver.AddConstraint(firstVar, GetSingleExpression(Relationship, Multiplier, Constant), Priority);
            }
        }

        private Expression<Func<double, bool>> GetSingleExpression(LayoutRelationship constraintRelatedBy, double multiplier, double constant)
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

        private Expression<Func<double, double, bool>> GetExpression(LayoutRelationship constraintRelatedBy, double constraintMultiplier, double constraintConstant)
        {
            switch (constraintRelatedBy)
            {
                case LayoutRelationship.Equal:
                    return EqualsExpression(constraintMultiplier, constraintConstant);
                case LayoutRelationship.GreaterThanOrEqual:
                    return GreaterThanExpression(constraintMultiplier, constraintConstant);
                case LayoutRelationship.LessThanOrEqual:
                    return LessThanExpression(constraintMultiplier, constraintConstant);
                default:
                    throw new ArgumentOutOfRangeException(nameof(constraintRelatedBy), constraintRelatedBy, null);
            }
        }

        private static Expression<Func<double, double, bool>> GreaterThanExpression(double multiplier, double constant)
        {
            return (source, target) => source >= target * multiplier + constant;
        }

        private static Expression<Func<double, double, bool>> LessThanExpression(double multiplier, double constant)
        {
            return (source, target) => source <= target * multiplier + constant;
        }

        private static Expression<Func<double, double, bool>> EqualsExpression(double multiplier, double constant)
        {
            return (source, target) => source == target * multiplier + constant;
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
