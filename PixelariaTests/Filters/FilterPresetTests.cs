using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Filters;

namespace PixelariaTests.Filters
{
    /// <summary>
    /// Tests the FilterPreset class and related components
    /// </summary>
    [TestClass]
    public class FilterPresetTests
    {
        [TestMethod]
        public void TestEquality()
        {
            var filter1 = new TransparencyFilter { Transparency = 1.0f };
            var filter2 = new TransparencyFilter { Transparency = 0.0f };

            var preset1 = new FilterPreset("Preset 1", new IFilter[] { filter1 });
            var preset2 = new FilterPreset("Preset 2", new IFilter[] { filter2 });
            var preset3 = new FilterPreset("Preset 3", new IFilter[] { filter1 });

            Assert.IsFalse(preset1.Equals(preset2));
            Assert.IsTrue(preset1.Equals(preset3));
        }

        [TestMethod]
        public void TestEqualityUnordered()
        {
            var filter1 = new TransparencyFilter { Transparency = 1.0f };
            var filter2 = new TransparencyFilter { Transparency = 0.0f };

            var preset1 = new FilterPreset("Preset 1", new IFilter[] { filter1, filter2 });
            var preset2 = new FilterPreset("Preset 2", new IFilter[] { filter2, filter1 });

            Assert.IsTrue(preset1.Equals(preset2));
        }
    }
}
