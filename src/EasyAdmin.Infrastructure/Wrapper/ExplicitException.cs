namespace EasyAdmin.Infrastructure.Wrapper;

/// <summary>
/// 明确的异常
/// </summary>
/// <param name="message"></param>
public class ExplicitException(string message) : Exception(message);