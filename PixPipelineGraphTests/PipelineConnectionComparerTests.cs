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
    public class PipelineConnectionComparerTests
    {
        [TestMethod]
        public void TestEquals()
        {
            // Arrange
            var node1Id = Guid.NewGuid();
            var node2Id = Guid.NewGuid();

            var unitUnderTest = CreatePipelineConnectionComparer();
            IPipelineConnection x = new PipelineConnection(CreatePipelineOutput(node1Id, 0), CreatePipelineInput(node2Id, 0));
            IPipelineConnection y = new PipelineConnection(CreatePipelineOutput(node1Id, 0), CreatePipelineInput(node2Id, 0));

            // Act
            bool areEqual = unitUnderTest.Equals(x, y);

            // Assert
            Assert.IsTrue(areEqual);
        }

        [TestMethod]
        public void TestNotEquals()
        {
            // Arrange
            var node1Id = Guid.NewGuid();
            var node2Id = Guid.NewGuid();

            var unitUnderTest = CreatePipelineConnectionComparer();
            IPipelineConnection x = new PipelineConnection(CreatePipelineOutput(node1Id, 0), CreatePipelineInput(node2Id, 0));
            IPipelineConnection y = new PipelineConnection(CreatePipelineOutput(node1Id, 1), CreatePipelineInput(node2Id, 1));

            // Act
            bool areEqual = unitUnderTest.Equals(x, y);

            // Assert
            Assert.IsFalse(areEqual);
        }

        [TestMethod]
        public void TestGetHashCodeEqual()
        {
            // Arrange
            var node1Id = Guid.NewGuid();
            var node2Id = Guid.NewGuid();

            var unitUnderTest = CreatePipelineConnectionComparer();
            IPipelineConnection hash1 = new PipelineConnection(CreatePipelineOutput(node1Id, 0), CreatePipelineInput(node2Id, 0));
            IPipelineConnection hash2 = new PipelineConnection(CreatePipelineOutput(node1Id, 0), CreatePipelineInput(node2Id, 0));

            // Assert
            Assert.AreEqual(unitUnderTest.GetHashCode(hash1), unitUnderTest.GetHashCode(hash2));
        }

        [TestMethod]
        public void TestGetHashCodeNotEqual()
        {
            // Arrange
            var node1Id = Guid.NewGuid();
            var node2Id = Guid.NewGuid();

            var unitUnderTest = CreatePipelineConnectionComparer();
            IPipelineConnection hash1 = new PipelineConnection(CreatePipelineOutput(node1Id, 0), CreatePipelineInput(node2Id, 0));
            IPipelineConnection hash2 = new PipelineConnection(CreatePipelineOutput(node1Id, 1), CreatePipelineInput(node2Id, 1));

            // Assert
            Assert.AreNotEqual(unitUnderTest.GetHashCode(hash1), unitUnderTest.GetHashCode(hash2));
        }

        #region Instantiation

        private static PipelineConnectionComparer CreatePipelineConnectionComparer()
        {
            return new PipelineConnectionComparer();
        }

        private static InternalPipelineInput CreatePipelineInput(Guid nodeId, int index)
        {
            var id = new PipelineNodeId(nodeId);

            return new InternalPipelineInput(new PipelineNode(id), new PipelineInput(id, index), "input", new Type[0]);
        }

        private static InternalPipelineOutput CreatePipelineOutput(Guid nodeId, int index)
        {
            var id = new PipelineNodeId(nodeId);

            return new InternalPipelineOutput(new PipelineNode(id), new PipelineOutput(id, index), "output", typeof(object));
        }

        #endregion
    }
}
