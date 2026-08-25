using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Models.Animations
{
    public abstract class BaseAnimation 
    {
        public virtual UnitAnimationType Type { get; } 
    }

    public sealed class IdleAnimation : BaseAnimation
    {
        public override UnitAnimationType Type => UnitAnimationType.Idle;
    }

    public sealed class AttackAnimation : BaseAnimation
    {
        public override UnitAnimationType Type => UnitAnimationType.Attack;
    }

    public sealed class MoveAnimation : BaseAnimation
    {
        public override UnitAnimationType Type => UnitAnimationType.Move;

        public Vector3Int? Direction { get; set; }
        public bool IsActive { get; set; }
    }

    public sealed class DeadAnimation : BaseAnimation
    {
        public override UnitAnimationType Type => UnitAnimationType.Dead;
    }

}
