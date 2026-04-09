namespace PhoneNumberApi.Models;

public class PhoneNumber
{
    public int Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}