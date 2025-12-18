using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Zatca.EInvoice.Exceptions;
using Zatca.EInvoice.Helpers;

namespace Zatca.EInvoice.Certificates
{
    /// <summary>
    /// Builds a Certificate Signing Request (CSR) and private key for ZATCA e-invoicing.
    /// </summary>
    public class CertificateBuilder
    {
        private const string OidProduction = "ZATCA-Code-Signing";
        private const string OidTest = "PREZATCA-Code-Signing";
        private const string TemplateIdentifierOid = "1.3.6.1.4.1.311.20.2";

        private string _organizationIdentifier;
        private string _serialNumber;
        private string _commonName = string.Empty;
        private string _country = "SA";
        private string _organizationName = string.Empty;
        private string _organizationalUnitName = string.Empty;
        private string _address = string.Empty;
        private int _invoiceType = 1100;
        private bool _production = false;
        private string _businessCategory = string.Empty;

        private AsymmetricCipherKeyPair _keyPair;
        private Pkcs10CertificationRequest _csr;

        /// <summary>
        /// Sets the organization identifier (15 digits, starts and ends with 3).
        /// </summary>
        /// <param name="identifier">The organization identifier.</param>
        /// <returns>The current builder instance.</returns>
        public CertificateBuilder SetOrganizationIdentifier(string identifier)
        {
            if (!Regex.IsMatch(identifier, @"^3\d{13}3$"))
            {
                throw new CertificateBuilderException("Organization identifier must be 15 digits starting and ending with 3.");
            }
            _organizationIdentifier = identifier;
            return this;
        }

        /// <summary>
        /// Sets the serial number using solution name, model, and serial.
        /// </summary>
        /// <param name="solutionName">The solution name.</param>
        /// <param name="model">The device model.</param>
        /// <param name="serial">The device serial number.</param>
        /// <returns>The current builder instance.</returns>
        public CertificateBuilder SetSerialNumber(string solutionName, string model, string serial)
        {
            _serialNumber = $"1-{Sanitize(solutionName)}|2-{Sanitize(model)}|3-{Sanitize(serial)}";
            return this;
        }

        /// <summary>
        /// Sets the common name.
        /// </summary>
        /// <param name="name">The common name.</param>
        /// <returns>The current builder instance.</returns>
        public CertificateBuilder SetCommonName(string name)
        {
            _commonName = Sanitize(name);
            return this;
        }

        /// <summary>
        /// Sets the 2-character country code.
        /// </summary>
        /// <param name="country">The country code.</param>
        /// <returns>The current builder instance.</returns>
        public CertificateBuilder SetCountryName(string country)
        {
            if (country.Length != 2)
            {
                throw new CertificateBuilderException("Country code must be 2 characters.");
            }
            _country = country.ToUpperInvariant();
            return this;
        }

        /// <summary>
        /// Sets the organization name.
        /// </summary>
        /// <param name="name">The organization name.</param>
        /// <returns>The current builder instance.</returns>
        public CertificateBuilder SetOrganizationName(string name)
        {
            _organizationName = Sanitize(name);
            return this;
        }

        /// <summary>
        /// Sets the organizational unit name.
        /// </summary>
        /// <param name="name">The organizational unit name.</param>
        /// <returns>The current builder instance.</returns>
        public CertificateBuilder SetOrganizationalUnitName(string name)
        {
            _organizationalUnitName = Sanitize(name);
            return this;
        }

        /// <summary>
        /// Sets the address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>The current builder instance.</returns>
        public CertificateBuilder SetAddress(string address)
        {
            _address = Sanitize(address);
            return this;
        }

        /// <summary>
        /// Sets the invoice type (4-digit code, e.g., 1100 for standard+simplified).
        /// </summary>
        /// <param name="type">The invoice type.</param>
        /// <returns>The current builder instance.</returns>
        public CertificateBuilder SetInvoiceType(int type)
        {
            if (type < 0 || type > 9999)
            {
                throw new CertificateBuilderException("Invoice type must be a 4-digit number (0-9999).");
            }
            _invoiceType = type;
            return this;
        }

        /// <summary>
        /// Sets the production flag.
        /// </summary>
        /// <param name="production">True for production environment, false for testing.</param>
        /// <returns>The current builder instance.</returns>
        public CertificateBuilder SetProduction(bool production)
        {
            _production = production;
            return this;
        }

        /// <summary>
        /// Sets the business category.
        /// </summary>
        /// <param name="category">The business category.</param>
        /// <returns>The current builder instance.</returns>
        public CertificateBuilder SetBusinessCategory(string category)
        {
            _businessCategory = Sanitize(category);
            return this;
        }

        /// <summary>
        /// Generates the CSR and private key.
        /// </summary>
        public void Generate()
        {
            ValidateParameters();
            GenerateKeys();
        }

        /// <summary>
        /// Generates and saves the CSR and private key to files.
        /// </summary>
        /// <param name="csrPath">Path to save the CSR file (default: certificate.csr).</param>
        /// <param name="privateKeyPath">Path to save the private key file (default: private.pem).</param>
        public void GenerateAndSave(string csrPath = "certificate.csr", string privateKeyPath = "private.pem")
        {
            Generate();

            var csrContent = GetCsr();
            var storage = new Storage();

            try
            {
                storage.Write(csrPath, csrContent);
            }
            catch (ZatcaStorageException ex)
            {
                throw new CertificateBuilderException("Failed to save CSR.", ex.Context, ex);
            }

            SavePrivateKey(privateKeyPath);
        }

        /// <summary>
        /// Gets the CSR as a PEM-formatted string.
        /// </summary>
        /// <returns>The CSR content.</returns>
        public string GetCsr()
        {
            if (_csr == null)
            {
                throw new CertificateBuilderException("CSR not generated. Call Generate() first.");
            }

            var stringWriter = new StringWriter();
            var pemWriter = new PemWriter(stringWriter);
            pemWriter.WriteObject(_csr);
            pemWriter.Writer.Flush();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Saves the private key to a file.
        /// </summary>
        /// <param name="path">The file path.</param>
        public void SavePrivateKey(string path)
        {
            if (_keyPair == null)
            {
                throw new CertificateBuilderException("Private key not generated. Call Generate() first.");
            }

            try
            {
                var stringWriter = new StringWriter();
                var pemWriter = new PemWriter(stringWriter);
                pemWriter.WriteObject(_keyPair.Private);
                pemWriter.Writer.Flush();

                var storage = new Storage();
                storage.Write(path, stringWriter.ToString());
            }
            catch (Exception ex)
            {
                throw new CertificateBuilderException("Failed to save private key.", ex);
            }
        }

        /// <summary>
        /// Gets the private key as a PEM-formatted string.
        /// </summary>
        /// <returns>The private key content.</returns>
        public string GetPrivateKey()
        {
            if (_keyPair == null)
            {
                throw new CertificateBuilderException("Private key not generated. Call Generate() first.");
            }

            var stringWriter = new StringWriter();
            var pemWriter = new PemWriter(stringWriter);
            pemWriter.WriteObject(_keyPair.Private);
            pemWriter.Writer.Flush();
            return stringWriter.ToString();
        }

        private void ValidateParameters()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(_organizationIdentifier))
                errors.Add("organizationIdentifier");
            if (string.IsNullOrEmpty(_serialNumber))
                errors.Add("serialNumber");
            if (string.IsNullOrEmpty(_commonName))
                errors.Add("commonName");
            if (string.IsNullOrEmpty(_organizationName))
                errors.Add("organizationName");
            if (string.IsNullOrEmpty(_organizationalUnitName))
                errors.Add("organizationalUnitName");
            if (string.IsNullOrEmpty(_address))
                errors.Add("address");
            if (string.IsNullOrEmpty(_businessCategory))
                errors.Add("businessCategory");

            if (errors.Count > 0)
            {
                throw new CertificateBuilderException(
                    $"Missing required parameters: {string.Join(", ", errors)}");
            }
        }

        private void GenerateKeys()
        {
            try
            {
                // Generate EC key pair using secp256k1 curve
                var ecParams = CustomNamedCurves.GetByName("secp256k1");
                if (ecParams == null)
                {
                    ecParams = SecNamedCurves.GetByName("secp256k1");
                }

                var domainParams = new ECDomainParameters(
                    ecParams.Curve,
                    ecParams.G,
                    ecParams.N,
                    ecParams.H,
                    ecParams.GetSeed());

                var keyGenParams = new ECKeyGenerationParameters(domainParams, new SecureRandom());
                var keyGenerator = new ECKeyPairGenerator();
                keyGenerator.Init(keyGenParams);
                _keyPair = keyGenerator.GenerateKeyPair();

                // Create subject DN
                var subject = new X509Name(new DerObjectIdentifier[]
                {
                    X509Name.C,
                    X509Name.O,
                    X509Name.OU,
                    X509Name.CN
                }, new string[]
                {
                    _country,
                    _organizationName,
                    _organizationalUnitName,
                    _commonName
                });

                // Create CSR with extensions
                var attributes = CreateCsrAttributes();

                _csr = new Pkcs10CertificationRequest(
                    "SHA256withECDSA",
                    subject,
                    _keyPair.Public,
                    attributes,
                    _keyPair.Private);
            }
            catch (Exception ex)
            {
                throw new CertificateBuilderException("Failed to generate keys and CSR.", ex);
            }
        }

        private DerSet CreateCsrAttributes()
        {
            var extensions = new List<DerSequence>();

            // Add template identifier extension (1.3.6.1.4.1.311.20.2)
            var templateId = _production ? OidProduction : OidTest;
            var templateIdExtension = new DerSequence(
                new DerObjectIdentifier(TemplateIdentifierOid),
                new DerSet(new DerPrintableString(templateId))
            );
            extensions.Add(templateIdExtension);

            // Add Subject Alternative Name extension
            var sanExtension = CreateSubjectAlternativeNameExtension();
            extensions.Add(sanExtension);

            // Create extension request attribute
            var extensionOid = new DerObjectIdentifier("1.2.840.113549.1.9.14"); // extensionRequest
            var extensionRequest = new DerSequence(
                extensionOid,
                new DerSet((Asn1Encodable)new DerSequence(extensions.ToArray()))
            );

            return new DerSet((Asn1Encodable)extensionRequest);
        }

        private DerSequence CreateSubjectAlternativeNameExtension()
        {
            // Create DirectoryName for SAN
            var sanAttributes = new List<DerSequence>
            {
                new DerSequence(
                    X509Name.SerialNumber,
                    new DerPrintableString(_serialNumber)),
                new DerSequence(
                    new DerObjectIdentifier("0.9.2342.19200300.100.1.1"), // UID
                    new DerPrintableString(_organizationIdentifier)),
                new DerSequence(
                    new DerObjectIdentifier("2.5.4.12"), // title
                    new DerPrintableString(_invoiceType.ToString("D4"))),
                new DerSequence(
                    new DerObjectIdentifier("2.5.4.26"), // registeredAddress
                    new DerUtf8String(_address)),
                new DerSequence(
                    new DerObjectIdentifier("2.5.4.15"), // businessCategory
                    new DerUtf8String(_businessCategory))
            };

            var directoryName = new DerSequence(sanAttributes.ToArray());
            var taggedName = new DerTaggedObject(true, 4, directoryName); // 4 = directoryName

            var generalNames = new DerSequence(taggedName);
            var sanExtensionValue = new DerOctetString(generalNames.GetEncoded());

            var sanExtension = new DerSequence(
                new DerObjectIdentifier("2.5.29.17"), // subjectAltName
                sanExtensionValue
            );

            return sanExtension;
        }

        private string Sanitize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new CertificateBuilderException($"Input cannot be null or empty: {input}");
            }

            var trimmed = input.Trim();
            var sanitized = Regex.Replace(trimmed, @"[^a-zA-Z0-9\s\-_]", "");

            if (string.IsNullOrEmpty(sanitized))
            {
                throw new CertificateBuilderException($"Sanitization resulted in empty string for: {input}");
            }

            return sanitized;
        }
    }
}
