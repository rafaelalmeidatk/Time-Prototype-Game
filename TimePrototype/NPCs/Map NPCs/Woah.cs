namespace TimePrototype.NPCs
{
    public class Woah : NpcBase
    {
        public Woah(string name) : base(name)
        {
            RunOnTouch = true;
            Invisible = true;
        }

        protected override void createActionList()
        {
            executeAction(() => { Enabled = false; });
            playerMessage("Woah!");
            closePlayerMessage();
        }

        protected override void loadTexture() { }
    }
}
