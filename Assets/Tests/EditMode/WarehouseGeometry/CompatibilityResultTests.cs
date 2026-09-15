using NUnit.Framework;
using SpatialGrid.WarehouseGeometry;

namespace SpatialGrid.WarehouseGeometry.Tests
{
    public class CompatibilityResultTests
    {
        static ClearanceResult SampleClearance() => new ClearanceResult
        {
            requiredMeters = 1.915f,
            availableMeters = 7.509663f,
            differenceMeters = 5.594663f,
            status = ClearanceStatus.Pass,
            leftEntityId = "aws_robomaker_warehouse_ShelfF_01_001",
            rightEntityId = "aws_robomaker_warehouse_ShelfD_01_001",
        };

        [Test]
        public void FromClearance_PopulatesAllMinimumContractFields()
        {
            var result = CompatibilityResult.FromClearance(
                SampleClearance(), "geo003-20260910-144434",
                "Geometry preflight — one-way aisle-width check", "OTTO-1500-MIN-AISLE-WIDTH-ONE-WAY",
                "OTTO 1500 Spec Sheet", 1, "Min. Aisle Width 1915 mm (78 in) (One Way)",
                "AWS RoboMaker Small Warehouse World", "3c23a698bf0b4e366ddf8b084af507c519bd3483",
                "09a9a7d289fed16d710326a7676b3cabb0d71c4bbb00f3f62fbf2c1f27436da6",
                "AisleClearanceChecker.Check, axis X, Unity Play Mode glTFast import");

            Assert.AreEqual("1.0", result.schemaVersion);
            Assert.AreEqual("OTTO Motors by Rockwell Automation", result.robotManufacturer);
            Assert.AreEqual("OTTO 1500", result.robotModel);
            Assert.AreEqual(1.915f, result.requiredValue, 1e-6f);
            Assert.AreEqual(7.509663f, result.measuredValue, 1e-6f);
            Assert.AreEqual(5.594663f, result.signedDifference, 1e-6f);
            Assert.AreEqual("m", result.unit);
            Assert.AreEqual(ClearanceStatus.Pass, result.status);
            Assert.AreEqual("none (status=PASS, no blocking region)", result.blockerBinding);
            Assert.AreEqual("aws_robomaker_warehouse_ShelfF_01_001", result.boundaryA);
            Assert.AreEqual("aws_robomaker_warehouse_ShelfD_01_001", result.boundaryB);
        }

        [Test]
        public void ToJson_RoundTripsThroughJsonUtility()
        {
            var original = CompatibilityResult.FromClearance(
                SampleClearance(), "run-1", "check", "constraint",
                "doc", 1, "text", "env", "rev", "hash", "method");

            string json = original.ToJson(false);
            var restored = UnityEngine.JsonUtility.FromJson<CompatibilityResult>(json);

            Assert.AreEqual(original.runId, restored.runId);
            Assert.AreEqual(original.requiredValue, restored.requiredValue, 1e-6f);
            Assert.AreEqual(original.measuredValue, restored.measuredValue, 1e-6f);
            Assert.AreEqual(original.status, restored.status);
            Assert.AreEqual(original.boundaryA, restored.boundaryA);
            Assert.AreEqual(original.blockerBinding, restored.blockerBinding);
        }

        [Test]
        public void BlockerBinding_IsBoundaryAId_WhenNotPass()
        {
            var blocked = SampleClearance();
            blocked.status = ClearanceStatus.Blocked;
            var result = CompatibilityResult.FromClearance(
                blocked, "run", "check", "constraint", "doc", 1, "text", "env", "rev", "hash", "method");

            Assert.AreEqual(blocked.leftEntityId, result.blockerBinding);
        }
    }
}
