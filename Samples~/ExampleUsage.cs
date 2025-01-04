using UnityEngine;
using USignals;

public class ExampleUsage : MonoBehaviour
{
    private void Start()
    {
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
    }
}
