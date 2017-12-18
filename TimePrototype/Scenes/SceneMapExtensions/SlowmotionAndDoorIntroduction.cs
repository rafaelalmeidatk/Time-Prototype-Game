using Microsoft.Xna.Framework;
using Nez;
using Nez.Tweens;

namespace TimePrototype.Scenes.SceneMapExtensions
{
    public class SlowmotionAndDoorIntroduction : ISceneMapExtensionable
    {
        public Scene Scene { get; set; }
        private Text _instructionsText;

        private float _instructionsTextY;
        private float _instructionsTextAlpha;

        public void initialize()
        {
            var tiledMap = Scene.findEntity("tiledMap").getComponent<TiledMapComponent>();
            var keyObj = tiledMap.tiledMap.getObjectGroup("objects").objectWithName("InstructionsMessage");

            _instructionsText = Scene.createEntity("instructionsText")
                .addComponent(new Text(Graphics.instance.bitmapFont, "Press Z to slowmotion the time", Vector2.Zero, Color.White * 0.0f));
            _instructionsText.entity.setPosition(keyObj.position + 10 * Vector2.UnitY);
        }

        public void update()
        {
            _instructionsText.setLocalOffset(new Vector2(-62, _instructionsTextY));
            _instructionsText.setColor(Color.White * _instructionsTextAlpha);
        }

        public void receiveSceneMessage(string message)
        {
            if (message == "showInstructionsMessage")
            {
                this.tween("_instructionsTextY", -30.0f, 1.0f).setEaseType(EaseType.SineOut).start();
                this.tween("_instructionsTextAlpha", 1.0f, 1.0f).setEaseType(EaseType.SineIn).start();
            }
        }
    }
}
