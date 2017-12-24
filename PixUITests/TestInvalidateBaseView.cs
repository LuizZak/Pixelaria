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
using System.Drawing;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixUI;

namespace PixUITests
{
    public class TestInvalidateBaseView : BaseView
    {
        private readonly List<InvalidationRequest> _invalidationRequests = new List<InvalidationRequest>();

        public Region InvalidateRegion { get; set; }
        public ISpatialReference InvalidateReference { get; set; }

        public IReadOnlyList<InvalidationRequest> InvalidationRequests => _invalidationRequests;

        protected override void Invalidate(Region region, ISpatialReference reference)
        {
            _invalidationRequests.Add(new InvalidationRequest(region, reference));

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
            public Region InvalidateRegion { get; }
            public ISpatialReference InvalidateReference { get; }

            public InvalidationRequest(Region invalidateRegion, ISpatialReference invalidateReference)
            {
                InvalidateRegion = invalidateRegion.Clone();
                InvalidateReference = invalidateReference;
            }

            ~InvalidationRequest()
            {
                InvalidateRegion.Dispose();
            }
        }
    }

    public static class TestInvalidateBaseViewAssert
    {
        /// <summary>
        /// Asserts a given view's <see cref="BaseView.Bounds"/> was invalidated on this <see cref="TestInvalidateBaseView"/>.
        /// </summary>
        public static void AssertViewBoundsWhereInvalidated(this TestInvalidateBaseView invView, BaseView view)
        {

            using (var img = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(img))
            {
                var testReg = new Region((RectangleF)view.Bounds);

                if (!invView.InvalidationRequests.Any(r => r.InvalidateRegion.Equals(testReg, g) && Equals(r.InvalidateReference, view)))
                {
                    Assert.Fail($"View {view}'s bounds was not invalidated.");
                }
            }
        }

        /// <summary>
        /// Asserts a given view's <see cref="BaseView.GetFullBounds"/> was invalidated on this <see cref="TestInvalidateBaseView"/>.
        /// </summary>
        public static void AssertViewFullBoundsWhereInvalidated(this TestInvalidateBaseView invView, BaseView view)
        {

            using (var img = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(img))
            {
                var testReg = new Region((RectangleF)view.GetFullBounds());

                if (!invView.InvalidationRequests.Any(r => r.InvalidateRegion.Equals(testReg, g) && Equals(r.InvalidateReference, view)))
                {
                    Assert.Fail($"View {view}'s bounds was not invalidated.");
                }
            }
        }
    }
}
