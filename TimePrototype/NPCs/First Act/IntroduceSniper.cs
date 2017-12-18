using Nez;
using TimePrototype.Managers;

namespace TimePrototype.NPCs
{
    public class IntroduceSniper : NpcBase
    {
        public IntroduceSniper(string name) : base(name)
        {
            RunOnTouch = true;
            Invisible = true;
            Enabled = !Core.getGlobalManager<SystemManager>().getSwitch("introducedSnipers");
        }

        protected override void createActionList()
        {
            executeAction(() =>
            {
                Enabled = false;
                Core.getGlobalManager<SystemManager>().setSwitch("introducedSnipers", true);
            });
            playerMessage("Better pay attention to these snipers, if they see me I'm dead.");
            closePlayerMessage();
        }

        protected override void loadTexture() { }
    }
}
