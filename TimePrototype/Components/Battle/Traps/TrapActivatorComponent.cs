using Nez;

namespace TimePrototype.Components.Battle.Traps
{
    class TrapActivatorComponent : Component
    {
        public string name { get; }

        public TrapActivatorComponent(string name)
        {
            this.name = name;
        }
    }
}
