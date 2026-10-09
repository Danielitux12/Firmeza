using FluentValidation;
using Firmeza.Application.DTOs.Empresas;

namespace Firmeza.Application.Validators;

public class EmpresaValidator : AbstractValidator<SaveEmpresaDto>
{
    public EmpresaValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("La razón social o nombre de la empresa es obligatorio.")
            .Length(3, 150).WithMessage("La razón social debe tener entre 3 y 150 caracteres.")
            .Matches(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑüÜ\s\.,\-&'’()#]+$").WithMessage("La razón social contiene caracteres inválidos.");

        RuleFor(x => x.Nit)
            .NotEmpty().WithMessage("El NIT o identificación fiscal es obligatorio.")
            .Matches(@"^([0-9]{1,3}(\.[0-9]{3}){1,4}|[0-9]{5,15})(-[0-9kK])?$")
            .WithMessage("El NIT debe ser numérico o tener un formato fiscal válido (ej. 900123456-1 o 900.123.456-1).")
            .Length(5, 50).WithMessage("El NIT debe tener entre 5 y 50 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico de contacto es obligatorio.")
            .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
            .WithMessage("Ingresa un correo electrónico corporativo válido (ej. contacto@empresa.com).")
            .MaximumLength(150).WithMessage("El correo no puede exceder 150 caracteres.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("El teléfono de contacto es obligatorio.")
            .Matches(@"^[0-9+\-\s()]{7,20}$").WithMessage("Ingresa un número de teléfono válido (entre 7 y 20 dígitos y símbolos permitidos +, -, (, )).");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("La dirección principal es obligatoria.")
            .Length(5, 250).WithMessage("La dirección debe tener entre 5 y 250 caracteres.");
    }
}