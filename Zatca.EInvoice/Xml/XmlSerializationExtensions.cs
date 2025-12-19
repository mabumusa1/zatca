using System;
using System.Globalization;
using System.Xml.Linq;

namespace Zatca.EInvoice.Xml
{
    /// <summary>
    /// Extension methods for XML serialization following UBL standards.
    /// </summary>
    public static class XmlSerializationExtensions
    {
        private const string CurrencyIdAttribute = "currencyID";

        /// <summary>
        /// Formats an amount with 2 decimal places and adds the currencyID attribute.
        /// </summary>
        /// <param name="amount">The amount to format.</param>
        /// <param name="currencyId">The currency identifier (e.g., "SAR").</param>
        /// <returns>A formatted string representing the amount.</returns>
        public static string FormatAmount(this decimal amount, string currencyId = "SAR")
        {
            return amount.ToString("F2", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats an amount with 2 decimal places and adds the currencyID attribute.
        /// </summary>
        /// <param name="amount">The amount to format.</param>
        /// <param name="currencyId">The currency identifier (e.g., "SAR").</param>
        /// <returns>A formatted string representing the amount.</returns>
        public static string FormatAmount(this double amount, string currencyId = "SAR")
        {
            return amount.ToString("F2", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats a price with 4 decimal places.
        /// </summary>
        /// <param name="price">The price to format.</param>
        /// <returns>A formatted string representing the price.</returns>
        public static string FormatPrice(this decimal price)
        {
            return price.ToString("F4", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats a price with 4 decimal places.
        /// </summary>
        /// <param name="price">The price to format.</param>
        /// <returns>A formatted string representing the price.</returns>
        public static string FormatPrice(this double price)
        {
            return price.ToString("F4", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats a quantity with 6 decimal places.
        /// </summary>
        /// <param name="quantity">The quantity to format.</param>
        /// <returns>A formatted string representing the quantity.</returns>
        public static string FormatQuantity(this decimal quantity)
        {
            return quantity.ToString("F6", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats a quantity with 6 decimal places.
        /// </summary>
        /// <param name="quantity">The quantity to format.</param>
        /// <returns>A formatted string representing the quantity.</returns>
        public static string FormatQuantity(this double quantity)
        {
            return quantity.ToString("F6", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Creates an XML element for an amount with currencyID attribute.
        /// </summary>
        /// <param name="elementName">The name of the XML element.</param>
        /// <param name="amount">The amount value.</param>
        /// <param name="currencyId">The currency identifier (e.g., "SAR").</param>
        /// <returns>An <see cref="XElement"/> with the amount and currency attribute.</returns>
        public static XElement CreateAmountElement(XName elementName, decimal amount, string currencyId = "SAR")
        {
            return new XElement(elementName,
                new XAttribute(CurrencyIdAttribute, currencyId),
                amount.FormatAmount(currencyId));
        }

        /// <summary>
        /// Creates an XML element for an amount with currencyID attribute.
        /// </summary>
        /// <param name="elementName">The name of the XML element.</param>
        /// <param name="amount">The amount value.</param>
        /// <param name="currencyId">The currency identifier (e.g., "SAR").</param>
        /// <returns>An <see cref="XElement"/> with the amount and currency attribute.</returns>
        public static XElement CreateAmountElement(XName elementName, double amount, string currencyId = "SAR")
        {
            return new XElement(elementName,
                new XAttribute(CurrencyIdAttribute, currencyId),
                amount.FormatAmount(currencyId));
        }

        /// <summary>
        /// Creates an XML element for a price amount with currencyID attribute.
        /// </summary>
        /// <param name="elementName">The name of the XML element.</param>
        /// <param name="price">The price value.</param>
        /// <param name="currencyId">The currency identifier (e.g., "SAR").</param>
        /// <returns>An <see cref="XElement"/> with the price and currency attribute.</returns>
        public static XElement CreatePriceElement(XName elementName, decimal price, string currencyId = "SAR")
        {
            return new XElement(elementName,
                new XAttribute(CurrencyIdAttribute, currencyId),
                price.FormatPrice());
        }

        /// <summary>
        /// Creates an XML element for a price amount with currencyID attribute.
        /// </summary>
        /// <param name="elementName">The name of the XML element.</param>
        /// <param name="price">The price value.</param>
        /// <param name="currencyId">The currency identifier (e.g., "SAR").</param>
        /// <returns>An <see cref="XElement"/> with the price and currency attribute.</returns>
        public static XElement CreatePriceElement(XName elementName, double price, string currencyId = "SAR")
        {
            return new XElement(elementName,
                new XAttribute(CurrencyIdAttribute, currencyId),
                price.FormatPrice());
        }

        /// <summary>
        /// Creates an XML element for a quantity with unitCode attribute.
        /// </summary>
        /// <param name="elementName">The name of the XML element.</param>
        /// <param name="quantity">The quantity value.</param>
        /// <param name="unitCode">The unit code (e.g., "PCE" for piece).</param>
        /// <returns>An <see cref="XElement"/> with the quantity and unit code attribute.</returns>
        public static XElement CreateQuantityElement(XName elementName, decimal quantity, string unitCode)
        {
            return new XElement(elementName,
                new XAttribute("unitCode", unitCode),
                quantity.FormatQuantity());
        }

        /// <summary>
        /// Creates an XML element for a quantity with unitCode attribute.
        /// </summary>
        /// <param name="elementName">The name of the XML element.</param>
        /// <param name="quantity">The quantity value.</param>
        /// <param name="unitCode">The unit code (e.g., "PCE" for piece).</param>
        /// <returns>An <see cref="XElement"/> with the quantity and unit code attribute.</returns>
        public static XElement CreateQuantityElement(XName elementName, double quantity, string unitCode)
        {
            return new XElement(elementName,
                new XAttribute("unitCode", unitCode),
                quantity.FormatQuantity());
        }

        /// <summary>
        /// Formats a date to the UBL standard format (yyyy-MM-dd).
        /// </summary>
        /// <param name="date">The date to format.</param>
        /// <returns>A formatted string representing the date.</returns>
        public static string FormatDate(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats a time to the UBL standard format (HH:mm:ss).
        /// </summary>
        /// <param name="time">The time to format.</param>
        /// <returns>A formatted string representing the time.</returns>
        public static string FormatTime(this DateTime time)
        {
            return time.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats a percentage value.
        /// </summary>
        /// <param name="percent">The percentage to format.</param>
        /// <returns>A formatted string representing the percentage.</returns>
        public static string FormatPercent(this decimal percent)
        {
            return percent.ToString("F2", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats a percentage value.
        /// </summary>
        /// <param name="percent">The percentage to format.</param>
        /// <returns>A formatted string representing the percentage.</returns>
        public static string FormatPercent(this double percent)
        {
            return percent.ToString("F2", CultureInfo.InvariantCulture);
        }
    }
}
