﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SMC.Utilities.Queues;
using System;
using System.Collections.Generic;

namespace ItemQueueLib.Tests
{
    [TestClass]
    public class PredicateQueueTests
    {
        // ReSharper disable once InconsistentNaming
        private const string QUEUE_NAME = "predicate_queue_name";

        [TestMethod]
        public void ConstructorWithQueueNameTest()
        {
            var result = 0;
            var predicateQueue = new PredicateQueue<int>(QUEUE_NAME);
            predicateQueue.SetPredicate(value => value % 2 == 0);
            predicateQueue.SetAction(value => result = value);
            predicateQueue.Start();
            predicateQueue.Enqueue(2);
            predicateQueue.Stop(true);

            Assert.IsTrue(result == 2);
            Assert.AreEqual(predicateQueue.Name, QUEUE_NAME, false);
        }

        [TestMethod]
        public void ConstructorWithPredicateTest()
        {
            var result = 0;
            var predicateQueue = new PredicateQueue<int>(value => value % 2 == 0);
            predicateQueue.SetAction(value => result = value);
            predicateQueue.Start();
            predicateQueue.Enqueue(2);
            predicateQueue.Stop(true);

            Assert.IsTrue(result == 2);
        }

        [TestMethod]
        public void ConstructorWithPredicateActionTest()
        {
            var result = 0;
            var predicateQueue = new PredicateQueue<int>(value => value %2==0, value => result = value);
            predicateQueue.Start();
            predicateQueue.Enqueue(2);
            predicateQueue.Stop(true);

            Assert.IsTrue(result == 2);
        }

        [TestMethod]
        public void ConstructorWithPredicateActionCallbackTest()
        {
            var result = 0;
            var callbackSum = 0;
            var predicateQueue = new PredicateQueue<int>(value => value % 2 == 0, value => result = value, value => callbackSum += value);
            predicateQueue.Start();
            predicateQueue.Enqueue(1);
            predicateQueue.Enqueue(2);
            predicateQueue.Stop(true);

            Assert.IsTrue(result == 2);
            Assert.IsTrue(callbackSum == 2);
        }

        [TestMethod]
        public void ConstructorWithPredicateQueueNameTest()
        {
            var result = 0;
            var predicateQueue = new PredicateQueue<int>(value => value % 2==0, QUEUE_NAME);
            predicateQueue.SetAction(value => result = value);
            predicateQueue.Start();
            predicateQueue.Enqueue(2);
            predicateQueue.Stop(true);

            Assert.IsTrue(result == 2);
            Assert.AreEqual(predicateQueue.Name, QUEUE_NAME, false);
        }

        [TestMethod]
        public void ConstructorWithAllParametersTest()
        {
            var result = 0;
            var callbackSum = 0;
            var predicateQueue = new PredicateQueue<int>(value => value % 2 == 0, value => result = value, value => callbackSum += value, QUEUE_NAME);
            predicateQueue.Start();
            predicateQueue.Enqueue(1);
            predicateQueue.Enqueue(2);
            predicateQueue.Stop(true);

            Assert.IsTrue(result == 2);
            Assert.IsTrue(callbackSum == 2);
            Assert.AreEqual(predicateQueue.Name, QUEUE_NAME, false);
        }


        [TestMethod]
        public void SumOfEvenTests()
        {
            var sum = 0;
            var predicateQueue = new PredicateQueue<int>();
            predicateQueue.SetAction(value => sum += value);
            predicateQueue.SetPredicate(value => value % 2 == 0);
            predicateQueue.Enqueue(0);
            predicateQueue.Enqueue(1);
            predicateQueue.Enqueue(2);
            predicateQueue.Enqueue(3);
            predicateQueue.Enqueue(4);
            predicateQueue.Enqueue(5);
            predicateQueue.Enqueue(6);
            predicateQueue.Enqueue(7);
            predicateQueue.Enqueue(8);
            predicateQueue.Enqueue(9);
            predicateQueue.Enqueue(10);

            predicateQueue.Start();
            predicateQueue.Stop(true);

            Assert.IsTrue(sum == (0 + 2 + 4 + 6 + 8 + 10));
        }

        [TestMethod]
        public void SumOfEvensListTest()
        {
            var sum = 0;
            var predicateQueue = new PredicateQueue<int>();
            predicateQueue.SetAction(value => sum += value);
            predicateQueue.SetPredicate(value => value % 2 == 0);
            predicateQueue.Enqueue(new List<int>() { 0,1,2,3,4,5,6,7,8,9,10 });
            predicateQueue.Start();
            predicateQueue.Stop(true);

            Assert.IsTrue(sum == (0 + 2 + 4 + 6 + 8 + 10));
        }

        [TestMethod]
        public void ZeroValueTest()
        {
            var sum = 0;
            var predicateQueue = new PredicateQueue<int>();
            predicateQueue.SetAction(value => sum += value);
            predicateQueue.SetPredicate(value => value % 2 == 0);
            predicateQueue.Enqueue(0);
            predicateQueue.Enqueue(1);

            predicateQueue.Start();
            predicateQueue.Stop(true);

            Assert.IsTrue(sum ==0);
        }

        [TestMethod]
        public void MultiplicationOddsTest()
        {
            var sum = 1;
            var predicateQueue = new PredicateQueue<int>();
            predicateQueue.SetAction(value => sum *= value);
            predicateQueue.SetPredicate(value => value % 2 != 0);
            predicateQueue.Enqueue(1);
            predicateQueue.Enqueue(2);
            predicateQueue.Enqueue(3);
            predicateQueue.Enqueue(4);
            predicateQueue.Enqueue(5);
            predicateQueue.Enqueue(6);
            predicateQueue.Enqueue(7);
            predicateQueue.Enqueue(8);
            predicateQueue.Enqueue(9);
            predicateQueue.Enqueue(10);

            predicateQueue.Start();
            predicateQueue.Stop(true);

            Assert.IsTrue(sum == (1*3*5*7*9));
        }

        [TestMethod]
        public void StringConcatTest()
        {
            var result = string.Empty;
            var predicateQueue = new PredicateQueue<string>();
            predicateQueue.SetAction(value => result = string.Concat(result, value));
            predicateQueue.SetPredicate(value=> value.Contains("Hello") || value.Contains("World"));
            predicateQueue.Enqueue("Hello");
            predicateQueue.Enqueue(", World!");

            predicateQueue.Start();
            predicateQueue.Stop(true);

            Assert.AreEqual(result, "Hello, World!", false);
        }

        [TestMethod]
        public void StringConcatListTest()
        {
            var result = string.Empty;
            var predicateQueue = new PredicateQueue<string>();
            predicateQueue.SetAction(value => result = string.Concat(result, value));
            predicateQueue.SetPredicate(value => value.Contains("Hello") || value.Contains("World"));
            predicateQueue.Enqueue(new List<string>() { "Hello", ", World!" });

            predicateQueue.Start();
            predicateQueue.Stop(true);

            Assert.AreEqual(result, "Hello, World!", false);
        }

        [TestMethod]
        public void CallbackTest()
        {
            var result = 0;
            var callbackSum = 0;
            var predicateQueue = new PredicateQueue<int>();
            predicateQueue.SetPredicate(value => value % 2 == 0);
            predicateQueue.SetAction(value => result = value);
            predicateQueue.SetCallback(value => callbackSum += value);
            predicateQueue.Start();
            predicateQueue.Enqueue(2);
            predicateQueue.Stop(true);

            Assert.IsTrue(result == 2);
            Assert.IsTrue(callbackSum == 2);
        }

        [TestMethod]
        [ExpectedException(typeof(NoActionException))]
        public void NoActionSetTest()
        {
            var predicateQueue = new PredicateQueue<int>();
            predicateQueue.SetPredicate(value=>true);
            predicateQueue.Start();
        }

        [TestMethod]
        [ExpectedException(typeof(NoPredicateException))]
        public void NoPredicateSetTest()
        {
            var predicateQueue = new PredicateQueue<int>();
            predicateQueue.Start();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullActionSetTest()
        {
            var predicateQueue = new PredicateQueue<int>();
            predicateQueue.SetAction(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullCallbackSetTest()
        {
            var predicateQueue = new PredicateQueue<int>();
            predicateQueue.SetCallback(null);
        }
    }
}