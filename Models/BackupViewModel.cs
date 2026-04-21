namespace eCommerceMotoRepuestos.Models;

public class BackupViewModel
{
    public required string BackupFilePath { get; init; }
    public bool BackupExists { get; init; }
    public DateTime? BackupLastWriteLocal { get; init; }
}
