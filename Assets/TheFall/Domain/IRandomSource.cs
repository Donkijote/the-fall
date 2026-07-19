namespace TheFall.Domain
{
    public interface IRandomSource
    {
        int NextInt(int exclusiveUpperBound);
    }
}
