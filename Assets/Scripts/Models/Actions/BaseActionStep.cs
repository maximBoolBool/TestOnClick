using Assets.Db.Enums;
using Assets.Scripts.Models.Conditions;

namespace Assets.Scripts.Models.Actions
{
    public abstract class BaseActionStep
    {
        public virtual ActionStepType Type { get; }
        public int Order { get; set; }
        public int? BeforeStepResult { get; set; }

        // Чтобы случайно не было такого,
        // чтобы первый по порядку пытается смотреть на результаты
        public bool NeedCheckPreviousResult => BeforeStepResult != null && Order != 1;
    }

    public class ActionDamageStep : BaseActionStep
    {
        public override ActionStepType Type => ActionStepType.Damage;

        public int MaxDamageValue { get; set; }

        public int MinDamageValue { get; set; }
    }

    public class EffectWithDurationStep : BaseActionStep
    {
        public override ActionStepType Type => ActionStepType.EffectWithDuration;

        public int BaseChanceToHit { get; set; }

        // Считаем что если Duration null значит действие бесконечное и его можно снять только вручную
        public int? Duration { get; set; }

        public BaseCondition Condition { get; set; }

        public TargetType Target { get; set; }
    }
}
