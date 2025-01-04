# USignals

This package provides a simple and easy to use signal system for Unity.

## Installation

To install this package, you can copy this url to the package manager:

```txt
https://github.com/Yuuko-Games/USignals.git
```

## Usage

To use this package, you need to know the two types of signals that are available:

- `Signal<T>(value)`: A signal that holds a value of type `T`.
- `Signal<T>(func, signals)`: A signal that holds a value of type `T` that is calculated by a function that depends on other signals.

## Basic example code

```csharp
var signalA = new Signal<int>(5);
var signalB = new Signal<int>(10);

Debug.Log($"Initial signalA value: {signalA.Value}");
Debug.Log($"Initial signalB value: {signalB.Value}");

var signalC = new Signal<int>(() => signalA.Value + signalB.Value, signalA, signalB);
var signalD = new Signal<int>(() => signalC.Value * 2, signalC);

Debug.Log($"Initial signalC value: {signalB.Value}"); // 15
Debug.Log($"Initial signalD value: {signalD.Value}"); // 30

signalA.Value = 15; // signalA, signalC and signalD will be updated

Debug.Log($"Current signalA value: {signalA.Value}"); // 15
Debug.Log($"Current signalB value: {signalB.Value}"); // 10 -- didn't change
Debug.Log($"Current signalC value: {signalC.Value}"); // 25
Debug.Log($"Current signalD value: {signalD.Value}"); // 50
```

## Displaying the signals values in the console

```csharp
var signal = new Signal<int>(5);

Debug.Log($"Displays the signal's value (int -> string): {signal.Value}");
Debug.Log($"Displays the signal's value (string): {signal}");
```

## Detecting loops

If you create a loop in the signals, the package will throw an exception. For example:

```csharp
Signal<int> signalA = null;
Signal<int> signalB = new Signal<int>(() => signalA.Value + 1);

signalA = new Signal<int>(() => signalB.Value + 1);

Debug.Log(signalA.Value); // This will throw an Exception because of the circular reference
```

> [!WARNING]  
> For this exact reason, it is REALLY RECOMMENDED to build readonly variables for the signals when working on MonoBehaviour scripts, and only update them values from the given API.

## Best practices

Basically, you should never do this:

- DON'T initialize the signals as `null`
- DON'T assign the signals to a new Signal value after the initialization

You should always do this:

- DO initialize the signals with a value or a function
- DO use the signals as readonly variables when working on MonoBehaviour scripts
- DO update the signals values from the given API (like `signal.Value = newValue`)
- You can check when a value changes by using the `OnChanged` event with:

```csharp
signal.OnChanged += () => {
    Debug.Log($"Signal value changed to: {signal.Value}");
};
```

## License

This package is licensed under the [MIT License](LICENSE).
