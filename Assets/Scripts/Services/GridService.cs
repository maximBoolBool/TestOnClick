using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface IGridService
    {
        Vector3Int ToGridCordinates(Unit unit);
        Vector3Int ToGridCordinates(Vector3 position);
        Vector3Int[] ToGridCordinates(Vector3[] positions);
        Vector3[] FromGridCordinates(Vector3Int[] positions);
        Vector3 FromGridCordinates(Vector3Int position);
    }

    public class GridService : IGridService
    {
        [Inject(Id = Constants.GroundTilemap)]
        private readonly Tilemap _groundTilemap;

        public Vector3Int ToGridCordinates(Unit unit)
        {
            return ToGridCordinates(unit.transform.position);
        }

        public Vector3Int ToGridCordinates(Vector3 position)
        {
            return ToGridCordinates(new Vector3[] { position }).First();
        }

        public Vector3Int[] ToGridCordinates(Vector3[] positions)
        {
            return positions.Select(p => _groundTilemap.WorldToCell(p)).ToArray();
        }

        public Vector3[] FromGridCordinates(Vector3Int[] positions)
        {
            return positions.Select(p => _groundTilemap.GetCellCenterWorld(p)).ToArray();
        }

        public Vector3 FromGridCordinates(Vector3Int position)
        {
            return FromGridCordinates(new Vector3Int[] { position }).First();
        }
    }
}