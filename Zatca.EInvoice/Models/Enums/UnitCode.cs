namespace Zatca.EInvoice.Models.Enums;

/// <summary>
/// Provides constants for standardized unit codes.
/// For further extension, see:
/// - UNECE Recommendation 20: http://tfig.unece.org/contents/recommendation-20.htm
/// - UNECE Document (ZIP): http://www.unece.org/fileadmin/DAM/cefact/recommendations/rec20/rec20_Rev7e_2010.zip
/// </summary>
public static class UnitCode
{
    /// <summary>
    /// Unit (C62)
    /// </summary>
    public const string UNIT = "C62";

    /// <summary>
    /// Piece (H87)
    /// </summary>
    public const string PIECE = "H87";

    /// <summary>
    /// Month (MON)
    /// </summary>
    public const string MON = "MON";

    /// <summary>
    /// Piece - PCE code
    /// </summary>
    public const string PCE = "PCE";
}
