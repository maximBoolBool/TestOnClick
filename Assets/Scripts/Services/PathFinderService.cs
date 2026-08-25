using Assets.Scripts.Managers.UnitManager;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IPathFinderService
    {
        (Vector3Int, int)[] FindPath(Vector3Int start, Vector3Int end, Unit currentUnit);
    }

    public class PathFinderService : IPathFinderService
    {
        [Inject]
        private readonly IGridService _gridService;

        [Inject]
        private readonly IMovementCostService _movementCostService;

        public (Vector3Int, int)[] FindPath(Vector3Int start, Vector3Int end, Unit currentUnit)
        {
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
                    if (closed.Contains(neighbor) || !_movementCostService.IsWalkable(neighbor, _gridService.ToGridCordinates(currentUnit)))
                    {
                        continue;
                    }
                    int tentativeGCost = gCost[current] + _movementCostService.GetMovementCost(neighbor, dir);
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

        private int Heuristic(Vector3Int a, Vector3Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }
}
