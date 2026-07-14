using System.Text.Json.Serialization;

namespace PedidosJsonToXml.Api.Models;

public class PedidoRequestJson
{
    [JsonPropertyName("enviarPedido")]
    public EnviarPedidoData EnviarPedido { get; set; } = null!;
}

public class EnviarPedidoData
{
    [JsonPropertyName("numPedido")]
    public string NumPedido { get; set; } = string.Empty;

    [JsonPropertyName("cantidadPedido")]
    public int CantidadPedido { get; set; }

    [JsonPropertyName("codigoEAN")]
    public string CodigoEAN { get; set; } = string.Empty;

    [JsonPropertyName("nombreProducto")]
    public string NombreProducto { get; set; } = string.Empty;

    [JsonPropertyName("numDocumento")]
    public string NumDocumento { get; set; } = string.Empty;

    [JsonPropertyName("direccion")]
    public string Direccion { get; set; } = string.Empty;
}