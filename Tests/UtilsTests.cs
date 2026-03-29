using System;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using USignals;

[TestFixture]
public class UtilsTests
{
    [Test]
    public void SignalAsset_TriggersEventsCorrectly()
    {
        var asset = ScriptableObject.CreateInstance<IntSignalAsset>();

        int changedCount = 0;
        int updatedCount = 0;

        asset.OnChanged += () => changedCount++;
        asset.OnUpdated += () => updatedCount++;

        asset.Value = 1;
        asset.Value = 1;
        asset.Value = 2;

        Assert.AreEqual(2, changedCount);
        Assert.AreEqual(3, updatedCount);

        UnityEngine.Object.DestroyImmediate(asset);
    }

    [Test]
    public void TMPTextSignalBinder_UpdatesTextFromSignal()
    {
        var asset = ScriptableObject.CreateInstance<IntSignalAsset>();
        asset.Value = 7;

        var gameObject = new GameObject("TMPTextSignalBinder_Test");
        var text = gameObject.AddComponent<TextMeshProUGUI>();
        var binder = gameObject.AddComponent<TMPTextSignalBinder>();

        SetPrivateField(binder, "_format", "Value: {0}");
        SetPrivateField(binder, "_useChangedEvent", true);
        InvokePrivateMethod(binder, "Awake");
        binder.SetSource(asset);
        InvokePrivateMethod(binder, "OnEnable");

        Assert.AreEqual("Value: 7", text.text);

        asset.Value = 9;
        Assert.AreEqual("Value: 9", text.text);

        UnityEngine.Object.DestroyImmediate(gameObject);
        UnityEngine.Object.DestroyImmediate(asset);
    }

    [Test]
    public void ActiveFromBoolSignal_TogglesGameObject()
    {
        var asset = ScriptableObject.CreateInstance<BoolSignalAsset>();
        asset.Value = false;

        var gameObject = new GameObject("ActiveFromBoolSignal_Test");
        var binder = gameObject.AddComponent<ActiveFromBoolSignal>();

        SetPrivateField(binder, "_invert", false);
        SetPrivateField(binder, "_useChangedEvent", true);
        binder.SetSource(asset);
        InvokePrivateMethod(binder, "OnEnable");

        Assert.IsFalse(gameObject.activeSelf);

        asset.Value = true;
        Assert.IsTrue(gameObject.activeSelf);

        UnityEngine.Object.DestroyImmediate(gameObject);
        UnityEngine.Object.DestroyImmediate(asset);
    }

    [Test]
    public void ActiveFromBoolSignal_InvertsValueWhenConfigured()
    {
        var asset = ScriptableObject.CreateInstance<BoolSignalAsset>();
        asset.Value = true;

        var gameObject = new GameObject("ActiveFromBoolSignal_Invert_Test");
        var binder = gameObject.AddComponent<ActiveFromBoolSignal>();

        SetPrivateField(binder, "_invert", true);
        SetPrivateField(binder, "_useChangedEvent", true);
        binder.SetSource(asset);
        InvokePrivateMethod(binder, "OnEnable");

        Assert.IsFalse(gameObject.activeSelf);

        asset.Value = false;
        Assert.IsTrue(gameObject.activeSelf);

        UnityEngine.Object.DestroyImmediate(gameObject);
        UnityEngine.Object.DestroyImmediate(asset);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
        {
            throw new InvalidOperationException($"Field '{fieldName}' not found on {target.GetType().Name}.");
        }

        field.SetValue(target, value);
    }

    private static void InvokePrivateMethod(object target, string methodName)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (method == null)
        {
            throw new InvalidOperationException($"Method '{methodName}' not found on {target.GetType().Name}.");
        }

        method.Invoke(target, null);
    }
}
