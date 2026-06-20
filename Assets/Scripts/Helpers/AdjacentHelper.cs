using UnityEngine;

namespace Assets.Scripts.Helpers
{
    public static class AdjacentHelper
    {
        public const int CloseActionRange = 1;

        public static bool IsAdjacent(
            Vector3Int currentPosition,
            Vector3Int targetPosition,
            int actionRange
        )
        {
            var deltaX = Mathf.Abs(targetPosition.x - currentPosition.x);
            var deltaY = Mathf.Abs(targetPosition.y - currentPosition.y);
            return Mathf.Max(deltaX, deltaY) <= actionRange && !(deltaX == 0 && deltaY == 0);
        }
    }
}
