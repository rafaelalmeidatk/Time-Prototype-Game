using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.Tiled;
using TimePrototype.Components;
using TimePrototype.Components.Battle;
using TimePrototype.Components.GraphicComponents;
using TimePrototype.Components.Map;
using TimePrototype.Components.Player;
using TimePrototype.Components.Windows;
using TimePrototype.Managers;
using TimePrototype.NPCs;
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

        public const int PLAYER_LAYER = 1;
        public const int ENEMY_LAYER = 2;
        public const int PROJECTILES_LAYER = 3;
        public const int BUSHES_LAYER = 4;

        //--------------------------------------------------
        // Tags

        public const int PROJECTILES = 1;
        public const int BUSHES = 2;

        //--------------------------------------------------
        // Map

        private TiledMapComponent _tiledMapComponent;
        private TiledMap _tiledMap;

        //--------------------------------------------------
        // Paths

        private MapPath[] _paths;

        //----------------------//------------------------//

        public override void initialize()
        {
            addRenderer(new DefaultRenderer());
            setupMap();
            setupPlayer();
            setupPaths();
            setupEnemies();
            setupEntityProcessors();
            setupNpcs();
            setupBushes();
            setupTransfers();
        }

        private void setupMap()
        {
            var sysManager = Core.getGlobalManager<SystemManager>();
            var mapId = sysManager.MapId;
            _tiledMap = content.Load<TiledMap>(string.Format("maps/map{0}", mapId));
            sysManager.setTiledMapComponent(_tiledMap);

            var tiledEntity = createEntity("tiled-map");
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
            var sysManager = Core.getGlobalManager<SystemManager>();

            var collisionLayer = _tiledMap.properties["collisionLayer"];
            Vector2? playerSpawn;

            if (sysManager.SpawnPosition.HasValue)
            {
                playerSpawn = sysManager.SpawnPosition;
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

            player.addComponent(new PlatformerObject(_tiledMap));
            player.addComponent<TextWindowComponent>();

            var tail = player.addComponent(new TimedSpriteTail() { fadeDuration = 1f});
            tail.renderLayer = MISC_RENDER_LAYER;

            var playerComponent = player.addComponent<PlayerComponent>();
            playerComponent.sprite.renderLayer = PLAYER_RENDER_LAYER;

            var box = createEntity("box");
            box.transform.position = playerSpawn.Value + 100 * Vector2.UnitX;
            box.addComponent(new TiledMapMover(_tiledMap.getLayer<TiledTileLayer>(collisionLayer)));
            box.addComponent(new BoxCollider(-16f, -16f, 32f, 32f));
            box.addComponent(new PlatformerObject(_tiledMap));
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
            addEntityProcessor(new HitscanSystem());
            addEntityProcessor(new BushSystem(findEntity("player").getComponent<PlayerComponent>()));

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

                var npcEntity = createEntity(string.Format("{0}:{1}", npc.name, names[npc.name]));
                var npcComponent = (NpcBase)Activator.CreateInstance(Type.GetType("TimePrototype.NPCs." + npc.type), npc.name);
                npcComponent.setRenderLayer(MISC_RENDER_LAYER);
                npcComponent.ObjectRect = new Rectangle(0, 0, npc.width, npc.height);
                npcEntity.addComponent(npcComponent);
                npcEntity.addComponent<TextWindowComponent>();
                npcEntity.addComponent(new TiledMapMover(_tiledMap.getLayer<TiledTileLayer>(collisionLayer)));

                if (!npcComponent.Invisible)
                {
                    npcEntity.position = npc.position + new Vector2(npc.width, npc.height) / 2;
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
    }
}
