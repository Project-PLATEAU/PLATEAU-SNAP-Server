using Swashbuckle.AspNetCore.Annotations;

namespace PLATEAU.Snap.Models.Common;

public class PageData<T>
{
    [SwaggerSchema("データの総数", ReadOnly = true, Nullable = false)]
    public int TotalCount => Values.TotalCount;

    [SwaggerSchema("1ページのサイズ", ReadOnly = true, Nullable = false)]
    public int PageSize => Values.PageSize;

    [SwaggerSchema("現在のページ", ReadOnly = true, Nullable = false)]
    public int CurrentPage => Values.CurrentPage;

    [SwaggerSchema("総ページ数", ReadOnly = true, Nullable = false)]
    public int TotalPages => Values.TotalPages;

    [SwaggerSchema("次ページがあるかどうか", ReadOnly = true, Nullable = false)]
    public bool HasNext => Values.HasNext;

    [SwaggerSchema("データの配列", ReadOnly = true, Nullable = false)]
    public PageList<T> Values { get; set; }

    public PageData()
    {
        Values = new PageList<T>(new List<T>(), 0, 1, 1);
    }

    public PageData(PageList<T> values)
    {
        this.Values = values;
    }
}
