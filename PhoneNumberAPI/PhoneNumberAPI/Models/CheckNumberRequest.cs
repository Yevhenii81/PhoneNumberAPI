using System.ComponentModel.DataAnnotations;

namespace PhoneNumberApi.Models;

public class CheckNumberRequest
{
    [Required(ErrorMessage = "Phone number is required")]
    public string PhoneNumber { get; set; } = string.Empty;
}