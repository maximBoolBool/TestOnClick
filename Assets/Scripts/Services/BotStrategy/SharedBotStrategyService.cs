using Assets.UnitsCharacteristics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.Services.BotStrategy
{
    public interface ISharedBotStrategyService
    {
        Unit FindNearestEnemyOnGrid(Unit currentUnit);

        Vector3Int FindBestTileNearTarget(
            Vector3Int targetPos,
            Vector3Int currentPosition,
            Unit currentUnit
        );

        Vector3Int[] FindPath(
            Vector3Int start,
            Vector3Int end,
            Unit currentUnit
        );
    }

    public class SharedBotStrategyService : ISharedBotStrategyService
    {
        [Inject]
        private readonly IUnitManager _unitManager;

        [Inject]
        private readonly IGridService _gridService;

        [Inject(Id = Constants.GroundTilemap)]
        private readonly Tilemap _groundTilemap;

        // Переделать под сервис
        private Dictionary<TileBase, int> movementCosts = new();

        public Unit FindNearestEnemyOnGrid(Unit currentUnit)
        {
            Unit nearest = null;
            int minDist = int.MaxValue;
            foreach (var unit in _unitManager.Units)
            {
                if (unit == currentUnit || !currentUnit.Characteristic.Side.IsEnemyType(unit.Characteristic.Side))
                {
                    continue;
                }
                var currentUnitPosition = _gridService.ToGridCordinates(currentUnit);
                var targetUnitPosition = _gridService.ToGridCordinates(unit);
                int dist = Mathf.Abs(targetUnitPosition.x - currentUnitPosition.x) +
                           Mathf.Abs(targetUnitPosition.y - currentUnitPosition.y);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = unit;
                }
            }
            return nearest;
        }

        public Vector3Int FindBestTileNearTarget(
            Vector3Int targetPos,
            Vector3Int currentPosition,
            Unit currentUnit
        )
        {
            var bestTile = currentPosition;
            var bestScore = int.MaxValue;
            var reachableTiles = GetReachebleTiles(currentUnit);
            foreach (var tile in reachableTiles)
            {
                if (tile == currentPosition)
                {
                    continue;
                }
                int distToTarget = Mathf.Abs(tile.x - targetPos.x) +
                                  Mathf.Abs(tile.y - targetPos.y);
                int pathCost = GetPathCostTo(tile, currentPosition, currentUnit.ActualActionPoints);
                int score = distToTarget * 100 + pathCost;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestTile = tile;
                }
            }
            return bestTile;
        }

        public Vector3Int[] GetReachebleTiles(Unit currentUnit)
        {
            var reachableTiles = new List<Vector3Int>();
            var queue = new Queue<Vector3Int>();
            var costs = new Dictionary<Vector3Int, int>();
            queue.Enqueue(_gridService.ToGridCordinates(currentUnit));
            costs[_gridService.ToGridCordinates(currentUnit)] = 0;
            while (queue.Count > 0)
            {
                Vector3Int pos = queue.Dequeue();
                int currentCost = costs[pos];
                if (currentCost > currentUnit.ActualActionPoints) continue;
                if (IsWalkable(pos, _gridService.ToGridCordinates(currentUnit)))
                {
                    reachableTiles.Add(pos);
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
                    if (!costs.ContainsKey(neighbor) && IsWalkable(neighbor, _gridService.ToGridCordinates(currentUnit)))
                    {
                        int tileCost = GetMovementCost(neighbor, dir);
                        int newCost = currentCost + tileCost;
                        if (newCost <= currentUnit.ActualActionPoints)
                        {
                            costs[neighbor] = newCost;
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            return reachableTiles.ToArray();
        }

        public Vector3Int[] FindPath(Vector3Int start, Vector3Int end, Unit currentUnit)
        {
            var pathResult = new List<Vector3Int>();
            var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
            var gCost = new Dictionary<Vector3Int, int>();
            var fCost = new Dictionary<Vector3Int, int>();
            var closed = new HashSet<Vector3Int>();
            var open = new PriorityQueue<Vector3Int>();
            gCost[start] = 0;
            fCost[start] = Heuristic(start, end);
            open.Enqueue(start, fCost[start]);
            while (open.Count > 0)
            {
                Vector3Int current = open.Dequeue();
                if (current == end)
                {
                    return ReconstructPath(cameFrom, end).ToArray();
                }
                closed.Add(current);
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
                    Vector3Int neighbor = current + dir;
                    if (closed.Contains(neighbor) || !IsWalkable(neighbor, _gridService.ToGridCordinates(currentUnit)))
                    {
                        continue;
                    }
                    int tentativeGCost = gCost[current] + GetMovementCost(neighbor, dir);
                    if (!gCost.ContainsKey(neighbor) || tentativeGCost < gCost[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gCost[neighbor] = tentativeGCost;
                        fCost[neighbor] = gCost[neighbor] + Heuristic(neighbor, end);
                        open.Enqueue(neighbor, fCost[neighbor]);
                    }
                }
            }
            return Array.Empty<Vector3Int>();
        }

        private bool IsWalkable(Vector3Int pos, Vector3Int currentPosition)
        {
            if (pos == currentPosition)
            {
                return false;
            }
            var occupiedTiles = _unitManager.Units.Select(x => _gridService.ToGridCordinates(x)).ToList();
            if (occupiedTiles.Contains(pos))
            {
                return false;
            }
            var tile = _groundTilemap.GetTile(pos);
            return tile != null && (!movementCosts.ContainsKey(tile) || movementCosts[tile] > 0);
        }

        private int GetPathCostTo(
            Vector3Int target,
            Vector3Int currentPosition,
            int actualActionPoints
        )
        {
            var costs = new Dictionary<Vector3Int, int>();
            var queue = new Queue<Vector3Int>();
            queue.Enqueue(currentPosition);
            costs[currentPosition] = 0;
            Vector3Int[] directions = {
                new Vector3Int(1, 0, 0),
                new Vector3Int(-1, 0, 0),
                new Vector3Int(0, 1, 0),
                new Vector3Int(0, -1, 0),
                new Vector3Int(1, 1, 0),
                new Vector3Int(-1, 1, 0),
                new Vector3Int(1, -1, 0),
                new Vector3Int(-1, -1, 0)
            };
            while (queue.Count > 0)
            {
                var pos = queue.Dequeue();
                int cost = costs[pos];
                if (pos == target)
                    return cost;
                foreach (var dir in directions)
                {
                    var neighbor = pos + dir;
                    if (costs.ContainsKey(neighbor) || !IsWalkable(neighbor, currentPosition))
                    {
                        continue;
                    }
                    int moveCost = GetMovementCost(neighbor, dir);
                    int newCost = cost + moveCost;
                    if (newCost <= actualActionPoints)
                    {
                        costs[neighbor] = newCost;
                        queue.Enqueue(neighbor);
                    }
                }
            }
            return int.MaxValue;
        }

        private int GetMovementCost(Vector3Int pos, Vector3Int direction = default)
        {
            var tile = _groundTilemap.GetTile(pos);
            int baseCost = movementCosts.ContainsKey(tile) ? movementCosts[tile] : 1;
            bool isDiagonal = direction.x != 0 && direction.y != 0;
            return isDiagonal ? Mathf.CeilToInt(baseCost * 1.4f) : baseCost;
        }


        private List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
        {
            var path = new List<Vector3Int> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }
            path.Reverse();
            return path;
        }

        private int Heuristic(Vector3Int a, Vector3Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }
}
