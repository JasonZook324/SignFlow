namespace SignFlow.Application.Options;

public class StorageOptions
{
    public const string SectionName = "Storage";
    public string BlobConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "assets";
}
