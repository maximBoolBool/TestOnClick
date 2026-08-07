using System.Collections.Generic;
using UnityEngine;
using Assets.UnitsCharacteristics;
using Assets.Db.Models;
using Assets.Scripts.Models.Actions;
using Assets.Scripts.Models.Conditions;
using Assets.Scripts.Models.Slot;
using System;

namespace Assets.Scripts
{
    public class Unit : MonoBehaviour
    {
        public Guid UnitSessionId { get; set; }
        public string Name { get; set; }
        public int ActualActionPoints { get; set; }
        public int ActualHealthPoints { get; set; }
        public bool IsSelected { get; set; }
        public UnitCharacteristic Characteristic { get; set; }
        public List<BaseAction> Actions { get; set; } = new();
        public List<EquipmentSlot> EquipmentSlots { get; set; } = new();
        public List<BaseCondition> GlobalConditions { get; set; } = new();
        public List<(BaseCondition Condition, int DisappearancesTurn)> DuratationConditions { get; set; } = new();
        // TODO: Consider adding validation - checking for values less than zero
        public bool IsDead => ActualHealthPoints <= 0;
    }

    public static class UnitExtension
    {
        public static Unit SetCharacterictics(this Unit unit,  UnitEntity entity)
        {
            unit.Name = entity.Name;
            unit.ActualHealthPoints = entity.HealthPoints;
            unit.ActualActionPoints = entity.ActiveActionPoints;

            unit.Characteristic = new UnitCharacteristic()
            {
                HealthPoints = entity.HealthPoints,
                ActiveActionPoints = entity.ActiveActionPoints,
                ReactionActionPoints = entity.ReactionActionPoints,
                Agility = entity.Agility,
                MeleeSkill = entity.MeleeSkill,
                DefendSkill = entity.DefendSkill,
                Side = entity.Side
            };

            return unit;
        }
    }

    public class PriorityQueue<T>
    {
        private List<(T item, int priority)> elements = new();

        public int Count => elements.Count;

        public void Enqueue(T item, int priority)
        {
            elements.Add((item, priority));
            elements.Sort((a, b) => a.priority.CompareTo(b.priority));
        }

        public T Dequeue()
        {
            T item = elements[0].item;
            elements.RemoveAt(0);
            return item;
        }
    }
}