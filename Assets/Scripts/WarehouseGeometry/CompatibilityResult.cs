using System;
using UnityEngine;

namespace SpatialGrid.WarehouseGeometry
{
    // CON-001-MIN: the minimum reviewer-facing compatibility result, scoped to
    // exactly the fields TASK_LEDGER.md's "Minimum Result Contract for Demo"
    // lists as required to drive both the UI and the evidence record. This is
    // a documented subset of the full CON-001 schema (schema/version+round-trip
    // +invalid-data-test target), not a replacement for it.
    [Serializable]
    public struct CompatibilityResult
    {
        public string schemaVersion;
        public string runId;

        public string robotManufacturer;
        public string robotModel;

        public string checkName;
        public string constraintId;

        public float requiredValue;
        public string unit;
        public float measuredValue;
        public float signedDifference;
        public float tolerance;
        public ClearanceStatus status;

        public string machineSourceDocument;
        public int machineSourcePage;
        public string machineSourceText;

        public string environmentSource;
        public string sourceRevision;
        public string sceneFileHash;

        public string boundaryA;
        public string boundaryB;
        public string blockerBinding;
        public string measurementMethod;

        public static CompatibilityResult FromClearance(
            ClearanceResult clearance,
            string runId,
            string checkName,
            string constraintId,
            string machineSourceDocument,
            int machineSourcePage,
            string machineSourceText,
            string environmentSource,
            string sourceRevision,
            string sceneFileHash,
            string measurementMethod,
            float tolerance = 0.005f)
        {
            return new CompatibilityResult
            {
                schemaVersion = "1.0",
                runId = runId,
                robotManufacturer = "OTTO Motors by Rockwell Automation",
                robotModel = "OTTO 1500",
                checkName = checkName,
                constraintId = constraintId,
                requiredValue = clearance.requiredMeters,
                unit = "m",
                measuredValue = clearance.availableMeters,
                signedDifference = clearance.differenceMeters,
                tolerance = tolerance,
                status = clearance.status,
                machineSourceDocument = machineSourceDocument,
                machineSourcePage = machineSourcePage,
                machineSourceText = machineSourceText,
                environmentSource = environmentSource,
                sourceRevision = sourceRevision,
                sceneFileHash = sceneFileHash,
                boundaryA = clearance.leftEntityId,
                boundaryB = clearance.rightEntityId,
                blockerBinding = clearance.status == ClearanceStatus.Pass
                    ? "none (status=PASS, no blocking region)"
                    : clearance.leftEntityId,
                measurementMethod = measurementMethod,
            };
        }

        public string ToJson(bool prettyPrint = true) => JsonUtility.ToJson(this, prettyPrint);
    }
}
