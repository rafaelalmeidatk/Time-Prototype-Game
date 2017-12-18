using Nez;
using TimePrototype.Components.Player;
using TimePrototype.Managers;
using TimePrototype.Scenes;

namespace TimePrototype.NPCs
{
    public class Map8Barrier : NpcBase
    {
        public Map8Barrier(string name) : base(name)
        {
            RunOnTouch = true;
            Invisible = true;
        }

        protected override void createActionList()
        {
            executeAction(() => { Enabled = false; });
            playerMessage("I feel like...");
            playerMessage("I've been here already.");
            executeAction(() =>
            {
                var player = Core.getGlobalManager<SystemManager>().playerEntity;
                player.getComponent<PlayerComponent>().onFirstDistortion = true;
                (player.scene as SceneMap)?.sendMessageToExtensions("showInstructionsMessage");
            });
            closePlayerMessage();
        }

        protected override void loadTexture()
        { }
    }
}
