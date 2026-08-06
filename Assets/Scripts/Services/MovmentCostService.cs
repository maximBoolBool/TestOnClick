using UnityEngine;

namespace Assets.Scripts.Services
{
    public interface IMovementCostService
    {
        int GetMovementCost(Vector3Int pos, Vector3Int direction = default);
    }

    public class MovementCostService : IMovementCostService
    {

        public int GetMovementCost(Vector3Int pos, Vector3Int direction)
        {
            //PRT-9
            return 1;
            //var tile = _grid.GetTile(pos);
            //var baseCost = movementCosts.ContainsKey(tile) ? movementCosts[tile] : 1;
            //var isDiagonal = direction.x != 0 && direction.y != 0;
            //return isDiagonal ? Mathf.CeilToInt(baseCost * 1.4f) : baseCost;
        }
    }
}

