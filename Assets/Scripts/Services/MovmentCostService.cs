using Assets.Scripts.Enums;
using Assets.Scripts.Managers;
using Assets.Scripts.Managers.UnitManager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IMovementCostService
    {
        int GetMovementCost(Vector3Int pos, Vector3Int direction);

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
        private readonly IGridLayersManager _gridLayersManager;

        [Inject]
        private readonly IGameGlobalStateManager _gameGlobalStateManager;

        public int GetMovementCost(Vector3Int pos, Vector3Int direction)
        {
            var isDiagonal = direction.x != 0 && direction.y != 0;
            return isDiagonal ? 2 : 1;
        }

        public bool IsWalkable(Vector3Int pos, Vector3Int currentPosition)
        {
            if (pos == currentPosition)
            {
                return false;
            }

            var currentLayer = _gridLayersManager.GetCordinateRoomLayerType(currentPosition);

            var occupiedTiles = _unitManager.Units.Select(x => _gridService.ToGridCordinates(x)).ToList();
            if (occupiedTiles.Contains(pos))
            {
                return false;
            }

            var layersToCheck = currentLayer.GetLayerWalkableToCheck();

            var tilesToIgnore = _gameGlobalStateManager.GetIgnoreCordinatestoLayer(currentLayer);

            if (!tilesToIgnore.Contains(pos) && layersToCheck.Any(x => _gridLayersManager.HasTileOnLayer(pos, x)))
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

