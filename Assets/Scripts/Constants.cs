namespace Assets.Scripts
{
    public static class Constants
    {
        #region Zenject Game Object Ids

        public const string Grid = "Grid";
        public const string HighlightTilemap = "Highlight";
        public const string HighlightTile = "HighlightTile";
        public const string HoverTile = "HoverTile";
        public const string ActionButtonPanel = "ActionButtonPanel";
        public const string ActionButtonPrefab = "ActionButtonPrefab";
        public const string TurnCountText = "TurnCountText";
        public const string EnemyInfoPanel = "EnemyInfoPanel";
        public const string EquipemntPanel = "EquipmentPanel";
        public const string EquipemntSlotPrefab = "EquipemntSlotPrefab";
        public const string Camera = "MainCamera";

        public const string HealthBarSlider = "HealthBarSlider";
        public const string HealthBarText = "HealthBarText";

        public const string ActionPointsBarSlider = "ActionPointsBarSlider";
        public const string ActionPointsBarText = "ActionPointsBarText";

        #endregion

        #region Cloud Veil Constants

        public const string TopVeilCloudPart = "TopVeilCloudPart";
        public const string BottomVeilCloudPart = "BottomVeilCloudPart";
        public const string LeftVeilCloudPart = "LeftVeilCloudPart";
        public const string RightVeilCloudPart = "RightVeilCloudPart";

        #endregion

        #region Game Constants

        public const int MAX_EQUIPMENT_ITEMS_CELL_COUNT = 18;

        #endregion

        #region Animation Conditions

        public const string IsUnitMoving = "IsMoving";
        public const string UnitAttackTrigger = "AttackTrigger";
        public const string UnitDeadTrigger = "DeadTrigger";

        #endregion
    }
}