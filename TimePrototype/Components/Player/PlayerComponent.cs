using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Tiled;
using TimePrototype.Components.GraphicComponents;
using TimePrototype.Components.Sprites;
using TimePrototype.FSM;
using TimePrototype.Managers;
using TimePrototype.Scenes;

namespace TimePrototype.Components.Player
{
    public class PlayerComponent : Component, IUpdatable
    {
        //--------------------------------------------------
        // Animations

        public enum Animations
        {
            Stand,
            Walking,
            Shot
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
        // Returning in time

        public bool returningInTime;

        //--------------------------------------------------
        // Timed Tail

        private TimedSpriteTail _spriteTail;

        //--------------------------------------------------
        // Is on bush

        public bool isOnBush;

        //--------------------------------------------------
        // Can take damage

        public virtual bool canTakeDamage => true;

        //----------------------//------------------------//

        public override void initialize()
        {
            var texture = entity.scene.content.Load<Texture2D>(Content.Characters.player);

            _animationMap = new Dictionary<Animations, string>
            {
                {Animations.Stand, "stand"},
                {Animations.Walking, "walking"},
                {Animations.Shot, "shot"},
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

            // init fsm
            _fsm = new FiniteStateMachine<PlayerState, PlayerComponent>(this, new StandState());
        }

        public override void onAddedToEntity()
        {
            _spriteTail = entity.getComponent<TimedSpriteTail>();

            _platformerObject = entity.getComponent<PlatformerObject>();
            _platformerObject.setGetDeltaTimeFunc(GetDeltaTimeFunc);
        }

        public void destroyEntity()
        {
            entity.setEnabled(false);
            Core.startSceneTransition(new SquaresTransition(() => new SceneMap()));
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
            // Update FSM
            _fsm.update();

            _spriteTail.canSpawnMoreInstance = !returningInTime && sprite.enabled;

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
            bool appliedKb = false;
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
            return Core.getGlobalManager<InputManager>().isMovementAvailable() || _forceMovement;
        }

        public bool isOnGround()
        {
            return ForcedGround || _platformerObject.collisionState.below;
        }
    }
}
