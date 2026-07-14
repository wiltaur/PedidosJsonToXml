namespace PedidosJsonToXml.Api.Persistence;

public class AcmeSoapServiceSettings
{
    public string TargetUrl { get; set; } = string.Empty;
    public bool UsarContingenciaLocal { get; set; }
    public string SoapEnv { get; set; } = string.Empty;
    public string EnvNs { get; set; } = string.Empty;
}