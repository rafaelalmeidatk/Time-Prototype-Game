using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.Tweens;
using TimePrototype.Components.Battle;
using TimePrototype.Components.GraphicComponents;
using TimePrototype.FSM;
using TimePrototype.Managers;
using TimePrototype.Scenes;

namespace TimePrototype.Components.Player
{
    public class PlayerState : State<PlayerState, PlayerComponent>
    {
        protected InputManager _input => Core.getGlobalManager<InputManager>();

        public override void begin() { }

        public override void end() { }

        public void handleInput()
        {
            if (isMovementAvailable())
            {
                if (entity.isOnGround() && isMovementAvailable() && _input.JumpButton.isPressed)
                {
                    fsm.resetStackTo(new JumpingState(true));
                }

                if (_input.ReturnTimeButton.isPressed)
                {
                    fsm.pushState(new ReturnInTimeState());
                }

                if (_input.TimeSlowdownButton.isPressed)
                {
                    fsm.pushState(new TimeSlowdownState());
                }

                if (entity.isOnBush && _input.BushButton.isPressed)
                {
                    fsm.pushState(new OnBushState());
                }
            }
        }

        protected bool isMovementAvailable()
        {
            return Core.getGlobalManager<InputManager>().isMovementAvailable();
        }

        public override void update()
        {
            handleInput();
        }
    }

    public class StandState : PlayerState
    {
        public override void begin()
        {
            entity.SetAnimation(PlayerComponent.Animations.Stand);
        }

        public override void update()
        {
            base.update();

            if (!entity.isOnGround())
            {
                fsm.changeState(new JumpingState(false));
                return;
            }

            if (entity.isOnGround())
            {
                if (entity.Velocity.X > 0 || entity.Velocity.X < 0)
                {
                    entity.SetAnimation(PlayerComponent.Animations.Walking);
                }
                else
                {
                    entity.SetAnimation(PlayerComponent.Animations.Stand);
                }
            }
        }
    }

    public class JumpingState : PlayerState
    {
        private bool _needJump;

        public JumpingState(bool needJump)
        {
            _needJump = needJump;
        }

        public override void begin()
        {
            if (_needJump)
            {
                _needJump = false;
                entity.Jump();
            }
        }

        public override void update()
        {
            base.update();

            if (entity.isOnGround())
            {
                fsm.resetStackTo(new StandState());
            }
        }
    }

    public class ReturnInTimeState : PlayerState
    {
        private bool casted;
        public float delay;

        private const float ReturnDuration = 0.3f;

        public override void begin()
        {
            entity.returningInTime = true;
            entity.fadeTail();
        }

        public override void update()
        {
            if (!casted)
            {
                casted = true;

                var position = entity.getComponent<TimedSpriteTail>().lastInstancePosition();
                if (position != Vector2.Zero)
                {
                    entity.forcePosition = true;
                    entity.forcedPositionX = entity.entity.position.X;
                    entity.forcedPositionY = entity.entity.position.Y;
                    entity.tween("forcedPositionX", position.X, ReturnDuration).setEaseType(EaseType.ExpoInOut).start();
                    entity.tween("forcedPositionY", position.Y, ReturnDuration).setEaseType(EaseType.ExpoInOut).start();

                    delay = ReturnDuration;
                }

                return;
            }

            if (delay > 0.0f)
            {
                delay -= Time.unscaledDeltaTime;
            }
            else
            {
                fsm.resetStackTo(new StandState());
            }
        }

        public override void end()
        {
            entity.returningInTime = false;
            entity.platformerObject.velocity = Vector2.Zero;
            entity.forcePosition = false;
        }
    }

    public class TimeSlowdownState : PlayerState
    {
        private bool slowingDown;

        public override void begin()
        {
            Time.timeScale = 0.4f;
        }

        public override void update()
        {
            if (!_input.TimeSlowdownButton.isDown)
            {
                fsm.resetStackTo(new StandState());
            }
        }

        public override void end()
        {
            Time.timeScale = 1.0f;
        }
    }

    public class OnBushState : PlayerState
    {
        private Sprite _bushSprite;

        public override void begin()
        {
            entity.sprite.setEnabled(false);
            _input.IsLocked = true;

            // Find the bush to change the sprite
            var bush = Physics.overlapRectangle(entity.entity.getComponent<BoxCollider>().bounds,
                1 << SceneMap.BUSHES_LAYER);
            if (bush != null)
            {
                _bushSprite = bush.entity.getComponent<Sprite>();
                _bushSprite.setSubtexture(new Subtexture(
                    entity.entity.scene.content.Load<Texture2D>(Content.Misc.bush_with_player)));

                // Position the player on the same pos of the bush
                entity.entity.setPosition(bush.entity.position);
            }
        }

        public override void update()
        {
            if (_input.BushButton.isPressed)
            {
                fsm.resetStackTo(new StandState());
            }
        }

        public override void end()
        {
            _bushSprite.setSubtexture(new Subtexture(entity.entity.scene.content.Load<Texture2D>(Content.Misc.bush)));
            entity.sprite.setEnabled(true);
            _input.IsLocked = false;
        }
    }

    public class ShotingState : PlayerState
    {
        private bool _shot;

        public override void begin()
        {
            entity.SetAnimation(PlayerComponent.Animations.Shot);
        }

        public override void update()
        {
            if (!_shot)
            {
                _shot = true;
                shot();
            }
            if (entity.sprite.Looped)
            {
                fsm.popState();
            }
        }

        private void shot()
        {
            var start = entity.entity.position;
            var end = start + 200 * Vector2.UnitX;
            entity.createEntityOnMap()
                .addComponent(new HitscanComponent(start, end, false));
        }
    }
}
