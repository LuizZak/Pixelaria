using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Filters;

namespace PixelariaTests.PixelariaTests.Tests.Filters
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
            TransparencyFilter filter1 = new TransparencyFilter { Transparency = 1.0f };
            TransparencyFilter filter2 = new TransparencyFilter { Transparency = 0.0f };

            FilterPreset preset1 = new FilterPreset("Preset 1", new IFilter[] { filter1 });
            FilterPreset preset2 = new FilterPreset("Preset 2", new IFilter[] { filter2 });
            FilterPreset preset3 = new FilterPreset("Preset 3", new IFilter[] { filter1 });

            Assert.IsFalse(preset1.Equals(preset2));
            Assert.IsTrue(preset1.Equals(preset3));
        }

        [TestMethod]
        public void TestEqualityUnordered()
        {
            TransparencyFilter filter1 = new TransparencyFilter { Transparency = 1.0f };
            TransparencyFilter filter2 = new TransparencyFilter { Transparency = 0.0f };

            FilterPreset preset1 = new FilterPreset("Preset 1", new IFilter[] { filter1, filter2 });
            FilterPreset preset2 = new FilterPreset("Preset 2", new IFilter[] { filter2, filter1 });

            Assert.IsTrue(preset1.Equals(preset2));
        }
    }
}
