using Assets.Db.Enums;
using Assets.Scripts.Managers;
using Assets.Scripts.Managers.UnitManager;
using Assets.Scripts.Models.Animations;
using Assets.Scripts.Services;
using Assets.UnitsCharacteristics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.Behaviours
{
    public class UserUnitsBehaviour : MonoBehaviour
    {
        [Inject(Id = Constants.HighlightTilemap)]
        private readonly Tilemap _highlightTilemap;

        [Inject(Id = Constants.HighlightTile)]
        private readonly TileBase _highlightTile;

        [Inject(Id = Constants.HoverTile)]
        private readonly TileBase _hoverTile;

        [Inject]
        private readonly IUnitManager _unitManager;

        [Inject]
        private readonly IActionClickHandler _actionClickHandler;

        [Inject]
        private readonly IActionExecutionService _actionExecutionService;

        [Inject]
        private readonly ITurnManager _turnManager;

        [Inject]
        private readonly IHilightService _hilightService;

        [Inject]
        private readonly IGridService _gridService;

        [Inject] 
        private readonly IGameGlobalStateManager _gameGlobalStateManager;

        [Inject]
        private readonly IEnemyPanelService _enemyPanelServic;

        [Inject]
        private readonly IAnimationService _animationService;

        [Inject]
        private readonly IUnitPanelBarService _unitPanelBarService;

        [Inject]
        private readonly IMovementCostService _movementCostService;

        private List<Vector3Int> reachableTiles = new();
        private List<Vector3Int> path = new();
        private Vector3Int lastHoveredTile;
        private bool isHovering = false;
        private bool isUnitMoving = false;
        private bool isShowingUnitInfo = false;
        private const float moveSpeed = 3f;

        private void Update()
        {
            if (_gameGlobalStateManager.SelectedUnit == null)
            {
                var actualUnit = _unitManager.GetActualUserUnit;

                if(actualUnit == null)
                {
                    return;
                }

                _gameGlobalStateManager.SelectedUnit = actualUnit;
                reachableTiles = _hilightService.HilightReachebleTiles(_gameGlobalStateManager.SelectedUnit, reachableTiles);
            }

            var selectedUnit = _gameGlobalStateManager.SelectedUnit;
            // Временное решение пока вместе с переходом на 1 MonoBehaivor не перейдем на
            /*
             3. Альтернатива: использовать Pointer Events вместо ручного Mouse checking
            Более современный подход — повесить на объект с Tilemap (или пустой GameObject над камерой)
            скрипт с интерфейсами IPointerClickHandler, IPointerEnterHandler и т.д.
            Тогда вся обработка кликов по карте будет идти через ту же систему,
            что и UI, и конфликтов не будет вообще.
             */
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            var selectedAction = _actionClickHandler.SelectedAction;
            if (selectedAction != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    if (selectedAction.Type == ActionTargetType.SelfPeak)
                    {
                        _actionExecutionService.TryExecuteAction(
                            executor: selectedUnit,
                            action: selectedAction,
                            target: null
                        );
                        reachableTiles = _hilightService.HilightReachebleTiles(selectedUnit, reachableTiles);
                        return;
                    }
                    var mousePosition = Mouse.current.position.ReadValue();
                    var worldPos = Camera.main.ScreenToWorldPoint(mousePosition);
                    worldPos.z = 0;
                    var mouseTilePos = _gridService.ToGridCordinates(worldPos);
                    _enemyPanelServic.HideUnitInfo();
                    isShowingUnitInfo = false;

                    if (!_actionClickHandler.IsCanBeTarget(mouseTilePos))
                    {
                        Debug.LogWarning("Tile can not be target");
                        return;
                    }

                    _actionExecutionService.TryExecuteAction(
                        executor: selectedUnit,
                        action: selectedAction,
                        target: mouseTilePos
                    );
                    reachableTiles = _hilightService.HilightReachebleTiles(selectedUnit, reachableTiles);
                }
                return;
            }
            if (Mouse.current != null)
            {
                var mousePosition = Mouse.current.position.ReadValue();
                var worldPos = Camera.main.ScreenToWorldPoint(mousePosition);
                worldPos.z = 0;
                var mouseTilePos = _gridService.ToGridCordinates(worldPos);
                if (!isUnitMoving)
                {
                    if (reachableTiles.Contains(mouseTilePos))
                    {
                        if (!isHovering || mouseTilePos != lastHoveredTile)
                        {
                            if (isHovering)
                            {
                                _highlightTilemap.SetTile(lastHoveredTile, _highlightTile);
                            }
                            _highlightTilemap.SetTile(mouseTilePos, _hoverTile);
                            lastHoveredTile = mouseTilePos;
                            isHovering = true;
                        }
                    }
                    else if (isHovering)
                    {
                        _highlightTilemap.SetTile(lastHoveredTile, _highlightTile);
                        isHovering = false;
                    }
                }
                if (!isUnitMoving)
                {
                    if (Mouse.current.leftButton.wasPressedThisFrame)
                    {
                        if (reachableTiles.Contains(mouseTilePos))
                        {
                            _enemyPanelServic.HideUnitInfo();
                            isShowingUnitInfo = false;
                            FindPath(_gridService.ToGridCordinates(selectedUnit), mouseTilePos, selectedUnit);
                            StartCoroutine(MovePath(selectedUnit));
                        }
                    }
                    if (Mouse.current.rightButton.wasPressedThisFrame)
                    {
                        if (!isShowingUnitInfo)
                        {
                            var unit = _unitManager.Units
                                .Where(x => !x.IsSelected)
                                .Where(x => _gridService.ToGridCordinates(x.transform.position) == mouseTilePos)
                                .FirstOrDefault();

                            if(unit != null)
                            {
                                _enemyPanelServic.ShowUnitInfo(unit);
                                isShowingUnitInfo = true;
                            }
                        }
                        else
                        {
                            _enemyPanelServic.HideUnitInfo();
                            isShowingUnitInfo = false;
                        }
                    }
                }
            }
        }

        void FindPath(Vector3Int start, Vector3Int end, Unit unit)
        {
            path.Clear();
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
                if (closed.Contains(current))
                {
                    continue;
                }
                closed.Add(current);
                if (current == end)
                {
                    path = ReconstructPath(cameFrom, end);
                    return;
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
                foreach (Vector3Int dir in directions)
                {
                    Vector3Int neighbor = current + dir;
                    if (closed.Contains(neighbor) || !_movementCostService.IsWalkable(unit, neighbor))
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
        }

        IEnumerator MovePath(Unit unit)
        {
            var direction = path[1] - path[0];

            _animationService.SwitchUnitAnimation(
                unit,
                new MoveAnimation 
                {
                    IsActive = true,
                    Direction = direction
                }
            );

            isUnitMoving = true;
            _hilightService.HighlightTiles(false, reachableTiles, unit);
            for (var i = 1; i < path.Count; i++)
            {
                var step = path[i];
                var prevStep = path[i - 1];
                var dir = step - prevStep;

                var stepCost = _movementCostService.GetMovementCost(step, dir);

                unit.ActualActionPoints -= stepCost;

                var worldTarget = _gridService.FromGridCordinates(step);
            
                var distance = Vector3.Distance(unit.transform.position, worldTarget);
                var duration = distance / moveSpeed;
                var elapsed = 0f;
                var startPos = unit.transform.position;
                while (elapsed < duration)
                {
                    unit.transform.position = Vector3.Lerp(startPos, worldTarget, elapsed / duration);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                unit.transform.position = worldTarget;
            }

            isUnitMoving = false;
            _animationService.SwitchUnitAnimation(
                unit,
                new MoveAnimation 
                { 
                    IsActive = false,
                    Direction = null
                }
            );

            if (unit.Characteristic.Side == SideType.UserSide)
            {
                if (unit.ActualActionPoints <= 0)
                {
                    _turnManager.DeactivateUnit(unit);
                    yield break;
                }
            }

            _unitPanelBarService.SetUnitActionPoints(
                actualActionPoint: unit.ActualActionPoints,
                maxActionPoint: unit.Characteristic.ActiveActionPoints
            );

            reachableTiles = _hilightService.HilightReachebleTiles(unit, reachableTiles);
        }

        private int Heuristic(Vector3Int a, Vector3Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        private List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
        {
            List<Vector3Int> path = new() { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }
            path.Reverse();
            return path;
        }
    }
}