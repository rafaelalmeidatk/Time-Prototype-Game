using Microsoft.Xna.Framework;
using Nez;
using TimePrototype.FSM;

namespace TimePrototype.Components.Battle.Enemies
{
    public class EnemyPatrolState : State<EnemyPatrolState, EnemyPatrolComponent>
    {
        protected BoxCollider playerCollider => entity.playerCollider;

        public override void begin() { }

        public override void end() { }

        public override void update() { }
    }

    public class EnemyPatrolWalkingState : EnemyPatrolState
    {
        private float _side = -1;
        private ITimer _timer;

        public override void begin()
        {
            _side = entity.patrolStartRight ? -1 : 1;
            switchSide();
            entity.sprite.play("walking");
        }

        public void switchSide()
        {
            _timer?.stop();
            _side *= -1;
            entity.forceMovement(Vector2.UnitX * _side);
            _timer = Core.schedule(entity.patrolTime, entity, t =>
            {
                switchSide();
            });
        }

        public override void update()
        {
            var po = entity.getComponent<PlatformerObject>();
            if (entity.sprite.getDirection() == 1 && po.collisionState.right)
            {
                po.velocity = -po.maxMoveSpeed * 0.8f * Vector2.UnitX;
                switchSide();
            }
            if (entity.sprite.getDirection() == -1 && po.collisionState.left)
            {
                po.velocity = po.maxMoveSpeed * 0.8f * Vector2.UnitX;
                switchSide();
            }
            if (entity.canSeeThePlayer() && !entity.playerIsOnBush())
            {
                _timer.stop();
                entity.forceMovement(Vector2.Zero);
                //fsm.pushState(new EnemyOn());
            }
        }
    }
}
