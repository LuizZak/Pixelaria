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

namespace PixUI.LayoutSystem
{
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
        internal ILayoutVariablesContainer Container;

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
        public LayoutAnchor? SecondAnchor { get; }

        /// <summary>
        /// The relationship for this constraint
        /// </summary>
        public LayoutRelationship Relationship { get; }

        internal LayoutConstraint(LayoutAnchor firstAnchor, LayoutAnchor? secondAnchor, LayoutRelationship relationship, [NotNull] ClStrength priority)
        {
            if (secondAnchor != null)
            {
                if (ReferenceEquals(firstAnchor.container, secondAnchor.Value.container))
                {
                    throw new ArgumentException($"Cannot create constraints that relate a view to itself: ${firstAnchor.Variable().Name} and ${secondAnchor.Value.Variable().Name}");
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

            Container?.ViewInHierarchy?.LayoutConstraints.Remove(this);
            FirstAnchor.container.ViewInHierarchy.AffectingConstraints.Remove(this);
            SecondAnchor?.container.ViewInHierarchy.AffectingConstraints.Remove(this);

            FirstAnchor.container.SetNeedsLayout();
            SecondAnchor?.container.SetNeedsLayout();
        }

        internal void BuildConstraints([NotNull] ClSimplexSolver solver)
        {
            if (!IsEnabled)
                return;

            var firstVar = FirstAnchor.Variable();

            if (SecondAnchor != null)
            {
                var secondAnchor = SecondAnchor.Value;

                // Create an expression of the form:
                //
                // first [==|<=|>=] (second - containerLocation) * multiplier + containerLocation + offset
                //
                // The container is a reference to the first common ancestor between each anchor,
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

                var container = Container ?? secondAnchor.container;

                ClLinearExpression secondExpression;

                if (Math.Abs(Multiplier - 1) < float.Epsilon)
                {
                    secondExpression = new ClLinearExpression(secondAnchor.Variable())
                        .Plus(new ClLinearExpression(Constant));
                }
                else
                {
                    secondExpression = new ClLinearExpression(secondAnchor.Variable())
                        .Minus(secondAnchor.RelativeExpression(container))
                        .Times(Multiplier)
                        .Plus(secondAnchor.RelativeExpression(container))
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

        private static Expression<Func<double, bool>> GetSingleExpression(LayoutRelationship constraintRelatedBy, double constant)
        {
            switch (constraintRelatedBy)
            {
                case LayoutRelationship.Equal:
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
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

        public static LayoutConstraint Create(LayoutAnchor firstAnchor, LayoutRelationship relationship = LayoutRelationship.Equal, ClStrength priority = null, float constant = 0, float multiplier = 1)
        {
            var constraint = new LayoutConstraint(firstAnchor, null, relationship, priority ?? ClStrength.Strong)
            {
                Constant = constant,
                Multiplier = multiplier
            };

            firstAnchor.container.AffectingConstraints.Add(constraint);
            firstAnchor.container.ViewInHierarchy?.LayoutConstraints.Add(constraint);
            
            constraint.Container = firstAnchor.container;

            firstAnchor.container.SetNeedsLayout();

            return constraint;
        }

        public static LayoutConstraint Create(LayoutAnchor firstAnchor, LayoutAnchor secondAnchor, LayoutRelationship relationship = LayoutRelationship.Equal, ClStrength priority = null, float constant = 0, float multiplier = 1)
        {
            var view1 = firstAnchor.container.ViewInHierarchy;
            var view2 = secondAnchor.container.ViewInHierarchy;

            if (view1 == null)
            {
                if (firstAnchor.container is LayoutGuide)
                {
                    throw new ArgumentException($"Attempting to create constraint referencing {nameof(LayoutGuide)} not added to a view");
                }
                else
                {
                    throw new ArgumentException($"Attempting to create constraint referencing invalid {nameof(ILayoutVariablesContainer)}");
                }
            }
            if (view2 == null)
            {
                if (secondAnchor.container is LayoutGuide)
                {
                    throw new ArgumentException($"Attempting to create constraint referencing {nameof(LayoutGuide)} not added to a view");
                }
                else
                {
                    throw new ArgumentException($"Attempting to create constraint referencing invalid {nameof(ILayoutVariablesContainer)}");
                }
            }
            
            var ancestor = view1.CommonAncestor(view2);
            
            var constraint = new LayoutConstraint(firstAnchor, secondAnchor, relationship, priority ?? ClStrength.Strong)
            {
                Constant = constant, Multiplier = multiplier
            };

            firstAnchor.container.AffectingConstraints.Add(constraint);
            secondAnchor.container.AffectingConstraints.Add(constraint);

            if (ancestor == null)
                throw new ArgumentException("Cannot create constraints between views in different hierarchies");

            ancestor.LayoutConstraints.Add(constraint);
            constraint.Container = ancestor;

            firstAnchor.container.SetNeedsLayout();
            secondAnchor.container.SetNeedsLayout();

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
