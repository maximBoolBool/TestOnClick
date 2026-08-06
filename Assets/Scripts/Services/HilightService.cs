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
        [Inject(Id = Constants.HighlightTilemap)]
        private readonly Tilemap _highlightTilemap;

        [Inject(Id = Constants.HighlightTile)]
        private readonly TileBase _highlightTile;

        [Inject]
        private readonly IGridService _gridService;

        [Inject]
        private readonly IMovementCostService _movementCostService;

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

            foreach (var pos in reachableTiles)
            {
                _highlightTilemap.SetTile(pos, _highlightTile);
            }
            ;
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

                if (IsWalkable(unit, pos))
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
                    if (!costs.ContainsKey(neighbor) && IsWalkable(unit, neighbor))
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

        private bool IsWalkable(Unit unit, Vector3Int pos)
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