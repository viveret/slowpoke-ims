namespace SlowPokeIMS.Core.Collections;


public static class AsyncLinq
{
    public static async Task<IEnumerable<T>> WhereAsync<T>(this IEnumerable<T> list, Func<T, Task<bool>> value)
    {
        return (await Task.WhenAll(list.Select(async v => (v, await value(v))))).Where(v => v.Item2).Select(v => v.v);
    }

    public static async Task<T> SingleAsync<T>(this IEnumerable<T> list, Func<T, Task<bool>> value)
    {
        return (await Task.WhenAll(list.Select(async v => (v, await value(v))))).Single(v => v.Item2).v;
    }

    public static async Task<T> SingleOrDefaultAsync<T>(this IEnumerable<T> list, Func<T, Task<bool>> value)
    {
        return (await Task.WhenAll(list.Select(async v => (v, await value(v))))).SingleOrDefault(v => v.Item2).v;
    }


    public static async Task<IEnumerable<T>> OrderByAsync<T, TOrderBy>(this IEnumerable<T> list, Func<T, Task<TOrderBy>> value)
    {
        return (await Task.WhenAll(list.Select(async v => (v, await value(v)))))
                .OrderBy(v => v.Item2).Select(v => v.v);
    }

    public static async Task<IEnumerable<T>> OrderByDescendingAsync<T, TOrderBy>(this IEnumerable<T> list, Func<T, Task<TOrderBy>> value)
    {
        return (await Task.WhenAll(list.Select(async v => (v, await value(v)))))
                .OrderByDescending(v => v.Item2).Select(v => v.v);
    }

    public static async Task<IEnumerable<T>> OrderByAsync<T, TOrderBy>(this IEnumerable<T> list, Func<T, Task<TOrderBy>> value, IComparer<TOrderBy> comparer)
    {
        return (await Task.WhenAll(list.Select(async v => (v, await value(v)))))
                .OrderBy(v => v.Item2, comparer).Select(v => v.v);
    }

    public static async Task<IEnumerable<T>> OrderByDescendingAsync<T, TOrderBy>(this IEnumerable<T> list, Func<T, Task<TOrderBy>> value, IComparer<TOrderBy> comparer)
    {
        return (await Task.WhenAll(list.Select(async v => (v, await value(v)))))
                .OrderByDescending(v => v.Item2, comparer).Select(v => v.v);
    }




    public static async Task<IEnumerable<T>> ConcatAsync<T>(this Task<IEnumerable<T>> list, Task<IEnumerable<T>> list2)
    {
        return (await list).Concat(await list2);
    }

    public static async Task<IEnumerable<T>> ConcatAsync<T>(this Task<IEnumerable<T>> list, IEnumerable<T> list2)
    {
        return (await list).Concat(list2);
    }
}