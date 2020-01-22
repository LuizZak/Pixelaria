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
        public void TestStrengthFromPriorityRequired()
        {
            // Required priority
            Assert.AreEqual(LayoutConstraintHelpers.StrengthFromPriority(1000), ClStrength.Required);
            Assert.AreEqual(LayoutConstraintHelpers.StrengthFromPriority(9999), ClStrength.Required);
        }

        [TestMethod]
        public void TestStrengthFromPriorityStrong()
        {
            Assert.AreEqual(LayoutConstraintHelpers.StrengthFromPriority(750), ClStrength.Strong);

            AssertStrengthEqual(
                LayoutConstraintHelpers.StrengthFromPriority(999),
                1.000f, 0.996f, 0.996f);
            AssertStrengthEqual(
                LayoutConstraintHelpers.StrengthFromPriority(990),
                1.000f, 0.960f, 0.960f);
            AssertStrengthEqual(
                LayoutConstraintHelpers.StrengthFromPriority(915),
                1.000f, 0.660f, 0.660f);
            AssertStrengthEqual(
                LayoutConstraintHelpers.StrengthFromPriority(900),
                1.000f, 0.600f, 0.600f);
            AssertStrengthEqual(
                LayoutConstraintHelpers.StrengthFromPriority(751),
                1.000f, 0.004f, 0.004f);
        }

        [TestMethod]
        public void TestStrengthFromPriorityMedium()
        {
            Assert.AreEqual(LayoutConstraintHelpers.StrengthFromPriority(500), ClStrength.Medium);

            AssertStrengthEqual(
                LayoutConstraintHelpers.StrengthFromPriority(501),
                0.004f, 0.004f, 0.004f);
            AssertStrengthEqual(
                LayoutConstraintHelpers.StrengthFromPriority(251),
                0.000f, 0.004f, 0.004f);
        }

        [TestMethod]
        public void TestStrengthFromPriorityWeak()
        {
            Assert.AreEqual(LayoutConstraintHelpers.StrengthFromPriority(250), ClStrength.Weak);

            AssertStrengthEqual(
                LayoutConstraintHelpers.StrengthFromPriority(100),
                0.000f, 0.000f, 0.400f);
            AssertStrengthEqual(
                LayoutConstraintHelpers.StrengthFromPriority(1),
                0.000f, 0.000f, 0.004f);
        }

        private void AssertStrengthEqual([NotNull] ClStrength strength, double w1, double w2, double w3)
        {
            string MakeString(double d1, double d2, double d3)
            {
                return $"{{ w1: {d1:#0.000}, w2: {d2:#0.000}, w3: {d3:#0.000} }}";
            }

            var weights = ExtractStrengthWeights(strength);
            Assert.AreEqual(3, weights.Length);

            const double delta = 0.001;
            if (Math.Abs(w1 - weights[0]) >= delta || Math.Abs(w2 - weights[1]) >= delta || Math.Abs(w3 - weights[2]) >= delta)
            {
                Assert.Fail($"Expected weights {MakeString(w1, w2, w3)} within {delta:#0.000} tolerance, received {MakeString(weights[0], weights[1], weights[2])}");
            }
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
