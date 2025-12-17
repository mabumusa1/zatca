namespace Zatca.EInvoice.Models.References;

/// <summary>
/// Represents payment means information.
/// </summary>
public class PaymentMeans
{
    private string? _paymentMeansCode;
    private string? _instructionNote;
    private string? _paymentId;

    /// <summary>
    /// Gets or sets the payment means code.
    /// </summary>
    public string? PaymentMeansCode
    {
        get => _paymentMeansCode;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Payment means code cannot be empty.");
            _paymentMeansCode = value;
        }
    }

    /// <summary>
    /// Gets or sets the instruction note.
    /// </summary>
    public string? InstructionNote
    {
        get => _instructionNote;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Instruction note cannot be empty.");
            _instructionNote = value;
        }
    }

    /// <summary>
    /// Gets or sets the payment ID.
    /// </summary>
    public string? PaymentId
    {
        get => _paymentId;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Payment ID cannot be empty.");
            _paymentId = value;
        }
    }

    /// <summary>
    /// Gets or sets the payee financial account.
    /// Note: Consider creating a specific FinancialAccount type if needed.
    /// </summary>
    public object? PayeeFinancialAccount { get; set; }
}
