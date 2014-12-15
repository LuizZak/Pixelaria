using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Data;
using Pixelaria.Data.Persistence;
using Pixelaria.Utils;
using PixelariaTests.Generators;

namespace PixelariaTests.Data.Persistence
{
    /// <summary>
    /// Tests PixelariaFile, PixelariaFileLoader, and PixelariaFileSaver functionalities and related components
    /// </summary>
    [TestClass]
    public class PersistenceTests
    {
        [TestMethod]
        public void TestPersistence()
        {
            Stream stream = new MemoryStream();
            Bundle bundle = BundleGenerator.GenerateTestBundle(0);

            PixelariaFile originalFile = new PixelariaFile(bundle, stream);

            PixelariaFileSaver.Save(originalFile);

            // Test if the memory stream is now filled
            Assert.IsTrue(stream.Length > 0, "After a call to PixelariaFileSaver.Save(), the pixelaria file's stream should not be empty");

            // Bring the bundle back with a PixelariaFileLoader
            PixelariaFile newFile = new PixelariaFile(new Bundle(""), stream);
            stream.Position = 0;
            PixelariaFileLoader.Load(newFile);

            Assert.AreEqual(originalFile.LoadedBundle, newFile.LoadedBundle, "After persisting a file and loading it back up again, the bundles must be equal");

            // Save the bundle a few more times to test resilience of the save/load process
            newFile.CurrentStream.Position = 0;
            PixelariaFileLoader.Load(newFile);
            newFile.CurrentStream.Position = 0;
            PixelariaFileSaver.Save(newFile);

            Assert.IsTrue(
                Utilities.ByteArrayCompare(((MemoryStream) newFile.CurrentStream).GetBuffer(),
                    ((MemoryStream) originalFile.CurrentStream).GetBuffer()), "Two streams that represent the same Pixelaria File should be bitwise equal");

            Assert.AreEqual(originalFile.LoadedBundle, newFile.LoadedBundle, "After persisting a file and loading it back up again, the bundles must be equal");
        }
    }
}