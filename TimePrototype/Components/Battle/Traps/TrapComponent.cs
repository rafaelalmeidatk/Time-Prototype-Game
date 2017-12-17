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
        // Cooldown

        public float cooldown;

        protected TrapComponent(string activatorName)
        {
            this.activatorName = activatorName;
        }

        public override void onAddedToEntity()
        {
            entity.setTag(SceneMap.TRAPS);
        }

        public abstract void doAction();

        public void update()
        {
            if (cooldown > 0.0f)
                cooldown -= Time.deltaTime;
        }
    }
}
