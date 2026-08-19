namespace VGS.RetailOS.Shared.BuildingBlocks.Pagination;

public class PaginatedList<T>
{
    public IReadOnlyCollection<T> Items { get; }
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages { get; }

    public PaginatedList(IReadOnlyCollection<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}
