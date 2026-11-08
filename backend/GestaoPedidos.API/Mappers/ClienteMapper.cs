using GestaoPedidos.API.DTOs.Clientes;
using GestaoPedidos.API.Entities;

namespace GestaoPedidos.API.Mappers;

public static class ClienteMapper
{
    public static ClienteResponse toResponse(Cliente cliente)
    {
        return new ClienteResponse
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Email = cliente.Email,
            Telefone = cliente.Telefone,
            DataCriacao = cliente.DataCriacao
        };
    }

    public static Cliente toEntity(CriarClienteRequest request)
    {
        return new Cliente
        {
            Nome = request.Nome,
            Email = request.Email,
            Telefone = request.Telefone,
            DataCriacao = DateTime.UtcNow
        };
    }

    public static void UpdateEntity(
        AtualizarClienteRequest request,
        Cliente cliente)
    {
        cliente.Nome = request.Nome;
        cliente.Email = request.Email;
        cliente.Telefone = request.Telefone;
    }
}