using ShipIt.Core.Enums;

public class SourceRepositoryDto
{
    public SourceRepositoryProvider Provider { get; set; }

    public string Owner { get; set; } = string.Empty;

    public string RepositoryName { get; set; } = string.Empty;

    public string DefaultBranch { get; set; } = "main";

    public bool IsPrivate { get; set; }
}