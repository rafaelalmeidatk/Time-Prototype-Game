using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using System.Collections.Generic;
using System.Linq;

namespace TimePrototype.Components.GraphicComponents
{
    class TimedSpriteTail : SpriteTrail
    {
        public float delayBetweenInstances = 0.3f;
        public bool canSpawnMoreInstance = true;

        private float _nextInstanceDelay;
        private bool _forcedFade;

        public Vector2 lastInstancePosition()
        {
            if (_liveSpriteTrailInstances.Count == 0)
                return Vector2.Zero;

            return _liveSpriteTrailInstances[0].position;
        }

        public List<Vector2> getPositions()
        {
            return _liveSpriteTrailInstances.Select(x => x.position).ToList();
        }

        public override void update()
        {
            if (_isFirstInstance)
            {
                _nextInstanceDelay = delayBetweenInstances;
                _isFirstInstance = false;
            }
            else if (canSpawnMoreInstance)
            {
                _nextInstanceDelay -= Time.unscaledDeltaTime;
                if (_nextInstanceDelay <= 0)
                {
                    _nextInstanceDelay = delayBetweenInstances;
                    spawnInstance();
                }
            }

            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);

            // update any live instances
            for (var i = _liveSpriteTrailInstances.Count - 1; i >= 0; i--)
            {
                if (_forcedFade)
                {
                    _liveSpriteTrailInstances[i].forceFade();
                }
                if (_liveSpriteTrailInstances[i].update())
                {
                    _availableSpriteTrailInstances.Push(_liveSpriteTrailInstances[i]);
                    _liveSpriteTrailInstances.RemoveAt(i);
                }
                else
                {
                    // calculate our min/max for the bounds
                    Vector2.Min(ref min, ref _liveSpriteTrailInstances[i].position, out min);
                    Vector2.Max(ref max, ref _liveSpriteTrailInstances[i].position, out max);
                }
            }

            _forcedFade = false;

            _bounds.location = min;
            _bounds.width = max.X - min.X;
            _bounds.height = max.Y - min.Y;
            _bounds.inflate(_sprite.width, _sprite.height);

            // nothing left to render. disable ourself
            if (_awaitingDisable && _liveSpriteTrailInstances.Count == 0)
                enabled = false;
        }

        public void forceFade()
        {
            _forcedFade = true;
        }
    }
}
