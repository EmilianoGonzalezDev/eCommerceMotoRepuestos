namespace eCommerceMotoRepuestos.Models;

/// <summary>
/// Public demo settings (section "Demo"). When enabled, the demo accounts are shown on the login page,
/// cannot be edited, and the data can be reset to the seed.
/// </summary>
public class DemoOptions
{
    public const string SectionName = "Demo";

    public bool Enabled { get; set; }

    /// <summary>
    /// Secret expected in the X-Demo-Reset-Token header by the scheduled reset. Empty disables that endpoint.
    /// </summary>
    public string ResetToken { get; set; } = string.Empty;

    public List<DemoAccount> Accounts { get; set; } = [];

    public bool IsDemoAccount(string? email)
    {
        return Enabled
            && !string.IsNullOrWhiteSpace(email)
            && Accounts.Any(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase));
    }
}

public class DemoAccount
{
    public string Label { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
