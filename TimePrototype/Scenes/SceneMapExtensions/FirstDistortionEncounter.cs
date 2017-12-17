using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.Tweens;
using TimePrototype.Managers;

namespace TimePrototype.Scenes.SceneMapExtensions
{
    public class FirstDistortionEncounter : ISceneMapExtensionable
    {
        public Scene Scene { get; set; }

        private Sprite _distortionSprite;
        private float _distortionAlpha;

        private Text _instructionsText;

        private float _instructionsTextY;
        private float _instructionsTextAlpha;

        public void initialize()
        {
            _distortionAlpha = 1.0f;

            var distortionTexture = Scene.content.Load<Texture2D>(Content.Characters.distortion);
            var distortionEntity = Scene.createEntity("distortion");
            _distortionSprite = distortionEntity.addComponent(new Sprite(distortionTexture));
            _distortionSprite.renderLayer = SceneMap.MISC_RENDER_LAYER;

            var tiledMap = Scene.findEntity("tiledMap").getComponent<TiledMapComponent>();
            var distortionObj = tiledMap.tiledMap.getObjectGroup("objects").objectWithName("distortion");

            distortionEntity.position = new Vector2(distortionObj.x + distortionObj.width / 2,
                distortionObj.y + distortionObj.height - distortionTexture.Height / 2);

            Core.getGlobalManager<SystemManager>().distortionPosition = distortionEntity.position;

            _instructionsText = Scene.createEntity("distortionText")
                .addComponent(new Text(Graphics.instance.bitmapFont, "Press X to return in time", Vector2.Zero, Color.White * 0.0f));
            _instructionsText.entity.setPosition(_distortionSprite.entity.position);
        }

        public void update()
        {
            _instructionsText.setLocalOffset(new Vector2(-62, _instructionsTextY));
            _instructionsText.setColor(Color.White * _instructionsTextAlpha);
            _distortionSprite.setColor(Color.White * _distortionAlpha);
        }

        public void receiveSceneMessage(string message)
        {
            if (message == "removeDistortion")
            {
                this.tween("_distortionAlpha", 0.0f, 1.0f).setEaseType(EaseType.SineIn).start();
                this.tween("_instructionsTextAlpha", 0.0f, 1.0f).setEaseType(EaseType.SineIn).start();
            }
            if (message == "showInstructionsMessage")
            {
                this.tween("_instructionsTextY", -30.0f, 1.0f).setEaseType(EaseType.SineOut).start();
                this.tween("_instructionsTextAlpha", 1.0f, 1.0f).setEaseType(EaseType.SineIn).start();
            }
        }
    }
}
