using Assets.Db;
using Assets.Db.Models;
using Assets.Scripts;
using Assets.Scripts.Models.Equipment;
using Assets.Scripts.Services;
using Assets.UnitsCharacteristics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public interface IUnitManager
{
    void GenerateUnits();

    void RefreshUnitsActionPoints();

    void SetActualHealthPoins();

    void SetStartEquipment();

    void SetSharedEquipemt(int? oldOrder, int newOrder, ISlotEquipment equipment);

    void SwapSharedEquipmentOrders(int leftOrder, int rightOrder);

    void RemoveEquipmentFromSharedStorage(int order);

    void SwapEquipmentUnit(
        string unitName,
        int fromOrder,
        int toOrder,
        CharacterEquipmentSlotType slotType
    );

    List<Unit> Units { get; }

    List<(ISlotEquipment Equipment, int Order)> SharedEquipment { get; }

    Unit? GetActualUserUnit { get; }
}

public class UnitManager : IUnitManager
{
    private readonly List<Unit> _units = new();
    private List<(ISlotEquipment Equipment, int Order)> _sharedEquipments = new();

    [Inject]
    private readonly UnitFactory _factory;

    [Inject]
    private readonly IGridService _gridService;

    [Inject(Id = Constants.RedWarriorAnimatorController)]
    private readonly AnimatorOverrideController _redWarriorAnimatorController;

    [Inject(Id = Constants.BlueWarriorAnimatorController)]
    private readonly AnimatorOverrideController _blueWarriorAnimatorController;

    [Inject(Id = Constants.BlueMonkAnimatorController)]
    private readonly AnimatorOverrideController _blueMonkAnimatorController;

    [Inject]
    private readonly StaticDb _staticDb;

    public static UnitManager Instance {  get; private set; }

    public UnitManager()
    {
        Instance = this;
    }

    public List<Unit> Units => _units;

    public List<(ISlotEquipment Equipment, int Order)> SharedEquipment => _sharedEquipments;

    public Unit? GetActualUserUnit => _units
        .Where(x => x.IsSelected)
        .Where(x => x.Characterictics.Side == SideType.UserSide)
        .FirstOrDefault();

    public void GenerateUnits()
    {
        GenerateUserUnits();
        GenerateAiUnits();
    }

    public void SetStartEquipment()
    {
        _sharedEquipments.AddRange(EquipemntHelper.GetStartedEquipment().Select((x, i) => (x, i)));
    }

    public void RefreshUnitsActionPoints()
    {
        _units.ForEach(x => x.ActualActionPoints = x.Characterictics.ActiveActionPoints);
    }

    public void SetActualHealthPoins()
    {
        _units.ForEach(x => x.ActualHealthPoints = x.Characterictics.HealthPoints);
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
            Debug.LogError("В списке нет предметов с таким порядком");
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
        var unit = _units.FirstOrDefault(x => x.Name == unitName) ?? throw new Exception("Бля не найду юнита");

        var fromSlot = unit.EqupmentSlots
            .Where(x => x.Type == slotType)
            .Where(x => x.Order == fromOrder)
            .FirstOrDefault()
            ?? throw new Exception("Бля не найду юнита");

        var toSlot = unit.EqupmentSlots
            .Where(x => x.Type == slotType)
            .Where(x => x.Order == toOrder)
            .FirstOrDefault();

        var clearOrders = toSlot == null ? new int[1]{ fromOrder } : new int[2] { fromOrder, toOrder };
        unit.EqupmentSlots.RemoveAll(x => clearOrders.Contains(x.Order) && x.Type == slotType);

        if (toSlot != null)
        {

            toSlot.Order = fromOrder;
            unit.EqupmentSlots.Add(toSlot);
        }

        fromSlot.Order = toOrder;
        unit.EqupmentSlots.Add(fromSlot);
    }

    private void GenerateAiUnits()
    {
        var units = GetUnitsData(GetEnemyLevelUnitIds());

        var i = 0;
        foreach(var unit in units)
        {
            var generatePosition = new Vector3Int((i + 1) * 3, (i + 1) * 3, 0);

            var unitItem = _factory.Create();
            unitItem.transform.position = _gridService.FromGridCordinates(generatePosition);
            unitItem.SetCharacterictics(unit);
            var animator = unitItem.GetComponent<Animator>();
            animator.runtimeAnimatorController = _redWarriorAnimatorController;

            _units.Add(unitItem);
            i++;
        }
    }

    private void GenerateUserUnits()
    {
        var units = GetUnitsData(GetUserUnitIds());

        var i = 0;
        foreach (var unit in units)
        {
            var generatePosition = new Vector3Int(i * 2, i * 4, 0);

            var unitItem = _factory.Create();
            unitItem.transform.position = _gridService.FromGridCordinates(generatePosition);
            unitItem.SetCharacterictics(unit);
            var animator = unitItem.GetComponent<Animator>();
            animator.runtimeAnimatorController = unit.Name == "Monk" ? _blueMonkAnimatorController : _blueWarriorAnimatorController;

            _units.Add(unitItem);
            i++;
        }
    }

    private int[] GetUserUnitIds()
    {
        return new[]{ 1, 2}; 
    }

    private int[] GetEnemyLevelUnitIds()
    {
        return new[] {3};
    }

    private UnitEntity[] GetUnitsData(int[] ids)
    {
        return _staticDb.Units.Where(x => ids.Contains(x.Id)).ToArray();
    }
}