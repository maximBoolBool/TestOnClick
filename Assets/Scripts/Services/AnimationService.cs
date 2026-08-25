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
                case AttackAnimation _:
                    unitAnimator.SetTrigger(Constants.UNIT_ATTACK_TRIGGER);
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

                    unitAnimator.SetBool(Constants.IS_UNIT_MOVING, moveAnimation.IsActive);
                    break;
                case DeadAnimation _:
                    unitAnimator.SetTrigger(Constants.UNIT_DEAD_TRIGGER);
                    break;
                case IdleAnimation _:
                default:
                    throw new Exception();
            }
        }
    }
}