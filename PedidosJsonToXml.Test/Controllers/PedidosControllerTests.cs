using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PedidosJsonToXml.Api.Controllers;
using PedidosJsonToXml.Api.Interfaces;
using PedidosJsonToXml.Api.Models;
using System.Xml.Linq;

namespace PedidosJsonToXml.Test.Controllers;

public class PedidosControllerTests
{
    private readonly Mock<IValidator<PedidoRequestJson>> _validatorMock;
    private readonly Mock<ISoapTransformerService> _transformerMock;
    private readonly Mock<IAcmeSoapClientService> _soapClientMock;
    private readonly PedidosController _controller;

    public PedidosControllerTests()
    {
        _validatorMock = new Mock<IValidator<PedidoRequestJson>>();
        _transformerMock = new Mock<ISoapTransformerService>();
        _soapClientMock = new Mock<IAcmeSoapClientService>();
        _controller = new PedidosController(_transformerMock.Object, _soapClientMock.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task EnviarPedido_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var failures = new[]
        {
            new ValidationFailure("FieldA", "must not be empty")
        };
        var invalidResult = new ValidationResult(failures);

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<PedidoRequestJson>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invalidResult);

        var request = new PedidoRequestJson();

        // Act
        var actionResult = await _controller.EnviarPedido(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        Assert.NotNull(badRequestResult.Value);

        // Verificar que no se llamó al transformer ni al cliente SOAP
        _transformerMock.Verify(t => t.ConvertirPedidoToSoap(It.IsAny<EnviarPedidoData>()), Times.Never);
        _soapClientMock.Verify(c => c.EnviarPedidoAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task EnviarPedido_ReturnsOk_WithTransformedResult_WhenValidationSucceeds()
    {
        // Arrange
        var validResult = new ValidationResult();
        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<PedidoRequestJson>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validResult);

        var soapEnvelopeXmlString = "<Envelope><Body>Pedido</Body></Envelope>";
        var soapEnvelopeXml = XDocument.Parse(soapEnvelopeXmlString);
        var soapResponseXml = "<Response><Status>OK</Status></Response>";
        var resultadoEsperado = new PedidoResponseJson
        {
            EnviarPedidoRespuesta = new()
            {
                CodigoEnvio = "12345",
                Estado = "OK"
            }
        };

        _transformerMock
            .Setup(t => t.ConvertirPedidoToSoap(It.IsAny<EnviarPedidoData>()))
            .Returns(soapEnvelopeXml);

        _soapClientMock
            .Setup(c => c.EnviarPedidoAsync(It.Is<string>(s => s.Contains("Pedido") || s.Contains("Envelope"))))
            .ReturnsAsync(soapResponseXml);

        _transformerMock
            .Setup(t => t.ConvertirSoapToResponseJson(It.Is<string>(s => s == soapResponseXml)))
            .Returns(resultadoEsperado);

        var request = new PedidoRequestJson();

        // Act
        var actionResult = await _controller.EnviarPedido(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(resultadoEsperado, okResult.Value);

        // Verificar invocaciones
        _transformerMock.Verify(t => t.ConvertirPedidoToSoap(It.IsAny<EnviarPedidoData>()), Times.Once);
        _soapClientMock.Verify(c => c.EnviarPedidoAsync(It.IsAny<string>()), Times.Once);
        _transformerMock.Verify(t => t.ConvertirSoapToResponseJson(It.Is<string>(s => s == soapResponseXml)), Times.Once);
    }
}