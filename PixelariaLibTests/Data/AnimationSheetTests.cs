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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelariaLib.Data;
using PixelariaLibTests.TestGenerators;

namespace PixelariaLibTests.Data
{
    /// <summary>
    /// Tests AnimationSheet class functionality and related components
    /// </summary>
    [TestClass]
    public class AnimationSheetTests
    {
        [TestMethod]
        public void TestAnimationSheetClone()
        {
            var sheet1 = AnimationSheetGenerator.GenerateAnimationSheet("TestSheet", 2, 64, 64, 10, 0);
            var sheet2 = sheet1.Clone();

            Assert.AreEqual(sheet1, sheet2, "Animation sheets copied with the Clone() method must be equal");

            // Modify one of the sheet's properties
            var frame = sheet2.Animations[0].Frames[0] as Frame;
            frame?.SetFrameBitmap(FrameGenerator.GenerateDifferentFrom(sheet1.Animations[0].Frames[0].GetComposedBitmap()));

            Assert.AreNotEqual(sheet1, sheet2, "After modification of a cloned animation sheet's animation's frame, it must no longer be considered equal to the original");

            // Modify one of the sheet's properties
            frame = sheet2.Animations[0].Frames[0] as Frame;
            frame?.SetFrameBitmap(FrameGenerator.GenerateDifferentFrom(sheet1.Animations[0].Frames[0].GetComposedBitmap()));

            sheet2.ExportSettings = new AnimationExportSettings {ExportJson = !sheet1.ExportSettings.ExportJson};

            Assert.AreNotEqual(sheet1, sheet2, "After modification of a cloned animation sheet's export settings, it must no longer be considered equal to the original");
        }

        [TestMethod]
        public void TestAnimationSheetFrameCount()
        {
            var r = new Random();

            for (int i = 0; i < 10; i++)
            {
                var animCount = r.Next(1, 10);
                var frameCount = r.Next(1, 10);
                var sheet1 = AnimationSheetGenerator.GenerateAnimationSheet("TestSheet", animCount, 64, 64, frameCount, 0);

                Assert.AreEqual(sheet1.GetFrameCount(), animCount * frameCount, "GetFrameCount() must return the frame count of all the animations in a sprite sheet");
            }
        }

        [TestMethod]
        public void TestAnimationIndex()
        {
            var sheet = new AnimationSheet("TestSheet");
            var anim1 = new Animation("TestAnim1", 16, 16);
            var anim2 = new Animation("TestAnim2", 16, 16);
            var anim3 = new Animation("TestAnim2", 16, 16);

            sheet.AddAnimation(anim1);
            sheet.AddAnimation(anim2);

            Assert.AreEqual(0, sheet.IndexOfAnimation(anim1), "The IndexOfAnimation() must return the index at which the specified animation is on the sprite sheet's internal container");
            Assert.AreEqual(1, sheet.IndexOfAnimation(anim2), "The IndexOfAnimation() must return the index at which the specified animation is on the sprite sheet's internal container");
            Assert.AreEqual(-1, sheet.IndexOfAnimation(anim3), "The IndexOfAnimation() must return -1 to animations that the sheet doesn't contain");
        }

        [TestMethod]
        public void TestAnimationInsert()
        {
            var sheet = new AnimationSheet("TestSheet");
            var anim1 = new Animation("TestAnim1", 16, 16);
            var anim2 = new Animation("TestAnim2", 16, 16);
            var anim3 = new Animation("TestAnim3", 16, 16);

            sheet.AddAnimation(anim1);
            sheet.InsertAnimation(anim2, 0);
            sheet.InsertAnimation(anim3, 0);

            Assert.AreEqual(2, sheet.IndexOfAnimation(anim1), "The InsertAnimation() method must bump the animations forward one index");
            Assert.AreEqual(0, sheet.IndexOfAnimation(anim3), "The InsertAnimation() method must insert animations in the specified position");
        }
    }
}