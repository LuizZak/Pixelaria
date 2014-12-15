using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Data;
using PixelariaTests.Generators;

namespace PixelariaTests.Data
{
    /// <summary>
    /// Tests the Bundle class functionalities and their respective components
    /// </summary>
    [TestClass]
    public class BundleTests
    {
        [TestMethod]
        public void TestBundleEquality()
        {
            Bundle bundle1 = BundleGenerator.GenerateTestBundle(0);
            Bundle bundle2 = bundle1.Clone();

            Assert.AreEqual(bundle1, bundle2, "After a Clone() operation, both Bundles must be equal");

            // Modify the new bundle
            bundle2.RemoveAnimationFromAnimationSheet(bundle2.Animations[0], bundle2.AnimationSheets[0]);

            Assert.AreNotEqual(bundle1, bundle2, "Equal bundles after a Clone() operation must not be equal after a successful call to RemoveAnimationFromAnimationSheet()");
        }
    }
}