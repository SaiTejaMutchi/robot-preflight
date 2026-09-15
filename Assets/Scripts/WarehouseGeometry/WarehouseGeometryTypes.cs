using System;
using UnityEngine;

namespace SpatialGrid.WarehouseGeometry
{
    public enum ClearanceAxis { X, Z }
    public enum ClearanceStatus { Pass, Blocked, Review }

    [Serializable]
    public struct ClearanceResult
    {
        public float requiredMeters;
        public float availableMeters;
        public float differenceMeters;
        public ClearanceStatus status;
        public string leftEntityId;
        public string rightEntityId;
    }
}
