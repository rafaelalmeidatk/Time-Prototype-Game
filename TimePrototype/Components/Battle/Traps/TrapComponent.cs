using System.Runtime.Remoting;
using Nez;
using Nez.Sprites;
using TimePrototype.Scenes;

namespace TimePrototype.Components.Battle.Traps
{
    public abstract class TrapComponent : Component, IUpdatable
    {
        //--------------------------------------------------
        // Sprite

        protected Sprite sprite;

        //--------------------------------------------------
        // Activator Name

        public string activatorName;

        //--------------------------------------------------
        // Automatic

        public bool isAuto;
        public float delay;

        //--------------------------------------------------
        // Cooldown

        public float cooldown;
        public float initialDelay;

        //----------------------//------------------------//

        protected TrapComponent(string activatorName, bool isAuto, float delay, float initialDelay)
        {
            this.activatorName = activatorName;
            this.isAuto = isAuto;
            this.delay = delay;
            this.initialDelay = initialDelay;
            if (isAuto)
            {
                cooldown = initialDelay == 0.0f ? delay : initialDelay;
            }
        }

        public override void onAddedToEntity()
        {
            entity.setTag(SceneMap.TRAPS);
        }

        public abstract void doAction();

        public void update()
        {
            if (cooldown > 0.0f)
            {
                cooldown -= Time.deltaTime;
                if (isAuto && cooldown <= 0.0f)
                {
                    cooldown = delay;
                    doAction();
                }
            }
        }
    }
}
