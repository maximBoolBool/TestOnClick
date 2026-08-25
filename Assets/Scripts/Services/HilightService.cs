using Assets.Scripts.Managers;
using Assets.UnitsCharacteristics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.Services
{
    // Выпелить(переделать), как перейдем на EventSystem в подсветке нужных тайлов
    // Вынести в общий сервис movementCosts/IsWalkable/GetMovementCost
    public interface IHilightService
    {
        void HighlightTiles(
            bool highlight,
            List<Vector3Int> reachableTiles,
            Unit unit
        );

        List<Vector3Int> HilightReachebleTiles(
            Unit unit,
            List<Vector3Int> reachableTiles
        );
    }

    public class HiligthService : IHilightService
    {
        [Inject(Id = Constants.HIGHLIGHT_TILEMAP)]
        private readonly Tilemap _highlightTilemap;

        [Inject]
        private readonly IGridService _gridService;

        [Inject]
        private readonly IMovementCostService _movementCostService;

        [Inject]
        private readonly IAddresableResourceManager _addresableResourceManager;

        public void HighlightTiles(bool highlight, List<Vector3Int> reachableTiles, Unit unit)
        {
            if (unit.Characteristic.Side == SideType.EnemySide)
            {
                return;
            }

            if (!highlight)
            {
                // Баг с выбором тайла для прехода поправить как будет время/желание
                _highlightTilemap.ClearAllTiles();
                return;
            }

            var highlightTile = _addresableResourceManager.GetTileBase(TilesAdressableResourceNames.HIGHLIGHT_TILE_NAME);

            foreach (var pos in reachableTiles)
            {
                _highlightTilemap.SetTile(pos, highlightTile);
            }            
        }

        public List<Vector3Int> HilightReachebleTiles(Unit unit, List<Vector3Int> reachableTiles)
        {
            HighlightTiles(
                false,
                reachableTiles,
                unit
            );

            var newReachableTiles = new List<Vector3Int>();
            var queue = new Queue<Vector3Int>();
            var costs = new Dictionary<Vector3Int, int>();
            queue.Enqueue(_gridService.ToGridCordinates(unit));
            costs[_gridService.ToGridCordinates(unit)] = 0;

            while (queue.Count > 0)
            {
                var pos = queue.Dequeue();
                int currentCost = costs[pos];
                if (currentCost > unit.ActualActionPoints)
                {
                    continue;
                }

                if (_movementCostService.IsWalkable(unit, pos))
                {
                    newReachableTiles.Add(pos);
                }

                Vector3Int[] directions = {
                    new(1, 0, 0),
                    new(-1, 0, 0),
                    new(0, 1, 0),
                    new(0, -1, 0),
                    new(1, 1, 0),
                    new(-1, 1, 0),
                    new(1, -1, 0),
                    new(-1, -1, 0)
                };

                foreach (var dir in directions)
                {
                    Vector3Int neighbor = pos + dir;
                    if (!costs.ContainsKey(neighbor) && _movementCostService.IsWalkable(unit, neighbor))
                    {
                        int tileCost = _movementCostService.GetMovementCost(neighbor, dir);
                        int newCost = currentCost + tileCost;
                        if (newCost <= unit.ActualActionPoints)
                        {
                            costs[neighbor] = newCost;
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            HighlightTiles(
                true,
                newReachableTiles,
                unit
            );

            return newReachableTiles;
        }
    }
}