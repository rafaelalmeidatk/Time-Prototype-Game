using Nez;
using TimePrototype.Components.Battle.Traps;
using TimePrototype.Scenes;

namespace TimePrototype.Systems
{
    public class ActivatorsSystem : EntityProcessingSystem
    {
        public ActivatorsSystem() : base(new Matcher().one(typeof(TrapActivatorComponent))) { }

        public override void process(Entity entity)
        {
            var trapActivatorComponent = entity.getComponent<TrapActivatorComponent>();
            var collider = entity.getComponent<BoxCollider>();

            var collisionRect = Physics.overlapRectangle(collider.bounds, 1 << SceneMap.PLAYER_LAYER);
            if (collisionRect != null)
            {
                var traps = entity.scene.findEntitiesWithTag(SceneMap.TRAPS);
                foreach (var trap in traps)
                {
                    var trapComponent = trap.getComponent<TrapComponent>();
                    if (trapComponent.cooldown <= 0.0f && trapComponent.activatorName == trapActivatorComponent.name)
                    {
                        trapComponent.doAction();
                        trapComponent.cooldown = 1.5f;
                    }
                }
            }
        }
    }
}
