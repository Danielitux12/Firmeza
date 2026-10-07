using System.ComponentModel.DataAnnotations;

namespace Firmeza.Domain.Enums;

public enum ProductStatus
{
    [Display(Name = "Disponible")]
    Available = 1,

    [Display(Name = "No disponible")]
    Unavailable = 2,

    [Display(Name = "Descontinuado")]
    Discontinued = 3
}
