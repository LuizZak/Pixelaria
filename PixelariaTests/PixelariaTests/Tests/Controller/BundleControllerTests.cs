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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Pixelaria.Controllers.DataControllers;
using Pixelaria.Data;

using PixelariaTests.PixelariaTests.Generators;

namespace PixelariaTests.PixelariaTests.Tests.Controller
{
    /// <summary>
    /// Tests the BundleContrillerTests and related components
    /// </summary>
    [TestClass]
    public class BundleControllerTests
    {
        [TestMethod]
        public void TestGetAnimations()
        {
            // Tests the GetAnimations method

            var bundle = new Bundle("Test");
            var anim1 = AnimationGenerator.GenerateAnimation("Test0", 32, 32, 1);
            var anim2 = AnimationGenerator.GenerateAnimation("Test1", 32, 32, 1);
            bundle.AddAnimation(anim1);
            bundle.AddAnimation(anim2);

            var controller = new BundleController(bundle);

            // Fetch animations from controller
            var animations = controller.GetAnimations();

            Assert.AreEqual(animations.Length, 2);

            // IDs shuuld be ordered
            Assert.AreEqual(animations[0].Id, anim1.ID);
            Assert.AreEqual(animations[1].Id, anim2.ID);
        }

        [TestMethod]
        public void TestGetAnimationByName()
        {
            // Tests the GetAnimationByName method

            var bundle = new Bundle("Test");
            var anim1 = AnimationGenerator.GenerateAnimation("Test0", 32, 32, 1);
            var anim2 = AnimationGenerator.GenerateAnimation("Test1", 32, 32, 1);
            bundle.AddAnimation(anim1);
            bundle.AddAnimation(anim2);

            var controller = new BundleController(bundle);

            // Fetch animations from controller
            var animId1 = controller.GetAnimationByName("Test0");
            var animId2 = controller.GetAnimationByName("Test1");
            var animId3 = controller.GetAnimationByName("Non-Existant");

            Assert.AreEqual(anim1.ID, animId1.Id);
            Assert.AreEqual(anim2.ID, animId2.Id);
            Assert.IsNull(animId3);
        }

        [TestMethod]
        public void TestGetAnimationMetadata()
        {
            // Tests the GetAnimationMetadata method

            var bundle = new Bundle("Test");
            var anim1 = AnimationGenerator.GenerateAnimation("Test0", 32, 32, 1);
            var anim2 = AnimationGenerator.GenerateAnimation("Test1", 32, 32, 2);
            bundle.AddAnimation(anim1);
            bundle.AddAnimation(anim2);

            var controller = new BundleController(bundle);

            var meta1 = controller.GetAnimationMetadata(controller.GetAnimationByName("Test0"));
            var meta2 = controller.GetAnimationMetadata(controller.GetAnimationByName("Test1"));

            Assert.AreEqual(meta1?.Name, "Test0");
            Assert.AreEqual(meta1?.FrameCount, 1);

            Assert.AreEqual(meta2?.Name, "Test1");
            Assert.AreEqual(meta2?.FrameCount, 2);
        }

        [TestMethod]
        public void TestGetAnimationMetadataInvalidId()
        {
            // Tests returning null in case an invalid animation id was used when trying to fetch the metadata for an animation

            var bundle = new Bundle("Test");
            var animation = AnimationGenerator.GenerateAnimation("Test0", 32, 32, 1);
            bundle.AddAnimation(animation);

            var controller = new BundleController(bundle);

            // This is a valid id
            var animationId = controller.GetAnimationByName("Test0");

            // Remove from the bundle - this invalidates the above animation id object
            bundle.RemoveAnimation(animation);

            Assert.IsNull(controller.GetAnimationMetadata(animationId));
        }
    }
}
