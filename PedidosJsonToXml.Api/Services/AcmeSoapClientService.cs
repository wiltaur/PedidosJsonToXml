using Microsoft.Extensions.Options;
using PedidosJsonToXml.Api.Interfaces;
using PedidosJsonToXml.Api.Persistence;
using Polly;
using Polly.Registry;
using System.Text;

namespace PedidosJsonToXml.Api.Services;

public class AcmeSoapClientService : IAcmeSoapClientService
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<AcmeSoapServiceSettings> _options;
    private readonly ILogger<AcmeSoapClientService> _logger;
    private readonly ResiliencePipeline _resiliencePipeline;

    public AcmeSoapClientService(HttpClient httpClient,
        IOptions<AcmeSoapServiceSettings> options,
        ILogger<AcmeSoapClientService> logger,
        ResiliencePipelineProvider<string> resiliencePipeline)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
        _resiliencePipeline = resiliencePipeline.GetPipeline("http-post-retry-pipeline");
    }

    public async Task<string> EnviarPedidoAsync(string soapEnvelopeXml)
    {
        var usarContingencia = _options.Value.UsarContingenciaLocal;

        try
        {
            var content = new StringContent(soapEnvelopeXml, Encoding.UTF8, "text/xml");
            content.Headers.TryAddWithoutValidation("SOAPAction", $"{_options.Value.EnvNs}/EnvioPedidoAcme");
            string xmlResponseString = string.Empty;
            try
            {
                await _resiliencePipeline.ExecuteAsync(async token =>
                {
                    var response = await _httpClient.PostAsync(_options.Value.TargetUrl, content);
                    xmlResponseString = await response.Content.ReadAsStringAsync(token);
                });
            }
            catch (Exception ex) when (!usarContingencia)
            {
                _logger.LogError(ex, "El servicio beeceptor falló definitivamente tras 3 reintentos exponenciales");
                throw;
            }

            if (usarContingencia && (xmlResponseString.Contains("nothing is configured")
                    || !xmlResponseString.Trim().StartsWith('<')))
                return ObtenerXmlMockDeSeguridad();

            return xmlResponseString;
        }
        catch (Exception) when (usarContingencia)
        {
            return ObtenerXmlMockDeSeguridad();
        }
    }

    private string ObtenerXmlMockDeSeguridad()
    {
        return $@"<soapenv:Envelope xmlns:soapenv=""http://xmlsoap.org"" xmlns:env=""{_options.Value.EnvNs}"">
                   <soapenv:Header/>
                   <soapenv:Body>
                      <env:EnvioPedidoAcmeResponse>
                         <EnvioPedidoResponse>
                            <Codigo>80375472</Codigo>
                            <Mensaje>Entregado exitosamente al cliente (Modo Contingencia Local)</Mensaje>
                         </EnvioPedidoResponse>
                      </env:EnvioPedidoAcmeResponse>
                   </soapenv:Body>
                </soapenv:Envelope>";
    }
}