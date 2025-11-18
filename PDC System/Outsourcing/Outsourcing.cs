public class Outsourcinginfo
{
    public string? Type1 { get; set; }

    public string? PlateName { get; set; }
    public string? PlateEmail { get; set; }
    public string? PlateContact { get; set; }
    public string? PlateCost { get; set; }

    public string? DigitalName { get; set; }
    public string? DigitalEmail { get; set; }
    public string? DigitalContact { get; set; }

    public string? AnyName { get; set; }
    public string? AnyEmail { get; set; }
    public string? AnyContact { get; set; }

    public string? Action { get; set; } // You can set "Edit", "Delete", etc.

    // Combine all names into one column
    public string Name
    {
        get
        {
            return string.Join(", ", new[] { PlateName, DigitalName, AnyName }.Where(n => !string.IsNullOrEmpty(n)));
        }
    }

    // Combine all emails into one column
    public string Email
    {
        get
        {
            return string.Join(", ", new[] { PlateEmail, DigitalEmail, AnyEmail }.Where(e => !string.IsNullOrEmpty(e)));
        }
    }

    // Combine all contacts into one column
    public string ContactNo
    {
        get
        {
            return string.Join(", ", new[] { PlateContact, DigitalContact, AnyContact }.Where(c => !string.IsNullOrEmpty(c)));
        }
    }

    // Type column
    public string Type => Type1 ?? "";
}
