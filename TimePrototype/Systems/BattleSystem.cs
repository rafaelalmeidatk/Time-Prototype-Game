using System.Collections.Generic;
using Nez;
using TimePrototype.Components.Battle;
using TimePrototype.Components.Sprites;

namespace TimePrototype.Systems
{
    class BattleSystem : EntityProcessingSystem
    {
        public List<Entity> Entities => _entities;

        public BattleSystem() : base(new Matcher().all(typeof(BattleComponent), typeof(AnimatedSprite), typeof(BoxCollider))) { }

        public override void process(Entity entity)
        {

        }
    }
}
