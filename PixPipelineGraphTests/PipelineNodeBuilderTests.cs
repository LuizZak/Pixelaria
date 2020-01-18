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
using PixPipelineGraph;

namespace PixPipelineGraphTests
{
    [TestClass]
    public class PipelineNodeBuilderTests
    {
        [TestMethod]
        public void TestCreateInput()
        {
            // Arrange
            var unitUnderTest = CreatePipelineNodeBuilder();

            // Act
            unitUnderTest.CreateInput("input", builder =>
            {
                builder.SetInputType(typeof(string));
                builder.SetInputType(typeof(int));
            });

            // Assert
            Assert.AreEqual(unitUnderTest.Build(CreatePipelineNodeId()).InternalInputs.Count, 1);
        }

        [TestMethod]
        public void TestCreateOutput()
        {
            // Arrange
            var unitUnderTest = CreatePipelineNodeBuilder();

            // Act
            unitUnderTest.CreateOutput("output");

            // Assert
            Assert.AreEqual(unitUnderTest.Build(CreatePipelineNodeId()).InternalOutputs.Count, 1);
        }

        #region Instantiation

        private static PipelineNodeId CreatePipelineNodeId()
        {
            var id = new PipelineNodeId(Guid.Empty);
            return id;
        }

        private static PipelineNodeBuilder CreatePipelineNodeBuilder()
        {
            return new PipelineNodeBuilder(new MockPipelineNodeProvider(), new PipelineNodeKind("custom"));
        }

        #endregion
    }
}
