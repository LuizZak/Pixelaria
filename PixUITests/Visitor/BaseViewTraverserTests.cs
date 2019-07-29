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
using PixUI;
using PixUI.Visitor;
using Rhino.Mocks;

namespace PixUITests.Visitor
{
    [TestClass]
    public class BaseViewTraverserTests
    {
        public BaseView Root;
        public BaseView Child1;
        public BaseView Child2;
        public BaseView GrandChild1;

        [TestInitialize]
        public void TestInitialize()
        {
            Root = new BaseView("Root");
            Child1 = new BaseView("Child 1");
            Child2 = new BaseView("Child 2");
            GrandChild1 = new BaseView("Grandchild 1");

            Root.AddChild(Child1);
            Root.AddChild(Child2);
            Child1.AddChild(GrandChild1);
        }

        [TestMethod]
        public void TestVisit()
        {
            // Arrange
            const string state = "abc";
            var visitor = MockRepository.GenerateMock<IBaseViewVisitor<object>>();
            var sut = new BaseViewTraverser<object>(state, visitor);

            using (visitor.GetMockRepository().Ordered())
            {
                // Root
                visitor.Expect(v => v.ShouldVisitView(state, Root)).Return(true);
                visitor.Expect(v => v.OnVisitorEnter(state, Root));
                visitor.Expect(v => v.VisitView(state, Root)).Return(VisitViewResult.VisitChildren);

                // Root > Child 1
                visitor.Expect(v => v.ShouldVisitView(state, Child1)).Return(true);
                visitor.Expect(v => v.OnVisitorEnter(state, Child1));
                visitor.Expect(v => v.VisitView(state, Child1)).Return(VisitViewResult.VisitChildren);

                // Root > Child 1 > Grandchild 1
                visitor.Expect(v => v.ShouldVisitView(state, GrandChild1)).Return(true);
                visitor.Expect(v => v.OnVisitorEnter(state, GrandChild1));
                visitor.Expect(v => v.VisitView(state, GrandChild1)).Return(VisitViewResult.VisitChildren);
                visitor.Expect(v => v.OnVisitorExit(state, GrandChild1));

                // Root > Child 1
                visitor.Expect(v => v.OnVisitorExit(state, Child1));

                // Root > Child 2
                visitor.Expect(v => v.ShouldVisitView(state, Child2)).Return(true);
                visitor.Expect(v => v.OnVisitorEnter(state, Child2));
                visitor.Expect(v => v.VisitView(state, Child2)).Return(VisitViewResult.VisitChildren);
                visitor.Expect(v => v.OnVisitorExit(state, Child2));

                // Root
                visitor.Expect(v => v.OnVisitorExit(state, Root));
            }

            // Act
            sut.Visit(Root);

            // Assert
            visitor.VerifyAllExpectations();
        }

        [TestMethod]
        public void TestIgnoreWhenShouldVisitViewReturnsFalse()
        {
            // Arrange
            const string state = "abc";
            var visitor = MockRepository.GenerateMock<IBaseViewVisitor<object>>();
            var sut = new BaseViewTraverser<object>(state, visitor);

            using (visitor.GetMockRepository().Ordered())
            {
                // Root
                visitor.Expect(v => v.ShouldVisitView(state, Root)).Return(true);
                visitor.Expect(v => v.OnVisitorEnter(state, Root));
                visitor.Expect(v => v.VisitView(state, Root)).Return(VisitViewResult.VisitChildren);

                // Root > Child 1
                visitor.Expect(v => v.ShouldVisitView(state, Child1)).Return(false);
                
                // Root > Child 2
                visitor.Expect(v => v.ShouldVisitView(state, Child2)).Return(true);
                visitor.Expect(v => v.OnVisitorEnter(state, Child2));
                visitor.Expect(v => v.VisitView(state, Child2)).Return(VisitViewResult.VisitChildren);
                visitor.Expect(v => v.OnVisitorExit(state, Child2));

                // Root
                visitor.Expect(v => v.OnVisitorExit(state, Root));
            }

            // Act
            sut.Visit(Root);

            // Assert
            visitor.VerifyAllExpectations();
        }
    }
}
