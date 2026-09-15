using NUnit.Framework;
using SpatialGrid.WarehouseGeometry;
using UnityEngine;

public class AisleClearanceCheckerTests
{
    static Bounds Rack(float z) => new(Vector3.forward * z + Vector3.up * 1.15f, new Vector3(1.6f,2.3f,.6f));

    [Test]
    public void ControlledFixture_IsExactlyBlockedByPointNineSevenFiveMeters()
    {
        var result = AisleClearanceChecker.Check(Rack(-.77f), Rack(.77f), ClearanceAxis.Z, 1.915f);
        Assert.That(result.availableMeters, Is.EqualTo(.940f).Within(.0001f));
        Assert.That(result.differenceMeters, Is.EqualTo(-.975f).Within(.0001f));
        Assert.That(result.status, Is.EqualTo(ClearanceStatus.Blocked));
    }

    [Test] public void PublicFixture_IsBlockedAtOnePointSevenFiveMeters() => Assert.That(AisleClearanceChecker.Check(Rack(-1.175f),Rack(1.175f),ClearanceAxis.Z,1.915f).status, Is.EqualTo(ClearanceStatus.Blocked));
    [Test] public void Equality_IsReview() => Assert.That(AisleClearanceChecker.Check(Rack(-1.2575f),Rack(1.2575f),ClearanceAxis.Z,1.915f).status, Is.EqualTo(ClearanceStatus.Review));
    [Test] public void WithinTolerance_IsReview() => Assert.That(AisleClearanceChecker.Check(Rack(-1.2595f),Rack(1.2595f),ClearanceAxis.Z,1.915f).status, Is.EqualTo(ClearanceStatus.Review));
    [Test] public void ClearlyBlocked_IsBlocked() => Assert.That(AisleClearanceChecker.Check(Rack(-.8f),Rack(.8f),ClearanceAxis.Z,1.915f).status, Is.EqualTo(ClearanceStatus.Blocked));
    [Test] public void ClearlyPasses_IsPass() => Assert.That(AisleClearanceChecker.Check(Rack(-1.4f),Rack(1.4f),ClearanceAxis.Z,1.915f).status, Is.EqualTo(ClearanceStatus.Pass));
    [Test] public void InvalidRequirement_IsRejected() => Assert.Throws<System.ArgumentOutOfRangeException>(() => AisleClearanceChecker.Check(Rack(-1),Rack(1),ClearanceAxis.Z,0));
}
