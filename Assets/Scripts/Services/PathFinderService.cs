using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IPathFinderService
    {
        (Vector3Int, int)[] FindPath(Vector3Int start, Vector3Int end, Unit currentUnit);
    }

    public class PathFinderService : IPathFinderService
    {
        private Dictionary<TileBase, int> movementCosts = new();

        [Inject(Id = Constants.GroundTilemap)]
        private readonly Tilemap _groundTilemap;

        [Inject]
        private readonly IGridService _gridService;

        [Inject]
        private readonly IUnitManager _unitManager;

        public (Vector3Int, int)[] FindPath(Vector3Int start, Vector3Int end, Unit currentUnit)
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
                /*if (current == end)
                {
                    return ReconstructPath(cameFrom, end).ToArray();
                }*/
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
            return Array.Empty<(Vector3Int, int)>();
        }

        private int GetMovementCost(Vector3Int pos, Vector3Int direction = default)
        {
            var tile = _groundTilemap.GetTile(pos);
            int baseCost = movementCosts.ContainsKey(tile) ? movementCosts[tile] : 1;
            bool isDiagonal = direction.x != 0 && direction.y != 0;
            return isDiagonal ? Mathf.CeilToInt(baseCost * 1.4f) : baseCost;
        }

        private int Heuristic(Vector3Int a, Vector3Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
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

        /*private List<(Vector3Int, int)> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
        {
            var path = new List<Vector3Int> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }
            path.Reverse();
            return path;
        }*/
    }
}
