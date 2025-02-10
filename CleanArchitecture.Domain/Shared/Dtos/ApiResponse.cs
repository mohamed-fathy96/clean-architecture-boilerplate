namespace CleanArchitecture.Domain.Shared.Dtos;

public class ApiResponse<TData> where TData : class
{
    public int Status { get; set; }
    public string Title { get; set; }
    public string Detail { get; set; }
    public List<string> Errors { get; set; }
    public TData Data { get; set; }

    public static ApiResponse<TData> Success(TData data)
    {
        return new ApiResponse<TData>
        {
            Status = 200,
            Title = "Success",
            Data = data
        };
    }
}

public class ApiResponse
{
    public int Status { get; set; }
    public string Title { get; set; }
    public string Detail { get; set; }
    public List<string> Errors { get; set; }

    public static ApiResponse Success()
    {
        return new ApiResponse
        {
            Status = 200,
            Title = "Success",
        };
    }
}
