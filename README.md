# Item Queue
A simple item queue for executing actions and/or delegates with enqueued data on a background thread.

## Features
- Provides three different queue types: ActionQueue, FunctionQueue, and PredicateQueue (explained in the [Usage](#usage) section)
- Runs on a background thread
- Uses concurrent queues for thread safety
- Uses thread synchronization events to prevent having to poll the queue
- Provides events and callbacks for keeping the caller informed of the queue's progress
- Implements IDisposable
- All public methods are full documented
- Unit tests included for all three queues which demonstrates additional features and usage patterns.

## Usage
This library provides three queues depending on the application's specific needs.  These queues are:
- ActionQueue<T>
- FunctionQueue<T>
- PredicateQueue<T>

Each of these will be discussed in the following sections.

### ActionQueue<T>
This queue is used to execute the specified action on each member that is added to the queue.  The following example sums all the items in the queue:
```c#
var sum = 0;
var actionQueue = new ActionQueue<int>();
actionQueue.SetAction(value => sum += value);
actionQueue.Start();
actionQueue.Enqueue(1);
actionQueue.Enqueue(2);
actionQueue.Stop(true);
```
In both of these example, the value of ```sum``` will be ```3``` after the queue finishes processing the items.

### FunctionQueue<T>
The function queue can be used to apply a user-defined function to the items in the queue.  The specified action will then be called with the result of the function call.
In the following example, the specified function will double the value of the item in the queue.  The action will then be called to retrieve this value:
```c#
var result = 0;
var functionQueue = new FunctionQueue<int, int>();
functionQueue.SetFunction(value => value * 2);
functionQueue.SetAction(value => result = value);
functionQueue.Start();
functionQueue.Enqueue(1);
functionQueue.Stop(true);
```
In the above example, the value of ```result``` will be ```2``` since the specified function doubles the value in the queue (which only a single value of ```1``` was enqueued) and
the action just sets ```result``` to the value passed to it from the result of the function call.

### PredicateQueue<T>
The predicate queue is a specialized function-type queue.  For the predicate queue, you specify a function (i.e. a predicate) which returns a ```bool```.  If the result of the predicate
is ```true``` then the value that was enqueued is passed to the specified action:
```c#
var sum = 0;
var predicateQueue = new PredicateQueue<int>();
predicateQueue.SetAction(value => sum += value);
predicateQueue.SetPredicate(value => value % 2 == 0);
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
```
In the above example, the value of ```sum``` will be ```30```.  This is because the specified predicate only returns ```true``` for even numbers.  Therefore the specified action will only perform a sum
of the even numbers in the queue (i.e. 2 + 4 + 6 + 8 + 10 == 30).  If the odd numbers needed to be summed, the predicate could just be changed to ```predicateQueue.SetPredicate(value => value % 2 != 0);```
with everything else remaining the same.

### Callbacks
All queues also support callbacks when the specified action completed without throwing any exceptions.  The callbacks for the ```ActionQueue<T>``` and ```PredicateQueue<T>``` queues work exactly the same way.
The following is an example of a callback being uses just to sum the results of the queue with a separate variable:
```c#
var sum = 0;
var callbackSum = 0;
var actionQueue = new ActionQueue<int>();
actionQueue.SetAction(value => sum += value);
actionQueue.SetCallback(value => callbackSum += value);
actionQueue.Start();
actionQueue.Enqueue(1);
actionQueue.Enqueue(2);
actionQueue.Stop(true);
```
In above example, both the value of ```sum``` and ```callbackSum``` will be ```3```.

However, the callback of the ```FunctionQueue<T>``` as the signature of ```Action<T, TResult>```.  This is because it includes the original value that was enqueued as well as the result of the specified function 
(which is passed to the specified action).  Therefore, it is possible to see the before and after values for any necessary purposes (e.g. debugging).
```c#
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
```
In the above example, the values of the variables is as follows:
- ```result``` is ```2``` because the function doubles the enqueued value of ```1``` and passes it to the specified action
- ```callbackInitialValue``` is ```1``` because this was the enqueued value that was passed to the specified function
- ```callbackResultValue``` is ```2``` as this was the result of the function

### Stopping
To stop a queue, the queue's ```Stop()``` method needs to be called.  This method takes one argument named ```processRemainingItems```.  If the value of ```processRemainingItems``` is ```true```, the queue will finish
processing any items remaining the queue.  However, if the value is ```false``` any items still in the queue will be discarded before completing its shutdown.

### Events
The queues can raise three different events:
- ```StatusChanged```: This event is raised when the status of the queue has changed (e.g. from waiting for new data to processing new items)
- ```ActionCompleted```: This event is raised when the specified action has been completed on a specified action and contains the data that was passed to the action, whether the action completed successfully, and a 
possible message for more information.  This event can be used in place of callbacks in certain use-cases.
- ```OnErrorOccurred```: This event is raised when an exception occurs during the processing of times.  When possible, it will contain the specific items that caused an exception, a message, and the ```Exception``` 
that was thrown.

### IDisposable
All three queues manage their data using threads in order to not block the main UI thread.  In order to prevent any threads from handing around longer than needed, all the queues implement the ```IDisposable```
interface, which makes sure that the threads and queues are cleaned up when they are no longer needed.  Therefore, where possible, it is best to use a ```using``` block:
```c#
var sum = 0;
using(var actionQueue = new ActionQueue<int>())
{
	actionQueue.SetAction(value => sum += value);
	actionQueue.Start();
	actionQueue.Enqueue(1);
	actionQueue.Enqueue(2);
	actionQueue.Stop(true);
}
```
At the end of the ```using```, the threads will be terminated and the queues cleaned up.  In use-cases where a ```using``` block is not appropriate, the ```Stop()``` method should be called when processing can be
halted and the ```Dispose()``` method should be called when the queue should be deconstructed.

## Advanced Features
All the queues support some advanced features in regards to the threading aspect of the library, specifically:
- Queue Name: A queue name can be specified using one of the overloaded constructors.  This name cannot be changed once set and is used as the queue's thread's name.  If name of the queue that was set can be retrieved via the read-only ```Name``` property.
- Thread Properties:
  - Background: The ```Start()``` method has overloads, some of which accept an ```isBackground``` parameter.  This specifies whether or not the queue's thread is a background thread or not.  When not specified, it is set to ```true``` by default.
  - Priority: Some of the ```Start()``` method overloads also accept a ```ThreadPriority``` enum which specifies the queue's thread priority.  When not specified, the thread priority is set to normal (i.e. ```ThreadPriority.Normal```)

## Contributing
Contributing to this project is welcome.  However, we ask that you please follow our [contributing guidelines](./CONTRIBUTING.md) to help ensure consistency.

## Versions
**0.1.0** - Initial Release
  - *Please note that this is a **PREVIEW** version and there is no guarantee that any method signatures or functionality will remaing the same*

This project uses [SemVer](http://semver.org) for versioning.

## Authors
- Dominick Marciano Jr.

## License
Copyright (c) 2019 Dominick Marciano Jr., Sci-Med Coding.  All rights reserved

See [LICENSE](./LICENSE) for full licesning terms.