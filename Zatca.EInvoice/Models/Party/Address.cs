namespace Zatca.EInvoice.Models.Party;

/// <summary>
/// Represents an address for XML serialization.
/// </summary>
public class Address
{
    private string? _streetName;
    private string? _additionalStreetName;
    private string? _buildingNumber;
    private string? _plotIdentification;
    private string? _cityName;
    private string? _postalZone;
    private string? _country;
    private string? _countrySubentity;
    private string? _citySubdivisionName;

    /// <summary>
    /// Gets or sets the street name.
    /// </summary>
    public string? StreetName
    {
        get => _streetName;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Street name cannot be empty.");
            _streetName = value;
        }
    }

    /// <summary>
    /// Gets or sets the additional street name.
    /// </summary>
    public string? AdditionalStreetName
    {
        get => _additionalStreetName;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Additional street name cannot be empty.");
            _additionalStreetName = value;
        }
    }

    /// <summary>
    /// Gets or sets the building number.
    /// </summary>
    public string? BuildingNumber
    {
        get => _buildingNumber;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Building number cannot be empty.");
            _buildingNumber = value;
        }
    }

    /// <summary>
    /// Gets or sets the plot identification.
    /// </summary>
    public string? PlotIdentification
    {
        get => _plotIdentification;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Plot identification cannot be empty.");
            _plotIdentification = value;
        }
    }

    /// <summary>
    /// Gets or sets the city name.
    /// </summary>
    public string? CityName
    {
        get => _cityName;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("City name cannot be empty.");
            _cityName = value;
        }
    }

    /// <summary>
    /// Gets or sets the postal zone.
    /// </summary>
    public string? PostalZone
    {
        get => _postalZone;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Postal zone cannot be empty.");
            _postalZone = value;
        }
    }

    /// <summary>
    /// Gets or sets the country code.
    /// </summary>
    public string? Country
    {
        get => _country;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Country cannot be empty.");
            _country = value;
        }
    }

    /// <summary>
    /// Gets or sets the country subentity.
    /// </summary>
    public string? CountrySubentity
    {
        get => _countrySubentity;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Country subentity cannot be empty.");
            _countrySubentity = value;
        }
    }

    /// <summary>
    /// Gets or sets the city subdivision name.
    /// </summary>
    public string? CitySubdivisionName
    {
        get => _citySubdivisionName;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("City subdivision name cannot be empty.");
            _citySubdivisionName = value;
        }
    }
}
