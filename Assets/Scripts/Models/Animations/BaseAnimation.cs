using Assets.Scripts.Enums;

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

        public bool IsActive { get; set; }
    }

    public sealed class DeadAnimation : BaseAnimation
    {
        public override UnitAnimationType Type => UnitAnimationType.Dead;
    }

}
