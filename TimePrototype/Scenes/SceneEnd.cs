using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using TimePrototype.Managers;

namespace TimePrototype.Scenes
{
    class SceneEnd : Scene
    {
        private int _index;
        private float _currentScreenAlpha;
        private bool _transitioning;

        public override void initialize()
        {
            addRenderer(new DefaultRenderer());
            clearColor = new Color(35, 35, 35);

            _index = -1;

            createEntity("end_screen_0")
                .addComponent(new Sprite(content.Load<Texture2D>(Content.Misc.end_screen_0)))
                .transform.position = virtualSize.ToVector2() / 2;

            createEntity("end_screen_1")
                .addComponent(new Sprite(content.Load<Texture2D>(Content.Misc.end_screen_1)))
                .setColor(Color.White * 0.0f)
                .transform.position = virtualSize.ToVector2() / 2;

            createEntity("end_screen_2")
                .addComponent(new Sprite(content.Load<Texture2D>(Content.Misc.end_screen_2)))
                .setColor(Color.White * 0.0f)
                .transform.position = virtualSize.ToVector2() / 2;

            createEntity("end_screen_3")
                .addComponent(new Sprite(content.Load<Texture2D>(Content.Misc.end_screen_3)))
                .setColor(Color.White * 0.0f)
                .transform.position = virtualSize.ToVector2() / 2;

            createEntity("end_screen_4")
                .addComponent(new Sprite(content.Load<Texture2D>(Content.Misc.end_screen_4)))
                .setColor(Color.White * 0.0f)
                .transform.position = virtualSize.ToVector2() / 2;
        }

        public override void update()
        {
            base.update();

            if (_transitioning)
            {
                findEntity($"end_screen_{_index + 1}")
                    .getComponent<Sprite>()
                    .setColor(Color.White * _currentScreenAlpha);

                if (_currentScreenAlpha >= 1.0f)
                {
                    _transitioning = false;
                    _currentScreenAlpha = 0.0f;
                }
            }

            if (!_transitioning && _index < 3 && Keyboard.GetState().GetPressedKeys().Length > 0)
            {
                this.tween("_currentScreenAlpha", 1.0f, 1.0f).start();
                _transitioning = true;
                _index++;
            }
        }
    }
}
