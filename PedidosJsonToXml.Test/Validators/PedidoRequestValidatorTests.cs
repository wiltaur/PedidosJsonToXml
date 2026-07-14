using System.Reflection;
using PedidosJsonToXml.Api.Models;
using PedidosJsonToXml.Api.Validators;

namespace PedidosJsonToXml.Test.Validators;

public class PedidoRequestValidatorTests
{
    private static PedidoRequestJson BuildValidRequest()
    {
        var request = new PedidoRequestJson();

        var enviarPedidoProp = typeof(PedidoRequestJson).GetProperty("EnviarPedido",
            BindingFlags.Public | BindingFlags.Instance);

        if (enviarPedidoProp == null)
            throw new InvalidOperationException("PedidoRequestJson does not have a public 'EnviarPedido' property.");

        var enviarPedidoType = enviarPedidoProp.PropertyType;
        var enviarPedidoInstance = Activator.CreateInstance(enviarPedidoType) ?? throw new InvalidOperationException("Unable to create instance of EnviarPedido type.");

        // Helper to set property value by name
        void Set(string name, object value)
        {
            var p = enviarPedidoType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (p == null) throw new InvalidOperationException($"EnviarPedido type is missing property '{name}'.");
            p.SetValue(enviarPedidoInstance, value);
        }

        Set("NumPedido", "12345");
        Set("CantidadPedido", 1);
        Set("CodigoEAN", "1234567890123");
        Set("NombreProducto", "Producto de prueba");
        Set("NumDocumento", "A12345678");
        Set("Direccion", "Calle Ejemplo 123");

        enviarPedidoProp.SetValue(request, enviarPedidoInstance);

        return request;
    }

    private static FluentValidation.Results.ValidationResult Validate(PedidoRequestJson request)
    {
        var validator = new PedidoRequestValidator();
        return validator.Validate(request);
    }

    [Fact]
    public void When_EnviarPedido_IsNull_ShouldHaveRequiredError()
    {
        var request = new PedidoRequestJson
        {
            // EnviarPedido left as null
        };

        var result = Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EnviarPedido" &&
                                           e.ErrorMessage == "El objeto 'enviarPedido' es obligatorio.");
    }

    [Fact]
    public void ValidRequest_ShouldBeValid()
    {
        var request = BuildValidRequest();

        var result = Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void NumPedido_Empty_ShouldHaveError()
    {
        var request = BuildValidRequest();
        SetEnviarPedidoProperty(request, "NumPedido", string.Empty);

        var result = Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EnviarPedido.NumPedido" &&
                                           e.ErrorMessage == "El Número del pedido es obligatorio.");
    }

    [Fact]
    public void NumPedido_NonNumeric_ShouldHaveError()
    {
        var request = BuildValidRequest();
        SetEnviarPedidoProperty(request, "NumPedido", "ABC123");

        var result = Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Equals("EnviarPedido.NumPedido"));
    }

    [Fact]
    public void CodigoEAN_Empty_ShouldHaveError()
    {
        var request = BuildValidRequest();
        SetEnviarPedidoProperty(request, "CodigoEAN", string.Empty);

        var result = Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EnviarPedido.CodigoEAN");
    }

    [Fact]
    public void CodigoEAN_NonNumeric_ShouldHaveError()
    {
        var request = BuildValidRequest();
        SetEnviarPedidoProperty(request, "CodigoEAN", "EANABC");

        var result = Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EnviarPedido.CodigoEAN");
    }

    [Fact]
    public void NombreProducto_Empty_ShouldHaveError()
    {
        var request = BuildValidRequest();
        SetEnviarPedidoProperty(request, "NombreProducto", string.Empty);

        var result = Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EnviarPedido.NombreProducto");
    }

    [Fact]
    public void NumDocumento_Empty_ShouldHaveError()
    {
        var request = BuildValidRequest();
        SetEnviarPedidoProperty(request, "NumDocumento", string.Empty);

        var result = Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EnviarPedido.NumDocumento");
    }

    [Fact]
    public void Direccion_Empty_ShouldHaveError()
    {
        var request = BuildValidRequest();
        SetEnviarPedidoProperty(request, "Direccion", string.Empty);

        var result = Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EnviarPedido.Direccion");
    }

    // Reflection helper to set EnviarPedido.<propertyName> on a PedidoRequestJson instance
    private static void SetEnviarPedidoProperty(PedidoRequestJson request, string propertyName, object value)
    {
        var enviarPedidoProp = typeof(PedidoRequestJson).GetProperty("EnviarPedido",
            BindingFlags.Public | BindingFlags.Instance);

        if (enviarPedidoProp == null)
            throw new InvalidOperationException("PedidoRequestJson does not have a public 'EnviarPedido' property.");

        var enviarPedidoInstance = enviarPedidoProp.GetValue(request)
                                 ?? throw new InvalidOperationException("EnviarPedido instance is null. Ensure BuildValidRequest created it.");

        var prop = enviarPedidoInstance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop == null)
            throw new InvalidOperationException($"EnviarPedido type does not contain a property named '{propertyName}'.");

        prop.SetValue(enviarPedidoInstance, value);
    }
}