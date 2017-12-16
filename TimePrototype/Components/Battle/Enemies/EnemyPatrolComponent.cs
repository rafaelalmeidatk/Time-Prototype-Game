using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using System.Collections.Generic;
using TimePrototype.Components.Colliders;
using TimePrototype.Components.Sprites;
using TimePrototype.FSM;
using TimePrototype.Scenes;

namespace TimePrototype.Components.Battle.Enemies
{
    public class EnemyPatrolComponent : EnemyComponent
    {
        //--------------------------------------------------
        // Finite State Machine

        private FiniteStateMachine<EnemyPatrolState, EnemyPatrolComponent> _fsm;
        public FiniteStateMachine<EnemyPatrolState, EnemyPatrolComponent> FSM => _fsm;

        //----------------------//------------------------//

        public EnemyPatrolComponent(bool patrolStartRight) : base(patrolStartRight)
        {
        }

        public override void initialize()
        {
            base.initialize();

            // Init sprite
            var texture = entity.scene.content.Load<Texture2D>(Content.Characters.placeholder);
            sprite = entity.addComponent(new AnimatedSprite(texture, "stand"));
            sprite.CreateAnimation("stand", 0.25f);
            sprite.AddFrames("stand", new List<Rectangle>
            {
                new Rectangle(0, 0, 32, 32),
            });

            sprite.CreateAnimation("walking", 0.15f);
            sprite.AddFrames("walking", new List<Rectangle>
            {
                new Rectangle(0, 0, 32, 32),
            });

            sprite.CreateAnimation("fire", 0.15f);
            sprite.AddFrames("fire", new List<Rectangle>
            {
                new Rectangle(32, 0, 32, 32),
            });

            sprite.CreateAnimation("hit", 0.1f, false);
            sprite.AddFrames("hit", new List<Rectangle>
            {
                new Rectangle(0, 0, 32, 32),
            });

            sprite.CreateAnimation("dying", 0.1f, false);
            sprite.AddFrames("dying", new List<Rectangle>
            {
                new Rectangle(0, 0, 32, 32),
            });

            // FSM
            _fsm = new FiniteStateMachine<EnemyPatrolState, EnemyPatrolComponent>(this, new EnemyPatrolWalkingState());

            // View range
            areaOfSight = entity.addComponent(new AreaOfSightCollider(-24, -12, 120, 12));
        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            // Change move speed
            platformerObject.maxMoveSpeed = 60;
            platformerObject.moveSpeed = 60;
        }

        public void shoot()
        {
            var shot = entity.scene.createEntity("projectile");
            var direction = sprite.spriteEffects == SpriteEffects.FlipHorizontally ? -1 : 1;
            shot.addComponent(new ProjectileComponent(direction));
            var position = entity.getComponent<BoxCollider>().absolutePosition;
            shot.transform.position = position;
        }

        public override void onHit(Vector2 knockback)
        {
            //(entity.scene as SceneMap)?.startScreenShake(0.7f, 100);
            base.onHit(knockback);
            //FSM.changeState(new EnemyOneHitState());
        }

        public override void onDeath()
        {
            base.onDeath();
            //FSM.changeState(new EnemyOneDyingState());
        }

        public override void update()
        {
            _fsm.update();
            base.update();
        }
    }
}
