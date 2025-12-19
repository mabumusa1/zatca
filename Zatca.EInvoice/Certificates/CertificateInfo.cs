using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;
using System.IO;

namespace Zatca.EInvoice.Certificates
{
    /// <summary>
    /// Certificate wrapper class providing access to certificate properties and operations.
    /// </summary>
    public class CertificateInfo
    {
        private readonly X509Certificate2 _certificate;
        private readonly AsymmetricKeyParameter _privateKey;
        private readonly string _secret;

        /// <summary>
        /// Gets the raw certificate content.
        /// </summary>
        public string RawCertificate { get; }

        /// <summary>
        /// Gets the X509 certificate.
        /// </summary>
        public X509Certificate2 Certificate => _certificate;

        /// <summary>
        /// Gets the certificate issuer.
        /// </summary>
        public string Issuer => _certificate.Issuer;

        /// <summary>
        /// Gets the certificate subject.
        /// </summary>
        public string Subject => _certificate.Subject;

        /// <summary>
        /// Gets the certificate serial number.
        /// </summary>
        public string SerialNumber => _certificate.SerialNumber;

        /// <summary>
        /// Gets the secret key used for authentication.
        /// </summary>
        public string Secret => _secret;

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateInfo"/> class from certificate and private key strings.
        /// </summary>
        /// <param name="certificateContent">The certificate content (PEM format).</param>
        /// <param name="privateKeyContent">The private key content (PEM format).</param>
        /// <param name="secret">The secret key for authentication.</param>
        public CertificateInfo(string certificateContent, string privateKeyContent, string secret)
        {
            if (string.IsNullOrWhiteSpace(certificateContent))
                throw new ArgumentNullException(nameof(certificateContent));
            if (string.IsNullOrWhiteSpace(privateKeyContent))
                throw new ArgumentNullException(nameof(privateKeyContent));

            RawCertificate = certificateContent;
            _secret = secret;

            // Load certificate
            _certificate = LoadCertificateFromPem(certificateContent);

            // Load private key
            _privateKey = LoadPrivateKeyFromPem(privateKeyContent);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateInfo"/> class from an X509Certificate2.
        /// </summary>
        /// <param name="certificate">The X509 certificate.</param>
        /// <param name="privateKey">The private key.</param>
        /// <param name="secret">The secret key for authentication.</param>
        public CertificateInfo(X509Certificate2 certificate, AsymmetricKeyParameter privateKey, string secret)
        {
            _certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
            _privateKey = privateKey;
            _secret = secret;
            RawCertificate = ExportCertificateToPem(certificate);
        }

        /// <summary>
        /// Gets the private key as BouncyCastle AsymmetricKeyParameter.
        /// </summary>
        /// <returns>The private key.</returns>
        public AsymmetricKeyParameter GetPrivateKey()
        {
            return _privateKey;
        }

        /// <summary>
        /// Gets the public key bytes.
        /// </summary>
        /// <returns>The public key bytes.</returns>
        public byte[] GetPublicKeyBytes()
        {
            return _certificate.GetPublicKey();
        }

        /// <summary>
        /// Gets the certificate hash (SHA-256).
        /// </summary>
        /// <returns>Base64-encoded certificate hash.</returns>
        public string GetCertificateHash()
        {
            using (var sha256 = SHA256.Create())
            {
                var certBytes = Encoding.UTF8.GetBytes(RawCertificate);
                var hash = sha256.ComputeHash(certBytes);
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Creates the authorization header for ZATCA API.
        /// </summary>
        /// <returns>The authorization header value.</returns>
        public string GetAuthorizationHeader()
        {
            var encoded = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{Convert.ToBase64String(Encoding.UTF8.GetBytes(RawCertificate))}:{_secret}"));
            return $"Basic {encoded}";
        }

        /// <summary>
        /// Gets the formatted issuer details.
        /// </summary>
        /// <returns>Formatted issuer string.</returns>
        public string GetFormattedIssuer()
        {
            var issuer = _certificate.Issuer;
            var parts = issuer.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Array.Reverse(parts);
            return string.Join(", ", parts).Trim();
        }

        /// <summary>
        /// Gets the raw public key in base64 format (without headers).
        /// </summary>
        /// <returns>Base64-encoded public key.</returns>
        public string GetRawPublicKeyBase64()
        {
            var publicKeyPem = ExportPublicKeyToPem(_certificate);
            return publicKeyPem
                .Replace("-----BEGIN PUBLIC KEY-----", "")
                .Replace("-----END PUBLIC KEY-----", "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Trim();
        }

        /// <summary>
        /// Gets the certificate signature.
        /// </summary>
        /// <returns>The certificate signature bytes.</returns>
        public byte[] GetCertificateSignature()
        {
            var signature = _certificate.GetRawCertData();

            // Parse the certificate to get the actual signature
            var bcCert = new X509CertificateParser().ReadCertificate(signature);
            var sig = bcCert.GetSignature();

            // Remove the extra prefix byte if present
            if (sig.Length > 0 && sig[0] == 0x00)
            {
                var result = new byte[sig.Length - 1];
                Array.Copy(sig, 1, result, 0, result.Length);
                return result;
            }

            return sig;
        }

        private X509Certificate2 LoadCertificateFromPem(string certificateContent)
        {
            try
            {
                // Check if the content is in PEM format (has headers)
                if (certificateContent.Contains("-----BEGIN CERTIFICATE-----"))
                {
                    // Use CreateFromPem for proper PEM handling in .NET 5+
                    return X509Certificate2.CreateFromPem(certificateContent.AsSpan());
                }
                else
                {
                    // Raw base64 content - decode directly
                    var certBytes = Convert.FromBase64String(certificateContent.Trim());
#pragma warning disable SYSLIB0057 // X509Certificate2 constructor is obsolete in .NET 9
                    return new X509Certificate2(certBytes);
#pragma warning restore SYSLIB0057
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load certificate from PEM content.", ex);
            }
        }

        private AsymmetricKeyParameter LoadPrivateKeyFromPem(string privateKeyContent)
        {
            try
            {
                string pemContent = privateKeyContent;

                // If the content doesn't have PEM headers, wrap it
                if (!privateKeyContent.Contains("-----BEGIN"))
                {
                    // Raw base64 - wrap with PKCS#8 private key headers
                    pemContent = "-----BEGIN PRIVATE KEY-----\n" +
                                 privateKeyContent.Trim() +
                                 "\n-----END PRIVATE KEY-----";
                }

                using (var reader = new StringReader(pemContent))
                {
                    var pemReader = new PemReader(reader);
                    var keyObject = pemReader.ReadObject();

                    if (keyObject is AsymmetricCipherKeyPair keyPair)
                    {
                        return keyPair.Private;
                    }
                    else if (keyObject is AsymmetricKeyParameter keyParam)
                    {
                        return keyParam;
                    }
                    else
                    {
                        throw new InvalidOperationException("Unexpected PEM object type.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load private key from PEM content.", ex);
            }
        }

        private string ExportCertificateToPem(X509Certificate2 certificate)
        {
            var builder = new StringBuilder();
            builder.AppendLine("-----BEGIN CERTIFICATE-----");
            builder.AppendLine(Convert.ToBase64String(certificate.RawData, Base64FormattingOptions.InsertLineBreaks));
            builder.AppendLine("-----END CERTIFICATE-----");
            return builder.ToString();
        }

        private string ExportPublicKeyToPem(X509Certificate2 certificate)
        {
            var publicKey = certificate.GetPublicKey();
            var builder = new StringBuilder();
            builder.AppendLine("-----BEGIN PUBLIC KEY-----");
            builder.AppendLine(Convert.ToBase64String(publicKey, Base64FormattingOptions.InsertLineBreaks));
            builder.AppendLine("-----END PUBLIC KEY-----");
            return builder.ToString();
        }

        /// <summary>
        /// Loads certificate from a file.
        /// </summary>
        /// <param name="certificatePath">Path to the certificate file.</param>
        /// <param name="privateKeyPath">Path to the private key file.</param>
        /// <param name="secret">The secret key.</param>
        /// <returns>A new CertificateInfo instance.</returns>
        public static CertificateInfo LoadFromFile(string certificatePath, string privateKeyPath, string secret)
        {
            var certContent = File.ReadAllText(certificatePath);
            var keyContent = File.ReadAllText(privateKeyPath);
            return new CertificateInfo(certContent, keyContent, secret);
        }
    }
}
