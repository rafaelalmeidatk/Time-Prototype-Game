using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using TimePrototype.Components.Sprites;
using TimePrototype.Scenes;

namespace TimePrototype.Components.Battle
{
    public class ProjectileComponent : Component
    {
        //--------------------------------------------------
        // Sprite

        public AnimatedSprite sprite;

        //--------------------------------------------------
        // Direction, Speed and Type

        private readonly int _direction;
        private readonly float _speed;

        //----------------------//------------------------//

        public ProjectileComponent(int direction, float speed)
        {
            _direction = direction;
            _speed = speed;
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
            var scale = Time.timeScale < 1 ? 0.1f : Time.timeScale;
            var deltaTime = Time.unscaledDeltaTime * scale;
            var velx = (float)Math.Cos(entity.transform.rotation) * _speed * _direction * deltaTime;
            var vely = (float)Math.Sin(entity.transform.rotation * _direction) * _speed * deltaTime + 0.4f;
            var vel = new Vector2(velx, vely);
            entity.setPosition(entity.position + vel);
        }
    }
}
