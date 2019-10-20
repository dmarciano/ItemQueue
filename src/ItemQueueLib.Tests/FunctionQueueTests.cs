using Microsoft.VisualStudio.TestTools.UnitTesting;
using SMC.Utilities.Queues;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ItemQueueLib.Tests
{
    [TestClass]
    public class FunctionQueueTests
    {
        // ReSharper disable once InconsistentNaming
        private const string QUEUE_NAME = "function_queue_name";

        [TestMethod]
        public void ConstructorWithQueueNameTest()
        {
            var result = 0;
            var functionQueue = new FunctionQueue<int, int>(QUEUE_NAME);
            functionQueue.SetFunction(value => value * 2);
            functionQueue.SetAction(value => result = value);
            functionQueue.Start();
            functionQueue.Enqueue(1);
            functionQueue.Stop(true);

            Assert.IsTrue(result == 2);
            Assert.AreEqual(functionQueue.Name, QUEUE_NAME, false);
        }

        [TestMethod]
        public void ConstructorWithFunctionTest()
        {
            var result = 0;
            var functionQueue = new FunctionQueue<int, int>(value => value * 2);
            functionQueue.SetAction(value => result = value);
            functionQueue.Start();
            functionQueue.Enqueue(1);
            functionQueue.Stop(true);

            Assert.IsTrue(result == 2);
        }

        [TestMethod]
        public void ConstructorWithFunctionActionTest()
        {
            var result = 0;
            var functionQueue = new FunctionQueue<int, int>(value => value * 2, value => result = value);
            functionQueue.Start();
            functionQueue.Enqueue(1);
            functionQueue.Stop(true);

            Assert.IsTrue(result == 2);
        }

        [TestMethod]
        public void ConstructorWithFunctionCallbackTest()
        {
            var result = 0;
            var callbackInitialValue = 0;
            var callbackResultValue = 0;
            var functionQueue = new FunctionQueue<int, int>(value => value * 2, (i, r) =>
            {
                callbackInitialValue = i;
                callbackResultValue = r;
            });
            functionQueue.SetAction(value => result += value);
            functionQueue.Start();
            functionQueue.Enqueue(2);
            functionQueue.Stop(true);

            Assert.IsTrue(result == 4);
            Assert.IsTrue(callbackInitialValue == 2);
            Assert.IsTrue(callbackResultValue == 4);
        }

        [TestMethod]
        public void ConstructorWithFunctionActionCallbackTest()
        {
            var result = 0;
            var callbackInitialValue = 0;
            var callbackResultValue = 0;
            var functionQueue = new FunctionQueue<int, int>(value => value * 2, value => result += value, (i, r) =>
            {
                callbackInitialValue = i;
                callbackResultValue = r;
            });

            functionQueue.Start();
            functionQueue.Enqueue(2);
            functionQueue.Stop(true);

            Assert.IsTrue(result == 4);
            Assert.IsTrue(callbackInitialValue == 2);
            Assert.IsTrue(callbackResultValue == 4);
        }

        [TestMethod]
        public void ConstructorWithFunctionQueueNameTest()
        {
            var sum = 0;
            var functionQueue = new FunctionQueue<int, int>(value => sum += value, QUEUE_NAME);
            functionQueue.SetAction(value => sum = value);
            functionQueue.Start();
            functionQueue.Enqueue(1);
            functionQueue.Enqueue(2);
            functionQueue.Stop(true);

            Assert.IsTrue(sum == 3);
            Assert.AreEqual(functionQueue.Name, QUEUE_NAME, false);
        }

        [TestMethod]
        public void ConstructorWithFunctionCallbackQueueTest()
        {
            var result = 0;
            var callbackInitialValue = 0;
            var callbackResultValue = 0;
            var functionQueue = new FunctionQueue<int, int>(value => value *2, (i, r) =>
            {
                callbackInitialValue = i;
                callbackResultValue = r;
            }, QUEUE_NAME);
            functionQueue.SetAction(value => result += value);
            functionQueue.Start();
            functionQueue.Enqueue(2);
            functionQueue.Stop(true);

            Assert.IsTrue(result == 4);
            Assert.IsTrue(callbackInitialValue == 2);
            Assert.IsTrue(callbackResultValue == 4);
            Assert.AreEqual(functionQueue.Name, QUEUE_NAME, false);
        }

        [TestMethod]
        public void ConstructorWithAllParametersTest()
        {
            var result = 0;
            var callbackInitialValue = 0;
            var callbackResultValue = 0;
            var functionQueue = new FunctionQueue<int, int>(value => value * 2, value => result += value,(i, r) =>
            {
                callbackInitialValue = i;
                callbackResultValue = r;
            }, QUEUE_NAME);
            functionQueue.Start();
            functionQueue.Enqueue(2);
            functionQueue.Stop(true);

            Assert.IsTrue(result == 4);
            Assert.IsTrue(callbackInitialValue == 2);
            Assert.IsTrue(callbackResultValue == 4);
            Assert.AreEqual(functionQueue.Name, QUEUE_NAME, false);
        }

        [TestMethod]
        public void DoubleValueTest()
        {
            var result = 0;
            var functionQueue = new FunctionQueue<int, int>();
            functionQueue.SetFunction(value => value * 2);
            functionQueue.SetAction(value => result = value);
            functionQueue.Start();
            functionQueue.Enqueue(1);
            functionQueue.Stop(true);

            Assert.IsTrue(result == 2);
        }

        [TestMethod]
        public void DoubleValueListTest()
        {
            var result = 0;
            var functionQueue = new FunctionQueue<int, int>();
            functionQueue.SetFunction(value => value * 2);
            functionQueue.SetAction(value => result = value);
            functionQueue.Start();
            functionQueue.Enqueue(new List<int>() { 1 });
            functionQueue.Stop(true);

            Assert.IsTrue(result == 2);
        }

        [TestMethod]
        public void MultipleDoubleValueTest()
        {
            var sum = 0;
            var functionQueue = new FunctionQueue<int, int>();
            functionQueue.SetFunction(value => value * 2);
            functionQueue.SetAction(value => sum += value);
            functionQueue.Start();
            functionQueue.Enqueue(1);
            functionQueue.Enqueue(2);
            functionQueue.Enqueue(3);
            functionQueue.Enqueue(4);
            functionQueue.Enqueue(5);
            functionQueue.Stop(true);

            Assert.IsTrue(sum == 1 * 2 + 2 * 2 + 3 * 2 + 4 * 2 + 5 * 2);
        }

        [TestMethod]
        public void MultipleDoubleListTest()
        {
            var sum = 0;
            var functionQueue = new FunctionQueue<int, int>();
            functionQueue.SetFunction(value => value * 2);
            functionQueue.SetAction(value => sum += value);
            functionQueue.Start();
            functionQueue.Enqueue(new List<int>() { 1, 2, 3, 4, 5 });
            functionQueue.Stop(true);

            Assert.IsTrue(sum == 1 * 2 + 2 * 2 + 3 * 2 + 4 * 2 + 5 * 2);
        }

        [TestMethod]
        public void CharCountTest()
        {
            var charCount = 0;
            var functionQueue = new FunctionQueue<string, int>();
            functionQueue.SetFunction(value => value.Length);
            functionQueue.SetAction(value => charCount += value);
            functionQueue.Start();
            functionQueue.Enqueue("Hello");
            functionQueue.Enqueue(", World!");
            functionQueue.Stop(true);

            Assert.IsTrue(charCount == "Hello, World!".Length);
        }

        [TestMethod]
        public void CharCountListTest()
        {
            var charCount = 0;
            var functionQueue = new FunctionQueue<string, int>();
            functionQueue.SetFunction(value => value.Length);
            functionQueue.SetAction(value => charCount += value);
            functionQueue.Start();
            functionQueue.Enqueue(new List<string>() { "Hello", ", World!" });
            functionQueue.Stop(true);

            Assert.IsTrue(charCount == "Hello, World!".Length);
        }

        [TestMethod]
        public void DoubleValueCallbackTest()
        {
            var result = 0;
            var callbackInitialValue = 0;
            var callbackResultValue = 0;
            var functionQueue = new FunctionQueue<int, int>();
            functionQueue.SetFunction(value => value * 2);
            functionQueue.SetCallback((i, r) =>
            {
                callbackInitialValue = i;
                callbackResultValue = r;
            });
            functionQueue.SetAction(value => result = value);
            functionQueue.Start();
            functionQueue.Enqueue(1);
            functionQueue.Stop(true);

            Assert.IsTrue(result == 2);
            Assert.IsTrue(callbackInitialValue == 1);
            Assert.IsTrue(callbackResultValue==2);
        }

        [TestMethod]
        [ExpectedException(typeof(NoFunctionException))]
        public void NoFunctionSetTest()
        {
            var functionQueue = new FunctionQueue<int, int>();
            functionQueue.Start();
        }

        [TestMethod]
        [ExpectedException(typeof(NoActionException))]
        public void NoActionSetTest()
        {
            var functionQueue = new FunctionQueue<int, int>();
            functionQueue.SetFunction(value => value * 2);
            functionQueue.Start();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullActionSetTest()
        {
            var functionQueue = new FunctionQueue<int, int>();
            functionQueue.SetAction(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullCallbackSetTest()
        {
            var functionQueue = new FunctionQueue<int, int>();
            functionQueue.SetCallback(null);
        }
    }
}