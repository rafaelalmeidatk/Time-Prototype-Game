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
        public override void begin()
        {
            entity.sprite.play("walking");
            entity.forceMovement(Vector2.UnitX * entity.currentPatrolSide());
        }

        public void switchSide()
        {
            entity.switchPatrolSide();
            entity.forceMovement(Vector2.UnitX * entity.currentPatrolSide());
        }

        public override void update()
        {
            if (entity.sprite.getDirection() == 1 && entity.entity.position.X > entity.path.End.X)
            {
                switchSide();
                entity.unseeThePlayer();
            }
            else if (entity.sprite.getDirection() == -1 && entity.entity.position.X < entity.path.Start.X)
            {
                switchSide();
                entity.unseeThePlayer();
            }
            else if (entity.sawThePlayer() && entity.sprite.getDirection() != entity.currentPatrolSide())
            {
                switchSide();
            }

            if (entity.attackCooldown > 0.0)
            {
                entity.attackCooldown -= Time.deltaTime;
                return;
            }

            if (entity.canSeeThePlayer() && !entity.playerIsOnBush())
            {
                entity.attackCooldown = 1.0f;
                entity.forceMovement(Vector2.Zero);
                fsm.pushState(new EnemyPatrolFireState());
            }
        }

        public override void end()
        {
            entity.forceMovement(Vector2.Zero);
        }
    }

    public class EnemyPatrolFireState : EnemyPatrolState
    {
        public override void begin()
        {
            entity.sprite.play("fire");
            entity.turnToPlayer();
            entity.shoot();
        }

        public override void update()
        {
            if (entity.sprite.Looped)
            {
                fsm.resetStackTo(new EnemyPatrolWalkingState());
            }
        }
    }
}
