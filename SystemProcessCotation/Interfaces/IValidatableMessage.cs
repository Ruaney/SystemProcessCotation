/// <summary>
/// Contrato para mensagens de domínio que podem validar seu conteúdo antes de
/// serem entregues por um consumidor do barramento.
/// </summary>
public interface IValidatableMessage
{
    bool IsValid { get; }
}
