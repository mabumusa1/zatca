namespace Zatca.EInvoice.CLI.Models;

/// <summary>
/// Generic wrapper for command execution results.
/// </summary>
/// <typeparam name="T">The type of the result data.</typeparam>
public class CommandResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> InfoMessages { get; set; } = new();

    public static CommandResult<T> Ok(T data) => new()
    {
        Success = true,
        Data = data
    };

    public static CommandResult<T> Fail(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };

    public CommandResult<T> WithWarning(string warning)
    {
        Warnings.Add(warning);
        return this;
    }

    public CommandResult<T> WithInfo(string info)
    {
        InfoMessages.Add(info);
        return this;
    }
}

/// <summary>
/// Non-generic command result for void operations.
/// </summary>
public class CommandResult : CommandResult<object>
{
    public static CommandResult Ok() => new() { Success = true };

    public new static CommandResult Fail(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };
}
