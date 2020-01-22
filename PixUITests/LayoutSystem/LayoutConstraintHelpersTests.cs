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

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixUI.LayoutSystem;
using Cassowary;
using JetBrains.Annotations;

namespace PixUITests.LayoutSystem
{
    [TestClass]
    public class LayoutConstraintHelpersTests
    {
        [TestMethod]
        public void TestStrengthFromPriority()
        {
            // Required priority
            Assert.AreEqual(LayoutConstraintHelpers.StrengthFromPriority(1000), ClStrength.Required);
            Assert.AreEqual(LayoutConstraintHelpers.StrengthFromPriority(9999), ClStrength.Required);

            AssertStrengthEqual(
                LayoutConstraintHelpers.StrengthFromPriority(900),
                0.9f, 0.0f, 0.0f);
            AssertStrengthEqual(
                LayoutConstraintHelpers.StrengthFromPriority(990),
                0.9f, 0.9f, 0.0f);
            AssertStrengthEqual(
                LayoutConstraintHelpers.StrengthFromPriority(100),
                0.1f, 0.0f, 0.0f);
            AssertStrengthEqual(
                LayoutConstraintHelpers.StrengthFromPriority(915),
                0.9f, 0.1f, 0.5f);
            AssertStrengthEqual(
                LayoutConstraintHelpers.StrengthFromPriority(1),
                0.0f, 0.0f, 0.1f);
        }

        private void AssertStrengthEqual([NotNull] ClStrength strength, double w1, double w2, double w3)
        {
            var weights = ExtractStrengthWeights(strength);
            Assert.AreEqual(3, weights.Length);

            Assert.AreEqual(w1, weights[0], 0.001f);
            Assert.AreEqual(w2, weights[1], 0.001f);
            Assert.AreEqual(w3, weights[2], 0.001f);
        }

        private double[] ExtractStrengthWeights([NotNull] ClStrength strength)
        {
            var field = strength.SymbolicWeight.GetType().GetField("_values", BindingFlags.NonPublic | BindingFlags.Instance);

            Debug.Assert(field != null, nameof(field) + " != null");
            var collection = (ReadOnlyCollection<double>) field.GetValue(strength.SymbolicWeight);

            return collection.ToArray();
        }
    }
}
