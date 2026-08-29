namespace Assets.Scripts.Models
{
    public interface IActionPeakItem{}
    public interface ISelfTargetActionPeakItem : IActionPeakItem { };
    public interface IEnemyUnitActionPeakItem : IActionPeakItem { };    
    public interface IAreaTargetItem : IActionPeakItem { };
}