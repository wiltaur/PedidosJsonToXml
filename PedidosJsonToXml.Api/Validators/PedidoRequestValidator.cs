using FluentValidation;
using PedidosJsonToXml.Api.Models;

namespace PedidosJsonToXml.Api.Validators;

public class PedidoRequestValidator : AbstractValidator<PedidoRequestJson>
{
    public PedidoRequestValidator()
    {
        RuleFor(x => x.EnviarPedido)
            .NotNull()
            .WithMessage("El objeto 'enviarPedido' es obligatorio.");

        When(x => x.EnviarPedido != null, () =>
        {
            RuleFor(x => x.EnviarPedido.NumPedido)
                .NotEmpty().WithMessage("El Número del pedido es obligatorio.")
                .Matches(@"^\d+$").WithMessage("El Número del pedido debe contener solo números.");

            RuleFor(x => x.EnviarPedido.CantidadPedido)
                .GreaterThan(0)
                .WithMessage("La cantidad del pedido debe ser al menos 1.");

            RuleFor(x => x.EnviarPedido.CodigoEAN)
                .NotEmpty().WithMessage("El código EAN es obligatorio.")
                .Matches(@"^\d+$").WithMessage("El código EAN debe contener solo números.");

            RuleFor(x => x.EnviarPedido.NombreProducto)
                .NotEmpty().WithMessage("El nombre del producto es obligatorio.");

            RuleFor(x => x.EnviarPedido.NumDocumento)
                .NotEmpty().WithMessage("El número de documento es obligatorio.");

            RuleFor(x => x.EnviarPedido.Direccion)
                .NotEmpty().WithMessage("La dirección de envío es obligatoria.");
        });
    }
}