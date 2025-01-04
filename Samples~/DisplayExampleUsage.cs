using UnityEngine;
using USignals;

public class DisplayExampleUsage : MonoBehaviour
{
    private void Start()
    {
        var signal = new Signal<int>(5);

        Debug.Log($"Displays the signal's value (int -> string): {signal.Value}");
        Debug.Log($"Displays the signal's value (string): {signal}");
    }
}
