using Assets.Db;
using Assets.Scripts.Behaviours;
using Assets.Scripts.Factory;
using Assets.Scripts.Managers;
using Assets.Scripts.Managers.UnitManager;
using Assets.Scripts.Services;
using Assets.Scripts.Services.BotStrategy;
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
        private Tilemap _highlighTilemap;

        [SerializeField]
        private GameObject _healthBarSliderGO;

        [SerializeField]
        private TextMeshProUGUI _healthBarText;

        [SerializeField]
        private GameObject _actionPointsBarSliderGO;

        [SerializeField]
        private TextMeshProUGUI _actionPointsBarText;

        [SerializeField]
        private GameObject _actionButtonPanel;

        [SerializeField]
        private TextMeshProUGUI _moveCounterText;

        [SerializeField]
        private GameObject _enemyPanelInfo;

        [SerializeField]
        private GameObject _equipmentPanel;

        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private GameObject _topCloudVeil;

        [SerializeField]
        private GameObject _bottomCloudVeil;

        [SerializeField]
        private GameObject _leftCloudVeil;

        [SerializeField]
        private GameObject _rightCloudVeil;

        [SerializeField]
        private Grid _grid;

        [SerializeField]
        private GameObject _unitInfoPanelIcon;

        [SerializeField]
        private GameObject _unitQueuePanel;

        [SerializeField]
        private GameObject _unitsGO;

        [SerializeField]
        private GameObject _interactiveItemsGO;

        public override void InstallBindings()
        {
            Container.Bind<GameObject>()
                .WithId(Constants.INTERACTIVE_ITEMS_GO_NAME)
                .FromInstance(_interactiveItemsGO)
                .AsCached()
                .NonLazy();

            Container.Bind<GameObject>()
                .WithId(Constants.UNITS_GO_NAME)
                .FromInstance(_unitsGO)
                .AsCached()
                .NonLazy();

            Container.Bind<GameObject>()
                .WithId(Constants.UNIT_QUEUE_PANEL)
                .FromInstance(_unitQueuePanel)
                .AsCached()
                .NonLazy();

            Container.Bind<GameObject>()
                .WithId(Constants.UNIT_INFORMATION_PANEL_ICON)
                .FromInstance(_unitInfoPanelIcon)
                .AsCached()
                .NonLazy();

            Container.Bind<IGridLayersManager>()
                .To<GridLayersManager>()
                .AsSingle()
                .NonLazy();

            Container.Bind<Grid>()
                .WithId(Constants.GRID)
                .FromInstance(_grid)
                .AsCached()
                .NonLazy();

            Container.Bind<GameObject>()
                .WithId(Constants.TOP_VEIL_CLOUD_PART)
                .FromInstance(_topCloudVeil)
                .AsCached()
                .NonLazy();

            Container.Bind<GameObject>()
                .WithId(Constants.BOTTOM_VEIL_CLOUD_PART)
                .FromInstance(_bottomCloudVeil)
                .AsCached()
                .NonLazy();

            Container.Bind<GameObject>()
                .WithId(Constants.LEFT_VEIL_CLOUD_PART)
                .FromInstance(_leftCloudVeil)
                .AsCached()
                .NonLazy();

            Container.Bind<GameObject>()
                .WithId(Constants.RIGHT_VEIL_CLOUD_PART)
                .FromInstance(_rightCloudVeil)
                .AsCached()
                .NonLazy();

            var healthBarSlider = _healthBarSliderGO.GetComponent<Slider>();
            var actionPointsBarSlider = _actionPointsBarSliderGO.GetComponent<Slider>();

            _enemyPanelInfo.SetActive(false);

            _equipmentPanel.SetActive(false);

            Container.BindInterfacesAndSelfTo<StaticDb>()
                .FromMethod(CreateStaticDb)
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<ProgressDb>()
                .FromMethod(CreateProgressDb)
                .AsSingle()
                .NonLazy();

            Container.Bind<IEquipmentUiManager>()
                .To<EquipmentUiManager>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IMovementCostService>()
                .To<MovementCostService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<ILocationService>()
                .To<Services.LocationService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IRoomSpawnService>()
                .To<RoomSpawnService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IGameGlobalStateManager>()
                .To<GameGlobalStateManager>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IRoomService>()
                .To<RoomService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<Camera>()
                .WithId(Constants.CAMERA)
                .FromInstance(_camera)
                .AsCached();

            Container.Bind<IUIAnimationService>()
                .To<UIAnimationService>()
                .AsSingle()
                .NonLazy();
            
            Container.Bind<GameObject>()
                .WithId(Constants.EQUIPMENT_SCREEN)
                .FromInstance(_equipmentPanel)
                .AsCached();

            Container.Bind<GameObject>()
                .WithId(Constants.ENEMY_INFO_PANEL)
                .FromInstance(_enemyPanelInfo)
                .AsCached();

            Container.Bind<TextMeshProUGUI>()
                .WithId(Constants.TURN_COUNT_TEXT)
                .FromInstance(_moveCounterText)
                .AsCached();

            Container.Bind<Slider>()
                .WithId(Constants.HEALTH_BAR_SLIDER)
                .FromInstance(healthBarSlider)
                .AsCached();

            Container.Bind<Slider>()
                .WithId(Constants.ACTION_POINTS_BAR_SLIDER)
                .FromInstance(actionPointsBarSlider)
                .AsCached();

            Container.Bind<TextMeshProUGUI>()
                .WithId(Constants.ACTION_POINTS_BAR_TEXT)
                .FromInstance(_actionPointsBarText)
                .AsCached();

            Container.Bind<Tilemap>()
                .WithId(Constants.HIGHLIGHT_TILEMAP)
                .FromInstance(_highlighTilemap)
                .AsCached();

            Container.Bind<GameObject>()
                .WithId(Constants.ACTION_BUTTON_PANEL)
                .FromInstance(_actionButtonPanel)
                .AsCached();

            Container.Bind<TextMeshProUGUI>()
                .WithId(Constants.HEALTH_BAR_TEXT)
                .FromInstance(_healthBarText)
                .AsCached();

            Container.BindFactory<Unit, UnitFactory>()
                     .FromComponentInNewPrefab(_unitPrefab.gameObject);

            Container.Bind<ICameraService>()
                .To<CameraService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IUnitPanelBarService>()
                .To<UnitPanelBarService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IGridService>()
                .To<GridService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IGridLayerService>()
                .To<GridLayerService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IUnitManager>()
                .To<UnitManager>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IAddresableResourceManager>()
                .To<AddresableResourceManager>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IUnitQueueUiService>()
                .To<UnitQueueUiManager>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IPathFinderService>()
                .To<PathFinderService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IMoveService>()
                .To<MoveService>()
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

            Container.Bind<IUnitConditionService>()
                .To<UnitConditionService>()
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

            Container.Bind<ISharedBotStrategyService>()
                .To<SharedBotStrategyService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IBotExecutionTurnService>()
                .To<BotExecutionTurnService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IAggressiveStrategyBotService>()
                .To<AggressiveStrategyBotService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IDefensiveStrategyBotService>()
                .To<DefensiveStrategyBotService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<ISupportStrategyService>()
                .To<SupportStrategyService>()
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

            Container.Bind<IGameSceneStartManager>()
                .To<GameSceneStartManager>()
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