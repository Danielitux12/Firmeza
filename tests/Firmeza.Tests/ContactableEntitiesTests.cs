using Firmeza.Domain.Entities;

namespace Firmeza.Tests;

public class ContactableEntitiesTests
{
    [Fact]
    public void Empresa_IsValid_RequiresNameNitAndValidEmail()
    {
        var empresa = new Empresa
        {
            Name = "Materiales Norte",
            Nit = "900123456-1",
            Email = "ventas@materiales.example"
        };

        Assert.True(empresa.IsValid());
        Assert.False(new Empresa { Name = "Materiales Norte", Nit = "", Email = "bad" }.IsValid());
    }

    [Fact]
    public void Trabajador_IsValid_RequiresWorkDetailsAndValidEmail()
    {
        var trabajador = new Trabajador
        {
            Name = "Ana Ruiz",
            DocumentNumber = "12345678",
            Position = "Vendedora",
            Salary = 1500,
            Email = "ana@firmeza.example"
        };

        Assert.True(trabajador.IsValid());
        Assert.False(new Trabajador { Name = "Ana Ruiz", DocumentNumber = "", Position = "", Salary = -1, Email = "bad" }.IsValid());
    }

    [Fact]
    public void EntityBase_ActivateAndDeactivate_ToggleIsActive()
    {
        var cliente = new Cliente();

        Assert.True(cliente.IsActive);
        cliente.Deactivate();
        Assert.False(cliente.IsActive);
        cliente.Activate();
        Assert.True(cliente.IsActive);
    }
}