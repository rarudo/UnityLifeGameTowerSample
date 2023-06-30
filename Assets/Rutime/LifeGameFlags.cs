namespace LifeGame3D
{
    [System.Flags]
    public enum LifeGameFlags
    {
        Alive = 1 << 0,
        Masked = 1 << 1,
    }
}
