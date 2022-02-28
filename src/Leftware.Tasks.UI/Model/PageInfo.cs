namespace Leftware.Tasks.UI.Model;

internal class PageInfo
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }

    public PageInfo()
    {
        CurrentPage = 1;
        TotalPages = 1;
        PageSize = int.MaxValue;
    }
}
