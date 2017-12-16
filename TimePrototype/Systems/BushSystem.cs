using Nez;
using TimePrototype.Components.Map;
using TimePrototype.Components.Player;
using TimePrototype.Scenes;

namespace TimePrototype.Systems
{
    public class BushSystem : EntityProcessingSystem
    {
        private readonly PlayerComponent _playerComponent;

        public BushSystem(PlayerComponent playerComponent) : base(new Matcher().one(typeof(BushComponent)))
        {
            _playerComponent = playerComponent;
        }

        public override void process(Entity entity)
        {
            var collider = entity.getComponent<BoxCollider>();

            var collisionRect = Physics.overlapRectangle(collider.bounds, 1 << SceneMap.PLAYER_LAYER);
            if (collisionRect != null)
            {
                _playerComponent.isOnBush = true;
            }
        }
    }
}
