using System.Text.Json.Serialization;

namespace PedidosJsonToXml.Api.Models;

public class PedidoResponseJson
{
    [JsonPropertyName("enviarPedidoRespuesta")]
    public EnviarPedidoRespuestaData EnviarPedidoRespuesta { get; set; } = null!;
}

public class EnviarPedidoRespuestaData
{
    [JsonPropertyName("codigoEnvio")]
    public string CodigoEnvio { get; set; } = string.Empty;

    [JsonPropertyName("estado")]
    public string Estado { get; set; } = string.Empty;
}