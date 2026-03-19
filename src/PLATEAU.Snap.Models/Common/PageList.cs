using Microsoft.EntityFrameworkCore;

namespace PLATEAU.Snap.Models.Common;

public class PageList<T> : List<T>
{
    public int CurrentPage { get; private set; }

    public int TotalPages { get; private set; }

    public int PageSize { get; private set; }

    public int TotalCount { get; private set; }

    public bool HasPrevious => CurrentPage > 1;

    public bool HasNext => CurrentPage < TotalPages;

    public PageList(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        AddRange(items);
    }

    public static PageList<T> ToPageList(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return new PageList<T>(items, count, pageNumber, pageSize);
    }

    public static async Task<PageList<T>> ToPageListAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PageList<T>(items, count, pageNumber, pageSize);
    }

    public static PageList<TResult> ToPageListWithSelect<TResult>(IQueryable<T> source, Func<T, TResult> selector, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return new PageList<TResult>(items.Select(x => selector.Invoke(x)).ToList(), count, pageNumber, pageSize);
    }

    public static async Task<PageList<TResult>> ToPageListWithSelectAsync<TResult>(IQueryable<T> source, Func<T, TResult> selector, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PageList<TResult>(items.Select(x => selector.Invoke(x)).ToList(), count, pageNumber, pageSize);
    }

    public static PageList<T> ToPageList<TSource>(IQueryable<TSource> source, Func<TSource, T> selector, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(x => selector(x)).ToList();
        return new PageList<T>(items, count, pageNumber, pageSize);
    }

    public static async Task<PageList<T>> ToPageListAsync<TSource>(IQueryable<TSource> source, Func<TSource, T> selector, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(x => selector(x)).ToListAsync();
        return new PageList<T>(items, count, pageNumber, pageSize);
    }

    public PageData<T> CreatePageData()
    {
        return new PageData<T>(this);
    }

    public PageData<TResult> CreatePageDataWithSelect<TResult>(Func<T, TResult> selector)
    {
        return new PageData<TResult>(new PageList<TResult>(this.Select(selector.Invoke).ToList(), TotalCount, CurrentPage, PageSize));
    }

    public async Task<PageData<TResult>> CreatePageDataWithSelectAsync<TResult>(Func<T, Task<TResult>> asyncSelector)
    {
        // 非同期セレクタをすべての要素に適用し、並列に実行
        var selectedList = await Task.WhenAll(this.Select(asyncSelector));

        return new PageData<TResult>(
            new PageList<TResult>(selectedList.ToList(), TotalCount, CurrentPage, PageSize)
        );
    }
}
