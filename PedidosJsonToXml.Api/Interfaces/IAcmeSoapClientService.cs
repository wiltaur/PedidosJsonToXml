namespace PedidosJsonToXml.Api.Interfaces;

public interface IAcmeSoapClientService
{
    Task<string> EnviarPedidoAsync(string soapEnvelopeXml);
}