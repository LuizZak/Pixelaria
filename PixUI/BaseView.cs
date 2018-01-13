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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

using JetBrains.Annotations;
using PixCore.Geometry;
using PixUI.Controls;

namespace PixUI
{
    /// <summary>
    /// A base view is a small contained class for constructing element hierarchies with support
    /// for fast querying of location and transformation.
    /// 
    /// Base views feature support for nesting, parent traversal, and location/size specification.
    /// 
    /// Used to render Export Pipeline UI elements.
    /// </summary>
    public class BaseView : IEquatable<BaseView>, ISpatialReference, IRegionInvalidateable
    {
        private Vector _size;
        private Vector _location;
        private Color _strokeColor = Color.Black;
        private float _strokeWidth = 1;
        private Vector _scale = Vector.Unit;
        private float _rotation;

        /// <summary>
        /// Gets the parent view, if any, of this base view.
        /// 
        /// Area is relative to this base view's Location, if present.
        /// </summary>
        [CanBeNull]
        public BaseView Parent { get; private set; }
        
        /// <summary>
        /// A customizable string for identifying this view during debug mode.
        /// </summary>
        [CanBeNull]
        public string DebugName { get; set; }

        /// <summary>
        /// Transformation matrix.
        /// 
        /// Used to alter rendering and hit testing.
        /// 
        /// This value is inherited by child base views.
        /// </summary>
        public Matrix LocalTransform
        {
            get
            {
                var matrix = new Matrix();

                matrix.Scale(Scale.X, Scale.Y);
                matrix.Translate(Location.X, Location.Y);
                matrix.RotateAt(Rotation, PointF.Empty);

                return matrix;
            }
        }

        /// <summary>
        /// Children of this base view
        /// </summary>
        protected List<BaseView> children { get; } = new List<BaseView>();

        /// <summary>
        /// Gets or sets the center point of this view's <see cref="AABB"/> when projected
        /// on the parent view.
        /// 
        /// If Parent == null, returns the AABB's center property on get, and
        /// set is ignored..
        /// </summary>
        public Vector Center
        {
            get
            {
                if (Parent == null)
                    return Bounds.Center;

                return Parent.ConvertFrom(Bounds.Center, this);
            }
            set
            {
                if (Parent == null)
                    return;

                Location = value - Bounds.Size / 2;
            }
        }

        /// <summary>
        /// Gets all children of this base view
        /// </summary>
        public BaseView[] Children => children.ToArray();

        /// <summary>
        /// Top-left location of view, in pixels
        /// </summary>
        public virtual Vector Location
        {
            get => _location;
            set
            {
                InvalidateFullBounds();
                _location = value;
                InvalidateFullBounds();
            }
        }

        /// <summary>
        /// Size of view, in pixels
        /// </summary>
        public virtual Vector Size
        {
            get => _size;
            set
            {
                Debug.Assert(!float.IsNaN(_size.X), "!float.IsNaN(_size.X)");
                Debug.Assert(!float.IsNaN(_size.Y), "!float.IsNaN(_size.Y)");

                if (_size == value)
                    return;

                Invalidate();
                _size = value;
                OnResize();
                Invalidate();
            }
        }

        /// <summary>
        /// This view's width size, not counting children bounds.
        /// 
        /// Shortcut for <see cref="Size"/>'s <see cref="Vector.X"/>
        /// </summary>
        public float Width => Size.X;

        /// <summary>
        /// This view's local height size, not counting children bounds.
        /// 
        /// Shortcut for <see cref="Size"/>'s <see cref="Vector.Y"/>
        /// </summary>
        public float Height => Size.Y;

        /// <summary>
        /// Relative scale of this base view
        /// </summary>
        public Vector Scale
        {
            get => _scale;
            set
            {
                InvalidateFullBounds();
                _scale = value;
                InvalidateFullBounds();
            }
        }

        /// <summary>
        /// Gets or sets the rotation of this view, in degrees.
        /// Rotations are relative to the view's top-left corner.
        /// </summary>
        public float Rotation
        {
            get => _rotation;
            set
            {
                InvalidateFullBounds();
                _rotation = value;
                InvalidateFullBounds();
            }
        }

        /// <summary>
        /// The stroke color of this bezier path
        /// </summary>
        public virtual Color StrokeColor
        {
            get => _strokeColor;
            set
            {
                _strokeColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// The width of the line for this bezier path view
        /// </summary>
        public virtual float StrokeWidth
        {
            get => _strokeWidth;
            set
            {
                Invalidate();
                _strokeWidth = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Returns the local bounds of this view, with 0 x 0 mapping to its local top-left pixel
        /// </summary>
        public virtual AABB Bounds => new AABB(Vector.Zero, Size);

        /// <summary>
        /// Returns the local bounds of this view, converted to the parent's frame coordinates.
        /// 
        /// If no parent is present, <see cref="Bounds"/> is returned instead.
        /// </summary>
        public virtual AABB FrameOnParent => Parent == null ? Bounds : ConvertTo(Bounds, Parent);

        public BaseView() : this(null)
        {
            
        }

        /// <summary>
        /// Initializes this base view with a debug name string attached.
        /// </summary>
        public BaseView([CanBeNull] string debugName)
        {
            DebugName = debugName;
        }

        /// <summary>
        /// Sets the <see cref="Location"/> and <see cref="Size"/> to a given <see cref="AABB"/> value.
        /// </summary>
        public void SetFrame(AABB aabb)
        {
            Location = aabb.Minimum;
            Size = aabb.Size;
        }

        /// <summary>
        /// Adds a base view as the child of this base view.
        /// </summary>
        public virtual void AddChild([NotNull] BaseView child)
        {
            // Check recursiveness
            var cur = this;
            while (cur != null)
            {
                if(Equals(cur, child))
                    throw new ArgumentException(@"Cannot add BaseView as child of itself", nameof(child));

                cur = cur.Parent;
            }

            child.Parent?.RemoveChild(child);

            child.Parent = this;
            children.Add(child);

            child.InvalidateFullBounds();
        }

        /// <summary>
        /// Inserts a base view as the child of this base view at a given index.
        /// </summary>
        public virtual void InsertChild(int index, [NotNull] BaseView child)
        {
            // Check recursiveness
            var cur = this;
            while (cur != null)
            {
                if (Equals(cur, child))
                    throw new ArgumentException(@"Cannot add BaseView as child of itself", nameof(child));

                cur = cur.Parent;
            }

            child.Parent?.RemoveChild(child);

            child.Parent = this;
            children.Insert(index, child);

            child.InvalidateFullBounds();
        }

        /// <summary>
        /// Removes a given child from this base view
        /// </summary>
        public virtual void RemoveChild([NotNull] BaseView child)
        {
            if(!Equals(child.Parent, this))
                throw new ArgumentException(@"Child BaseView passed in is not a direct child of this base view", nameof(child));
            
            child.InvalidateFullBounds();

            child.Parent = null;
            children.Remove(child);
        }

        /// <summary>
        /// If this view has a parent, it removes itself as a child of that parent view.
        /// 
        /// Same as calling <code><see cref="Parent"/>?.RemoveChild(this)</code>
        /// </summary>
        public void RemoveFromParent()
        {
            Parent?.RemoveChild(this);
        }

        /// <summary>
        /// Returns whether this view is a direct or indirect child of another view
        /// </summary>
        public bool IsDescendentOf([NotNull] BaseView view)
        {
            var v = this;
            while (v != null)
            {
                if (Equals(v, view))
                    return true;

                v = v.Parent;
            }

            return false;
        }

        /// <summary>
        /// Performs a hit test operation on the area of this, and all child
        /// base views, for the given absolute coordinates point.
        /// 
        /// Returns the base view that has its origin closest to the given point.
        /// 
        /// Only returns an instance if its area is contained within the given point.
        /// 
        /// The <see cref="inflatingArea"/> argument can be used to inflate the
        /// area of the views to perform less precise hit tests.
        /// </summary>
        [CanBeNull]
        public BaseView HitTestClosest(Vector point, Vector inflatingArea)
        {
            BaseView closestV = null;
            float closestD = float.PositiveInfinity;

            // Search children first
            for (int i = children.Count - 1; i >= 0; i--)
            {
                var baseView = children[i];
                var ht = baseView.HitTestClosest(point * baseView.LocalTransform.Inverted(), inflatingArea);
                if (ht != null)
                {
                    var center = ht.ConvertTo(ht.Bounds.Center, this);

                    var distance = center.Distance(point);
                    if (distance < closestD)
                    {
                        closestV = ht;
                        closestD = distance;
                    }
                }
            }

            // Test this instance now
            if (Contains(point, inflatingArea) &&
                Bounds.Center.Distance(point) < closestD)
                closestV = this;

            return closestV;
        }

        /// <summary>
        /// Performs a hit test operation on the area of this, and all child
        /// base views, for the given absolute coordinates point.
        /// 
        /// Returns the first base view that crosses the point.
        /// 
        /// The <see cref="inflatingArea"/> argument can be used to inflate the
        /// area of the views to perform less precise hit tests.
        /// </summary>
        [CanBeNull]
        public BaseView ViewUnder(Vector point, Vector inflatingArea)
        {
            // Search children first
            for (int i = children.Count - 1; i >= 0; i--)
            {
                var baseView = children[i];
                var ht = baseView.ViewUnder(point * baseView.LocalTransform.Inverted(), inflatingArea);
                if (ht != null)
                {
                    return ht;
                }
            }

            // Test this instance now
            if (Contains(point, inflatingArea))
                return this;

            return null;
        }

        /// <summary>
        /// Performs a hit test operation on the area of this, and all child
        /// base views, for the given absolute coordinates point.
        /// 
        /// Returns the first base view that crosses the point and returns true
        /// for <see cref="predicate"/>.
        /// 
        /// The <see cref="inflatingArea"/> argument can be used to inflate the
        /// area of the views to perform less precise hit tests.
        /// </summary>
        [CanBeNull]
        public BaseView ViewUnder(Vector point, Vector inflatingArea, Func<BaseView, bool> predicate)
        {
            // Search children first
            for (int i = children.Count - 1; i >= 0; i--)
            {
                var baseView = children[i];
                var ht = baseView.ViewUnder(point * baseView.LocalTransform.Inverted(), inflatingArea, predicate);
                if (ht != null)
                    return ht;
            }

            // Test this instance now
            if (Contains(point, inflatingArea) && predicate(this))
                return this;

            return null;
        }

        /// <summary>
        /// Returns an enumerable of all views that cross the given point.
        /// 
        /// The <see cref="inflatingArea"/> argument can be used to inflate the
        /// area of the views to perform less precise hit tests.
        /// </summary>
        /// <param name="point">Point to test</param>
        /// <param name="inflatingArea">Used to inflate the area of the views to perform less precise hit tests.</param>
        /// <returns>An enumerable where each view returned crosses the given point.</returns>
        [ItemNotNull]
        [NotNull]
        public IEnumerable<BaseView> ViewsUnder(Vector point, Vector inflatingArea)
        {
            var views = new List<BaseView>();

            InternalViewsUnder(point, inflatingArea, views);

            return views;
        }

        private void InternalViewsUnder(Vector point, Vector inflatingArea, ICollection<BaseView> target)
        {
            // Search children first
            for (int i = children.Count - 1; i >= 0; i--)
            {
                var baseView = children[i];

                var vector = point * baseView.LocalTransform.Inverted();
                // Early out this view
                if (!baseView.Contains(vector, inflatingArea))
                    continue;

                baseView.InternalViewsUnder(vector, inflatingArea, target);
            }

            // Test this instance now
            if (Contains(point, inflatingArea))
                target.Add(this);
        }

        /// <summary>
        /// Returns an enumerable of all views that cross the given <see cref="AABB"/> bounds.
        /// 
        /// The <see cref="inflatingArea"/> argument can be used to inflate the
        /// area of the views to perform less precise hit tests.
        /// 
        /// The AABB is converted into local bounds for each subview, so distortion may
        /// occur and result in inaccurate results for views that are rotated. There are 
        /// no alternatives for this, currently.
        /// </summary>
        /// <param name="aabb">AABB to test</param>
        /// <param name="inflatingArea">Used to inflate the area of the views to perform less precise hit tests.</param>
        /// <returns>An enumerable where each view returned intersects the given <see cref="AABB"/>.</returns>
        [ItemNotNull]
        [NotNull]
        public IEnumerable<BaseView> ViewsUnder(AABB aabb, Vector inflatingArea)
        {
            var views = new List<BaseView>();
            InternalViewsUnder(aabb, inflatingArea, views);

            return views;
        }
        
        private void InternalViewsUnder(AABB aabb, Vector inflatingArea, ICollection<BaseView> target)
        {
            // Search children first
            for (int i = children.Count - 1; i >= 0; i--)
            {
                var baseView = children[i];
                var transformed = aabb.TransformedBounds(baseView.LocalTransform.Inverted());

                // Early-out the view
                if (!baseView.Intersects(transformed, inflatingArea))
                    continue;

                baseView.InternalViewsUnder(transformed, inflatingArea, target);
            }

            // Test this instance now
            if (Intersects(aabb, inflatingArea))
                target.Add(this);
        }

        /// <summary>
        /// Returns true if the given vector point intersects this view's area.
        /// 
        /// Children views' bounds do not affect the hit test- it happens only on this view's <see cref="AABB"/> area.
        /// </summary>
        /// <param name="point">Point to test</param>
        public virtual bool Contains(Vector point)
        {
            return Contains(point, Vector.Zero);
        }

        /// <summary>
        /// Returns true if the given vector point intersects this view's area when
        /// inflated by a specified ammount.
        /// 
        /// Children views' bounds do not affect the hit test- it happens only on this view's <see cref="AABB"/> area.
        /// </summary>
        /// <param name="point">Point to test</param>
        /// <param name="inflatingArea">Used to inflate the area of the view to perform less precise hit tests.</param>
        public virtual bool Contains(Vector point, Vector inflatingArea)
        {
            return Bounds.Inflated(inflatingArea).Contains(point);
        }

        /// <summary>
        /// Returns true if the given <see cref="AABB"/> intersects this view's area when
        /// inflated by a specified ammount.
        /// 
        /// Children views' bounds do not affect the hit test- it happens only on this view's <see cref="AABB"/> area.
        /// </summary>
        /// <param name="aabb">AABB to test</param>
        /// <param name="inflatingArea">Used to inflate the area of the view to perform less precise hit tests.</param>
        public virtual bool Intersects(AABB aabb, Vector inflatingArea)
        {
            return Bounds.Inflated(inflatingArea).Intersects(aabb);
        }

        /// <summary>
        /// Converts a point from a given <see cref="BaseView"/>'s local coordinates to this
        /// base view's coordinates.
        /// 
        /// If <see cref="from"/> is null, converts from screen coordinates.
        /// </summary>
        public Vector ConvertFrom(Vector point, ISpatialReference from)
        {
            // Convert point to global, if it's currently local to from
            var global = point;
            if (from != null)
                global = from.GetAbsoluteTransform() * point;

            var matrix = GetAbsoluteTransform().Inverted();
            return global * matrix;
        }

        /// <summary>
        /// Converts a point from this <see cref="BaseView"/>'s local coordinates to a given
        /// base view's coordinates.
        /// 
        /// If <see cref="to"/> is null, converts from this node to screen coordinates.
        /// </summary>
        public Vector ConvertTo(Vector point, ISpatialReference to)
        {
            if (to == null)
                return point * GetAbsoluteTransform();

            return to.ConvertFrom(point, this);
        }

        /// <summary>
        /// Converts an AABB from a given <see cref="BaseView"/>'s local coordinates to this
        /// base view's coordinates.
        /// 
        /// If <see cref="from"/> is null, converts from screen coordinates.
        /// </summary>
        public AABB ConvertFrom(AABB aabb, ISpatialReference from)
        {
            // Convert point to global, if it's currently local to from
            var global = aabb;
            if (from != null)
                global = aabb.TransformedBounds(from.GetAbsoluteTransform());

            var matrix = GetAbsoluteTransform().Inverted();
            return global.TransformedBounds(matrix);
        }

        /// <summary>
        /// Converts an AABB from this <see cref="BaseView"/>'s local coordinates to a given
        /// base view's coordinates.
        /// 
        /// If <see cref="to"/> is null, converts from this node to screen coordinates.
        /// </summary>
        public AABB ConvertTo(AABB aabb, ISpatialReference to)
        {
            if (to == null)
                return aabb.TransformedBounds(GetAbsoluteTransform());

            return to.ConvertFrom(aabb, this);
        }
        
        /// <summary>
        /// Gets the absolute matrix for this base view's coordinates system.
        /// 
        /// Multiplying a point (0, 0) by this matrix results in a point that lands on
        /// the top-left corner of this base view's coordinate system.
        /// </summary>
        public Matrix GetAbsoluteTransform()
        {
            if (Parent == null)
            {
                return LocalTransform;
            }

            var total = Parent.GetAbsoluteTransform().Clone();
            total.Multiply(LocalTransform);

            return total;
        }

        /// <summary>
        /// Gets the full bounds of this <see cref="BaseView"/> (in its local coordinates system),
        /// counting the child bounds as well.
        /// </summary>
        public AABB GetFullBounds()
        {
            var bounds = Bounds;

            foreach (var view in children)
            {
                var childBounds = view.GetFullBounds();

                // Convert to local coordinates
                childBounds = childBounds.Corners.Transform(view.LocalTransform).Area();

                bounds = bounds.Union(childBounds);
            }

            return bounds;
        }

        /// <summary>
        /// Method called whenever the <see cref="Size"/> property of this view is updated.
        /// </summary>
        protected virtual void OnResize()
        {
            
        }

        #region Redraw Region Management

        /// <summary>
        /// Returns the total rectangle that is invalidated when calling <see cref="Invalidate"/>
        /// </summary>
        public virtual AABB BoundsForInvalidate()
        {
            return Bounds.Inflated(StrokeWidth, StrokeWidth);
        }

        /// <summary>
        /// Returns the total rectangle that is invalidated when calling <see cref="InvalidateFullBounds"/>
        /// </summary>
        public virtual AABB BoundsForInvalidateFullBounds()
        {
            return GetFullBounds().Inflated(StrokeWidth, StrokeWidth);
        }

        /// <summary>
        /// Invalidates the entirety of this view's drawing region on its parent.
        /// 
        /// The invalidation is propagated through the parent view chain until the root
        /// view, which may handle it in ways such as invalidating a window screen region.
        /// </summary>
        public virtual void Invalidate()
        {
            Invalidate(BoundsForInvalidate());
        }

        /// <summary>
        /// Invalidates the entirety of this view's drawing region on its parent, including
        /// bounds of all of its hierarchy. See <see cref="GetFullBounds()"/>.
        /// 
        /// The invalidation is propagated through the parent view chain until the root
        /// view, which may handle it in ways such as invalidating a window screen region.
        /// </summary>
        public virtual void InvalidateFullBounds()
        {
            Invalidate(BoundsForInvalidateFullBounds());
        }
        
        /// <summary>
        /// Invalidates a given rectangle on this view.
        /// 
        /// The invalidation is propagated through the parent view chain until the root
        /// view, which may handle it in ways such as invalidating a window screen region.
        /// </summary>
        protected virtual void Invalidate(AABB rectangle)
        {
            Invalidate(new RedrawRegion(rectangle, this), this);
        }

        /// <summary>
        /// Invalidates a given region on this view.
        /// 
        /// The invalidation is propagated through the parent view chain until the root
        /// view, which may handle it in ways such as invalidating a window screen region.
        /// </summary>
        /// <param name="region">Region that was invalidated</param>
        /// <param name="reference">
        /// A spatial reference location  that specifies in which space the <see cref="region"/> parameter is situated in.
        /// Usually the <see cref="GetAbsoluteTransform()"/> of the view that was invalidated.
        /// </param>
        protected virtual void Invalidate(RedrawRegion region, ISpatialReference reference)
        {
            Parent?.Invalidate(region, reference);
        }
        
        #endregion

        public bool Equals(BaseView other)
        {
            return ReferenceEquals(this, other);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return DebugName != null ? $"{DebugName} : {{{base.ToString()}}}" : base.ToString();
        }
    }
}