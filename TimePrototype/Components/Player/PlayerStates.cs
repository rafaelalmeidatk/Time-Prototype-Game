﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.Tweens;
using TimePrototype.Components.Battle;
using TimePrototype.Extensions;
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

                if (entity.canReturnInTime() && _input.ReturnTimeButton.isPressed)
                {
                    fsm.pushState(new ReturnInTimeState());
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
            entity.SetAnimation(PlayerComponent.Animations.Jumping);
            if (_needJump)
            {
                AudioManager.jump.Play(0.8f);
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
        private bool _casted;
        private bool _castedAttack;
        private float _delay;

        private Vector2 _start;
        private Vector2 _end;

        private const float ReturnDuration = 0.3f;

        private TrailRibbon _trailRibbon;

        public override void begin()
        {
            AudioManager.rewind.Play(1.0f, 1.0f, -0.5f);

            entity.entity.removeComponent<TrailRibbon>();
            _trailRibbon = entity.entity.addComponent(new TrailRibbon {ribbonRadius = 9, startColor = Color.WhiteSmoke, endColor = Color.White});
            _trailRibbon.ribbonsToDraw = 1.0f;

            entity.returningInTime = true;
            entity.fadeTail();
        }

        public override void update()
        {
            if (!_casted)
            {
                _casted = true;

                var position = entity.getReturnInTimePosition();
                if (position != Vector2.Zero)
                {
                    _start = entity.entity.position;
                    _end = position;

                    entity.forcePosition = true;
                    entity.forcedPositionX = entity.entity.position.X;
                    entity.forcedPositionY = entity.entity.position.Y;
                    entity.tween("forcedPositionX", position.X, ReturnDuration).setEaseType(EaseType.ExpoInOut).start();
                    entity.tween("forcedPositionY", position.Y, ReturnDuration).setEaseType(EaseType.ExpoInOut).start();

                    _trailRibbon.tween("ribbonsToDraw", 0.0f, 4f).setEaseType(EaseType.ExpoOut).start();

                    _delay = ReturnDuration;
                }

                return;
            }

            if (_delay > 0.0f)
            {
                _delay -= Time.unscaledDeltaTime;
                if (!_castedAttack && _delay <= ReturnDuration / 2)
                {
                    _castedAttack = true;
                    var hits = new RaycastHit[10];
                    var affected = Physics.linecastAll(_start, _end, hits, 1 << SceneMap.ENEMY_LAYER);
                    for (var i = 0; i < affected; i++)
                    {
                        var enemy = hits[i].collider.entity.getComponent<BattleComponent>();
                        enemy.onHit(hits[i].normal);
                    }
                }
            }
            else
            {
                fsm.resetStackTo(new StandState());
            }
        }

        public override void end()
        {
            _trailRibbon.fixPosition = entity.entity.position;
            entity.returningInTime = false;
            entity.platformerObject.velocity = Vector2.Zero;
            entity.forcePosition = false;
        }
    }

    public class OnBushState : PlayerState
    {
        private Sprite _bushSprite;
        private bool _goBack;

        public override void begin()
        {
            entity.isInsideBush = true;

            entity.sprite.setEnabled(false);
            _input.IsLocked = true;

            // Play the sound
            AudioManager.bush.Play(1.0f);

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
            else
            {
                // Something went wrong, go back!
                _goBack = true;
            }
        }

        public override void update()
        {
            if (_goBack)
            {
                fsm.resetStackTo(new StandState());
                return;
            }
            if (_input.BushButton.isPressed || _input.JumpButton.isPressed)
            {
                fsm.resetStackTo(new StandState());
            }
        }

        public override void end()
        {
            entity.isInsideBush = false;

            _bushSprite?.setSubtexture(new Subtexture(entity.entity.scene.content.Load<Texture2D>(Content.Misc.bush)));
            entity.sprite.setEnabled(true);
            _input.IsLocked = false;

            AudioManager.bush.Play(1.0f, 0.5f, 1.0f);
        }
    }

    public class DyingState : PlayerState
    {
        public override void begin()
        {
            _input.IsLocked = true;
            entity.SetAnimation(PlayerComponent.Animations.Dying);
        }
    }
}
