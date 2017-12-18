using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.Tiled;
using TimePrototype.Components.Battle;
using TimePrototype.Components.GraphicComponents;
using TimePrototype.Components.Sprites;
using TimePrototype.FSM;
using TimePrototype.Managers;
using TimePrototype.Scenes;

namespace TimePrototype.Components.Player
{
    public class PlayerComponent : Component, IUpdatable, IBattleEntity
    {
        //--------------------------------------------------
        // Animations

        public enum Animations
        {
            Stand,
            Walking,
            Shot,
            Dying
        }

        private Dictionary<Animations, string> _animationMap;

        //--------------------------------------------------
        // Sprite

        public AnimatedSprite sprite;

        //--------------------------------------------------
        // Platformer Object

        PlatformerObject _platformerObject;
        public PlatformerObject platformerObject => _platformerObject;

        //--------------------------------------------------
        // Collision State

        public TiledMapMover.CollisionState CollisionState => _platformerObject.collisionState;
        public bool ForcedGround { get; set; }

        //--------------------------------------------------
        // Velocity

        public Vector2 Velocity => _platformerObject.velocity;

        //--------------------------------------------------
        // Finite State Machine

        private FiniteStateMachine<PlayerState, PlayerComponent> _fsm;
        public FiniteStateMachine<PlayerState, PlayerComponent> FSM => _fsm;

        //--------------------------------------------------
        // Forced Movement

        private bool _forceMovement;
        private Vector2 _forceMovementVelocity;
        private bool _walljumpForcedMovement;

        //--------------------------------------------------
        // Knockback

        private Vector2 _knockbackVelocity;
        private Vector2 _knockbackTick;

        //--------------------------------------------------
        // Forced position

        public bool forcePosition;
        public float forcedPositionX;
        public float forcedPositionY;

        //--------------------------------------------------
        // Battler

        private BattleComponent _battleComponent;

        //--------------------------------------------------
        // Returning in time

        public bool returningInTime;

        //--------------------------------------------------
        // Timed Tail

        private TimedSpriteTail _spriteTail;

        //--------------------------------------------------
        // Is on bush

        public bool isOnBush;
        public bool isInsideBush;

        //--------------------------------------------------
        // Can take damage

        public virtual bool canTakeDamage => !returningInTime && !isInsideBush;

        //--------------------------------------------------
        // Is with key

        public bool isWithKey;

        //--------------------------------------------------
        // Slowdown power

        private const float SLOWDOWN_DURATION = 2.0f;
        private const float SLOWDOWN_COOLDOWN = 2.0f;
        private float _slowdownPower;
        private float _slowdownCooldown;
        public bool isSlowingdownTheTime { get; private set; }

        //--------------------------------------------------
        // On first distortion

        public bool onFirstDistortion;
        public bool returnInTimeOnFirstDistortion;

        //--------------------------------------------------
        // Timer

        private TimerComponent _timer;

        //--------------------------------------------------
        // Distortion cursor

        private Sprite _distortionCursorSprite;

        //----------------------//------------------------//

        public override void initialize()
        {
            var texture = entity.scene.content.Load<Texture2D>(Content.Characters.player);

            _animationMap = new Dictionary<Animations, string>
            {
                {Animations.Stand, "stand"},
                {Animations.Walking, "walking"},
                {Animations.Shot, "shot"},
                {Animations.Dying, "dying"},
            };

            var am = _animationMap;

            sprite = entity.addComponent(new AnimatedSprite(texture, am[Animations.Stand]));
            sprite.CreateAnimation(am[Animations.Stand], 0.1f);
            sprite.AddFrames(am[Animations.Stand], new List<Rectangle>()
            {
                new Rectangle(0, 0, 32, 32),
            });

            sprite.CreateAnimation(am[Animations.Walking], 0.1f);
            sprite.AddFrames(am[Animations.Walking], new List<Rectangle>()
            {
                new Rectangle(32, 0, 32, 32),
                new Rectangle(64, 0, 32, 32),
                new Rectangle(96, 0, 32, 32),
                new Rectangle(128, 0, 32, 32),
            });

            sprite.CreateAnimation(am[Animations.Shot], 0.1f);
            sprite.AddFrames(am[Animations.Shot], new List<Rectangle>()
            {
                new Rectangle(32, 0, 32, 32),
                new Rectangle(32, 0, 32, 32),
            });

            sprite.CreateAnimation(am[Animations.Dying], 0.1f);
            sprite.AddFrames(am[Animations.Dying], new List<Rectangle>()
            {
                new Rectangle(0, 0, 32, 32),
                new Rectangle(0, 0, 32, 32),
            });

            // init fsm
            _fsm = new FiniteStateMachine<PlayerState, PlayerComponent>(this, new StandState());

            // init slowdown
            _slowdownPower = SLOWDOWN_DURATION;
            _slowdownCooldown = 0.0f;
        }

        public override void onAddedToEntity()
        {
            _spriteTail = entity.getComponent<TimedSpriteTail>();

            _platformerObject = entity.getComponent<PlatformerObject>();
            _platformerObject.setGetDeltaTimeFunc(GetDeltaTimeFunc);

            _battleComponent = entity.getComponent<BattleComponent>();
            _battleComponent.battleEntity = this;
            _battleComponent.ImmunityDuration = 0.5f;
            _battleComponent.destroyEntityAction = destroyEntity;

            _timer = entity.scene.findEntity("timer").getComponent<TimerComponent>();
            _distortionCursorSprite = entity.scene.findEntity("distortionCursor").getComponent<Sprite>();
        }

        public void destroyEntity()
        {
            entity.setEnabled(false);
            Core.startSceneTransition(new WindTransition(() => new SceneMap()));
        }

        public void onHit(Vector2 knockback)
        {
            if (returningInTime) return;
            //(entity.scene as SceneMap)?.startScreenShake(1, 200);
            _knockbackTick = new Vector2(0.06f, 0.04f);
            _knockbackVelocity = new Vector2(knockback.X * 60, -5);
        }

        public void onDeath()
        {
            FSM.changeState(new DyingState());
        }

        public void forceMovement(Vector2 velocity, bool walljumpForcedMovement = false)
        {
            if (velocity == Vector2.Zero)
            {
                _forceMovement = false;
            }
            else
            {
                _forceMovement = true;
                _forceMovementVelocity = velocity;
            }
            _walljumpForcedMovement = walljumpForcedMovement;
        }

        public void fadeTail()
        {
            _spriteTail.forceFade();
        }

        public void update()
        {
            // Slowdown
            if (Core.getGlobalManager<InputManager>().TimeSlowdownButton.isDown && canSlowdownTime())
            {
                isSlowingdownTheTime = true;
                Time.timeScale = 0.4f;
                _slowdownPower -= Time.unscaledDeltaTime;
                if (_slowdownPower <= 0.0f)
                {
                    _slowdownCooldown = SLOWDOWN_COOLDOWN;
                }
            }
            else
            {
                isSlowingdownTheTime = false;
                Time.timeScale = 1.0f;
                if (_slowdownCooldown <= 0.0f)
                {
                    _slowdownPower = Math.Min(_slowdownPower + Time.unscaledDeltaTime, SLOWDOWN_DURATION);
                }
            }

            if (_slowdownCooldown > 0.0f)
            {
                _timer.setByPorcentage((SLOWDOWN_COOLDOWN - _slowdownCooldown) / SLOWDOWN_COOLDOWN, true);
                _slowdownCooldown -= Time.unscaledDeltaTime;
                if (_slowdownCooldown <= 0.0f)
                {
                    _slowdownPower = SLOWDOWN_DURATION;
                }
            }
            else
            {
                if (_slowdownPower == SLOWDOWN_DURATION)
                {
                    _timer.setVisible(false);
                }
                else
                {
                    _timer.setVisible(true);
                    _timer.setByPorcentage(_slowdownPower / SLOWDOWN_DURATION, false);
                }
            }

            // Update FSM
            _fsm.update();

            _spriteTail.canSpawnMoreInstance = !returningInTime && sprite.enabled;
            var distortionPosition = getReturnInTimePosition();
            _distortionCursorSprite.entity.setPosition(distortionPosition - new Vector2(0, 17));
            _distortionCursorSprite.setEnabled(distortionPosition != Vector2.Zero);

            // apply knockback before movement
            if (applyKnockback())
                return;

            if (forcePosition)
            {
                var pos = new Vector2(forcedPositionX, forcedPositionY);
                entity.setPosition(pos);
            }

            var axis = Core.getGlobalManager<InputManager>().MovementAxis.value;
            var velocity = _forceMovement ? _forceMovementVelocity.X : axis;
            if (canMove() && (velocity > 0 || velocity < 0))
            {
                var po = _platformerObject;
                var mms = po.maxMoveSpeed;
                var moveSpeed = _walljumpForcedMovement ? po.gravity * mms : po.moveSpeed;

                if (velocity != Math.Sign(po.velocity.X))
                {
                    po.velocity.X = 0;
                }
                po.velocity.X = (int)MathHelper.Clamp(po.velocity.X + moveSpeed * velocity * Time.unscaledDeltaTime, -mms, mms);

                if (platformerObject.grabbingWall)
                {
                    po.velocity.X = po.grabbingWallSide * mms;
                    sprite.spriteEffects = po.grabbingWallSide == -1
                        ? SpriteEffects.FlipHorizontally
                        : SpriteEffects.None;
                }
                else
                {
                    sprite.spriteEffects = velocity < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                }
            }
            else
            {
                _platformerObject.velocity.X = 0;
            }

            ForcedGround = false;
        }

        private bool applyKnockback()
        {
            if (_knockbackTick.X > 0)
            {
                _knockbackTick.X -= Time.unscaledDeltaTime;
            }
            if (_knockbackTick.Y > 0)
            {
                _knockbackTick.Y -= Time.unscaledDeltaTime;
            }

            var mms = _platformerObject.maxMoveSpeed;
            var velx = _platformerObject.velocity.X;
            var vely = _platformerObject.velocity.Y;
            var appliedKb = false;
            if (_knockbackTick.X > 0)
            {
                _platformerObject.velocity.X = MathHelper.Clamp(velx + _platformerObject.moveSpeed * _knockbackVelocity.X * Time.unscaledDeltaTime, -mms, mms);
                appliedKb = true;
            }
            if (_knockbackTick.Y > 0)
            {
                _platformerObject.velocity.Y = MathHelper.Clamp(vely + _platformerObject.moveSpeed * _knockbackVelocity.Y * Time.unscaledDeltaTime, -mms, mms);
                appliedKb = true;
            }
            return appliedKb;
        }

        public void SetAnimation(Animations animation)
        {
            var animationStr = _animationMap[animation];
            if (sprite.CurrentAnimation != animationStr)
            {
                sprite.play(animationStr);
            }
        }

        public Entity createEntityOnMap()
        {
            return entity.scene.createEntity();
        }

        public void Jump()
        {
            _platformerObject.jump();
        }

        public float GetDeltaTimeFunc()
        {
            return Time.unscaledDeltaTime;
        }

        private bool canMove()
        {
            return (Core.getGlobalManager<InputManager>().isMovementAvailable() || _forceMovement) &&
                   !_battleComponent.Dying;
        }

        public bool isOnGround()
        {
            return ForcedGround || _platformerObject.collisionState.below;
        }

        public bool canSlowdownTime()
        {
            var sys = Core.getGlobalManager<SystemManager>();
            return !_battleComponent.Dying && !returningInTime && _slowdownPower > 0.0f &&
                   (sys.getSwitch("introducedSlowmotion") || sys.MapId > 5);
        }

        public bool canReturnInTime()
        {
            return !_battleComponent.Dying && onFirstDistortion || returnInTimeOnFirstDistortion ||
                   Core.getGlobalManager<SystemManager>().MapId > 8;
        }

        public Vector2 getReturnInTimePosition()
        {
            if (onFirstDistortion)
            {
                onFirstDistortion = false;
                returnInTimeOnFirstDistortion = true;
                _spriteTail.setEnabled(true);
                (entity.scene as SceneMap)?.sendMessageToExtensions("removeDistortion");
                return Core.getGlobalManager<SystemManager>().distortionPosition;
            }
            return entity.getComponent<TimedSpriteTail>().lastInstancePosition();
        }
    }
}
