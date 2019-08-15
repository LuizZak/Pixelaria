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
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixPipelineGraph;

namespace PixPipelineGraphTests
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class PipelineGraph_ComputationTests
    {
        [TestMethod]
        public void TestComputationGraph()
        {
            var bodyProvider = new MockPipelineBodyProvider();
            var graph = CreatePipelineGraph(bodyProvider);
            var multiplier = bodyProvider.Register(new[] {typeof(int)}, typeof(int), context =>
            {
                if (context.TryGetIndexedInputs(out IObservable<int> input))
                {
                    return AnyObservable.FromObservable(input.Select(i => i * 2));
                }
                
                return PipelineBodyInvocationResponse.Exception<int>(new InvalidOperationException("Expected integer input"));
            });
            var adder = bodyProvider.Register(new[] { typeof(int) }, typeof(int), context =>
            {
                if (context.TryGetIndexedInputs(out IObservable<int> input))
                {
                    return AnyObservable.FromObservable(input.Select(i => i + 2));
                }

                return PipelineBodyInvocationResponse.Exception<int>(new InvalidOperationException("Expected integer input"));
            });
            var source = bodyProvider.Register(Type.EmptyTypes, context => 5);
            var multiplierNode = graph.CreateNode(node =>
            {
                node.SetBody(multiplier);
                node.CreateInput("value", builder => { builder.SetInputType(typeof(int)); });
                node.CreateOutput("value", builder => { builder.SetOutputType(typeof(int)); });
            });
            var adderNode = graph.CreateNode(node =>
            {
                node.SetBody(adder);
                node.CreateInput("value", builder => { builder.SetInputType(typeof(int)); });
                node.CreateOutput("value", builder => { builder.SetOutputType(typeof(int)); });
            });
            var sourceNode = graph.CreateNode(node =>
            {
                node.SetBody(source);
                node.CreateOutput("value", builder => { builder.SetOutputType(typeof(int)); });
            });
            graph.Connect(sourceNode, multiplierNode);
            graph.Connect(multiplierNode, adderNode);

            var response = graph.Compute(graph.OutputsForNode(adderNode)[0]);

            AssertOutputEquals(response, new [] {12});
        }

        [TestMethod]
        public void TestComputationGraphInputWithMultipleOutputs()
        {
            var bodyProvider = new MockPipelineBodyProvider();
            var graph = CreatePipelineGraph(bodyProvider);
            var multiplier = bodyProvider.Register(new[] { typeof(int) }, typeof(int), context =>
            {
                if (context.TryGetIndexedInputs(out IObservable<int> input))
                {
                    return AnyObservable.FromObservable(input.Select(i => i * 2));
                }

                return PipelineBodyInvocationResponse.Exception<int>(new InvalidOperationException("Expected integer input"));
            });
            var adder = bodyProvider.Register(new[] { typeof(int) }, typeof(int), context =>
            {
                if (context.TryGetIndexedInputs(out IObservable<int> input))
                {
                    return AnyObservable.FromObservable(input.Select(i => i + 2));
                }

                return PipelineBodyInvocationResponse.Exception<int>(new InvalidOperationException("Expected integer input"));
            });
            var source1 = bodyProvider.Register(Type.EmptyTypes, context => 5);
            var source2 = bodyProvider.Register(Type.EmptyTypes, context => 7);
            var multiplierNode = graph.CreateNode(node =>
            {
                node.SetBody(multiplier);
                node.CreateInput("value", builder => { builder.SetInputType(typeof(int)); });
                node.CreateOutput("value", builder => { builder.SetOutputType(typeof(int)); });
            });
            var adderNode = graph.CreateNode(node =>
            {
                node.SetBody(adder);
                node.CreateInput("value", builder => { builder.SetInputType(typeof(int)); });
                node.CreateOutput("value", builder => { builder.SetOutputType(typeof(int)); });
            });
            var sourceNode1 = graph.CreateNode(node =>
            {
                node.SetBody(source1);
                node.CreateOutput("value", builder => { builder.SetOutputType(typeof(int)); });
            });
            var sourceNode2 = graph.CreateNode(node =>
            {
                node.SetBody(source2);
                node.CreateOutput("value", builder => { builder.SetOutputType(typeof(int)); });
            });

            graph.Connect(sourceNode1, multiplierNode);
            graph.Connect(sourceNode2, multiplierNode);
            graph.Connect(multiplierNode, adderNode);

            var response = graph.Compute(graph.OutputsForNode(adderNode)[0]);

            AssertOutputEquals(response, new[] { 12, 16 });
        }

        [TestMethod]
        public void TestComputationWithLambdaNodesCartesian()
        {
            var bodyProvider = new MockPipelineBodyProvider();
            var graph = CreatePipelineGraph(bodyProvider);
            var multiplier = graph.CreateFromLambda("multiplier", (int input) => input * 2);
            var adder = graph.CreateFromLambda("adder", (int v1, int v2) => v1 + v2);
            var divider = graph.CreateFromLambda("divider", (int input) => input / 2);
            var source1 = graph.CreateFromGenerator("source1", () => 1);
            var source2 = graph.CreateFromGenerator("source2", () => 2);
            graph.Connect(source1, multiplier);
            graph.Connect(source1, graph.InputsForNode(adder)[0]);
            graph.Connect(source2, divider);
            graph.Connect(source2, graph.InputsForNode(adder)[1]);
            graph.Connect(multiplier, graph.InputsForNode(adder)[0]);
            graph.Connect(divider, graph.InputsForNode(adder)[1]);

            var response = graph.Compute(graph.OutputsForNode(adder)[0]);

            AssertOutputEquals(response, new[] { 3, 2, 4, 3 });
        }

        [TestMethod]
        public void TestMultipleOutputs()
        {
            var bodyProvider = new MockPipelineBodyProvider();
            var graph = CreatePipelineGraph(bodyProvider);
            var dividerAndRem = graph.CreateNode(builder =>
            {
                builder.SetTitle("Divider and remainder");
                builder.CreateInput("dividend", typeof(int));
                builder.CreateInput("divisor", typeof(int));
                builder.CreateOutput("division", typeof(int));
                builder.CreateOutput("remainder", typeof(int));
                builder.SetBody(new PipelineBody(new PipelineBodyId(""), new []{typeof(int), typeof(int) }, typeof(int),
                    context =>
                    {
                        context.GetIndexedInputs(out IObservable<int> dividend, out IObservable<int> divisor);
                        return AnyObservable.FromObservable(dividend.SelectMany((dv, _) => { return divisor.Select(ds => (dv, ds)); }).Select(tuple => tuple.dv / tuple.ds));
                    }));
            });
            var source1 = graph.CreateFromGenerator("five", () => 5);
            var source2 = graph.CreateFromGenerator("two", () => 2);
            graph.Connect(source1, dividerAndRem);
            graph.Connect(source2, graph.InputsForNode(dividerAndRem)[1]);

            var resultDivision = graph.Compute(graph.OutputsForNode(dividerAndRem)[0]);
            var resultRemainder = graph.Compute(graph.OutputsForNode(dividerAndRem)[1]);

            AssertOutputEquals(resultDivision, new []{2});
            AssertOutputEquals(resultRemainder, new []{1});
        }

        [TestMethod]
        public void TestLazyInputExecution()
        {
            bool[] didInvokeSource1 = {false};
            bool[] didInvokeSource2 = {false};
            var bodyProvider = new MockPipelineBodyProvider();
            var graph = CreatePipelineGraph(bodyProvider);
            var incrementer = graph.CreateNode(builder =>
            {
                builder.CreateInput("value", typeof(int));
                builder.CreateInput("unused", typeof(int));
                builder.CreateOutput("result", typeof(int));
                builder.SetBody(new PipelineBody(new PipelineBodyId(""), new[] { typeof(int), typeof(int) }, typeof(int),
                    context =>
                    {
                        return AnyObservable.FromObservable(context.GetIndexedInput<int>(0).Select(i => i + 1));
                    }));
            });
            var source = graph.CreateFromGenerator("source", () =>
            {
                didInvokeSource1[0] = true;
                return 1;
            });
            var sourceUnused = graph.CreateFromGenerator("source (unused)", () =>
            {
                didInvokeSource2[0] = true;
                return 1;
            });
            graph.Connect(source, graph.InputsForNode(incrementer)[0]);
            graph.Connect(sourceUnused, graph.InputsForNode(incrementer)[1]);

            var result = graph.Compute(graph.OutputsForNode(incrementer)[0]);

            AssertOutputEquals(result, new [] {2});
            Assert.IsTrue(didInvokeSource1[0]);
            Assert.IsFalse(didInvokeSource2[0]);
        }

        [TestMethod]
        public void TestNotConnectedException()
        {
            var bodyProvider = new MockPipelineBodyProvider();
            var graph = CreatePipelineGraph(bodyProvider);
            var adder = graph.CreateFromLambda("adder", (int v1, int v2) => v1 + v2);

            var response = graph.Compute(graph.OutputsForNode(adder)[0]);

            var output = response.ToObservable<int>();

            try
            {
                var _ = output.ToEnumerable().ToList();
                Assert.Fail($"Expected to fail with {nameof(NotConnectedException)}");
            }
            catch (NotConnectedException)
            {
                // Success!
            }
            catch (Exception e)
            {
                Assert.Fail($"Expected to fail with {nameof(NotConnectedException)}, but failed with {e}");
            }
        }

        #region Instantiation

        private static PipelineGraph CreatePipelineGraph([CanBeNull] MockPipelineBodyProvider bodyProvider = null)
        {
            return new PipelineGraph(bodyProvider ?? new MockPipelineBodyProvider());
        }

        #endregion

        #region Assertion

        private void AssertOutputEquals<T>([NotNull] AnyObservable response, [NotNull] IReadOnlyList<T> values)
        {
            var array = response.ToObservable<T>().ToEnumerable().ToArray();

            if (!values.SequenceEqual(array))
            {
                string valuesString = string.Join(", ", values.Select(v => v.ToString()));
                string arrayString = string.Join(", ", array.Select(v => v.ToString()));

                Assert.Fail($"Expected sequence [{valuesString}] received [{arrayString}]");
            }
        }

        #endregion
    }
}
