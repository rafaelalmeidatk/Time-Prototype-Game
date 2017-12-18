using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;

namespace TimePrototype.Components.GraphicComponents
{
    public class LaserComponent : RenderableComponent, IUpdatable
    {
        private readonly int _width;

        public override float width => _width;
        public override float height => 1;

        private Vector2 _originalLocalOffset;

        public float X => _originalLocalOffset.X;
        public float Y => _originalLocalOffset.Y;

        public LaserComponent(Vector2 localOffset, int width)
        {
            _width = width;
            _originalLocalOffset = new Vector2(localOffset.X, localOffset.Y);
        }

        public void ApplyOffset(float x, float y)
        {
            var off = new Vector2(_originalLocalOffset.X + x, _originalLocalOffset.Y + y);
            setLocalOffset(off);
        }

        public void update()
        {
            var sprite = entity.getComponent<Sprite>();
            if (sprite != null)
            {
                var offsetX = 0.0f;
                if (sprite.spriteEffects == SpriteEffects.FlipHorizontally)
                    offsetX = -1.0f * (width + X * 2);
                ApplyOffset(offsetX, 0);
            }
        }

        public override void render(Graphics graphics, Camera camera)
        {
            var pos = entity.position + _localOffset;
            graphics.batcher.drawLine(pos, pos + new Vector2(_width, 0), Color.Red);
        }
    }
}
