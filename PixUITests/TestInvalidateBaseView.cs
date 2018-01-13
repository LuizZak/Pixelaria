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

using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixCore.Geometry;
using PixUI;
using PixUI.Controls;

namespace PixUITests
{
    public class TestInvalidateBaseView : BaseView
    {
        private readonly List<InvalidationRequest> _invalidationRequests = new List<InvalidationRequest>();

        public RedrawRegion InvalidateRegion { get; private set; }
        public ISpatialReference InvalidateReference { get; private set; }

        public IReadOnlyList<InvalidationRequest> InvalidationRequests => _invalidationRequests;

        protected override void Invalidate([NotNull] RedrawRegion region, [NotNull] ISpatialReference reference)
        {
            _invalidationRequests.Add(new InvalidationRequest(region, reference.GetAbsoluteTransform(), reference));

            InvalidateRegion = region;
            InvalidateReference = reference;
        }

        /// <summary>
        /// Resets invalidation request information.
        /// 
        /// Used by unit tests.
        /// </summary>
        public void _ResetInvalidation()
        {
            InvalidateRegion = null;
            InvalidateReference = null;
            _invalidationRequests.Clear();
        }

        /// <summary>
        /// Encapsulates an invalidation request made to a <see cref="TestInvalidateBaseView"/>.
        /// </summary>
        public class InvalidationRequest
        {
            public RedrawRegion InvalidateRegion { get; }
            public Matrix InvalidateMatrix { get; }
            public ISpatialReference InvalidateReference { get; }

            public InvalidationRequest([NotNull] RedrawRegion invalidateRegion, Matrix invalidateMatrix, ISpatialReference invalidateReference)
            {
                InvalidateRegion = invalidateRegion.Clone();
                InvalidateMatrix = invalidateMatrix;
                InvalidateReference = invalidateReference;
            }
        }
    }

    public static class TestInvalidateBaseViewAssert
    {
        /// <summary>
        /// Asserts a given view's <see cref="BaseView.Bounds"/> was invalidated on this <see cref="TestInvalidateBaseView"/>.
        /// </summary>
        public static void AssertViewBoundsWhereInvalidated([NotNull] this TestInvalidateBaseView invView, [NotNull] BaseView view)
        {
            var bounds = view.BoundsForInvalidate();

            if (!FindInvalidateRequest(invView, view, bounds))
            {
                var message = new StringBuilder();

                message.AppendLine($"View {view}'s bounds where not invalidated.");
                AddInvalidationInfoMessage(message, invView, bounds, view);

                Assert.Fail(message.ToString());
            }
        }

        /// <summary>
        /// Asserts a given view's <see cref="BaseView.GetFullBounds"/> was invalidated on this <see cref="TestInvalidateBaseView"/>.
        /// </summary>
        public static void AssertViewFullBoundsWhereInvalidated([NotNull] this TestInvalidateBaseView invView, [NotNull] BaseView view)
        {
            var bounds = view.BoundsForInvalidateFullBounds();
            
            if (!FindInvalidateRequest(invView, view, bounds))
            {
                var message = new StringBuilder();

                message.AppendLine($"View {view}'s total bounds where not invalidated.");
                AddInvalidationInfoMessage(message, invView, bounds, view);

                Assert.Fail(message.ToString());
            }
        }
        
        private static bool FindInvalidateRequest([NotNull] TestInvalidateBaseView invView, BaseView view, AABB bounds)
        {
            foreach (var r in invView.InvalidationRequests)
            {
                if (!Equals(r.InvalidateReference, view))
                    continue;

                if (r.InvalidateRegion.GetRectangles().Contains(bounds.TransformedBounds(r.InvalidateMatrix)))
                    return true;
            }

            return false;
        }

        private static void AddInvalidationInfoMessage([NotNull] StringBuilder builder, [NotNull] TestInvalidateBaseView invView, AABB bounds, [NotNull] BaseView view)
        {
            builder.AppendLine($"Expected invalidation request: {bounds} for view {view}");
            builder.AppendLine("Received invalidation requests:");

            for (int i = 0; i < invView.InvalidationRequests.Count; i++)
            {
                var req = invView.InvalidationRequests[i];

                string reqs = string.Join(", ", req.InvalidateRegion.GetRectangles().Select(r => r.ToString()));

                builder.Append($"{i + 1}: Regions: (");
                builder.Append(reqs);
                builder.AppendLine($") View: {req.InvalidateReference}");
            }
        }
    }
}
