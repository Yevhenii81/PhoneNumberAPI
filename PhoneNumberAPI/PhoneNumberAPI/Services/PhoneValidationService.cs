using PhoneNumbers;

namespace PhoneNumberApi.Services;

public class PhoneValidationService
{
    private readonly PhoneNumberUtil _phoneUtil;

    public PhoneValidationService()
    {
        _phoneUtil = PhoneNumberUtil.GetInstance();
    }

    public string? ValidateAndNormalize(string input)
    {
        try
        {
            var phoneNumber = _phoneUtil.Parse(input, null);

            if (!_phoneUtil.IsValidNumber(phoneNumber))
                return null;

            return _phoneUtil.Format(phoneNumber, PhoneNumberFormat.E164);
        }
        catch (NumberParseException)
        {
            return null;
        }
    }

    public string? GetRegion(string input)
    {
        try
        {
            var phoneNumber = _phoneUtil.Parse(input, null);
            return _phoneUtil.GetRegionCodeForNumber(phoneNumber);
        }
        catch (NumberParseException)
        {
            return null;
        }
    }
}