using Microsoft.Xna.Framework;
using TimePrototype.Components.Colliders;
using TimePrototype.Components.GraphicComponents;

namespace TimePrototype.Components.Battle.Enemies
{
    public class EnemySniperComponent : EnemyPatrolComponent
    {
        //--------------------------------------------------
        // Arrows speed

        protected override float _arrowSpeed => 3000.0f;

        //----------------------//------------------------//

        public EnemySniperComponent(bool patrolStartRight) : base(patrolStartRight)
        {
        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();

            var viewRange = (int) areaOfSight.width;
            entity.addComponent(new LaserComponent(new Vector2(0, -20), viewRange - 24));
        }

        protected override void createViewRange()
        {
            areaOfSight = entity.addComponent(new AreaOfSightCollider(-24, -12, 500, 12));
        }
    }
}
