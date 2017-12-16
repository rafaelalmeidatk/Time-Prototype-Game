using Nez;
using TimePrototype.Scenes;

namespace TimePrototype.Components.Map
{
    class BushComponent : Component
    {
        public BoxCollider collider;

        public override void onAddedToEntity()
        {
            collider = entity.getComponent<BoxCollider>();
            Flags.setFlagExclusive(ref collider.physicsLayer, SceneMap.BUSHES_LAYER);

            entity.setTag(SceneMap.BUSHES);
        }
    }
}
