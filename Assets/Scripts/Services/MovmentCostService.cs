using Assets.Scripts.Managers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

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
        private static Dictionary<TileBase, int> movementCosts = new();

        [Inject]
        private readonly IUnitManager _unitManager;

        [Inject]
        private readonly IGridService _gridService;

        [Inject]
        private readonly IGridLayerService _gridLayerService;

        [Inject]
        private readonly IGridLayersManager _gridLayersManager;

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
            if (pos == currentPosition)
            {
                return false;
            }

            var currentLayer = _gridLayerService.GetCordinateRoomLayerType(currentPosition);

            var occupiedTiles = _unitManager.Units.Select(x => _gridService.ToGridCordinates(x)).ToList();
            if (occupiedTiles.Contains(pos))
            {
                return false;
            }

            var tile = _gridLayersManager.GetTileOnLayer(pos, currentLayer);
            return tile != null && (!movementCosts.ContainsKey(tile) || movementCosts[tile] > 0);
        }

        public bool IsWalkable(Unit unit, Vector3Int pos)
        {
            var gridCordinate = _gridService.ToGridCordinates(unit);
            return IsWalkable(pos, gridCordinate);
        }
    }
}

