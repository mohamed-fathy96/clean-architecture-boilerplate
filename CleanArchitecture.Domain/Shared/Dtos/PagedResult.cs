namespace CleanArchitecture.Domain.Shared.Dtos;

public class PagedResult<T>
{
    public IEnumerable<T> Data { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }

    public PagedResult(IEnumerable<T> data, int page, int pageSize, int totalCount, int totalPages)
    {
        Data = data;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = totalPages;
    }
}
