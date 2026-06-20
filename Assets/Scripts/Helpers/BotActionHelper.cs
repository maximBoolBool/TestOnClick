using Assets.Scripts.Models.Actions;
using Assets.Scripts.Models.Conditions;

namespace Assets.Scripts.Helpers
{
    //Временное решение
    public static class BotActionHelper
    {
        public static BaseAction[] GetEnemyWarriorActions()
        {
            return new BaseAction[]
            {
                new EnemyUnitTargetAction()
                {
                    Name = "Hit",
                    PointCost = 4,
                    Description = "Kill, Smash",
                    Range = 1,
                    Steps = new[]
                    {
                        new ActionDamageStep()
                        {
                            MaxDamageValue = 20,
                            MinDamageValue = 10
                        }
                    }
                },
                new EnemyUnitTargetAction()
                {
                    Name = "Smash",
                    PointCost = 5,
                    Description = "Kill, Smash",
                    Range = 1,
                    Steps = new[]
                    {
                        new ActionDamageStep()
                        {
                            MaxDamageValue = 40,
                            MinDamageValue = 20
                        }
                    }
                }
            };
        }
        
        public static BaseAction[] GetEnemyMonkActions()
        {
            return new BaseAction[]
            {
                new EnemyUnitTargetAction
                {
                    Name = "Hit",
                    PointCost = 5,
                    Description = "Kill, Kill",
                    Range = 1,
                    Steps = new[]
                    {
                        new ActionDamageStep()
                        {
                            MaxDamageValue = 10,
                            MinDamageValue = 5
                        },
                    }
                },
                new AlianceUnitTargetAction
                {
                    Name = "Bless",
                    PointCost = 3,
                    Description = "Bless your friends",
                    Range = 3,
                    Steps = new[]
                    {
                        new EffectWithDurationStep
                        {
                            BaseChanceToHit = 100,
                            Duration = 3,
                            Target = Db.Enums.TargetType.ActionTarget,
                            Condition = new CharacteristicModifier()
                            {
                                Modifications = new System.Collections.Generic.Dictionary<CharecteristicsModifier, (double Value, CharecteristicsModifierOperation Operation)>()
                                {
                                    [CharecteristicsModifier.AttackSkill] = (Value: 10, Operation: CharecteristicsModifierOperation.ValueAdd),
                                    [CharecteristicsModifier.DeafendSkill] = (Value: 10, Operation: CharecteristicsModifierOperation.ValueAdd)
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
