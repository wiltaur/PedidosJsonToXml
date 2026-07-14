using Microsoft.Extensions.Options;
using PedidosJsonToXml.Api.Interfaces;
using PedidosJsonToXml.Api.Models;
using PedidosJsonToXml.Api.Persistence;
using System.Xml.Linq;

namespace PedidosJsonToXml.Api.Services;

public class SoapTransformerService(IOptions<AcmeSoapServiceSettings> options) : ISoapTransformerService
{
    public XDocument ConvertirPedidoToSoap(EnviarPedidoData data)
    {
        XNamespace SoapEnv = options.Value.SoapEnv;
        XNamespace EnvNs = options.Value.EnvNs;
        return new XDocument(
            new XElement(SoapEnv + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soapenv", SoapEnv.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "env", EnvNs.NamespaceName),
                new XElement(SoapEnv + "Header"),
                new XElement(SoapEnv + "Body",
                    new XElement(EnvNs + "EnvioPedidoAcme",
                        new XElement("EnvioPedidoRequest",
                            new XElement("pedido", data.NumPedido),
                            new XElement("Cantidad", data.CantidadPedido),
                            new XElement("EAN", data.CodigoEAN),
                            new XElement("Producto", data.NombreProducto),
                            new XElement("Cedula", data.NumDocumento),
                            new XElement("Direccion", data.Direccion)
                        )
                    )
                )
            )
        );
    }

    public PedidoResponseJson ConvertirSoapToResponseJson(string xmlString)
    {
        var xmlDoc = XDocument.Parse(xmlString);
        var responseElement = xmlDoc.Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "EnvioPedidoResponse") ?? throw new Exception("No se encontró el elemento 'EnvioPedidoResponse' en el XML recibido.");

        var codigo = responseElement.Elements().FirstOrDefault(x => x.Name.LocalName == "Codigo")?.Value ?? string.Empty;
        var mensaje = responseElement.Elements().FirstOrDefault(x => x.Name.LocalName == "Mensaje")?.Value ?? string.Empty;

        return new PedidoResponseJson
        {
            EnviarPedidoRespuesta = new EnviarPedidoRespuestaData
            {
                CodigoEnvio = codigo,
                Estado = mensaje
            }
        };
    }
}