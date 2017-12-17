using Nez;
using TimePrototype.Components.Player;

namespace TimePrototype.Systems
{
    public class KeySystem : ProcessingSystem
    {
        private bool _enabled;

        private readonly Entity _key;
        private readonly Entity _player;

        public KeySystem(Entity key, Entity player)
        {
            if (key == null) return;

            _key = key;
            _player = player;
            _enabled = true;
        }

        public override void process()
        {
            if (!_enabled || _player == null) return;
            CollisionResult collisionResult;

            if (_key.getComponent<BoxCollider>()
                .collidesWith(_player.getComponent<BoxCollider>(), out collisionResult))
            {
                _player.getComponent<PlayerComponent>().isWithKey = true;
                _key.destroy();
                _enabled = false;
            }
        }
    }
}
