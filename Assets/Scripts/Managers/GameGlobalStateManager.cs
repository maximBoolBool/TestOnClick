using Assets.Scripts.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Managers
{
    public interface IGameGlobalStateManager
    {
        Unit? SelectedUnit { get; set; }

        int ActualRoomId { get; set; }

        int ActualWaveOrder { get; set; }

        void SwitchGameStatus(GameStatus gameStatus);

        GameStatus GameStatus { get; }

        void InitignoreCordinates();

        Vector3Int[] GetIgnoreCordinatestoLayer(RoomLayerType layer);
    }

    public class GameGlobalStateManager : IGameGlobalStateManager
    {
        private GameStatus _gameStatus = GameStatus.Non;

        private readonly Dictionary<RoomLayerType, Vector3Int[]> _cordinatesToIgnoreSpiteRoom = new();

        [Inject]
        private readonly IGridLayersManager _gridLayersManager;

        public Unit SelectedUnit { get; set; }

        public GameStatus GameStatus => _gameStatus;

        public int ActualRoomId { get; set; }

        public int ActualWaveOrder { get; set; }

        public void SwitchGameStatus(GameStatus gameStatus)
        {
            _gameStatus = gameStatus;
        }

        public void InitignoreCordinates()
        {
            var groundLayers = new[] 
            {
                RoomLayerType.GroundLayer1,
                RoomLayerType.GroundLayer2,
                RoomLayerType.GroundLayer3,
                RoomLayerType.GroundLayer4
            };

            foreach (var layer in groundLayers)
            {
                _cordinatesToIgnoreSpiteRoom[layer] = _gridLayersManager.GetCordinatesToIgnoreSpriteRool(layer);
            }
        }

        public Vector3Int[] GetIgnoreCordinatestoLayer(RoomLayerType layer)
        {
            return _cordinatesToIgnoreSpiteRoom.TryGetValue(layer, out var cordinates)
                ? cordinates
                : Array.Empty<Vector3Int>();
        }
    }

    public enum GameStatus
    {
        ActiveTurn = 0,
        ReactiveTurn = 1,
        Loading = 2,
        Looting = 3,
        Deployment = 4,
        Non = 5
    }
}