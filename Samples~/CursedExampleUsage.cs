using UnityEngine;
using USignals;

public class CursedExampleUsage : MonoBehaviour
{
    private void Start()
    {
        Signal<int> signalA = null;
        Signal<int> signalB = new Signal<int>(() => signalA.Value + 1);

        signalA = new Signal<int>(() => signalB.Value + 1);

        // This will throw an Exception because of the circular reference
        Debug.Log(signalA.Value);
    }
}
