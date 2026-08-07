using System;

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

        public static RoomLayerType[] GetLayerWalkableToCheck(this RoomLayerType layer)
        {
            return layer switch
            {
                RoomLayerType.GroundLayer1 => new[] {RoomLayerType.GroundLayer2, RoomLayerType.CliffLayer2},
                RoomLayerType.GroundLayer2 => new[] {RoomLayerType.GroundLayer3, RoomLayerType.CliffLayer3},
                RoomLayerType.GroundLayer3 => new[] {RoomLayerType.GroundLayer4, RoomLayerType.CliffLayer4},
                RoomLayerType.GroundLayer4 => Array.Empty<RoomLayerType>(),

                RoomLayerType.WaterTilemap => Array.Empty<RoomLayerType>(),
                RoomLayerType.BaseWaterLayer => Array.Empty<RoomLayerType>(),

                RoomLayerType.CliffLayer4 => throw new NotImplementedException(),
                RoomLayerType.CliffLayer3 => throw new NotImplementedException(),
                RoomLayerType.CliffLayer2 => throw new NotImplementedException(),
                RoomLayerType.CliffLayer1 => throw new NotImplementedException(),

                _ => throw new NotImplementedException(),
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