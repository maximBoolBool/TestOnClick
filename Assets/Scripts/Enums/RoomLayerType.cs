namespace Assets.Scripts.Enums
{
    public enum RoomLayerType
    {
        GroundLayer4 = 24,
        GroundLayer3 = 23,
        GroundLayer2 = 22,
        GroundLayer1 = 21,
        WaterTilemap = 11,
        BaseWaterLayer = 10,
    }

    public static class RoomLayerTypeExtension
    {

        public static string GetRoomLayerGridName(this RoomLayerType type)
        {
            return type switch
            {
                RoomLayerType.GroundLayer4 => "GroundTilemapLevel4",
                RoomLayerType.GroundLayer3 => "GroundTilemapLevel3",
                RoomLayerType.GroundLayer2 => "GroundTilemapLevel2",
                RoomLayerType.GroundLayer1 => "GroundTilemapLevel1",
                RoomLayerType.WaterTilemap => "WaterTilemap",
                RoomLayerType.BaseWaterLayer => "WaterBaseTilemap",
                _ => throw new System.NotImplementedException()
            };
        }

        public static RoomLayerType? GetNextLayer(this RoomLayerType type)
        {
            return type switch
            {
                RoomLayerType.GroundLayer4 => null,
                RoomLayerType.GroundLayer3 => RoomLayerType.GroundLayer4,
                RoomLayerType.GroundLayer2 => RoomLayerType.GroundLayer3,
                RoomLayerType.GroundLayer1 => RoomLayerType.GroundLayer2,
                _ => throw new System.NotImplementedException()
            };
        }
    }

    public static class RoomLayerTypeHelper
    {
        public static RoomLayerType GetRoomLayerType(this string gridName)
        {
            return gridName switch
            {
                "GroundTilemapLevel4" => RoomLayerType.GroundLayer4,
                "GroundTilemapLevel3" => RoomLayerType.GroundLayer3,
                "GroundTilemapLevel2" => RoomLayerType.GroundLayer2,
                "GroundTilemapLevel1" => RoomLayerType.GroundLayer1,
                "WaterTilemap" => RoomLayerType.WaterTilemap,
                "WaterBaseTilemap" => RoomLayerType.BaseWaterLayer,
                _ => throw new System.ArgumentOutOfRangeException(nameof(gridName), $"Unknown grid name: {gridName}")
            };
        }
    }
}