using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez;
using TimePrototype.Managers;
using TimePrototype.Scenes;

namespace TimePrototype.NPCs
{
    public class IntroduceBush : NpcBase
    {
        public IntroduceBush(string name) : base(name)
        {
            RunOnTouch = true;
            Invisible = true;
            Enabled = !Core.getGlobalManager<SystemManager>().getSwitch("introducedBush");
        }

        protected override void createActionList()
        {
            executeAction(() =>
            {
                Enabled = false;
                Core.getGlobalManager<SystemManager>().setSwitch("introducedBush", true);
            });
            playerMessage("I can't let they see me.");
            playerMessage("These bushes may work.");
            executeAction(() =>
            {
                var systemManager = Core.getGlobalManager<SystemManager>();
                var player = systemManager.playerEntity;
                (player.scene as SceneMap)?.sendMessageToExtensions("showInstructionsMessage");
            });
            closePlayerMessage();
        }

        protected override void loadTexture() { }
    }
}
