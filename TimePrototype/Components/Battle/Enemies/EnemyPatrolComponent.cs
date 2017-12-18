using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using System.Collections.Generic;
using TimePrototype.Components.Colliders;
using TimePrototype.Components.Sprites;
using TimePrototype.FSM;

namespace TimePrototype.Components.Battle.Enemies
{
    public class EnemyPatrolComponent : EnemyComponent
    {
        //--------------------------------------------------
        // Finite State Machine

        private FiniteStateMachine<EnemyPatrolState, EnemyPatrolComponent> _fsm;
        public FiniteStateMachine<EnemyPatrolState, EnemyPatrolComponent> FSM => _fsm;

        //--------------------------------------------------
        // Cooldown

        public float attackCooldown;

        //--------------------------------------------------
        // Arrows speed

        protected virtual float _arrowSpeed => 1000.0f;

        //----------------------//------------------------//

        public EnemyPatrolComponent(bool patrolStartRight) : base(patrolStartRight)
        {
        }

        public override void initialize()
        {
            base.initialize();

            // Init sprite
            var texture = loadTexture();
            sprite = entity.addComponent(new AnimatedSprite(texture, "stand"));
            sprite.CreateAnimation("stand", 0.25f);
            sprite.AddFrames("stand", new List<Rectangle>
            {
                new Rectangle(0, 0, 32, 32),
            });

            sprite.CreateAnimation("walking", 0.08f);
            sprite.AddFrames("walking", new List<Rectangle>
            {
                new Rectangle(0, 0, 32, 32),
                new Rectangle(32, 0, 32, 32),
                new Rectangle(64, 0, 32, 32),
                new Rectangle(96, 0, 32, 32),
            });

            sprite.CreateAnimation("fire", 0.1f);
            sprite.AddFrames("fire", new List<Rectangle>
            {
                new Rectangle(0, 32, 32, 32),
                new Rectangle(0, 32, 32, 32),
            });

            sprite.CreateAnimation("reload", 0.1f, false);
            sprite.AddFrames("reload", new List<Rectangle>
            {
                new Rectangle(32, 32, 32, 32),
                new Rectangle(64, 32, 32, 32),
                new Rectangle(96, 32, 32, 32),
                new Rectangle(128, 32, 32, 32),
                new Rectangle(160, 32, 32, 32),
                new Rectangle(192, 32, 32, 32),
                new Rectangle(224, 32, 32, 32),
                new Rectangle(0, 64, 32, 32),
                new Rectangle(32, 64, 32, 32),
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
            createViewRange();
        }

        protected virtual Texture2D loadTexture()
        {
            return entity.scene.content.Load<Texture2D>(Content.Characters.enemy);
        }

        protected virtual void createViewRange()
        {
            areaOfSight = entity.addComponent(new AreaOfSightCollider(-24, -12, 110, 12));
        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            // Change move speed
            platformerObject.maxMoveSpeed = 4500;
            platformerObject.moveSpeed = 4500;
        }

        public void shoot()
        {
            var shot = entity.scene.createEntity("projectile");
            var direction = sprite.spriteEffects == SpriteEffects.FlipHorizontally ? -1 : 1;
            shot.addComponent(new ProjectileComponent(direction, _arrowSpeed));
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
