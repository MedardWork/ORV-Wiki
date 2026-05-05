namespace ORVWiki.Application.Common;

public record PaginationParams(int Page = 1, int PageSize = 20)
{
    public int SafePage => Page < 1 ? 1 : Page;
    public int SafePageSize => PageSize switch
    {
        < 1 => 1,
        > 100 => 100,
        _ => PageSize
    };

    public int Skip => (SafePage - 1) * SafePageSize;
}
