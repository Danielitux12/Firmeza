using System.Net.Mail;

namespace Firmeza.Domain.Entities;

public abstract class ContactableEntity : NamedEntity
{
    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    protected bool HasValidEmail()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            return false;
        }

        try
        {
            var address = new MailAddress(Email);
            return string.Equals(
                address.Address,
                Email,
                StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}