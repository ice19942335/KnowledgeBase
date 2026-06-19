namespace KnowledgeBase.SharedKernel.Storage;

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>
    /// Root directory shared by services that produce and consume document blobs.
    /// </summary>
    public string RootPath { get; set; } = "/var/lib/knowledgebase/blobs";
}
