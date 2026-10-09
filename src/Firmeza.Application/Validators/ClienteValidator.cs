using FluentValidation;
using Firmeza.Application.DTOs.Clientes;

namespace Firmeza.Application.Validators;

public class ClienteValidator : AbstractValidator<SaveClienteDto>
{
    public ClienteValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre completo es obligatorio.")
            .Length(3, 150).WithMessage("El nombre debe tener entre 3 y 150 caracteres.")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s\.,\-']+$").WithMessage("El nombre solo debe contener letras y espacios.");

        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("El número de documento es obligatorio.")
            .Matches(@"^[0-9]{6,12}$").WithMessage("El documento de identidad debe ser numérico y tener entre 6 y 12 dígitos.")
            .Length(6, 12).WithMessage("El documento debe tener entre 6 y 12 dígitos.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
            .WithMessage("Ingresa un correo electrónico válido (ej. usuario@dominio.com).")
            .MaximumLength(150).WithMessage("El correo no puede exceder 150 caracteres.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("El teléfono es obligatorio.")
            .Matches(@"^[0-9+\-\s()]{7,20}$").WithMessage("Ingresa un número de teléfono válido (entre 7 y 20 dígitos y símbolos permitidos +, -, (, )).");

        RuleFor(x => x.Age)
            .InclusiveBetween(18, 120).WithMessage("El cliente debe ser mayor de edad (entre 18 y 120 años).");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("La dirección de residencia es obligatoria.")
            .Length(5, 250).WithMessage("La dirección debe tener entre 5 y 250 caracteres.");
    }
}