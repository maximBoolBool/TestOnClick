using Unity.Mathematics;
using UnityEngine;
using Zenject;
using DG.Tweening;

namespace Assets.Scripts.Services
{
    public interface ICameraService
    {
        void SetCameraCordinates(Vector2 vector2);

        void SetCameraCordinates(float2 cordinates);

        void MoveCamera(float2 cordinates);

        void MoveCamera(Vector2 vector);

        void ZoomCamera(float zoomDelta);
    }

    public class CameraService : ICameraService
    {
        private const float ABS_X_MAX_VALUE = 10;
        private const float ABS_Y_MAX_VALUE = 10;

        private const float MIN_ZOOM = 1f;
        private const float MAX_ZOOM = 5f;

        private const float MOVE_SPEED = 12f;
        private const float MIN_MOVE_DURATION = 0.05f;
        private const float ZOOM_DURATION = 0.08f;

        [Inject(Id = Constants.Camera)]
        private readonly Camera _camera;

        private Tweener _moveTween;
        private Tweener _zoomTween;
        private float _targetZoom;
        private bool _isTargetZoomInitialized;

        #region Public methods

        public void SetCameraCordinates(Vector2 cordinates)
        {
            if (_moveTween != null && _moveTween.IsActive())
            {
                _moveTween.Kill();
            }

            var normalizedCordinates = GetNormalizedValue(cordinates.x, cordinates.y);
            _camera.transform.position = new Vector3(normalizedCordinates.x, normalizedCordinates.y, _camera.transform.position.z);
        }

        public void SetCameraCordinates(float2 cordinates)
        {
            if (_moveTween != null && _moveTween.IsActive())
            {
                _moveTween.Kill();
            }

            var normalizedCordinates = GetNormalizedValue(cordinates.x, cordinates.y);
            _camera.transform.position = new Vector3(normalizedCordinates.x, normalizedCordinates.y, _camera.transform.position.z);
        }

        public void MoveCamera(float2 cordinates)
        {
            var normalizedCordinates = GetNormalizedValue(cordinates.x, cordinates.y);
            var targetPosition = new Vector3(normalizedCordinates.x, normalizedCordinates.y, _camera.transform.position.z);
            var currentPosition = _camera.transform.position;
            var distance = Vector2.Distance(new Vector2(currentPosition.x, currentPosition.y), new Vector2(targetPosition.x, targetPosition.y));

            if (distance <= float.Epsilon)
            {
                return;
            }

            if (_moveTween != null && _moveTween.IsActive())
            {
                _moveTween.Kill();
            }

            var duration = math.max(distance / MOVE_SPEED, MIN_MOVE_DURATION);
            _moveTween = _camera.transform.DOMove(targetPosition, duration).SetEase(Ease.OutQuad);
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

        public void ZoomCamera(float zoomDelta)
        {
            if (!_isTargetZoomInitialized)
            {
                _targetZoom = _camera.orthographicSize;
                _isTargetZoomInitialized = true;
            }

            // Минус, чтобы прокрутка вперед приближала, а назад — отдаляла
            _targetZoom = math.clamp(_targetZoom - zoomDelta, MIN_ZOOM, MAX_ZOOM);

            if (_zoomTween != null && _zoomTween.IsActive())
            {
                _zoomTween.ChangeEndValue(_targetZoom, true);
                return;
            }

            // Ограничиваем зум, чтобы не уйти в бесконечность или минус
            _zoomTween = _camera.DOOrthoSize(_targetZoom, ZOOM_DURATION).SetEase(Ease.OutQuad);
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