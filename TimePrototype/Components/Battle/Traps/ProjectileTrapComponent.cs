using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;

namespace TimePrototype.Components.Battle.Traps
{
    class ProjectileTrapComponent : TrapComponent
    {

        //--------------------------------------------------
        // Props

        private int _side;
        private float _rotation;

        //----------------------//------------------------//

        public ProjectileTrapComponent(string activatorName, int side, float rotation) : base(activatorName)
        {
            _side = side;
            _rotation = rotation;
        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();

            // setup sprite
            var texture = entity.scene.content.Load<Texture2D>(Content.Misc.projectile_trap);
            sprite = entity.addComponent(new Sprite(texture));
            sprite.flipX = _side == 1;
            
            entity.rotation = _rotation;
        }

        public override void doAction()
        {
           shoot();
        }

        public void shoot()
        {
            var speed = 500; 

            var shot = entity.scene.createEntity("projectile");
            var direction = sprite.spriteEffects == SpriteEffects.FlipHorizontally ? -1 : 1;
            var position = entity.position;

            shot.addComponent(new ProjectileComponent(direction, speed, _rotation));
            shot.transform.position = position;
        }
    }
}
