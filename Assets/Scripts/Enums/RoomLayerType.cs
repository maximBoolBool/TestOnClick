namespace Assets.Scripts.Enums
{
    public enum RoomLayerType
    {
        GroundLayer4 = 59,
        CliffLayer4 = 50,
        GroundLayer3 = 49,
        CliffLayer3 = 40,
        GroundLayer2 = 39,
        CliffLayer2 = 30,
        GroundLayer1 = 29,
        CliffLayer1 = 20,
        WaterTilemap = 10,
        BaseWaterLayer = 0,
    }

    public static class RoomLayerTypeExtension
    {

        public static string GetRoomLayerGridName(this RoomLayerType type)
        {
            return type switch
            {
                RoomLayerType.CliffLayer4 => "ClifTilemapLevel4",
                RoomLayerType.CliffLayer3 => "ClifTilemapLevel3",
                RoomLayerType.CliffLayer2 => "ClifTilemapLevel2",
                RoomLayerType.CliffLayer1 => "ClifTilemapLevel1",

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
                "ClifTilemapLevel4" => RoomLayerType.GroundLayer4,
                "ClifTilemapLevel3" => RoomLayerType.GroundLayer3,
                "ClifTilemapLevel2" => RoomLayerType.GroundLayer2,
                "ClifTilemapLevel1" => RoomLayerType.GroundLayer1,

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