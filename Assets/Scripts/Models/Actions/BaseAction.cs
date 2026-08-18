using Assets.Db.Enums;

namespace Assets.Scripts.Models.Actions
{
    public abstract class BaseAction
    {
        public int PointCost { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public virtual ActionTargetType Type { get; }

        public BaseActionStep[] Steps { get; set; }
    }

    public abstract class UnitTargetAction : BaseAction
    {
        public int Range { get; set; }

        public bool IsCloseAction => Range == 1;
    }

    public class EnemyUnitTargetAction: UnitTargetAction
    {
        public override ActionTargetType Type => ActionTargetType.OtherSideUnitPeacks;
    }

    public class AlianceUnitTargetAction : UnitTargetAction
    {
        public override ActionTargetType Type => ActionTargetType.SideUnitPeack;
    }

    public class AreaTargetAction: BaseAction
    {
        public int Range { get; set; }

        public int Area { get; set; }

        public override ActionTargetType Type => ActionTargetType.AreaPeack;
    }
}
