using Nez;

namespace TimePrototype.Scenes.SceneMapExtensions
{
    public class LastMap : ISceneMapExtensionable
    {
        public Scene Scene { get; set; }

        private bool _startedTransition;
        private bool _deatachedEntities;

        public void initialize()
        {
        }

        public void update()
        {
            if (!_deatachedEntities)
            {
                _deatachedEntities = true;
                Scene.findEntity("player").detachFromScene();
                Scene.findEntity("distortionCursor").detachFromScene();
            }
            if (!Core.isOnTransition() && !_startedTransition)
            {
                _startedTransition = true;
                Core.startSceneTransition(new CrossFadeTransition(() => new SceneEnd()));
            }
        }

        public void receiveSceneMessage(string message)
        {
        }
    }
}
