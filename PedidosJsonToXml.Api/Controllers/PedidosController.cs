using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PedidosJsonToXml.Api.Interfaces;
using PedidosJsonToXml.Api.Models;

namespace PedidosJsonToXml.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly ISoapTransformerService _transformerService;
    private readonly IAcmeSoapClientService _soapClientService;
    private readonly IValidator<PedidoRequestJson> _validator;

    public PedidosController(ISoapTransformerService transformerService,
        IAcmeSoapClientService soapClientService,
        IValidator<PedidoRequestJson> validator)
    {
        _transformerService = transformerService;
        _soapClientService = soapClientService;
        _validator = validator;
    }

    [HttpPost("enviar")]
    public async Task<IActionResult> EnviarPedido([FromBody] PedidoRequestJson request)
    {
        var validationResult = await _validator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.ToDictionary());

            var soapEnvelope = _transformerService.ConvertirPedidoToSoap(request.EnviarPedido);
            var xmlResponseString = await _soapClientService.EnviarPedidoAsync(soapEnvelope.ToString());
            var resultadoJson = _transformerService.ConvertirSoapToResponseJson(xmlResponseString);

            return Ok(resultadoJson);
    }
}