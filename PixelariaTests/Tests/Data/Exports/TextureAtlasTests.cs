using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Data.Exports;
using PixelariaTests.Generators;

namespace PixelariaTests.Tests.Data.Exports
{
    /// <summary>
    /// Tests the TextureAtlas class' functionality and related components
    /// </summary>
    [TestClass]
    public class TextureAtlasTests
    {
        [TestMethod]
        public void TestTextureAtlasCreation()
        {
            TextureAtlas atlas = new TextureAtlas(AnimationSheetGenerator.GenerateDefaultAnimationExportSettings(), "TestAtlas");
        }
    }
}