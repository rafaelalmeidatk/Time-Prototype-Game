using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using TimePrototype.Components.Sprites;
using TimePrototype.Scenes;

namespace TimePrototype.Components.Battle
{
    class ProjectileComponent : Component, IUpdatable
    {
        //--------------------------------------------------
        // Sprite

        public AnimatedSprite sprite;

        //--------------------------------------------------
        // Direction, Speed and Type

        private int _direction;
        private float _speed;
        private int _type;

        //----------------------//------------------------//

        public ProjectileComponent(int direction)
        {
            _direction = direction;
        }

        public override void initialize()
        {
            var texture = entity.scene.content.Load<Texture2D>(Content.Misc.arrow);
            sprite = entity.addComponent(new AnimatedSprite(texture, "default"));
            sprite.CreateAnimation("default", 0.2f);
            sprite.AddFrames("default", new List<Rectangle>
            {
                new Rectangle(0, 0, 12, 5)
            });

            var collider = entity.addComponent(new BoxCollider(-6, -2, 12, 5));
            Flags.setFlagExclusive(ref collider.physicsLayer, SceneMap.PROJECTILES_LAYER);

            _speed = _type == 1 ? 100f : 300f;
            
            if (_direction < 0)
            {
                sprite.spriteEffects = SpriteEffects.FlipHorizontally;
            }
        }

        public override void onAddedToEntity()
        {
            entity.setTag(SceneMap.PROJECTILES_LAYER);
        }

        public void update()
        {
            var velx = (float)Math.Cos(entity.transform.rotation) * _speed * _direction * Time.deltaTime;
            var vely = (float)Math.Sin(entity.transform.rotation * _direction) * _speed * Time.deltaTime + 0.4f;
            var vel = new Vector2(velx, vely);
            entity.setPosition(entity.position + vel);
        }
    }
}
