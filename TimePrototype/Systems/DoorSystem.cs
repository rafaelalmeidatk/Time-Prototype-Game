using Nez;
using System.Collections.Generic;
using TimePrototype.Components.Player;

namespace TimePrototype.Systems
{
    public class DoorSystem : ProcessingSystem
    {
        private bool _enabled;

        private readonly Entity _door;
        private readonly Entity _player;

        public DoorSystem(Entity door, Entity player)
        {
            if (door == null) return;

            _door = door;
            _player = player;
            _enabled = true;
        }

        public override void process() { }

        protected override void lateProcess(List<Entity> entities)
        {
            if (!_enabled || _player == null) return;

            CollisionResult collisionResult;

            if (_door.getComponent<BoxCollider>()
                .collidesWith(_player.getComponent<BoxCollider>(), out collisionResult))
            {
                var playerComponent = _player.getComponent<PlayerComponent>();
                if (playerComponent.isWithKey)
                {
                    playerComponent.isWithKey = false;
                    _door.destroy();
                    _enabled = false;
                    return;
                }
                _player.transform.position += collisionResult.minimumTranslationVector;
            }
        }
    }
}
