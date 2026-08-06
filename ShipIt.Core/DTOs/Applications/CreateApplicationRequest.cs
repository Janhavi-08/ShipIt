public class CreateApplicationRequest
{
    public ApplicationDto Application { get; set; } = new();

    public SourceRepositoryDto SourceRepository { get; set; } = new();
}