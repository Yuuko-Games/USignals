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
- `Signal<T>(func)`: A signal that holds a value of type `T` that is calculated by a function that depends on other signals.

## Basic example code

```csharp
var signalA = new Signal<int>(5);
var signalB = new Signal<int>(10);

Debug.Log($"Initial signalA value: {signalA.Value}");
Debug.Log($"Initial signalB value: {signalB.Value}");

var signalC = new Signal<int>(() => signalA.Value + signalB.Value);
var signalD = new Signal<int>(() => signalC.Value * 2);

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

## Working with MonoBehaviour scripts

```csharp
private void Awake()
{
    // Use to initialize the signals
}

private void Start()
{
    // Use it to subscribe to OnUpdated/OnChanged events, to ensure that the signals are ready
}
```

## Real project usage examples

These are practical patterns you can copy into real Unity projects.

### UI state (health + death banner)

```csharp
public class PlayerHealth : MonoBehaviour
{
    public readonly Signal<int> health = new(100);
    public Signal<bool> isDead;

    private void Awake()
    {
        isDead = new Signal<bool>(() => health.Value <= 0);
    }

    private void Start()
    {
        health.OnChanged += () => healthText.text = $"{health.Value}";
        isDead.OnChanged += () => deadBanner.SetActive(isDead.Value);
    }
}
```

### Inventory count + capacity warning

```csharp
public class Inventory : MonoBehaviour
{
    public readonly Signal<int> itemCount = new(0);
    public readonly Signal<int> capacity = new(20);
    public Signal<bool> isFull;

    private void Awake()
    {
        isFull = new Signal<bool>(() => itemCount.Value >= capacity.Value);
    }
}
```

### Settings menu: master volume affects SFX/music

```csharp
public class AudioSettingsModel : MonoBehaviour
{
    public readonly Signal<float> master = new(1f);
    public readonly Signal<float> sfx = new(0.8f);
    public readonly Signal<float> music = new(0.6f);

    public Signal<float> sfxFinal;
    public Signal<float> musicFinal;

    private void Awake()
    {
        sfxFinal = new Signal<float>(() => master.Value * sfx.Value);
        musicFinal = new Signal<float>(() => master.Value * music.Value);
    }
}
```

### Quest progression and UI

```csharp
public class QuestState : MonoBehaviour
{
    public readonly Signal<int> completedSteps = new(0);
    public readonly Signal<int> totalSteps = new(5);
    public Signal<float> progress;

    private void Awake()
    {
        progress = new Signal<float>(() =>
            totalSteps.Value == 0 ? 0f : (float)completedSteps.Value / totalSteps.Value);
    }
}
```

### Combat: damage output from stats + buffs

```csharp
public class CombatStats : MonoBehaviour
{
    public readonly Signal<int> baseDamage = new(10);
    public readonly Signal<float> critMultiplier = new(1.5f);
    public readonly Signal<bool> isCrit = new(false);
    public Signal<float> currentDamage;

    private void Awake()
    {
        currentDamage = new Signal<float>(() =>
            baseDamage.Value * (isCrit.Value ? critMultiplier.Value : 1f));
    }
}
```

### Example

This is an example of how to use the signals in a MonoBehaviour script:

Suppose you have a `PlayerHealth` script that holds the player's health and if it's dead or not.

```csharp
using UnityEngine;
using USignals;

public class PlayerHealth : MonoBehaviour
{
    // Initialize the value signal in the declaration
    public readonly Signal<int> health = new(5);

    // Initialize the function signal in the Awake method
    public Signal<bool> isDead;

    private void Awake()
    {
        // Use to initialize the signals
        isDead = new Signal<bool>(() => health.Value <= 0);
    }

    // Imagine this is a damage event
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isDead.Value)
        {
            health.Value--;
        }
    }
}
```

In the `HealthDisplay` script, you can display the player's health and if the player is dead.

```csharp
using TMPro;
using UnityEngine;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text deadText;

    private void Start()
    {
        playerHealth.health.OnChanged += () => healthText.text = $"{playerHealth.health}";
        playerHealth.isDead.OnChanged += () => deadText.text = playerHealth.isDead.Value ? "Dead" : "Alive";
    }
}
```

## Best practices

Basically, you should never do this:

- DON'T `reassign` the signals to a new Signal value after the initialization.

> [!WARNING]  
> To avoid circular dependencies, it is REALLY RECOMMENDED to build readonly variables for the signals when working on MonoBehaviour scripts, and only update its values from the given API.

You should do this:

- DO initialize the signals with a `value` or a `function` that depends on other signals.
- DO update the signals values from the given API like:

```csharp
signal.Value = newValue;
```

- You can check when a value changes by using the `OnChanged` event with:

```csharp
signal.OnChanged += () => {
    Debug.Log($"Signal value changed to: {signal.Value}");
};
```

- On MonoBehaviour scripts
  - DO use the signals as `readonly` variables when working on MonoBehaviour scripts.
  - DO initizalize the signals in the `Awake` method.
  - DO subscribe to the `OnUpdated` or `OnChanged` event in the `Start` method.

## License

This package is licensed under the [MIT License](LICENSE).
