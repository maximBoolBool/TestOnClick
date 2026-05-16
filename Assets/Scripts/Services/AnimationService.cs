using Assets.Scripts;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public interface IAnimationService
    {
        void SwitchUnitAnimation(Unit unit, UnitAnimationType type, bool isActive);
    }

    public class AnimationService : IAnimationService
{
    public void SwitchUnitAnimation(Unit unit, UnitAnimationType type, bool isActive)
    {
        var unitAnimator = unit.GetComponent<Animator>();

        switch (type)
        {
            case UnitAnimationType.Attack:
                unitAnimator.SetTrigger(Constants.UnitAttackTrigger);
                break;
            case UnitAnimationType.Move:
                unitAnimator.SetBool(Constants.IsUnitMoving, isActive);
                break;
            case UnitAnimationType.Idle:
            default:
                throw new System.Exception();
        }
    }
}

    public enum UnitAnimationType
    {
        Idle = 0,
        Attack = 1,
        Move = 2,
    }
}