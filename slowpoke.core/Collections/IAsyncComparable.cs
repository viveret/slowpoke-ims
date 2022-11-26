namespace SlowPokeIMS.Core.Collections;


public interface IAsyncComparable
{
    Task<int> CompareTo(object other);
}

public interface IAsyncComparable<T>
{
    Task<int> CompareTo(T other);
}