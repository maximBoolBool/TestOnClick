using Assets.Db;
using Assets.Scripts.Behaviours;
using Assets.Scripts.Factory;
using Assets.Scripts.Managers;
using Assets.Scripts.Services;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Zenject;

namespace Assets.Scripts
{
    public class LoggerMonoInstaller : MonoInstaller
    {
        [SerializeField] 
        private Unit _unitPrefab;

        [SerializeField]
        private Tilemap _groundTilemap;

        [SerializeField]
        private Tilemap _highlighTilemap;

        [SerializeField]
        private TileBase _highlightTile;

        [SerializeField]
        private TileBase _hoverTile;

        [SerializeField]
        private GameObject _slidersGameObject;

        [SerializeField]
        private GameObject _actionButtonPanel;

        [SerializeField]
        private GameObject _actionButtonPrefab;

        [SerializeField]
        private TextMeshProUGUI _healthBarText;

        [SerializeField]
        private TextMeshProUGUI _actionPointText;

        [SerializeField]
        private TextMeshProUGUI _moveCounterText;

        [SerializeField]
        private GameObject _enemyPanelInfo;

        [SerializeField]
        private GameObject _equipmentPanel;

        [SerializeField]
        private GameObject _equipmentSlotPrefab;

        [SerializeField]
        private AnimatorOverrideController _redWarriorAnimatorController;

        [SerializeField]
        private AnimatorOverrideController _blueWarriorAnimationController;

        [SerializeField]
        private AnimatorOverrideController _blueMonkAnimationController;

        [SerializeField]
        private Camera _camera;

        public override void InstallBindings()
        {
            var slider = _slidersGameObject.GetComponent<Slider>();

            _enemyPanelInfo.SetActive(false);

            _equipmentPanel.SetActive(false);

            Container.Bind<StaticDb>()
                .FromMethod(CreateStaticDb)
                .AsSingle()
                .NonLazy();

            Container.Bind<ProgressDb>()
                .FromMethod(CreateProgressDb)
                .AsSingle()
                .NonLazy();

            Container.Bind<Camera>()
                .WithId(Constants.Camera)
                .FromInstance(_camera)
                .AsCached();

            Container.Bind<AnimatorOverrideController>()
                .WithId(Constants.BlueMonkAnimatorController)
                .FromInstance(_blueMonkAnimationController)
                .AsCached();

            Container.Bind<AnimatorOverrideController>()
                .WithId(Constants.BlueWarriorAnimatorController)
                .FromInstance(_blueWarriorAnimationController)
                .AsCached();

            Container.Bind<AnimatorOverrideController>()
                .WithId(Constants.RedWarriorAnimatorController)
                .FromInstance(_redWarriorAnimatorController)
                .AsCached();

            Container.Bind<GameObject>()
                .WithId(Constants.EquipemntSlotPrefab)
                .FromInstance(_equipmentSlotPrefab)
                .AsCached();

            Container.Bind<GameObject>()
                .WithId(Constants.EquipemntPanel)
                .FromInstance(_equipmentPanel)
                .AsCached();

            Container.Bind<GameObject>()
                .WithId(Constants.EnemyInfoPanel)
                .FromInstance(_enemyPanelInfo)
                .AsCached();

            Container.Bind<TextMeshProUGUI>()
                .WithId(Constants.TurnCountText)
                .FromInstance(_moveCounterText)
                .AsCached();

            Container.Bind<TextMeshProUGUI>()
                .WithId(Constants.ActionPointText)
                .FromInstance(_actionPointText)
                .AsCached();

            Container.Bind<Slider>()
                .WithId(Constants.HealthBarSlider)
                .FromInstance(slider)
                .AsSingle();

            Container.Bind<Tilemap>()
                .WithId(Constants.GroundTilemap)
                .FromInstance(_groundTilemap)
                .AsCached();

            Container.Bind<Tilemap>()
                .WithId(Constants.HighlightTilemap)
                .FromInstance(_highlighTilemap)
                .AsCached();

            Container.Bind<TileBase>()
                .WithId(Constants.HighlightTile)
                .FromInstance(_highlightTile)
                .AsCached();

            Container.Bind<TileBase>()
                .WithId(Constants.HoverTile)
                .FromInstance(_hoverTile)
                .AsCached();

            Container.Bind<GameObject>()
                .WithId(Constants.ActionButtonPanel)
                .FromInstance(_actionButtonPanel)
                .AsCached();

            Container.Bind<GameObject>()
                .WithId(Constants.ActionButtonPrefab)
                .FromInstance(_actionButtonPrefab)
                .AsCached();

            Container.Bind<TextMeshProUGUI>()
                .WithId(Constants.HealthBarText)
                .FromInstance(_healthBarText)
                .AsCached();

            Container.BindFactory<Unit, UnitFactory>()
                     .FromComponentInNewPrefab(_unitPrefab.gameObject);

            Container.Bind<ICameraService>()
                .To<CameraService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IHealthBarService>()
                .To<HealthBarService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IGameGlobalStateManager>()
                .To<GameGlobalStateManager>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IGridService>()
                .To<GridService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IRoomSpawnService>()
                .To<RoomSpawnService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IUnitManager>()
                .To<UnitManager>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IDamageService>()
                .To<DamageService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IActionUIService>()
                .To<ActionUIService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IActionClickHandler>()
                .To<ActionClickHandler>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IActionCostService>()
                .To<ActionCostService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IHitService>()
                .To<HitService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IActionExecutionService>()
                .To<ActionExecutionService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IConditionService>()
                .To<ConditionService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<ITurnManager>()
                .To<TurnManager>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IHilightService>()
                .To<HiligthService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IAiTurnService>()
                .To<AiTurnService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IEnemyPanelService>()
                .To<EnemyPanelService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IUiService>()
                .To<UiService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IEquipmentService>()
                .To<EquipmentService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IAnimationService>()
                .To<AnimationService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IRoomLoaderService>()
                .To<RoomLoaderService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<CameraInputController>().AsSingle();
        }

        private ProgressDb CreateProgressDb(InjectContext context)
        {
            const string dbName = "progress.db";

            var persistentPath = Path.Combine(Application.persistentDataPath, dbName);
            var isNeedInittables = !File.Exists(persistentPath);
            var db = new ProgressDb(persistentPath);

            if (isNeedInittables)
            {
                db.InitTables();
            }

            return db;
        }

        private StaticDb CreateStaticDb(InjectContext context)
        {
            const string dbName = "game_data.db";
            string persistentPath = Path.Combine(Application.persistentDataPath, dbName);
            string streamingPath = Path.Combine(Application.streamingAssetsPath, dbName);

            if (!File.Exists(persistentPath))
            {
                Debug.Log($"[Db] Файл не найден в PersistentData. Ищу в StreamingAssets: {streamingPath}");
                if (File.Exists(streamingPath))
                {
                    File.Copy(streamingPath, persistentPath);
                    Debug.Log("[Db] База успешно скопирована.");
                }
            }

            return new StaticDb(persistentPath);
        }
    }
}