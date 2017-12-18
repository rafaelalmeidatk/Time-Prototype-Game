using Nez;
using TimePrototype.Managers;
using TimePrototype.Scenes;

namespace TimePrototype.NPCs
{
    public class IntroduceSlowmotion : NpcBase
    {
        public IntroduceSlowmotion(string name) : base(name)
        {
            RunOnTouch = true;
            Invisible = true;
            Enabled = !Core.getGlobalManager<SystemManager>().getSwitch("introducedSlowmotion");
        }

        protected override void createActionList()
        {
            executeAction(() => { Enabled = false; });
            playerMessage("I won't have enough time to get the key and come back to the bush.");
            playerMessage("But if I try harder to recover my powers, maybe...");
            executeAction(() =>
            {
                var systemManager = Core.getGlobalManager<SystemManager>();
                systemManager.setSwitch("introducedSlowmotion", true);
                var player = systemManager.playerEntity;
                (player.scene as SceneMap)?.sendMessageToExtensions("showInstructionsMessage");
            });
            closePlayerMessage();
        }

        protected override void loadTexture() { }
    }
}
