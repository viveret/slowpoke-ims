namespace SlowPokeIMS.Core.Collections;


public interface IAsyncEquatable
{
    Task<bool> Equals(object other);
}

public interface IAsyncEquatable<T>
{
    Task<bool> Equals(T other);
}