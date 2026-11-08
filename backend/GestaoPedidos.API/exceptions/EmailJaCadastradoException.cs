namespace GestaoPedidos.API.Exceptions;

public class EmailJaCadastradoException : Exception
{
    public EmailJaCadastradoException(string email)
        : base($"O e-mail {email} já está cadastrado.")
    {
    }
}