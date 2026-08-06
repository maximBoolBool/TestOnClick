using UnityEngine;

namespace Assets.Scripts.Services
{
    public interface IMovementCostService
    {
        int GetMovementCost(Vector3Int pos, Vector3Int direction = default);

        bool IsWalkable(Unit unit, Vector3Int pos);

        bool IsWalkable(Vector3Int pos, Vector3Int currentPosition);
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

        public bool IsWalkable(Vector3Int pos, Vector3Int currentPosition)
        {
            //PRT-9
            return true;
            //if (pos == currentPosition)
            //{
            //    return false;
            //}
            //var occupiedTiles = _unitManager.Units.Select(x => _gridService.ToGridCordinates(x)).ToList();
            //if (occupiedTiles.Contains(pos))
            //{
            //    return false;
            //}
            //var tile = _grid.GetTile(pos);
            //return tile != null && (!movementCosts.ContainsKey(tile) || movementCosts[tile] > 0);
        }

        public bool IsWalkable(Unit unit, Vector3Int pos)
        {
            //PRT-9
            return true;
            //if (pos == _gridService.ToGridCordinates(unit))
            //{
            //    return false;
            //}

            //var ocuppaitedTiles = _unitManager.Units
            //    .Select(x => _gridService.ToGridCordinates(x))
            //    .ToArray();
            //if (ocuppaitedTiles.Contains(pos))
            //{
            //    return false;
            //}

            //var tile = _grid.GetTile(pos);
            //return tile != null && (!movementCosts.ContainsKey(tile) || movementCosts[tile] > 0);
        }
    }
}

