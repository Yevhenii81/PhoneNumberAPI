using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneNumberApi.Data;
using PhoneNumberApi.Models;
using PhoneNumberApi.Services;

namespace PhoneNumberApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PhoneNumberController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PhoneValidationService _validationService;
    public PhoneNumberController(
        AppDbContext context,
        PhoneValidationService validationService)
    {
        _context = context;
        _validationService = validationService;
    }

    [HttpPost("check_number")]
    public async Task<IActionResult> CheckNumber([FromBody] CheckNumberRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var normalizedNumber = _validationService.ValidateAndNormalize(request.PhoneNumber);

        if (normalizedNumber == null)
            return BadRequest(new { error = "Invalid phone number format. Please include country code (e.g. +1, +380, +44)" });

        var region = _validationService.GetRegion(request.PhoneNumber);

        var exists = await _context.PhoneNumbers
            .AnyAsync(p => p.Number == normalizedNumber);

        if (exists)
            return Conflict(new { error = "Phone number already exists.", number = normalizedNumber, region });

        var phoneNumber = new PhoneNumber { Number = normalizedNumber };
        _context.PhoneNumbers.Add(phoneNumber);
        await _context.SaveChangesAsync();

        return StatusCode(201, new
        {
            message = "Phone number added successfully",
            number = normalizedNumber,
            region 
        });
    }
}