using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;
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

        protected override Texture2D loadTexture()
        {
            return entity.scene.content.Load<Texture2D>(Content.Characters.enemySniper);
        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();

            var viewRange = (int) areaOfSight.width;
            entity.addComponent(new LaserComponent(new Vector2(6, -7), viewRange - 24));
        }

        protected override void createViewRange()
        {
            areaOfSight = entity.addComponent(new AreaOfSightCollider(-24, -12, 500, 12));
        }
    }
}
