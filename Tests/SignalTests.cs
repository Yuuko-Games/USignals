using System;
using NUnit.Framework;
using USignals;

[TestFixture]
public class SignalTests
{
    [Test]
    public void Signal_StoresInitialValue()
    {
        var signal = new Signal<int>(10);
        Assert.AreEqual(10, signal.Value);
    }

    [Test]
    public void Signal_UpdatesValue()
    {
        var signal = new Signal<int>(5);
        Assert.AreEqual(5, signal.Value);

        signal.Value = 20;
        Assert.AreEqual(20, signal.Value);
    }

    [Test]
    public void Signal_ThrowsOnComputedValueSet()
    {
        var dependency = new Signal<bool>(true);
        var signal = new Signal<int>(() => dependency.Value ? 42 : 0);
        Assert.Throws<InvalidOperationException>(() => signal.Value = 10);
    }

    [Test]
    public void Signal_TriggersOnUpdatedEvent()
    {
        var signal = new Signal<int>(1);
        bool eventTriggered = false;

        signal.OnUpdated += () => eventTriggered = true;
        signal.Value = 2;

        Assert.IsTrue(eventTriggered);
    }

    [Test]
    public void Signal_TriggersOnChangedOnlyOnChange()
    {
        var signal = new Signal<int>(1);
        bool eventTriggered = false;

        signal.OnChanged += () => eventTriggered = true;
        signal.Value = 1; // No change, event should not trigger
        Assert.IsFalse(eventTriggered);

        signal.Value = 2; // Value changes, event should trigger
        Assert.IsTrue(eventTriggered);
    }

    [Test]
    public void ComputedSignal_RecomputesOnDependencyChange()
    {
        var baseSignal = new Signal<int>(2);
        var computedSignal = new Signal<int>(() => baseSignal.Value * 2);

        Assert.AreEqual(4, computedSignal.Value);

        baseSignal.Value = 3;
        Assert.AreEqual(6, computedSignal.Value);
    }

    [Test]
    public void ComputedSignal_ThrowsOnNoImplicitDependencies()
    {
        Assert.Throws<InvalidOperationException>(() => new Signal<int>(() => 10));
    }

    [Test]
    public void Signal_Refresh_TriggersOnUpdated()
    {
        var signal = new Signal<int>(5);
        bool eventTriggered = false;

        signal.OnUpdated += () => eventTriggered = true;
        signal.Refresh();

        Assert.IsTrue(eventTriggered);
    }

    [Test]
    public void Signal_Dispose_CleansUp()
    {
        var signal = new Signal<int>(1);
        var computedSignal = new Signal<int>(() => signal.Value + 1);

        computedSignal.Dispose();

        Assert.DoesNotThrow(() => signal.Value = 2);
        Assert.AreEqual(2, signal.Value);
    }

    [Test]
    public void ComputedSignal_OnChanged_TriggersCorrectly()
    {
        var signal = new Signal<int>(2);
        var computedSignal = new Signal<int>(() => signal.Value % 2);

        bool signalDistinctTriggered = false;
        bool computedDistinctTriggered = false;

        signal.OnChanged += () => signalDistinctTriggered = true;
        computedSignal.OnChanged += () => computedDistinctTriggered = true;

        signal.Value = 4; // Same computed value, should not trigger computedDistinct
        Assert.IsTrue(signalDistinctTriggered);
        Assert.IsFalse(computedDistinctTriggered);

        signalDistinctTriggered = false;
        computedDistinctTriggered = false;

        signal.Value = 3; // Different computed value, should trigger both
        Assert.IsTrue(signalDistinctTriggered);
        Assert.IsTrue(computedDistinctTriggered);
    }

    [Test]
    public void ComputedSignal_OnUpdated_TriggersWhenDependencyChanges()
    {
        var baseSignal = new Signal<int>(2);
        var computedSignal = new Signal<int>(() => baseSignal.Value * 2);

        bool computedUpdatedTriggered = false;
        computedSignal.OnUpdated += () => computedUpdatedTriggered = true;

        baseSignal.Value = 3; // Should trigger OnUpdated on computedSignal

        Assert.IsTrue(computedUpdatedTriggered);
    }


    [Test]
    public void ComputedSignal_ChainedUpdatesWorkCorrectly()
    {
        var signalA = new Signal<int>(5);
        var signalB = new Signal<int>(10);
        var signalC = new Signal<int>(() => signalA.Value + signalB.Value);
        var signalD = new Signal<int>(() => signalC.Value * 2);

        Assert.AreEqual(15, signalC.Value);
        Assert.AreEqual(30, signalD.Value);

        signalA.Value = 15; // Should update signalC and signalD

        Assert.AreEqual(15, signalA.Value);
        Assert.AreEqual(10, signalB.Value);
        Assert.AreEqual(25, signalC.Value);
        Assert.AreEqual(50, signalD.Value);
    }

    [Test]
    public void ComputedSignal_UpdateCompute_RecomputesAndUpdatesChildren()
    {
        var baseSignal = new Signal<int>(2);
        var computedSignal = new Signal<int>(() => baseSignal.Value * 2);
        var childSignal = new Signal<int>(() => computedSignal.Value + 1);

        Assert.AreEqual(4, computedSignal.Value);
        Assert.AreEqual(5, childSignal.Value);

        computedSignal.UpdateCompute(() => baseSignal.Value * 3);

        Assert.AreEqual(6, computedSignal.Value);
        Assert.AreEqual(7, childSignal.Value);
    }

    [Test]
    public void Signal_UpdateCompute_ThrowsOnValueSignal()
    {
        var signal = new Signal<int>(3);

        Assert.Throws<InvalidOperationException>(() => signal.UpdateCompute(() => 10));
    }

    [Test]
    public void ComputedSignal_TracksDependenciesAndRecomputes()
    {
        var a = new Signal<int>(1);
        var b = new Signal<int>(2);
        var c = new Signal<int>(3);
        var computed = new Signal<int>(() => a.Value + b.Value * c.Value);

        Assert.AreEqual(7, computed.Value);

        a.Value = 5;
        Assert.AreEqual(11, computed.Value);

        b.Value = 4;
        Assert.AreEqual(17, computed.Value);

        c.Value = 10;
        Assert.AreEqual(45, computed.Value);
    }

    [Test]
    public void ComputedSignal_SwitchesTrackedDependenciesAcrossRecompute()
    {
        var selector = new Signal<bool>(true);
        var left = new Signal<int>(1);
        var right = new Signal<int>(10);
        var computed = new Signal<int>(() => selector.Value ? left.Value : right.Value);

        Assert.AreEqual(1, computed.Value);

        right.Value = 11;
        Assert.AreEqual(1, computed.Value);

        selector.Value = false;
        Assert.AreEqual(11, computed.Value);

        left.Value = 2;
        Assert.AreEqual(11, computed.Value);

        right.Value = 12;
        Assert.AreEqual(12, computed.Value);
    }

    [Test]
    public void ComputedSignal_TracksDependenciesAcrossComplexGraph()
    {
        var a = new Signal<int>(1);
        var b = new Signal<int>(2);
        var c = new Signal<int>(3);
        var d = new Signal<int>(4);
        var e = new Signal<int>(5);

        var sum1 = new Signal<int>(() => a.Value + b.Value + c.Value);
        var sum2 = new Signal<int>(() => d.Value + e.Value);
        var mix = new Signal<int>(() => sum1.Value * sum2.Value);
        var alt = new Signal<int>(() => mix.Value - a.Value + d.Value);
        var root = new Signal<int>(() => alt.Value + mix.Value + sum2.Value);

        Assert.AreEqual(120, root.Value);

        a.Value = 2;
        Assert.AreEqual(137, root.Value);

        d.Value = 6;
        Assert.AreEqual(169, root.Value);

        e.Value = 7;
        Assert.AreEqual(199, root.Value);

        b.Value = 10;
        Assert.AreEqual(407, root.Value);
    }
}
