namespace LifeGame3D
{
    public struct LifeGameEntity
    {
        public uint value;
    }

    public static class LifeGameEntityExtension
    {
        public static void AddFlag(ref this LifeGameEntity entity, LifeGameFlags flags)
        {
            entity.value |= (uint)flags;
        }

        public static bool IsFlag(ref this LifeGameEntity entity, LifeGameFlags flags)
        {
            return (entity.value & (int)flags) != 0;
        }
    }
}
