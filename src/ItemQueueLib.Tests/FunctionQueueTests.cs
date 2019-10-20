using Microsoft.VisualStudio.TestTools.UnitTesting;
using SMC.Utilities.Queues;
using System;
using System.Collections.Generic;

namespace ItemQueueLib.Tests
{
    [TestClass]
    public class FunctionQueueTests
    {
        [TestMethod]
        public void SummationTest()
        {
            var sum = 0;
            var functionQueue = new FunctionQueue<int>();
            functionQueue.SetAction(value => sum += value);
            functionQueue.Start();
            functionQueue.Enqueue(1);
            functionQueue.Stop(true);

            Assert.IsTrue(sum == 1);
        }

        [TestMethod]
        public void SummationListTest()
        {
            var sum = 0;
            var functionQueue = new FunctionQueue<int>();
            functionQueue.SetAction(value => sum += value);
            functionQueue.Start();
            functionQueue.Enqueue(new List<int>() { 1 });
            functionQueue.Stop(true);

            Assert.IsTrue(sum == 1);
        }

        [TestMethod]
        public void MultipleSummationTest()
        {
            var sum = 0;
            var functionQueue = new FunctionQueue<int>();
            functionQueue.SetAction(value => sum += value);
            functionQueue.Start();
            functionQueue.Enqueue(1);
            functionQueue.Enqueue(2);
            functionQueue.Enqueue(3);
            functionQueue.Enqueue(4);
            functionQueue.Enqueue(5);
            functionQueue.Stop(true);

            Assert.IsTrue(sum == 15);
        }

        [TestMethod]
        public void MultipleSummationListTest()
        {
            var sum = 0;
            var functionQueue = new FunctionQueue<int>();
            functionQueue.SetAction(value => sum += value);
            functionQueue.Start();
            functionQueue.Enqueue(new List<int>() { 1, 2, 3, 4, 5 });
            functionQueue.Stop(true);

            Assert.IsTrue(sum == 15);
        }

        [TestMethod]
        public void MultiplicationTest()
        {
            var result = 2;
            var functionQueue = new FunctionQueue<int>();
            functionQueue.SetAction(value => result *= value);
            functionQueue.Start();
            functionQueue.Enqueue(4);
            functionQueue.Stop(true);

            Assert.IsTrue(result == 8);
        }

        [TestMethod]
        public void StringConcatTest()
        {
            var result = string.Empty;
            var functionQueue = new FunctionQueue<string>();
            functionQueue.SetAction(value => result = string.Concat(result, value));
            functionQueue.Start();
            functionQueue.Enqueue("Hello");
            functionQueue.Enqueue(", World!");
            functionQueue.Stop(true);

            Assert.AreEqual(result, "Hello, World!", false);
        }

        [TestMethod]
        public void StringConcatListTest()
        {
            var result = string.Empty;
            var functionQueue = new FunctionQueue<string>();
            functionQueue.SetAction(value => result = string.Concat(result, value));
            functionQueue.Start();
            functionQueue.Enqueue(new List<string>() { "Hello", ", World!" });
            functionQueue.Stop(true);

            Assert.AreEqual(result, "Hello, World!", false);
        }

        //TODO: Test callbacks

        [TestMethod]
        [ExpectedException(typeof(NoActionException))]
        public void NoActionSetTest()
        {
            var functionQueue = new FunctionQueue<int>();
            functionQueue.Start();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullActionSetTest()
        {
            var functionQueue = new FunctionQueue<int>();
            functionQueue.SetAction(value => value++);
            functionQueue.SetAction(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullCallbackSetTest()
        {
            var functionQueue = new FunctionQueue<int>();
            functionQueue.SetCallback(null);
        }
    }
}