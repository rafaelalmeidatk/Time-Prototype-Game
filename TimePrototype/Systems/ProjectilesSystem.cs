using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.ECS.Components.Physics.Colliders;
using TimePrototype.Components.Battle;
using TimePrototype.Components.Sprites;

namespace TimePrototype.Systems
{
    public class ProjectilesSystem : EntityProcessingSystem
    {
        private readonly BattleComponent _playerBattler;
        private readonly BoxCollider _playerCollider;

        public ProjectilesSystem(Entity player) : base(new Matcher().one(typeof(ProjectileComponent)))
        {
            _playerBattler = player.getComponent<BattleComponent>();
            _playerCollider = player.getComponent<BoxCollider>();
        }

        public override void process(Entity entity)
        {
            CollisionResult collisionResult;
            var collider = entity.getComponent<Collider>();

            // shots vs map
            /*
            if (collider.collidesWithAnyOfType<MapBoxCollider>(out collisionResult))
            {
                if (missile != null)
                {
                    createMissileExplosionEffect(missile);
                }
                else
                {
                    createBulletEffect(entity.getComponent<ShotComponent>(), collisionResult.normal);
                }
                entity.destroy();
            }
            */

            // shots vs player
            if (_playerBattler.isOnImmunity() || _playerCollider == null) return;
            if (collider.collidesWith(_playerCollider, out collisionResult))
            {
                _playerBattler.onHit(collisionResult);
                entity.destroy();
            }
        }
    }
}
