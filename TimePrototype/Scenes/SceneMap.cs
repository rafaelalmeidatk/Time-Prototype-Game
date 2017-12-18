using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using TimePrototype.Components;
using TimePrototype.Components.Battle;
using TimePrototype.Components.Battle.Traps;
using TimePrototype.Components.GraphicComponents;
using TimePrototype.Components.Map;
using TimePrototype.Components.Player;
using TimePrototype.Components.Windows;
using TimePrototype.Managers;
using TimePrototype.NPCs;
using TimePrototype.Scenes.SceneMapExtensions;
using TimePrototype.Structs;
using TimePrototype.Systems;

namespace TimePrototype.Scenes
{
    class SceneMap : Scene
    {
        //--------------------------------------------------
        // Render Layers Constants

        public const int BACKGROUND_RENDER_LAYER = 10;
        public const int TILED_MAP_RENDER_LAYER = 9;
        public const int WATER_RENDER_LAYER = 6;
        public const int MISC_RENDER_LAYER = 5;
        public const int ENEMIES_RENDER_LAYER = 4;
        public const int PLAYER_RENDER_LAYER = 3;
        public const int PARTICLES_RENDER_LAYER = 2;
        public const int HUD_BACK_RENDER_LAYER = 1;
        public const int HUD_FILL_RENDER_LAYER = 0;

        //--------------------------------------------------
        // Layer Masks

        public const int MAP_LAYER = 0;
        public const int PLAYER_LAYER = 1;
        public const int ENEMY_LAYER = 2;
        public const int PROJECTILES_LAYER = 3;
        public const int BUSHES_LAYER = 4;

        //--------------------------------------------------
        // Tags

        public const int PROJECTILES = 1;
        public const int BUSHES = 2;
        public const int TRAPS = 3;

        //--------------------------------------------------
        // Map

        private TiledMapComponent _tiledMapComponent;
        private TiledMap _tiledMap;

        //--------------------------------------------------
        // Map Extensions

        private List<ISceneMapExtensionable> _mapExtensions;

        //--------------------------------------------------
        // Paths

        private MapPath[] _paths;

        //--------------------------------------------------
        // Player

        private PlayerComponent _playerComponent;
        public bool _lastSlowmotionState;
        public SoundEffectInstance _slowmotionSeInstance;

        //--------------------------------------------------
        // Heat and Vignette post processors

        private HeatDistortionPostProcessor _heatDistortionPostProcessor;
        private VignettePostProcessor _vignettePostProcessor;

        //----------------------//------------------------//

        public override void initialize()
        {
            addRenderer(new DefaultRenderer());
            clearColor = new Color(54, 72, 130);
            setupMap();
            setupPlayer();
            setupPaths();
            setupEnemies();
            setupTraps();
            setupNpcs();
            setupBushes();
            setupDoors();
            setupKeys();
            setupMapExtensions();
            setupPostProcessors();
            setupTransfers();
        }

        public override void onStart()
        {
            setupEntityProcessors();
            getEntityProcessor<NpcInteractionSystem>().mapStart();
            Core.getGlobalManager<InputManager>().IsLocked = false;

            _slowmotionSeInstance = AudioManager.slowmotion.CreateInstance();
            _slowmotionSeInstance.IsLooped = true;
            MediaPlayer.Play(AudioManager.malicious);
        }

        private void setupMap()
        {
            var sysManager = Core.getGlobalManager<SystemManager>();
            var mapId = sysManager.MapId;
            _tiledMap = content.Load<TiledMap>($"maps/map{mapId}");
            sysManager.setTiledMapComponent(_tiledMap);

            var tiledEntity = createEntity("tiledMap");
            var collisionLayer = _tiledMap.properties["collisionLayer"];
            var defaultLayers = _tiledMap.properties["defaultLayers"].Split(',').Select(s => s.Trim()).ToArray();

            _tiledMapComponent = tiledEntity.addComponent(new TiledMapComponent(_tiledMap, collisionLayer) { renderLayer = TILED_MAP_RENDER_LAYER });
            _tiledMapComponent.setLayersToRender(defaultLayers);

            if (_tiledMap.properties.ContainsKey("aboveWaterLayer"))
            {
                var aboveWaterLayer = _tiledMap.properties["aboveWaterLayer"];
                var tiledAboveWater = tiledEntity.addComponent(new TiledMapComponent(_tiledMap) { renderLayer = WATER_RENDER_LAYER });
                tiledAboveWater.setLayerToRender(aboveWaterLayer);
            }
        }

        private void setupPlayer()
        {
            var systemManager = Core.getGlobalManager<SystemManager>();

            var collisionLayer = _tiledMap.properties["collisionLayer"];
            Vector2? playerSpawn;

            if (systemManager.SpawnPosition.HasValue)
            {
                playerSpawn = systemManager.SpawnPosition;
            }
            else
            {
                playerSpawn = _tiledMap.getObjectGroup("objects").objectWithName("playerSpawn").position;
            }

            var player = createEntity("player");
            player.transform.position = playerSpawn.Value;
            player.addComponent(new TiledMapMover(_tiledMap.getLayer<TiledTileLayer>(collisionLayer)));
            var collider = player.addComponent(new BoxCollider(-7f, -9f, 15f, 25f));
            Flags.setFlagExclusive(ref collider.physicsLayer, PLAYER_LAYER);
            
            // Timer

            player.addComponent(new InteractionCollider(-20f, -6, 40, 22));
            player.addComponent(new BattleComponent());
            player.addComponent(new PlatformerObject(_tiledMap));
            player.addComponent<TextWindowComponent>();

            // Distortion Sprites (Tail)

            var tail = player.addComponent(new TimedSpriteTail { fadeDuration = 0.8f});
            tail.renderLayer = MISC_RENDER_LAYER;
            tail.setEnabled(systemManager.MapId > 8);

            // Player Component

            var playerComponent = player.addComponent<PlayerComponent>();
            playerComponent.sprite.renderLayer = PLAYER_RENDER_LAYER;
            
            // Timer
            createEntity("timer")
                .addComponent(new TimerComponent(player))
                .setVisible(false);

            // Distortion cursor
            createEntity("distortionCursor")
                .addComponent(new Sprite(content.Load<Texture2D>(Content.Misc.distortion_cursor)))
                .renderLayer = MISC_RENDER_LAYER;

            // Set player
            _playerComponent = playerComponent;
            systemManager.setPlayer(player);
        }
        
        private void setupPaths()
        {
            var pathObjectGroup = _tiledMap.getObjectGroup("paths");
            if (pathObjectGroup == null) return;

            var objects = pathObjectGroup.objects;

            _paths = new MapPath[objects.Length];
            for (var i = 0; i < objects.Length; i++)
            {
                _paths[i] = new MapPath
                {
                    Name = objects[i].name,
                    Start = objects[i].position,
                    End = objects[i].position + new Vector2(objects[i].width, objects[i].height)
                };
            }
        }

        private void setupEnemies()
        {
            var collisionLayer = _tiledMap.properties["collisionLayer"];

            var enemiesGroup = _tiledMap.getObjectGroup("enemies");
            if (enemiesGroup == null) return;
            foreach (var enemy in enemiesGroup.objects)
            {
                var entity = createEntity("enemy");
                entity.addComponent(new TiledMapMover(_tiledMap.getLayer<TiledTileLayer>(collisionLayer)));
                entity.addComponent(new PlatformerObject(_tiledMap));
                entity.addComponent<BattleComponent>();
                var collider = entity.addComponent(new BoxCollider(-16f, -16f, 32f, 32f));
                Flags.setFlagExclusive(ref collider.physicsLayer, ENEMY_LAYER);

                var patrolStartRight = bool.Parse(enemy.properties.ContainsKey("patrolStartRight")
                    ? enemy.properties["patrolStartRight"]
                    : "false");

                var instance = createEnemyInstance(enemy.type, patrolStartRight);
                var enemyComponent = entity.addComponent(instance);
                enemyComponent.sprite.renderLayer = ENEMIES_RENDER_LAYER;
                enemyComponent.playerCollider = findEntity("player").getComponent<BoxCollider>();

                if (enemy.properties.ContainsKey("path"))
                {
                    var pathName = enemy.properties["path"];
                    var path = _paths.First(x => x.Name == pathName);
                    enemyComponent.path = path;
                }

                entity.transform.position = enemy.position + new Vector2(enemy.width, enemy.height) / 2;
            }
        }

        private EnemyComponent createEnemyInstance(string enemyName, bool patrolStartRight)
        {
            var enemiesNamespace = typeof(BattleComponent).Namespace + ".Enemies";
            var type = Type.GetType(enemiesNamespace + "." + enemyName + "Component");
            if (type != null)
            {
                return Activator.CreateInstance(type, new object[] { patrolStartRight }) as EnemyComponent;
            }
            return null;
        }

        private void setupTraps()
        {
            var trapsObjectGroup = _tiledMap.getObjectGroup("traps");
            if (trapsObjectGroup == null) return;
            foreach (var trap in trapsObjectGroup.objects)
            {
                var entity = createEntity("trap");
                switch (trap.type)
                {
                    case "activator":
                        entity
                            .addComponent(new TrapActivatorComponent(trap.name))
                            .addComponent(new BoxCollider(-8, -4, 16, 5))
                            .addComponent(new Sprite(content.Load<Texture2D>(Content.Misc.activator)))
                            .transform.position = -1 * Vector2.UnitY;
                        break;
                    case "projectile":
                        var rotation = Mathf.deg2Rad * trap.rotation;
                        var activator = trap.properties.ContainsKey("activator") ? trap.properties["activator"] : "";
                        var isAuto = trap.properties.ContainsKey("auto") && bool.Parse(trap.properties["auto"]);
                        var delay = trap.properties.ContainsKey("delay") ? float.Parse(trap.properties["delay"]) : 0.0f;
                        var initialDelay = trap.properties.ContainsKey("initialDelay")
                            ? float.Parse(trap.properties["initialDelay"])
                            : 0.0f;
                        entity.addComponent(new ProjectileTrapComponent(activator, isAuto, delay, initialDelay, 0, rotation));
                        break;
                }
                var rad = Mathf.deg2Rad * trap.rotation;
                var center = new Vector2(trap.width, trap.height) / 2;
                var rot = new Vector2(Mathf.cos(rad), Mathf.sin(rad));
                var rotCenter = new Vector2(center.X * rot.X - center.Y * rot.Y,
                    center.X * rot.X + center.Y * rot.Y);
                entity.transform.position = entity.position + trap.position + rotCenter;
            }
        }

        private void setupEntityProcessors()
        {
            var player = findEntity("player");
            var playerComponent = player.getComponent<PlayerComponent>();

            addEntityProcessor(new CameraSystem(player)
            {
                mapLockEnabled = true,
                mapSize = new Vector2(_tiledMap.widthInPixels, _tiledMap.heightInPixels),
                followLerp = 0.08f,
                deadzoneSize = new Vector2(20, 10)
            });

            addEntityProcessor(new BattleSystem());
            addEntityProcessor(new ProjectilesSystem(player));
            addEntityProcessor(new HitscanSystem());
            addEntityProcessor(new BushSystem(playerComponent));
            addEntityProcessor(new DoorSystem(findEntity("door"), player));
            var key = findEntity("key");
            addEntityProcessor(new KeySystem(key, player));
            addEntityProcessor(new ActivatorsSystem());

            addEntityProcessor(new TransferSystem(new Matcher().all(typeof(TransferComponent)), player));
            addEntityProcessor(new NpcInteractionSystem(playerComponent));
        }

        private void setupNpcs()
        {
            var npcObjects = _tiledMap.getObjectGroup("npcs");
            if (npcObjects == null) return;

            var collisionLayer = _tiledMap.properties["collisionLayer"];
            var names = new Dictionary<string, int>();
            foreach (var npc in npcObjects.objects)
            {
                names[npc.name] = names.ContainsKey(npc.name) ? ++names[npc.name] : 0;

                var npcEntity = createEntity($"{npc.name}:{names[npc.name]}");
                var npcComponent = (NpcBase)Activator.CreateInstance(Type.GetType("TimePrototype.NPCs." + npc.type), npc.name);
                npcComponent.setRenderLayer(MISC_RENDER_LAYER);
                npcComponent.ObjectRect = new Rectangle(0, 0, npc.width, npc.height);
                npcEntity.addComponent(npcComponent);
                npcEntity.addComponent<TextWindowComponent>();
                npcEntity.addComponent(new TiledMapMover(_tiledMap.getLayer<TiledTileLayer>(collisionLayer)));
                npcEntity.position = npc.position;

                if (!npcComponent.Invisible)
                {
                    npcEntity.addComponent(new PlatformerObject(_tiledMap));
                }

                // Props
                if (npc.properties.ContainsKey("flipX") && npc.properties["flipX"] == "true")
                {
                    npcComponent.FlipX = true;
                }
                if (npc.properties.ContainsKey("autorun") && npc.properties["autorun"] == "true")
                {
                    getEntityProcessor<NpcInteractionSystem>().addAutorun(npcComponent);
                }
            }
        }

        private void setupBushes()
        {
            var bushesObjects = _tiledMap.getObjectGroup("bushes");
            if (bushesObjects == null) return;
            
            foreach (var bushObj in bushesObjects.objects)
            {
                var entity = createEntity("bush");
                entity
                    .addComponent(new BoxCollider(-15, -12, 30, 20))
                    .addComponent(new Sprite(content.Load<Texture2D>(Content.Misc.bush)))
                    .addComponent(new BushComponent());

                entity.setPosition(bushObj.position + new Vector2(bushObj.width, bushObj.height) / 2 + 7 * Vector2.UnitY);
            }
        }

        private void setupDoors()
        {
            var objectGroup = _tiledMap.getObjectGroup("objects");
            var doorObj = objectGroup?.objectsWithType("door");
            if (doorObj?.Count == 0) return;

            var entity = createEntity("door");
            entity
                .addComponent(new BoxCollider(-16, -32, 32, 64))
                .addComponent(new Sprite(content.Load<Texture2D>(Content.Misc.door)));

            entity.setPosition(doorObj[0].position + new Vector2(doorObj[0].width, doorObj[0].height) / 2);
        }

        private void setupKeys()
        {
            var objectGroup = _tiledMap.getObjectGroup("objects");
            var keyObj = objectGroup?.objectsWithType("key");
            if (keyObj?.Count == 0) return;

            var entity = createEntity("key");
            entity
                .addComponent(new BoxCollider(-6, -3, 12, 6))
                .addComponent(new Sprite(content.Load<Texture2D>(Content.Misc.key)));

            entity.setPosition(keyObj[0].position + new Vector2(keyObj[0].width, keyObj[0].height) / 2);
        }

        private void setupMapExtensions()
        {
            _mapExtensions = new List<ISceneMapExtensionable>();

            if (!_tiledMap.properties.ContainsKey("mapExtensions")) return;

            var extensions = _tiledMap.properties["mapExtensions"].Split(',').Select(s => s.Trim()).ToArray();

            foreach (var extension in extensions)
            {
                var extensionInstance = (ISceneMapExtensionable)Activator.CreateInstance(Type.GetType("TimePrototype.Scenes.SceneMapExtensions." + extension));
                extensionInstance.Scene = this;
                extensionInstance.initialize();
                _mapExtensions.Add(extensionInstance);
            }
        }

        private void setupPostProcessors()
        {
            var bloom = addPostProcessor(new BloomPostProcessor(2));
            bloom.setBloomSettings(new BloomSettings(0.5f, 0.4f, 0.6f, 1, 1, 1));

            var scanlines = addPostProcessor(new ScanlinesPostProcessor(1));
            scanlines.effect.attenuation = 0.04f;
            scanlines.effect.linesFactor = 1500f;

            _heatDistortionPostProcessor = addPostProcessor(new HeatDistortionPostProcessor(5));
            _heatDistortionPostProcessor.enabled = false;
            _vignettePostProcessor = addPostProcessor(new VignettePostProcessor(1));
            _vignettePostProcessor.enabled = false;
        }

        private void setupTransfers()
        {
            var transfers = _tiledMap.getObjectGroup("transfers");
            if (transfers == null) return;

            var names = new Dictionary<string, int>();
            foreach (var transferObj in transfers.objects)
            {
                names[transferObj.name] = names.ContainsKey(transferObj.name) ? ++names[transferObj.name] : 0;

                var entity = createEntity(string.Format("{0}:{1}", transferObj.name, names[transferObj.name]));
                entity.addComponent(new TransferComponent(transferObj));
            }
        }

        public void reserveTransfer(TransferComponent transferComponent)
        {
            Core.getGlobalManager<SystemManager>().setMapId(transferComponent.destinyId);
            Core.getGlobalManager<SystemManager>().setSpawnPosition(transferComponent.destinyPosition);
            Core.startSceneTransition(new FadeTransition(() => new SceneMap()));
        }

        public void reserveTransfer(int mapId, int mapX, int mapY)
        {
            var spawnPosition = new Vector2(mapX, mapY);
            Core.getGlobalManager<SystemManager>().setMapId(mapId);
            Core.getGlobalManager<SystemManager>().setSpawnPosition(spawnPosition);
            Core.startSceneTransition(new FadeTransition(() => new SceneMap()));
        }

        public void sendMessageToExtensions(string message)
        {
            foreach (var extension in _mapExtensions)
            {
                extension.receiveSceneMessage(message);
            }
        }

        public override void update()
        {
            base.update();

            // Update post processors
            if (_playerComponent.isSlowingdownTheTime != _lastSlowmotionState)
            {
                _lastSlowmotionState = _playerComponent.isSlowingdownTheTime;
                if (_lastSlowmotionState)
                {
                    _slowmotionSeInstance.Volume = 1.0f;
                    _slowmotionSeInstance.Play();
                    MediaPlayer.Volume = 0.7f;
                }
                else
                {
                    _slowmotionSeInstance.Volume = 0.0f;
                    _slowmotionSeInstance.Stop();
                    MediaPlayer.Volume = 1.0f;
                }
            }
            _heatDistortionPostProcessor.enabled = _playerComponent.isSlowingdownTheTime;
            //_vignettePostProcessor.enabled = _playerComponent.isSlowingdownTheTime;

            // Update extensions
            _mapExtensions.ForEach(extension => extension.update());
        }
    }
}
