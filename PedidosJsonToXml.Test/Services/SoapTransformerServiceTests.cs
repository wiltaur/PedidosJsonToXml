using Microsoft.Extensions.Options;
using PedidosJsonToXml.Api.Services;
using PedidosJsonToXml.Api.Models;
using PedidosJsonToXml.Api.Persistence;

namespace PedidosJsonToXml.Test.Services;

public class SoapTransformerServiceTests
{
    private static SoapTransformerService CreateService()
    {
        var settings = new AcmeSoapServiceSettings
        {
            // Valores usados en las pruebas: pueden ser cualquier URI válido de namespace
            SoapEnv = "http://schemas.xmlsoap.org/soap/envelope/",
            EnvNs = "http://acme.example.com/env"
        };

        return new SoapTransformerService(Options.Create(settings));
    }

    [Fact]
    public void ConvertirPedidoToSoap_ReturnsExpectedXml()
    {
        // Arrange
        var service = CreateService();
        var data = new EnviarPedidoData
        {
            NumPedido = "12345",
            CantidadPedido = 10,
            CodigoEAN = "7890123456789",
            NombreProducto = "ProductoPrueba",
            NumDocumento = "C-987654",
            Direccion = "Calle Falsa 123"
        };

        // Act
        var doc = service.ConvertirPedidoToSoap(data);

        // Assert - estructura general
        Assert.NotNull(doc);
        Assert.NotNull(doc.Root);
        Assert.Equal("Envelope", doc.Root.Name.LocalName);
        Assert.Equal("http://schemas.xmlsoap.org/soap/envelope/", doc.Root.Name.NamespaceName);

        // Buscar el request
        var request = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "EnvioPedidoRequest");
        Assert.NotNull(request);

        // Verificar valores de los elementos dentro del request
        string GetValue(string localName) =>
            request.Elements().FirstOrDefault(e => e.Name.LocalName == localName)?.Value ?? string.Empty;

        Assert.Equal(data.NumPedido, GetValue("pedido"));
        Assert.Equal(data.CantidadPedido, Convert.ToInt32(GetValue("Cantidad")));
        Assert.Equal(data.CodigoEAN, GetValue("EAN"));
        Assert.Equal(data.NombreProducto, GetValue("Producto"));
        Assert.Equal(data.NumDocumento, GetValue("Cedula"));
        Assert.Equal(data.Direccion, GetValue("Direccion"));
    }

    [Fact]
    public void ConvertirSoapToResponseJson_ParsesResponseCorrectly()
    {
        // Arrange
        var service = CreateService();

        var xmlResponse = @"<?xml version=""1.0"" encoding=""utf-8""?>
                <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:env=""http://acme.example.com/env"">
                  <soapenv:Body>
                    <env:EnvioPedidoResponse>
                      <Codigo>OK-200</Codigo>
                      <Mensaje>Pedido recibido correctamente</Mensaje>
                    </env:EnvioPedidoResponse>
                  </soapenv:Body>
                </soapenv:Envelope>";

        // Act
        var result = service.ConvertirSoapToResponseJson(xmlResponse);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.EnviarPedidoRespuesta);
        Assert.Equal("OK-200", result.EnviarPedidoRespuesta.CodigoEnvio);
        Assert.Equal("Pedido recibido correctamente", result.EnviarPedidoRespuesta.Estado);
    }
}