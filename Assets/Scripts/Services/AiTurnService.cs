using Assets.Scripts;
using Assets.Scripts.Managers;
using Assets.Scripts.Models.Conditions;
using Assets.UnitsCharacteristics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IAiTurnService
    {
        void ExecuteAiTurn(Unit unit);
    }

    public class AiTurnService : IAiTurnService
    {
        // Переделать под сервис
        private Dictionary<TileBase, int> movementCosts = new();
        public float moveSpeed = 3f;

        [Inject]
        private readonly IUnitManager _unitManager;

        [Inject]
        private readonly IConditionService _conditionService;

        [Inject]
        private readonly ITurnManager _turnManager;

        [Inject]
        private readonly IDamageService _damageService;

        [Inject]
        private readonly IHitService _hitService;

        [Inject(Id = Constants.GroundTilemap)]
        private readonly Tilemap _groundTilemap;

        [Inject]
        private readonly IGridService _gridService;

        [Inject]
        private readonly IAnimationService _animationService;

        public void ExecuteAiTurn(Unit unit)
        {
            unit.StartCoroutine(AiTurnRoutine(unit));
        }

        private IEnumerator AiTurnRoutine(Unit currentUnit)
        {
            var target = FindNearestEnemyOnGrid(currentUnit);
            if (target != null)
            {
                var targetPosition = _gridService.ToGridCordinates(target);
                var bestTile = FindBestTileNearTarget(
                    targetPosition,
                    _gridService.ToGridCordinates(currentUnit),
                    currentUnit
                );
                var path = FindPath(_gridService.ToGridCordinates(currentUnit), bestTile, currentUnit);
                yield return currentUnit.StartCoroutine(MovePath(currentUnit, path));
                const int damageActionPointCost = 3;
                while (currentUnit.ActualActionPoints >= damageActionPointCost)
                {
                    if (IsAdjacent(_gridService.ToGridCordinates(target), _gridService.ToGridCordinates(currentUnit))
                        && currentUnit.ActualActionPoints >= damageActionPointCost
                    )
                    {
                        if (_hitService.IsHit(
                            target.Characteristic.DefendSkill,
                            currentUnit.Characteristic.MeleeSkill,
                            new Dictionary<HitModifierType, int>())
                        )
                        {
                            _damageService.SetUnitDamage(target, 3);
                        }
                        currentUnit.ActualActionPoints -= damageActionPointCost;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            DeselectUnit(currentUnit);
        }

        private Unit FindNearestEnemyOnGrid(Unit currentUnit)
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

        private bool IsAdjacent(Vector3Int targetPosition, Vector3Int currentPosition)
        {
            var deltaX = Mathf.Abs(targetPosition.x - currentPosition.x);
            var deltaY = Mathf.Abs(targetPosition.y - currentPosition.y);
            return Mathf.Max(deltaX, deltaY) <= 1 && !(deltaX == 0 && deltaY == 0);
        }

        private void DeselectUnit(Unit unit)
        {
            _conditionService.ExecuteConditionEffect(unit, ConditionEffectStartType.OnTurnEnd);
            _conditionService.ActualizeUnitConditions(unit);
            if (unit.Characteristic.Side == SideType.UserSide)
            {

            }

            _turnManager.EndTurn();
        }

        private Vector3Int FindBestTileNearTarget(
            Vector3Int targetPos,
            Vector3Int currentPosition,
            Unit currentUnit
        )
        {
            var bestTile = currentPosition;
            var bestScore = int.MaxValue;
            var reachableTiles = HilightReachebleTiles(currentUnit);
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

        private int GetMovementCost(Vector3Int pos, Vector3Int direction = default)
        {
            var tile = _groundTilemap.GetTile(pos);
            int baseCost = movementCosts.ContainsKey(tile) ? movementCosts[tile] : 1;
            bool isDiagonal = direction.x != 0 && direction.y != 0;
            return isDiagonal ? Mathf.CeilToInt(baseCost * 1.4f) : baseCost;
        }

        public IEnumerator MovePath(Unit unit, Vector3Int[] path)
        {
            if (path.Length <= 1)
            {
                yield break;
            }

            // Включаем анимацию движения
            _animationService.SwitchUnitAnimation(unit, UnitAnimationType.Move, true);

            for (int i = 1; i < path.Length; i++)
            {
                Vector3Int step = path[i];
                Vector3Int prevStep = path[i - 1];
                Vector3Int dir = step - prevStep;
                int stepCost = GetMovementCost(step, dir);
                unit.ActualActionPoints -= stepCost;

                var worldTarget = _groundTilemap.GetCellCenterWorld(step);
                float distance = Vector3.Distance(unit.transform.position, worldTarget);
                float duration = distance / moveSpeed;
                float elapsed = 0;
                Vector3 startPos = unit.transform.position;

                while (elapsed < duration)
                {
                    unit.transform.position = Vector3.Lerp(startPos, worldTarget, elapsed / duration);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                unit.transform.position = worldTarget;
            }

            // Выключаем анимацию движения
            _animationService.SwitchUnitAnimation(unit, UnitAnimationType.Move, false);
        }

        private Vector3Int[] FindPath(Vector3Int start, Vector3Int end, Unit currentUnit)
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
                new Vector3Int(1, 0, 0),
                new Vector3Int(-1, 0, 0),
                new Vector3Int(0, 1, 0),
                new Vector3Int(0, -1, 0),
                new Vector3Int(1, 1, 0),
                new Vector3Int(-1, 1, 0),
                new Vector3Int(1, -1, 0),
                new Vector3Int(-1, -1, 0)
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

        private int Heuristic(Vector3Int a, Vector3Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
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

        public Vector3Int[] HilightReachebleTiles(Unit currentUnit)
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
                new Vector3Int(1, 0, 0),
                new Vector3Int(-1, 0, 0),
                new Vector3Int(0, 1, 0),
                new Vector3Int(0, -1, 0),
                new Vector3Int(1, 1, 0),
                new Vector3Int(-1, 1, 0),
                new Vector3Int(1, -1, 0),
                new Vector3Int(-1, -1, 0)
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
    }
}