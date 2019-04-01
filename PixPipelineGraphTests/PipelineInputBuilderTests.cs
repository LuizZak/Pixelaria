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
using PixPipelineGraph;
using System;
using System.Linq;

namespace PixPipelineGraphTests
{
    [TestClass]
    public class PipelineInputBuilderTests
    {
        [TestMethod]
        public void TestAddInputType()
        {
            // Arrange
            var unitUnderTest = CreatePipelineInputBuilder();

            // Act
            unitUnderTest.AddInputType(typeof(string));
            unitUnderTest.AddInputType(typeof(int));

            // Assert
            var result = RunBuild(unitUnderTest, "input");
            Assert.IsTrue(result.DataTypes.Contains(typeof(string)));
            Assert.IsTrue(result.DataTypes.Contains(typeof(int)));
            Assert.AreEqual(result.DataTypes.Count, 2);
            Assert.AreEqual(result.Name, "input");
        }

        [TestMethod]
        public void TestSetName()
        {
            // Arrange
            var unitUnderTest = CreatePipelineInputBuilder();

            // Act
            unitUnderTest.SetName("new_input");

            // Assert
            var result = RunBuild(unitUnderTest, "input");
            Assert.AreEqual(result.Name, "new_input");
        }

        #region Instantiation

        private static InternalPipelineInput RunBuild(PipelineInputBuilder builder, string name)
        {
            var node = CreatePipelineNode();
            return builder.Build(node, node.NextAvailableInputId(), name);
        }

        private static PipelineNodeId CreatePipelineNodeId()
        {
            var id = new PipelineNodeId(Guid.Empty);
            return id;
        }

        private static PipelineNode CreatePipelineNode()
        {
            var id = CreatePipelineNodeId();
            return new PipelineNode(id);
        }

        private static PipelineInputBuilder CreatePipelineInputBuilder()
        {
            return new PipelineInputBuilder();
        }

        #endregion
    }
}
