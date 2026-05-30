using Unity.Mathematics;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Services
{
    public interface ICameraService
    {
        void SetCameraCordinates(Vector2 vector2);

        void SetCameraCordinates(float2 cordinates);

        void MoveCamera(float2 cordinates);

        void MoveCamera(Vector2 vector);
    }

    public class CameraService : ICameraService
    {
        private const float ABS_X_MAX_VALUE = 10;
        private const float ABS_Y_MAX_VALUE = 10;

        private const float MOVE_DELTA = 0.00002f;

        [Inject(Id = Constants.Camera)]
        private readonly Camera _camera;

        #region Public methods

        public void SetCameraCordinates(Vector2 cordinates)
        {
            var normalizedCordinates = GetNormalizedValue(cordinates.x, cordinates.y);
            _camera.transform.position = new Vector3(normalizedCordinates.x, normalizedCordinates.y, _camera.transform.position.z);
        }

        public void SetCameraCordinates(float2 cordinates)
        {
            var normalizedCordinates = GetNormalizedValue(cordinates.x, cordinates.y);
            _camera.transform.position = new Vector3(normalizedCordinates.x, normalizedCordinates.y, _camera.transform.position.z);
        }

        public void MoveCamera(float2 cordinates)
        {
            var normalizedCordinates = GetNormalizedValue(cordinates.x, cordinates.y);

            while (true)
            {
                var currentPosition = new float2(_camera.transform.position.x, _camera.transform.position.y);
                var direction = normalizedCordinates - currentPosition;
                var distance = math.length(direction);

                if (distance <= MOVE_DELTA)
                {
                    _camera.transform.position = new Vector3(normalizedCordinates.x, normalizedCordinates.y, _camera.transform.position.z);
                    break;
                }

                var normalizedDirection = math.normalize(direction);
                var newPosition = currentPosition + normalizedDirection * MOVE_DELTA;
                _camera.transform.position = new Vector3(newPosition.x, newPosition.y, _camera.transform.position.z);
            }
        }

        public void MoveCamera(Vector2 vector)
        {
            float2 cordinates = new()
            {
                x = vector.x,
                y = vector.y
            };

            MoveCamera(cordinates);
        }

        #endregion

        #region Private methods

        private static float2 GetNormalizedValue(float x, float y)
        {
            float2 normalized = new()
            {
                x = math.clamp(x, -ABS_X_MAX_VALUE, ABS_X_MAX_VALUE),
                y = math.clamp(y, -ABS_Y_MAX_VALUE, ABS_Y_MAX_VALUE)
            };

            return normalized;
        }

        #endregion
    }
}