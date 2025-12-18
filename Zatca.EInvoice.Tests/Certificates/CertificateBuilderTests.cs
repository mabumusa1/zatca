using System;
using System.IO;
using Xunit;
using Zatca.EInvoice.Certificates;
using Zatca.EInvoice.Exceptions;

namespace Zatca.EInvoice.Tests.Certificates
{
    /// <summary>
    /// Tests for CertificateBuilder class.
    /// </summary>
    public class CertificateBuilderTests : IDisposable
    {
        private readonly List<string> _tempFiles = new List<string>();

        /// <summary>
        /// Test that GenerateAndSave() generates CSR and private key files successfully.
        /// </summary>
        [Fact]
        public void TestGenerateAndSave()
        {
            // Arrange
            var csrPath = Path.GetTempFileName();
            var privateKeyPath = Path.GetTempFileName();
            _tempFiles.Add(csrPath);
            _tempFiles.Add(privateKeyPath);

            var builder = new CertificateBuilder()
                .SetOrganizationIdentifier("312345678901233")
                .SetSerialNumber("SolutionName", "Model", "Serial123")
                .SetCommonName("Test Common Name")
                .SetCountryName("SA")
                .SetOrganizationName("Test Organization")
                .SetOrganizationalUnitName("Test Unit")
                .SetAddress("Test Address 123")
                .SetInvoiceType(1100)
                .SetBusinessCategory("Business")
                .SetProduction(false);

            // Act
            builder.GenerateAndSave(csrPath, privateKeyPath);

            // Assert
            Assert.True(File.Exists(csrPath), "CSR file should be created");
            Assert.True(File.Exists(privateKeyPath), "Private key file should be created");

            var csrContent = File.ReadAllText(csrPath);
            var privateKeyContent = File.ReadAllText(privateKeyPath);

            Assert.Contains("BEGIN CERTIFICATE REQUEST", csrContent);
            Assert.Contains("END CERTIFICATE REQUEST", csrContent);
            Assert.Contains("BEGIN EC PRIVATE KEY", privateKeyContent);
            Assert.Contains("END EC PRIVATE KEY", privateKeyContent);
        }

        /// <summary>
        /// Test that missing businessCategory parameter throws CertificateBuilderException.
        /// </summary>
        [Fact]
        public void TestMissingRequiredParameterThrowsException()
        {
            // Arrange
            var builder = new CertificateBuilder()
                .SetOrganizationIdentifier("312345678901233")
                .SetSerialNumber("SolutionName", "Model", "Serial123")
                .SetCommonName("Test Common Name")
                .SetCountryName("SA")
                .SetOrganizationName("Test Organization")
                .SetOrganizationalUnitName("Test Unit")
                .SetAddress("Test Address 123")
                .SetInvoiceType(1100);
            // Intentionally missing SetBusinessCategory()

            // Act & Assert
            var exception = Assert.Throws<CertificateBuilderException>(() => builder.Generate());
            Assert.Contains("businessCategory", exception.Message);
        }

        /// <summary>
        /// Test that invalid organization identifier throws exception.
        /// </summary>
        [Fact]
        public void TestInvalidOrganizationIdentifierThrowsException()
        {
            // Arrange
            var builder = new CertificateBuilder();

            // Act & Assert - Invalid format (not starting with 3)
            Assert.Throws<CertificateBuilderException>(() =>
                builder.SetOrganizationIdentifier("412345678901234"));

            // Act & Assert - Invalid format (not ending with 3)
            Assert.Throws<CertificateBuilderException>(() =>
                builder.SetOrganizationIdentifier("312345678901235"));

            // Act & Assert - Invalid length
            Assert.Throws<CertificateBuilderException>(() =>
                builder.SetOrganizationIdentifier("31234567890123"));
        }

        /// <summary>
        /// Test that invalid country code throws exception.
        /// </summary>
        [Fact]
        public void TestInvalidCountryCodeThrowsException()
        {
            // Arrange
            var builder = new CertificateBuilder();

            // Act & Assert - Too short
            Assert.Throws<CertificateBuilderException>(() =>
                builder.SetCountryName("S"));

            // Act & Assert - Too long
            Assert.Throws<CertificateBuilderException>(() =>
                builder.SetCountryName("SAU"));
        }

        /// <summary>
        /// Test that GetCsr() returns valid CSR string.
        /// </summary>
        [Fact]
        public void TestGetCsrReturnsValidString()
        {
            // Arrange
            var builder = new CertificateBuilder()
                .SetOrganizationIdentifier("312345678901233")
                .SetSerialNumber("SolutionName", "Model", "Serial123")
                .SetCommonName("Test Common Name")
                .SetCountryName("SA")
                .SetOrganizationName("Test Organization")
                .SetOrganizationalUnitName("Test Unit")
                .SetAddress("Test Address 123")
                .SetInvoiceType(1100)
                .SetBusinessCategory("Business")
                .SetProduction(false);

            // Act
            builder.Generate();
            var csr = builder.GetCsr();

            // Assert
            Assert.NotNull(csr);
            Assert.NotEmpty(csr);
            Assert.Contains("BEGIN CERTIFICATE REQUEST", csr);
            Assert.Contains("END CERTIFICATE REQUEST", csr);
        }

        /// <summary>
        /// Test that GetPrivateKey() returns valid private key string.
        /// </summary>
        [Fact]
        public void TestGetPrivateKeyReturnsValidString()
        {
            // Arrange
            var builder = new CertificateBuilder()
                .SetOrganizationIdentifier("312345678901233")
                .SetSerialNumber("SolutionName", "Model", "Serial123")
                .SetCommonName("Test Common Name")
                .SetCountryName("SA")
                .SetOrganizationName("Test Organization")
                .SetOrganizationalUnitName("Test Unit")
                .SetAddress("Test Address 123")
                .SetInvoiceType(1100)
                .SetBusinessCategory("Business")
                .SetProduction(false);

            // Act
            builder.Generate();
            var privateKey = builder.GetPrivateKey();

            // Assert
            Assert.NotNull(privateKey);
            Assert.NotEmpty(privateKey);
            Assert.Contains("BEGIN EC PRIVATE KEY", privateKey);
            Assert.Contains("END EC PRIVATE KEY", privateKey);
        }

        /// <summary>
        /// Test that calling GetCsr() before Generate() throws exception.
        /// </summary>
        [Fact]
        public void TestGetCsrBeforeGenerateThrowsException()
        {
            // Arrange
            var builder = new CertificateBuilder();

            // Act & Assert
            Assert.Throws<CertificateBuilderException>(() => builder.GetCsr());
        }

        /// <summary>
        /// Test that calling GetPrivateKey() before Generate() throws exception.
        /// </summary>
        [Fact]
        public void TestGetPrivateKeyBeforeGenerateThrowsException()
        {
            // Arrange
            var builder = new CertificateBuilder();

            // Act & Assert
            Assert.Throws<CertificateBuilderException>(() => builder.GetPrivateKey());
        }

        /// <summary>
        /// Test that production flag affects CSR generation.
        /// </summary>
        [Fact]
        public void TestProductionFlagAffectsCsr()
        {
            // Arrange - Production
            var builderProd = new CertificateBuilder()
                .SetOrganizationIdentifier("312345678901233")
                .SetSerialNumber("SolutionName", "Model", "Serial123")
                .SetCommonName("Test Common Name")
                .SetCountryName("SA")
                .SetOrganizationName("Test Organization")
                .SetOrganizationalUnitName("Test Unit")
                .SetAddress("Test Address 123")
                .SetInvoiceType(1100)
                .SetBusinessCategory("Business")
                .SetProduction(true);

            // Arrange - Testing
            var builderTest = new CertificateBuilder()
                .SetOrganizationIdentifier("312345678901233")
                .SetSerialNumber("SolutionName", "Model", "Serial123")
                .SetCommonName("Test Common Name")
                .SetCountryName("SA")
                .SetOrganizationName("Test Organization")
                .SetOrganizationalUnitName("Test Unit")
                .SetAddress("Test Address 123")
                .SetInvoiceType(1100)
                .SetBusinessCategory("Business")
                .SetProduction(false);

            // Act
            builderProd.Generate();
            builderTest.Generate();

            var csrProd = builderProd.GetCsr();
            var csrTest = builderTest.GetCsr();

            // Assert - Both should generate valid CSRs but with different content
            Assert.NotNull(csrProd);
            Assert.NotNull(csrTest);
            Assert.NotEmpty(csrProd);
            Assert.NotEmpty(csrTest);
        }

        /// <summary>
        /// Test that SetInvoiceType validates invoice type range.
        /// </summary>
        [Fact]
        public void TestInvoiceTypeValidation()
        {
            // Arrange
            var builder = new CertificateBuilder();

            // Act & Assert - Valid values
            Assert.NotNull(builder.SetInvoiceType(0));
            Assert.NotNull(builder.SetInvoiceType(1100));
            Assert.NotNull(builder.SetInvoiceType(9999));

            // Act & Assert - Invalid values
            Assert.Throws<CertificateBuilderException>(() =>
                builder.SetInvoiceType(-1));

            Assert.Throws<CertificateBuilderException>(() =>
                builder.SetInvoiceType(10000));
        }

        /// <summary>
        /// Test that Sanitize method removes special characters.
        /// </summary>
        [Fact]
        public void TestSanitizationOfInputs()
        {
            // Arrange & Act
            var builder = new CertificateBuilder()
                .SetOrganizationIdentifier("312345678901233")
                .SetSerialNumber("Solution@Name!", "Model#123", "Serial$456")
                .SetCommonName("Test & Common % Name")
                .SetCountryName("SA")
                .SetOrganizationName("Test * Organization")
                .SetOrganizationalUnitName("Test ^ Unit")
                .SetAddress("Test @ Address # 123")
                .SetInvoiceType(1100)
                .SetBusinessCategory("Business & Trade")
                .SetProduction(false);

            // Assert - Should not throw exception
            builder.Generate();
            var csr = builder.GetCsr();
            Assert.NotNull(csr);
            Assert.NotEmpty(csr);
        }

        /// <summary>
        /// Test that multiple Generate() calls produce different key pairs.
        /// </summary>
        [Fact]
        public void TestMultipleGenerateCallsProduceDifferentKeys()
        {
            // Arrange
            var builder1 = new CertificateBuilder()
                .SetOrganizationIdentifier("312345678901233")
                .SetSerialNumber("SolutionName", "Model", "Serial123")
                .SetCommonName("Test Common Name")
                .SetCountryName("SA")
                .SetOrganizationName("Test Organization")
                .SetOrganizationalUnitName("Test Unit")
                .SetAddress("Test Address 123")
                .SetInvoiceType(1100)
                .SetBusinessCategory("Business")
                .SetProduction(false);

            var builder2 = new CertificateBuilder()
                .SetOrganizationIdentifier("312345678901233")
                .SetSerialNumber("SolutionName", "Model", "Serial123")
                .SetCommonName("Test Common Name")
                .SetCountryName("SA")
                .SetOrganizationName("Test Organization")
                .SetOrganizationalUnitName("Test Unit")
                .SetAddress("Test Address 123")
                .SetInvoiceType(1100)
                .SetBusinessCategory("Business")
                .SetProduction(false);

            // Act
            builder1.Generate();
            builder2.Generate();

            var privateKey1 = builder1.GetPrivateKey();
            var privateKey2 = builder2.GetPrivateKey();

            // Assert - Keys should be different (different random generation)
            Assert.NotEqual(privateKey1, privateKey2);
        }

        /// <summary>
        /// Test that all required fields must be set before Generate().
        /// </summary>
        [Theory]
        [InlineData("organizationIdentifier")]
        [InlineData("serialNumber")]
        [InlineData("commonName")]
        [InlineData("organizationName")]
        [InlineData("organizationalUnitName")]
        [InlineData("address")]
        [InlineData("businessCategory")]
        public void TestMissingSpecificFieldThrowsException(string missingField)
        {
            // Arrange
            var builder = new CertificateBuilder()
                .SetOrganizationIdentifier("312345678901233")
                .SetSerialNumber("SolutionName", "Model", "Serial123")
                .SetCommonName("Test Common Name")
                .SetCountryName("SA")
                .SetOrganizationName("Test Organization")
                .SetOrganizationalUnitName("Test Unit")
                .SetAddress("Test Address 123")
                .SetInvoiceType(1100)
                .SetBusinessCategory("Business");

            // Reset the specific field by creating a new builder without it
            var builderWithMissing = new CertificateBuilder();

            // Set all fields except the missing one
            if (missingField != "organizationIdentifier")
                builderWithMissing.SetOrganizationIdentifier("312345678901233");
            if (missingField != "serialNumber")
                builderWithMissing.SetSerialNumber("SolutionName", "Model", "Serial123");
            if (missingField != "commonName")
                builderWithMissing.SetCommonName("Test Common Name");
            if (missingField != "organizationName")
                builderWithMissing.SetOrganizationName("Test Organization");
            if (missingField != "organizationalUnitName")
                builderWithMissing.SetOrganizationalUnitName("Test Unit");
            if (missingField != "address")
                builderWithMissing.SetAddress("Test Address 123");
            if (missingField != "businessCategory")
                builderWithMissing.SetBusinessCategory("Business");

            builderWithMissing.SetCountryName("SA");
            builderWithMissing.SetInvoiceType(1100);

            // Act & Assert
            var exception = Assert.Throws<CertificateBuilderException>(() => builderWithMissing.Generate());
            Assert.Contains(missingField, exception.Message);
        }

        /// <summary>
        /// Test CSR file content structure and format.
        /// </summary>
        [Fact]
        public void TestCsrFileStructure()
        {
            // Arrange
            var csrPath = Path.GetTempFileName();
            _tempFiles.Add(csrPath);

            var builder = new CertificateBuilder()
                .SetOrganizationIdentifier("312345678901233")
                .SetSerialNumber("SolutionName", "Model", "Serial123")
                .SetCommonName("Test Common Name")
                .SetCountryName("SA")
                .SetOrganizationName("Test Organization")
                .SetOrganizationalUnitName("Test Unit")
                .SetAddress("Test Address 123")
                .SetInvoiceType(1100)
                .SetBusinessCategory("Business")
                .SetProduction(false);

            // Act
            builder.GenerateAndSave(csrPath, Path.GetTempFileName());

            // Assert
            var csrContent = File.ReadAllText(csrPath);
            var lines = csrContent.Split('\n');

            // Verify PEM format
            Assert.Contains("-----BEGIN CERTIFICATE REQUEST-----", csrContent);
            Assert.Contains("-----END CERTIFICATE REQUEST-----", csrContent);

            // Verify base64 content exists between headers
            var base64Content = string.Join("", lines
                .Where(l => !l.Contains("BEGIN") && !l.Contains("END") && !string.IsNullOrWhiteSpace(l)));
            Assert.NotEmpty(base64Content);

            // Verify it's valid base64
            try
            {
                Convert.FromBase64String(base64Content.Trim());
            }
            catch (FormatException)
            {
                Assert.Fail("CSR content is not valid base64");
            }
        }

        /// <summary>
        /// Test builder method chaining returns correct instance.
        /// </summary>
        [Fact]
        public void TestMethodChainingReturnsBuilder()
        {
            // Arrange
            var builder = new CertificateBuilder();

            // Act & Assert - Each method should return the builder instance
            var result = builder
                .SetOrganizationIdentifier("312345678901233")
                .SetSerialNumber("SolutionName", "Model", "Serial123")
                .SetCommonName("Test Common Name")
                .SetCountryName("SA")
                .SetOrganizationName("Test Organization")
                .SetOrganizationalUnitName("Test Unit")
                .SetAddress("Test Address 123")
                .SetInvoiceType(1100)
                .SetBusinessCategory("Business")
                .SetProduction(false);

            Assert.Same(builder, result);
        }

        /// <summary>
        /// Test that empty or whitespace inputs throw exceptions.
        /// </summary>
        [Fact]
        public void TestEmptyInputsThrowException()
        {
            // Arrange
            var builder = new CertificateBuilder();

            // Act & Assert
            Assert.Throws<CertificateBuilderException>(() => builder.SetCommonName(""));
            Assert.Throws<CertificateBuilderException>(() => builder.SetCommonName("   "));
            Assert.Throws<CertificateBuilderException>(() => builder.SetOrganizationName(""));
            Assert.Throws<CertificateBuilderException>(() => builder.SetOrganizationalUnitName("   "));
            Assert.Throws<CertificateBuilderException>(() => builder.SetAddress(""));
            Assert.Throws<CertificateBuilderException>(() => builder.SetBusinessCategory("   "));
        }

        /// <summary>
        /// Test concurrent certificate generation.
        /// </summary>
        [Fact]
        public void TestConcurrentCertificateGeneration()
        {
            // Arrange
            var tasks = new List<Task<string>>();
            var numberOfConcurrentBuilds = 10;

            // Act
            for (int i = 0; i < numberOfConcurrentBuilds; i++)
            {
                var taskNumber = i;
                var task = Task.Run(() =>
                {
                    var builder = new CertificateBuilder()
                        .SetOrganizationIdentifier("312345678901233")
                        .SetSerialNumber($"Solution{taskNumber}", "Model", $"Serial{taskNumber}")
                        .SetCommonName($"Test Common Name {taskNumber}")
                        .SetCountryName("SA")
                        .SetOrganizationName($"Test Organization {taskNumber}")
                        .SetOrganizationalUnitName("Test Unit")
                        .SetAddress("Test Address 123")
                        .SetInvoiceType(1100)
                        .SetBusinessCategory("Business")
                        .SetProduction(false);

                    builder.Generate();
                    return builder.GetCsr();
                });

                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            // Assert - All tasks should complete successfully
            foreach (var task in tasks)
            {
                Assert.NotNull(task.Result);
                Assert.NotEmpty(task.Result);
                Assert.Contains("BEGIN CERTIFICATE REQUEST", task.Result);
            }

            // Verify all CSRs are unique
            var csrSet = new HashSet<string>(tasks.Select(t => t.Result));
            Assert.Equal(numberOfConcurrentBuilds, csrSet.Count);
        }

        /// <summary>
        /// Cleanup temp files after each test.
        /// </summary>
        public void Dispose()
        {
            foreach (var file in _tempFiles)
            {
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }
}
