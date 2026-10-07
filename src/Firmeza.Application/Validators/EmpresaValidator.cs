using FluentValidation;
using Firmeza.Application.DTOs.Empresas;

namespace Firmeza.Application.Validators;

public class EmpresaValidator : AbstractValidator<SaveEmpresaDto>
{
    public EmpresaValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("La razón social o nombre es obligatorio.")
            .Length(3, 150).WithMessage("El nombre debe tener entre 3 y 150 caracteres.");

        RuleFor(x => x.Nit)
            .NotEmpty().WithMessage("El NIT es obligatorio.")
            .Length(5, 50).WithMessage("El NIT debe tener entre 5 y 50 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("Ingresa un correo electrónico válido.")
            .MaximumLength(150).WithMessage("El correo no puede exceder 150 caracteres.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("El teléfono es obligatorio.")
            .Matches(@"^[0-9+\-\s]{7,30}$").WithMessage("Ingresa un número de teléfono válido (solo dígitos y guiones).");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("La dirección es obligatoria.")
            .MaximumLength(250).WithMessage("La dirección no puede exceder 250 caracteres.");
    }
}