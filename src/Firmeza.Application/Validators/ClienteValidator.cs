using FluentValidation;
using Firmeza.Application.DTOs.Clientes;

namespace Firmeza.Application.Validators;

public class ClienteValidator : AbstractValidator<SaveClienteDto>
{
    public ClienteValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .Length(3, 150).WithMessage("El nombre debe tener entre 3 y 150 caracteres.");

        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("El número de documento es obligatorio.")
            .Length(5, 50).WithMessage("El documento debe tener entre 5 y 50 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("Ingresa un correo electrónico válido.")
            .MaximumLength(150).WithMessage("El correo no puede exceder 150 caracteres.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("El teléfono es obligatorio.")
            .Matches(@"^[0-9+\-\s]{7,30}$").WithMessage("Ingresa un número de teléfono válido (solo dígitos y guiones).");

        RuleFor(x => x.Age)
            .InclusiveBetween(0, 120).WithMessage("La edad debe estar entre 0 y 120 años.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("La dirección es obligatoria.")
            .MaximumLength(250).WithMessage("La dirección no puede exceder 250 caracteres.");
    }
}