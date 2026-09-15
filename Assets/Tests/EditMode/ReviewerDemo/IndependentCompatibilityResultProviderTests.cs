using System;
using NUnit.Framework;
using SpatialGrid.WarehouseGeometry;

namespace SpatialGrid.ReviewerDemo.Tests
{
    public class IndependentCompatibilityResultProviderTests
    {
        static ClearanceResult FrozenClearance() => new ClearanceResult
        {
            requiredMeters = IndependentCompatibilityResultProvider.RequiredMeters,
            availableMeters = IndependentCompatibilityResultProvider.FrozenAvailableMeters,
            differenceMeters = IndependentCompatibilityResultProvider.FrozenDifferenceMeters,
            status = ClearanceStatus.Pass,
            leftEntityId = IndependentCompatibilityResultProvider.BoundaryAFullName,
            rightEntityId = IndependentCompatibilityResultProvider.BoundaryBFullName,
        };

        [Test]
        public void Bind_ReturnsFrozenGeo003Fields_WithoutInventingValues()
        {
            var provider = new IndependentCompatibilityResultProvider();
            var result = provider.Bind(FrozenClearance());

            Assert.IsTrue(provider.HasResult);
            Assert.AreEqual("OTTO 1500", result.robotModel);
            Assert.AreEqual(1.915f, result.requiredValue, 1e-6f);
            Assert.AreEqual(7.509663f, result.measuredValue, 1e-6f);
            Assert.AreEqual(5.594663f, result.signedDifference, 1e-6f);
            Assert.AreEqual(ClearanceStatus.Pass, result.status);
            Assert.AreEqual("OTTO 1500 Spec Sheet", result.machineSourceDocument);
            Assert.AreEqual(1, result.machineSourcePage);
            Assert.AreEqual("OTTO 1500 Spec Sheet · p.1", IndependentCompatibilityResultProvider.SourceDisplayLine);
            Assert.AreEqual(IndependentCompatibilityResultProvider.BoundaryAFullName, result.boundaryA);
            Assert.AreEqual(IndependentCompatibilityResultProvider.BoundaryBFullName, result.boundaryB);
            Assert.AreEqual("ShelfF_01_001", IndependentCompatibilityResultProvider.ShortBoundaryDisplay(result.boundaryA));
            Assert.AreEqual("ShelfD_01_001", IndependentCompatibilityResultProvider.ShortBoundaryDisplay(result.boundaryB));
            Assert.IsTrue(IndependentCompatibilityResultProvider.MatchesFrozenIndependentMeasurement(result));
        }

        [Test]
        public void Bind_RejectsAlteredRequiredThreshold()
        {
            var provider = new IndependentCompatibilityResultProvider();
            var altered = FrozenClearance();
            altered.requiredMeters = 2.0f;
            Assert.Throws<InvalidOperationException>(() => provider.Bind(altered));
            Assert.IsFalse(provider.HasResult);
        }

        [Test]
        public void Bind_RejectsNonFrozenBoundaries()
        {
            var provider = new IndependentCompatibilityResultProvider();
            var altered = FrozenClearance();
            altered.leftEntityId = "some_other_shelf";
            Assert.Throws<InvalidOperationException>(() => provider.Bind(altered));
        }

        [Test]
        public void Clear_DropsCurrentResult_AndAllowsRebind()
        {
            var provider = new IndependentCompatibilityResultProvider();
            provider.Bind(FrozenClearance());
            Assert.IsTrue(provider.HasResult);
            provider.Clear();
            Assert.IsFalse(provider.HasResult);
            var again = provider.Bind(FrozenClearance());
            Assert.IsTrue(IndependentCompatibilityResultProvider.MatchesFrozenIndependentMeasurement(again));
        }
    }
}
