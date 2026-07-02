using Assets.Db.Enums;
using Assets.Scripts.Helpers;
using Assets.Scripts.Models.Actions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IActionClickHandler
    {
        void OnClick(BaseAction action);

        void CancelAction();

        bool IsCanBeTarget(Vector3Int position);

        void ClearActionTiles();

        bool IsHasFirstClick {  get; }

        BaseAction? SelectedAction { get; }    
    }

    public class ActionClickHandler : IActionClickHandler
    {
        [Inject]
        private readonly IUnitManager _unitManager;

        [Inject(Id = Constants.HighlightTilemap)]
        private readonly Tilemap _highlightTilemap;

        [Inject(Id = Constants.HoverTile)]
        private readonly TileBase _hoverTile;

        [Inject]
        private readonly IActionCostService _actionCostService;

        [Inject]
        private readonly IHilightService _hilightService;

        [Inject]
        private readonly IGridService _gridService;

        private List<Vector3Int> _actionChooseVectors = new();
        private BaseAction? _lastActionClick = null;

        public bool IsHasFirstClick => _lastActionClick != null;

        public BaseAction SelectedAction => _lastActionClick;

        public void OnClick(BaseAction action)
        {
            if(_lastActionClick != null)
            {
                if(_lastActionClick.Name == action.Name)
                {
                    SecondClick(action);
                    return;
                }
            }

            FirstButtonClick(action);
        }

        private void SecondClick(BaseAction action)
        {
            switch (action.Type)
            {
                case ActionTargetType.SelfPeak:
                    _lastActionClick = null;
                    break;
                case ActionTargetType.OtherSideUnitPeacks:
                case ActionTargetType.AreaPeack:
                    CancelAction();
                    break;
            }
        }

        public void CancelAction()
        {
            var currentUnit = _unitManager.Units.FirstOrDefault(x => x.IsSelected);
            if (currentUnit == null)
            {
                Debug.Log("No unit is currently selected");
                return;
            }
            _highlightTilemap.ClearAllTiles();
            _hilightService.HighlightTiles(true, _actionChooseVectors, currentUnit);
            _hilightService.HilightReachebleTiles(currentUnit, _actionChooseVectors);
            _lastActionClick = null;
            ClearActionTiles();
        }

        public bool IsCanBeTarget(Vector3Int position)
        {
            var canBeTarget = _actionChooseVectors.Contains(position);
            return canBeTarget;
        }

        public void ClearActionTiles()
        {
            _actionChooseVectors.Clear();
        }

        private void FirstButtonClick(BaseAction baseAction)
        {
            _lastActionClick = baseAction;
            var currentUnit = _unitManager.Units.FirstOrDefault(x => x.IsSelected);
            if (currentUnit == null)
            {
                Debug.LogError("No unit is currently selected");
                return;
            }

            if(!_actionCostService.IsActionAvaliable(points: currentUnit.ActualActionPoints, pointCost: baseAction.PointCost))
            {
                Debug.LogWarning("Not enough action points to perform this action");
                return;
            }

            switch (baseAction.Type)
            {
                case ActionTargetType.SelfPeak:
                    break;
                case ActionTargetType.AreaPeack:
                    break;
                case ActionTargetType.OtherSideUnitPeacks:
                    _highlightTilemap.ClearAllTiles();

                    var action = (EnemyUnitTargetAction)baseAction;
                    var enemiesAdjacentVectors = _unitManager.Units
                        .Where(x => x.Characteristic.Side != currentUnit.Characteristic.Side)
                        .Where(x => !x.IsDead)
                        .Where(x => AdjacentHelper.IsAdjacent(
                            _gridService.ToGridCordinates(currentUnit),
                            _gridService.ToGridCordinates(x),
                            action.Range
                        ))
                        .Select(x => _gridService.ToGridCordinates(x));

                    _actionChooseVectors.AddRange(enemiesAdjacentVectors);
                    foreach (var enemiVector in _actionChooseVectors)
                    {
                        _highlightTilemap.SetTile(enemiVector, _hoverTile);
                    }

                    break;
            }
        }
    }
}