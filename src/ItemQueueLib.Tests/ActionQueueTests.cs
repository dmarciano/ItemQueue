using Microsoft.VisualStudio.TestTools.UnitTesting;
using SMC.Utilities.Queues;
using System;
using System.Collections.Generic;

namespace ItemQueueLib.Tests
{
    [TestClass]
    public class ActionQueueTests
    {
        [TestMethod]
        public void SummationTest()
        {
            var sum = 0;
            var actionQueue = new ActionQueue<int>();
            actionQueue.SetAction(value => sum += value);
            actionQueue.Start();
            actionQueue.Enqueue(1);
            actionQueue.Stop(true);

            Assert.IsTrue(sum == 1);
        }

        [TestMethod]
        public void SummationListTest()
        {
            var sum = 0;
            var actionQueue = new ActionQueue<int>();
            actionQueue.SetAction(value => sum += value);
            actionQueue.Start();
            actionQueue.Enqueue(new List<int>() { 1 });
            actionQueue.Stop(true);

            Assert.IsTrue(sum == 1);
        }

        [TestMethod]
        public void MultipleSummationTest()
        {
            var sum = 0;
            var actionQueue = new ActionQueue<int>();
            actionQueue.SetAction(value => sum += value);
            actionQueue.Start();
            actionQueue.Enqueue(1);
            actionQueue.Enqueue(2);
            actionQueue.Enqueue(3);
            actionQueue.Enqueue(4);
            actionQueue.Enqueue(5);
            actionQueue.Stop(true);

            Assert.IsTrue(sum == 15);
        }

        [TestMethod]
        public void MultipleSummationListTest()
        {
            var sum = 0;
            var actionQueue = new ActionQueue<int>();
            actionQueue.SetAction(value => sum += value);
            actionQueue.Start();
            actionQueue.Enqueue(new List<int>() { 1, 2, 3, 4, 5 });
            actionQueue.Stop(true);

            Assert.IsTrue(sum == 15);
        }

        [TestMethod]
        public void MultiplicationTest()
        {
            var result = 2;
            var actionQueue = new ActionQueue<int>();
            actionQueue.SetAction(value => result *= value);
            actionQueue.Start();
            actionQueue.Enqueue(4);
            actionQueue.Stop(true);

            Assert.IsTrue(result == 8);
        }

        [TestMethod]
        public void StringConcatTest()
        {
            var result = string.Empty;
            var actionQueue = new ActionQueue<string>();
            actionQueue.SetAction(value => result = string.Concat(result, value));
            actionQueue.Start();
            actionQueue.Enqueue("Hello");
            actionQueue.Enqueue(", World!");
            actionQueue.Stop(true);

            Assert.AreEqual(result, "Hello, World!", false);
        }

        [TestMethod]
        public void StringConcatListTest()
        {
            var result = string.Empty;
            var actionQueue = new ActionQueue<string>();
            actionQueue.SetAction(value => result = string.Concat(result, value));
            actionQueue.Start();
            actionQueue.Enqueue(new List<string>() { "Hello", ", World!" });
            actionQueue.Stop(true);

            Assert.AreEqual(result, "Hello, World!", false);
        }

        //TODO: Test callbacks

        [TestMethod]
        [ExpectedException(typeof(NoActionException))]
        public void NoActionSetTest()
        {
            var actionQueue = new ActionQueue<int>();
            actionQueue.Start();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullActionSetTest()
        {
            var actionQueue = new ActionQueue<int>();
            actionQueue.SetAction(value => value++);
            actionQueue.SetAction(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullCallbackSetTest()
        {
            var actionQueue = new ActionQueue<int>();
            actionQueue.SetCallback(null);
        }
    }
}