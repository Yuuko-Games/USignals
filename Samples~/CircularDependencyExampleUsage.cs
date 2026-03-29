using System;
using UnityEngine;
using USignals;

public class CircularDependencyExampleUsage : MonoBehaviour
{
    private void Start()
    {
        var seed = new Signal<int>(0);
        var signalB = new Signal<int>(() => seed.Value + 1);
        var signalA = new Signal<int>(() => signalB.Value + 1);

        try
        {
            signalB.UpdateCompute(() => signalA.Value + 1);
        }
        catch (InvalidOperationException exception)
        {
            Debug.LogException(exception);
        }
    }
}
