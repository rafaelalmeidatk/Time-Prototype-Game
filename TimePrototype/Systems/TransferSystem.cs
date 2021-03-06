﻿using System.Collections.Generic;
using Nez;
using TimePrototype.Components.Map;
using TimePrototype.Scenes;

namespace TimePrototype.Systems
{
    class TransferSystem : EntityProcessingSystem
    {
        private Entity _player;
        private bool _enabled;

        public TransferSystem(Matcher matcher, Entity player) : base(matcher)
        {
            _player = player;
            _enabled = true;
        }

        protected override void process(List<Entity> entities)
        {
            if (!_enabled) return;
            base.process(entities);
        }

        public override void process(Entity entity)
        {
            CollisionResult collisionResult;
            if (entity.getComponent<BoxCollider>().collidesWith(_player.getComponent<BoxCollider>(), out collisionResult))
            {
                var sceneMap = (SceneMap)_scene;
                sceneMap.reserveTransfer(entity.getComponent<TransferComponent>());
                _enabled = false;
            }
        }
    }
}
