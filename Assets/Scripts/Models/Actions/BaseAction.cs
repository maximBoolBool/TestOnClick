using System;

public abstract class BaseAction
{
    public int PointCost { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public virtual ActionTargetType Type { get; }

    public BaseActionStep[] Steps { get; set; }
}

public class SelfTargetAction : BaseAction
{
    public override ActionTargetType Type => ActionTargetType.SelfPeak;
}

public class UnitTargetAction: BaseAction
{
    public int Range { get; set;}

    public override ActionTargetType Type => ActionTargetType.UnitPeack;

    public bool IsCloseAction => Range == 1;
}

public class AreaTargetAction: BaseAction
{
    public int Range { get; set; }

    public int Area { get; set; }

    public override ActionTargetType Type => ActionTargetType.AreaPeack;
}

public enum ActionTargetType
{
    SelfPeak = 0,
    UnitPeack = 1,
    AreaPeack = 2
}
