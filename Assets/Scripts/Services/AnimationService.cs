using Assets.Scripts.Helpers;
using Assets.Scripts.Models.Animations;
using System;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public interface IAnimationService
    {
        void SwitchUnitAnimation(Unit unit, BaseAnimation animation);
    }

    public class AnimationService : IAnimationService
    {
        public void SwitchUnitAnimation(Unit unit, BaseAnimation animation)
        {
            var unitAnimator = unit.GetUnitAnimator().GetComponent<Animator>();

            switch (animation)
            {
                case AttackAnimation attackAnimation:
                    unitAnimator.SetTrigger(Constants.UnitAttackTrigger);
                    break;
                case MoveAnimation moveAnimation:
                    if (moveAnimation.Direction.HasValue)
                    {
                        var spriteRender = unit.GetUnitAnimator().GetComponent<SpriteRenderer>();

                        if (moveAnimation.Direction.Value.x < 0)
                        {
                            spriteRender.flipX = true;
                        }
                        else if(moveAnimation.Direction.Value.x > 0)
                        {
                            spriteRender.flipX = false;
                        }
                    }

                    unitAnimator.SetBool(Constants.IsUnitMoving, moveAnimation.IsActive);
                    break;
                case DeadAnimation deadAnimation:
                    unitAnimator.SetTrigger(Constants.UnitDeadTrigger);
                    break;
                case IdleAnimation idleAnimation:
                default:
                    throw new Exception();
            }
        }
    }
}