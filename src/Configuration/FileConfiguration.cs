namespace codecrafters_http_server.src.Configuration;

public interface IFileConfiguration
{
  string FilesDirectory { get; }
}

public class FileConfiguration(string filesDirectory) : IFileConfiguration
{
  public string FilesDirectory { get; set; } = filesDirectory;
}
