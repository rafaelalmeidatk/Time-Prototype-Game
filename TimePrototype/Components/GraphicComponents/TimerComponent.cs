using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using System.Collections.Generic;
using TimePrototype.Components.Sprites;

namespace TimePrototype.Components.GraphicComponents
{
    class TimerComponent : Component, IUpdatable
    {
        //--------------------------------------------------
        // Sprite
        
        public AnimatedSprite sprite;

        //--------------------------------------------------
        // Player Entity

        private Entity _playerEntity;

        //--------------------------------------------------
        // Is visible

        private bool _isVisible;
        public bool isVisible;

        //----------------------//------------------------//

        public TimerComponent(Entity playerEntity)
        {
            _playerEntity = playerEntity;
        }

        public override void initialize()
        {
            // Init sprite
            var texture = entity.scene.content.Load<Texture2D>(Content.Misc.timer);
            sprite = entity.addComponent(new AnimatedSprite(texture, "0"));

            createNumeredFrames();
            createCooldownNumeredFrames();
        }

        private void createNumeredFrames()
        {
            for (var i = 0; i < 9; i++)
            {
                sprite.CreateAnimation(i.ToString(), 5f);
                sprite.AddFrames(i.ToString(), new List<Rectangle>
                {
                    new Rectangle(i * 8, 0, 8, 8)
                });
            }
        }

        private void createCooldownNumeredFrames()
        {
            for (var i = 0; i < 9; i++)
            {
                sprite.CreateAnimation($"cooldown_{i}", 5f);
                sprite.AddFrames($"cooldown_{i}", new List<Rectangle>
                {
                    new Rectangle(i * 8, 8, 8, 8)
                });
            }
        }

        public void setByPorcentage(float porc, bool cooldown)
        {
            // Don't touch
            var prefix = cooldown ? "cooldown_" : "";
            if (porc >= 1f) sprite.play($"{prefix}0");
            else if (porc >= .875f) sprite.play($"{prefix}1");
            else if (porc >= .75f) sprite.play($"{prefix}2");
            else if (porc >= .625f) sprite.play($"{prefix}3");
            else if (porc >= .5f) sprite.play($"{prefix}4");
            else if (porc >= .375f) sprite.play($"{prefix}5");
            else if (porc >= .25f) sprite.play($"{prefix}6");
            else if (porc >= .125f) sprite.play($"{prefix}7");
            else sprite.play($"{prefix}8");
        }

        public void update()
        {
            entity.setPosition(_playerEntity.position - 15 * Vector2.UnitY);
        }

        public void setVisible(bool visible)
        {
            _isVisible = visible;
            sprite.setEnabled(visible);
        }
    }
}
