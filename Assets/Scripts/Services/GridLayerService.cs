using Assets.Scripts.Enums;
using Assets.Scripts.Managers;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IGridLayerService
    {
        Vector3Int GetRoomCordinateFromGridCordinate(Vector3Int cordinate);

        Vector3Int GetRoomCordinateFromGlobalCordinate(Vector3 cordinate);
    }

    public class GridLayerService : IGridLayerService
    {
        [Inject]
        private readonly IGridService _gridService;

        [Inject]
        private readonly IGridLayersManager _gridLayersManager;

        //Порядоек важен
        private static readonly RoomLayerType[] _roomLayerTypes = new RoomLayerType[]
        {
            RoomLayerType.GroundLayer1,
            RoomLayerType.GroundLayer2,
            RoomLayerType.GroundLayer3,
            RoomLayerType.GroundLayer4
        };

        public Vector3Int GetRoomCordinateFromGlobalCordinate(Vector3 cordinate)
        {
            return GetRoomCordinateFromGridCordinate(_gridService.ToGridCordinates(cordinate));
        }

        public Vector3Int GetRoomCordinateFromGridCordinate(Vector3Int cordinate)
        {
            var hightGap = 0;
            var i = 0;

            while (true)
            {
                var nextLayer = _roomLayerTypes[i].GetNextLayer();

                if(nextLayer == null)
                {
                    break;
                }

                var hasTile = _gridLayersManager.HasTileOnLayer(new Vector3Int(cordinate.x, cordinate.y + hightGap, cordinate.z), nextLayer.Value);

                if (!hasTile)
                {
                    break;
                }

                hightGap++;
                i++;
            }

            return new Vector3Int(cordinate.x, cordinate.y + hightGap, cordinate.z);
        }
    }
}
