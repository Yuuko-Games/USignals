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
        var signal = new Signal<int>(() => 42, new Signal<bool>(true));
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
    public void Signal_TriggersOnUpdatedDistinctOnlyOnChange()
    {
        var signal = new Signal<int>(1);
        bool eventTriggered = false;

        signal.OnUpdatedDistinct += () => eventTriggered = true;
        signal.Value = 1; // No change, event should not trigger
        Assert.IsFalse(eventTriggered);

        signal.Value = 2; // Value changes, event should trigger
        Assert.IsTrue(eventTriggered);
    }

    [Test]
    public void ComputedSignal_RecomputesOnDependencyChange()
    {
        var baseSignal = new Signal<int>(2);
        var computedSignal = new Signal<int>(() => baseSignal.Value * 2, baseSignal);

        Assert.AreEqual(4, computedSignal.Value);

        baseSignal.Value = 3;
        Assert.AreEqual(6, computedSignal.Value);
    }

    [Test]
    public void ComputedSignal_ThrowsOnNoDependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new Signal<int>(() => 10));
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
        var computedSignal = new Signal<int>(() => signal.Value + 1, signal);

        computedSignal.Dispose();

        Assert.DoesNotThrow(() => signal.Value = 2);
        Assert.AreEqual(2, signal.Value);
    }

    [Test]
    public void ComputedSignal_OnUpdatedDistinct_TriggersCorrectly()
    {
        var signal = new Signal<int>(2);
        var computedSignal = new Signal<int>(() => signal.Value % 2, signal);

        bool signalDistinctTriggered = false;
        bool computedDistinctTriggered = false;

        signal.OnUpdatedDistinct += () => signalDistinctTriggered = true;
        computedSignal.OnUpdatedDistinct += () => computedDistinctTriggered = true;

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
        var computedSignal = new Signal<int>(() => baseSignal.Value * 2, baseSignal);

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
        var signalC = new Signal<int>(() => signalA.Value + signalB.Value, signalA, signalB);
        var signalD = new Signal<int>(() => signalC.Value * 2, signalC);

        Assert.AreEqual(15, signalC.Value);
        Assert.AreEqual(30, signalD.Value);

        signalA.Value = 15; // Should update signalC and signalD

        Assert.AreEqual(15, signalA.Value);
        Assert.AreEqual(10, signalB.Value);
        Assert.AreEqual(25, signalC.Value);
        Assert.AreEqual(50, signalD.Value);
    }
}
