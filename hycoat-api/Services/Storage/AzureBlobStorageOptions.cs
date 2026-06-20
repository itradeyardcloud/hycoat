namespace HycoatApi.Services.Storage;

public class AzureBlobStorageOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "hycoat-files";
    public bool CreateContainerIfNotExists { get; set; } = true;
}
