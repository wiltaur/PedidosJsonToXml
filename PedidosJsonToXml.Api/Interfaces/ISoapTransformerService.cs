using System.Xml.Linq;
using PedidosJsonToXml.Api.Models;

namespace PedidosJsonToXml.Api.Interfaces;

public interface ISoapTransformerService
{
    XDocument ConvertirPedidoToSoap(EnviarPedidoData data);
    PedidoResponseJson ConvertirSoapToResponseJson(string xmlString);
}