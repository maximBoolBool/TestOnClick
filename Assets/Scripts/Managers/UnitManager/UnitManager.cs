using Assets.Db;
using Assets.Db.Models;
using Assets.Scripts.Enums;
using Assets.Scripts.Factory;
using Assets.Scripts.Helpers;
using Assets.Scripts.Models.Equipment;
using Assets.Scripts.Services;
using Assets.UnitsCharacteristics;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Managers.UnitManager
{
    public interface IUnitManager
    {
        UniTask GenerateUnits();

        void RefreshUnitsActionPoints();

        void SetActualHealthPoins();

        void SetStartEquipment();

        void SetSharedEquipemt(
            int? oldOrder,
            int newOrder,
            ISlotEquipment equipment
        );

        void SwapSharedEquipmentOrders(int leftOrder, int rightOrder);

        void RemoveEquipmentFromSharedStorage(int order);

        void SwapEquipmentUnit(
            string unitName,
            int fromOrder,
            int toOrder,
            CharacterEquipmentSlotType slotType
        );

        UniTask GenerateWaveUnits(
            int roomId,
            int waveOrder,
            bool withDeleteActual
        );

        void SwitchUnitVisual(RoomLayerType actualLayer, RoomLayerType? previousLayer);

        List<Unit> Units { get; }

        List<(ISlotEquipment Equipment, int Order)> SharedEquipment { get; }

        Unit? GetActualUserUnit { get; }
    }

    public partial class UnitManager : IUnitManager
    {
        #region Manager States

        private readonly List<Unit> _units = new();
        private List<(ISlotEquipment Equipment, int Order)> _sharedEquipments = new();
        private readonly Dictionary<Guid, RoomLayerType> _unitSessionIds = new();

        #endregion

        [Inject]
        private readonly UnitFactory _unitFactory;

        [Inject]
        private readonly IRoomSpawnService _roomSpawnService;

        [Inject]
        private readonly IGridService _gridService;

        [Inject]
        private readonly StaticDb _staticDb;

        [Inject]
        private readonly IGameGlobalStateManager _gameGlobalStateManager;

        [Inject]
        private readonly IGridLayersManager _gridLayersManager;

        [Inject]
        private readonly IAddresableResourceManager _addresableResourceManager;

        public static UnitManager Instance {  get; private set; }

        public UnitManager()
        {
            Instance = this;
        }

        #region Public Unit Properties

        public List<Unit> Units => _units;

        public List<(ISlotEquipment Equipment, int Order)> SharedEquipment => _sharedEquipments;

        public Unit? GetActualUserUnit => _units
            .Where(x => x.IsSelected)
            .Where(x => x.Characteristic.Side == SideType.UserSide)
            .FirstOrDefault();

        #endregion

        public async UniTask GenerateUnits()
        {
            await GenerateUserUnits();
            await GenerateEnemyUnitsAsync(
                roomId: _gameGlobalStateManager.ActualRoomId,
                waveOrder: _gameGlobalStateManager.ActualWaveOrder
            );
        }

        public void SetStartEquipment()
        {
            _sharedEquipments.AddRange(EquipmentHelper.GetStartedEquipment().Select((x, i) => (x, i)));
        }

        public void RefreshUnitsActionPoints()
        {
            _units.ForEach(x => x.ActualActionPoints = x.Characteristic.ActiveActionPoints);
        }

        public void SetActualHealthPoins()
        {
            _units.ForEach(x => x.ActualHealthPoints = x.Characteristic.HealthPoints);
        }

        public void SetSharedEquipemt(int? oldOrder, int newOrder, ISlotEquipment equipment)
        {
            if(oldOrder != null)
            {
                _sharedEquipments.RemoveAll(x => x.Order == oldOrder);
            }

            _sharedEquipments.Add((equipment, newOrder));
        }

        public void SwapSharedEquipmentOrders(int leftOrder, int rightOrder)
        {
            var leftEquipment = _sharedEquipments.FirstOrDefault(x => x.Order == leftOrder);
            var rightEquipment = _sharedEquipments.FirstOrDefault(x => x.Order == rightOrder);

            if (leftEquipment.Equipment == null || rightEquipment.Equipment == null)
            {
                Debug.LogError($"Equipment not found with orders: {leftOrder} or {rightOrder}");
                return;
            }

            _sharedEquipments.RemoveAll(x => x.Order == leftOrder || x.Order == rightOrder);
            _sharedEquipments.AddRange(
                new (ISlotEquipment Equipment, int Order)[2]
                {
                    (leftEquipment.Equipment, rightOrder),
                    (rightEquipment.Equipment, leftOrder),
                }
            );

            _sharedEquipments = _sharedEquipments.OrderBy(x => x.Order).ToList();
        }

        public void RemoveEquipmentFromSharedStorage(int order)
        {
            _sharedEquipments.RemoveAll(x => x.Order == order);
        }

        public void SwapEquipmentUnit(
            string unitName,
            int fromOrder,
            int toOrder,
            CharacterEquipmentSlotType slotType
        )
        {
            var unit = _units.FirstOrDefault(x => x.Name == unitName);
            if (unit == null)
            {
                throw new InvalidOperationException($"Unit with name '{unitName}' not found");
            }

            var fromSlot = unit.EquipmentSlots
                .Where(x => x.Type == slotType)
                .Where(x => x.Order == fromOrder)
                .FirstOrDefault()
                ?? throw new InvalidOperationException($"Equipment slot of type '{slotType}' with order {fromOrder} not found for unit '{unitName}'");
            
            var toSlot = unit.EquipmentSlots
                .Where(x => x.Type == slotType)
                .Where(x => x.Order == toOrder)
                .FirstOrDefault();

            var clearOrders = toSlot == null ? new int[1]{ fromOrder } : new int[2] { fromOrder, toOrder };
            unit.EquipmentSlots.RemoveAll(x => clearOrders.Contains(x.Order) && x.Type == slotType);

            if (toSlot != null)
            {

                toSlot.Order = fromOrder;
                unit.EquipmentSlots.Add(toSlot);
            }

            fromSlot.Order = toOrder;
            unit.EquipmentSlots.Add(fromSlot);
        }

        public async UniTask GenerateWaveUnits(
            int roomId,
            int waveOrder,
            bool withDeleteActual
        )
        {
            if (withDeleteActual)
            {
                ClearEnemiesunits();
            }

            await GenerateEnemyUnitsAsync(
                roomId: roomId,
                waveOrder: waveOrder
            );
        }

        #region Private Methondes

        private void ClearEnemiesunits()
        {
            var deleteUnits = _units.Where(x => x.Characteristic.Side == SideType.EnemySide).ToArray();

            foreach (var unit in deleteUnits)
            {
                _units.Remove(unit);

                if (unit != null && unit.gameObject != null)
                {
                    UnityEngine.Object.Destroy(unit.gameObject);
                }
            }
        }

        private async UniTask GenerateEnemyUnitsAsync(int roomId, int waveOrder)
        {
            var enemiesCounts = _roomSpawnService.GetEnemyUnitIdCountsPairs(
                roomId: roomId,
                waveOrder: waveOrder
            );

            var units = GetUnitsData(enemiesCounts.Keys.ToArray());

            var i = 1;
            foreach (var unit in units)
            {
                enemiesCounts.TryGetValue(unit.Id, out var count);

                for (var j = 0; j < count; j++)
                {
                    var isMonk = unit.Name == "Monk";

                    var generatePosition = _gridLayersManager.GetRoomCordinateFromGridCordinate(new Vector3Int((i + j) * 3, (i + j) * 3, 0));
                    var unitItem = _unitFactory.Create();
                    unitItem.UnitSessionId = Guid.NewGuid();
                    unitItem.transform.position = _gridService.FromGridCordinates(generatePosition);
                    unitItem.SetCharacterictics(unit);
                    var animator = unitItem.GetUnitAnimator().GetComponent<Animator>();
                    animator.runtimeAnimatorController = isMonk
                        ? _addresableResourceManager.GetUnitOverrideAnimationController("RedMonkAnimatorOverrideController")                       
                        : _addresableResourceManager.GetUnitOverrideAnimationController("RedWarriorAnimatorOverrideContoller");
                    unitItem.Actions = (isMonk
                            ? BotActionHelper.GetEnemyMonkActions()
                            : BotActionHelper.GetEnemyWarriorActions()
                        )
                        .ToList();

                    var iconName = UnitAdressableLoaderHelper.GetUnitIconAddressableName(unit.Name, SideType.EnemySide);

                    unitItem.GetUnitIcon().GetComponent<SpriteRenderer>().sprite = _addresableResourceManager.GetUnitIconSprite(iconName);

                    unitItem.SwitchUnitVisual(UnitVisualType.Animation);

                    var unitLayer = _gridLayersManager.GetCordinateRoomLayerType(generatePosition);
                    _unitSessionIds.TryAdd(unitItem.UnitSessionId, unitLayer);

                    _units.Add(unitItem);
                }

                i += 2;
            }
        }

        private async UniTask GenerateUserUnits()
        {
            var units = GetUnitsData(GetUserUnitIds());

            var i = 0;
            foreach (var unit in units)
            {
                var generatePosition = _gridLayersManager.GetRoomCordinateFromGridCordinate(new Vector3Int((i + 1) * -3, (i + 1) * -3, 0));

                var isMonk = unit.Name == "Monk";

                var unitItem = _unitFactory.Create();
                unitItem.UnitSessionId = Guid.NewGuid();
                unitItem.transform.position = _gridService.FromGridCordinates(generatePosition);

                var unitLayer = _gridLayersManager.GetCordinateRoomLayerType(generatePosition);
                _unitSessionIds.TryAdd(unitItem.UnitSessionId, unitLayer);

                unitItem.SetCharacterictics(unit);
                var animator = unitItem.GetUnitAnimator().GetComponent<Animator>();
                animator.runtimeAnimatorController = isMonk
                    ? _addresableResourceManager.GetUnitOverrideAnimationController("BlueMonkAnimatorController") 
                    : _addresableResourceManager.GetUnitOverrideAnimationController("BlueWarriorAnimatorController");

                unitItem.Actions = (isMonk
                        ? BotActionHelper.GetEnemyMonkActions()
                        : BotActionHelper.GetEnemyWarriorActions()
                    )
                    .ToList();

                var iconName = UnitAdressableLoaderHelper.GetUnitIconAddressableName(unit.Name, SideType.UserSide);

                unitItem.GetUnitIcon().GetComponent<SpriteRenderer>().sprite = _addresableResourceManager.GetUnitIconSprite(iconName);

                unitItem.SwitchUnitVisual(UnitVisualType.Animation);

                _units.Add(unitItem);
                i++;
            }
        }

        private static int[] GetUserUnitIds()
        {
            return new[]{ 1, 2}; 
        }

        private UnitEntity[] GetUnitsData(int[] ids)
        {
            return _staticDb.Units.Where(x => ids.Contains(x.Id)).ToArray();
        }

        public void SwitchUnitVisual(RoomLayerType actualLayer, RoomLayerType? previousLayer)
        {
            var unitsInActualLayer = _unitSessionIds
                .Where(x => x.Value == actualLayer)
                .Select(x => _units.FirstOrDefault(u => u.UnitSessionId == x.Key))
                .Where(u => u != null)
                .ToArray();

            foreach (var unit in unitsInActualLayer)
            {
                unit.SwitchUnitVisual(UnitVisualType.Animation);
            }

            if (previousLayer == null)
            {
                return;
            }

            var needCheckLayerCross = previousLayer != null && actualLayer > previousLayer;

            var unitsInPreviousLayer = _unitSessionIds
                .Where(x => x.Value == previousLayer)
                .Select(x => _units.FirstOrDefault(u => u.UnitSessionId == x.Key))
                .Where(u => u != null)
                .Where(x => !needCheckLayerCross || NeedChangeUnitAnimationType(x.gameObject.transform.position, actualLayer)
                )
                .ToArray();

            foreach (var unit in unitsInPreviousLayer)
            {
                unit.SwitchUnitVisual(UnitVisualType.Icon);
            }
        }

        private bool NeedChangeUnitAnimationType(Vector3 cordinate, RoomLayerType checkLayer)
        {
            var gridCordinate = _gridService.ToGridCordinates(cordinate);
            return _gridLayersManager.HasTileOnLayer(gridCordinate, checkLayer);
        }

        #endregion
    }
}