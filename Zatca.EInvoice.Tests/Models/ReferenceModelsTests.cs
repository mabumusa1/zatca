using Zatca.EInvoice.Models.Party;
using Zatca.EInvoice.Models.References;

namespace Zatca.EInvoice.Tests.Models;

public class ReferenceModelsTests
{
    #region Contract Tests

    [Fact]
    public void Contract_Id_CanBeSet()
    {
        var contract = new Contract { Id = "CONTRACT-001" };
        contract.Id.Should().Be("CONTRACT-001");
    }

    [Fact]
    public void Contract_Id_AcceptsNull()
    {
        var contract = new Contract { Id = "CONTRACT-001" };
        contract.Id = null;
        contract.Id.Should().BeNull();
    }

    [Fact]
    public void Contract_Id_ThrowsOnEmpty()
    {
        var contract = new Contract();
        Action act = () => contract.Id = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Contract_Id_ThrowsOnWhitespace()
    {
        var contract = new Contract();
        Action act = () => contract.Id = "   ";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    #endregion

    #region DocumentReference Tests

    [Fact]
    public void DocumentReference_AllProperties_CanBeSet()
    {
        var docRef = new DocumentReference
        {
            Id = "DOC-001",
            UUID = "test-uuid",
            IssueDate = new DateOnly(2024, 1, 1),
            DocumentTypeCode = "130",
            DocumentDescription = "Purchase Order"
        };

        docRef.Id.Should().Be("DOC-001");
        docRef.UUID.Should().Be("test-uuid");
        docRef.IssueDate.Should().Be(new DateOnly(2024, 1, 1));
        docRef.DocumentTypeCode.Should().Be("130");
        docRef.DocumentDescription.Should().Be("Purchase Order");
    }

    #endregion

    #region InvoicePeriod Tests

    [Fact]
    public void InvoicePeriod_AllProperties_CanBeSet()
    {
        var period = new InvoicePeriod
        {
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 1, 31),
            Description = "Monthly Period"
        };

        period.StartDate.Should().Be(new DateOnly(2024, 1, 1));
        period.EndDate.Should().Be(new DateOnly(2024, 1, 31));
        period.Description.Should().Be("Monthly Period");
    }

    #endregion

    #region OrderReference Tests

    [Fact]
    public void OrderReference_Id_CanBeSet()
    {
        var orderRef = new OrderReference { Id = "PO-001" };
        orderRef.Id.Should().Be("PO-001");
    }

    [Fact]
    public void OrderReference_Id_ThrowsOnEmpty()
    {
        var orderRef = new OrderReference();
        Action act = () => orderRef.Id = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void OrderReference_Id_AcceptsNull()
    {
        var orderRef = new OrderReference { Id = "PO-001" };
        orderRef.Id = null;
        orderRef.Id.Should().BeNull();
    }

    [Fact]
    public void OrderReference_SalesOrderId_CanBeSet()
    {
        var orderRef = new OrderReference { SalesOrderId = "SO-001" };
        orderRef.SalesOrderId.Should().Be("SO-001");
    }

    [Fact]
    public void OrderReference_SalesOrderId_ThrowsOnEmpty()
    {
        var orderRef = new OrderReference();
        Action act = () => orderRef.SalesOrderId = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void OrderReference_SalesOrderId_AcceptsNull()
    {
        var orderRef = new OrderReference { SalesOrderId = "SO-001" };
        orderRef.SalesOrderId = null;
        orderRef.SalesOrderId.Should().BeNull();
    }

    #endregion

    #region Attachment Tests

    [Fact]
    public void Attachment_MimeCode_DefaultsToBase64()
    {
        var attachment = new Attachment();
        attachment.MimeCode.Should().Be("base64");
    }

    [Fact]
    public void Attachment_MimeCode_CanBeSet()
    {
        var attachment = new Attachment { MimeCode = "text/plain" };
        attachment.MimeCode.Should().Be("text/plain");
    }

    [Fact]
    public void Attachment_MimeCode_DefaultsToBase64OnNull()
    {
        var attachment = new Attachment();
        attachment.MimeCode = null!;
        attachment.MimeCode.Should().Be("base64");
    }

    [Fact]
    public void Attachment_EmbeddedDocumentBinaryObject_IsAliasForBase64Content()
    {
        var attachment = new Attachment();
        attachment.EmbeddedDocumentBinaryObject = "SGVsbG8gV29ybGQ=";
        attachment.Base64Content.Should().Be("SGVsbG8gV29ybGQ=");

        attachment.Base64Content = "VGVzdA==";
        attachment.EmbeddedDocumentBinaryObject.Should().Be("VGVzdA==");
    }

    [Fact]
    public void Attachment_SetBase64Content_SetsAllFields()
    {
        var attachment = new Attachment();
        attachment.SetBase64Content("SGVsbG8=", "test.txt", "text/plain");

        attachment.Base64Content.Should().Be("SGVsbG8=");
        attachment.FileName.Should().Be("test.txt");
        attachment.MimeType.Should().Be("text/plain");
    }

    [Fact]
    public void Attachment_Validate_ThrowsWhenNoContentProvided()
    {
        var attachment = new Attachment();
        Action act = () => attachment.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*filePath*externalReference*fileContent*");
    }

    [Fact]
    public void Attachment_Validate_ThrowsWhenBase64ContentWithoutMimeType()
    {
        var attachment = new Attachment { Base64Content = "SGVsbG8=" };
        Action act = () => attachment.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*mimeType*");
    }

    [Fact]
    public void Attachment_Validate_PassesWithExternalReference()
    {
        var attachment = new Attachment { ExternalReference = "https://example.com/doc.pdf" };
        Action act = () => attachment.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Attachment_Validate_PassesWithBase64ContentAndMimeType()
    {
        var attachment = new Attachment { Base64Content = "SGVsbG8=", MimeType = "text/plain" };
        Action act = () => attachment.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Attachment_Validate_ThrowsWhenFilePathDoesNotExist()
    {
        var attachment = new Attachment { FilePath = "/nonexistent/path/file.pdf" };
        Action act = () => attachment.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*does not exist*");
    }

    [Fact]
    public void Attachment_AllProperties_CanBeSet()
    {
        var attachment = new Attachment
        {
            FilePath = "/path/to/file",
            ExternalReference = "https://example.com/doc",
            Base64Content = "SGVsbG8=",
            FileName = "file.pdf",
            MimeType = "application/pdf"
        };

        attachment.FilePath.Should().Be("/path/to/file");
        attachment.ExternalReference.Should().Be("https://example.com/doc");
        attachment.Base64Content.Should().Be("SGVsbG8=");
        attachment.FileName.Should().Be("file.pdf");
        attachment.MimeType.Should().Be("application/pdf");
    }

    #endregion

    #region PaymentMeans Tests

    [Fact]
    public void PaymentMeans_PaymentMeansCode_CanBeSet()
    {
        var payment = new PaymentMeans { PaymentMeansCode = "10" };
        payment.PaymentMeansCode.Should().Be("10");
    }

    [Fact]
    public void PaymentMeans_PaymentMeansCode_ThrowsOnEmpty()
    {
        var payment = new PaymentMeans();
        Action act = () => payment.PaymentMeansCode = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void PaymentMeans_PaymentMeansCode_AcceptsNull()
    {
        var payment = new PaymentMeans { PaymentMeansCode = "10" };
        payment.PaymentMeansCode = null;
        payment.PaymentMeansCode.Should().BeNull();
    }

    [Fact]
    public void PaymentMeans_InstructionNote_CanBeSet()
    {
        var payment = new PaymentMeans { InstructionNote = "Pay within 30 days" };
        payment.InstructionNote.Should().Be("Pay within 30 days");
    }

    [Fact]
    public void PaymentMeans_InstructionNote_ThrowsOnEmpty()
    {
        var payment = new PaymentMeans();
        Action act = () => payment.InstructionNote = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void PaymentMeans_PaymentId_CanBeSet()
    {
        var payment = new PaymentMeans { PaymentId = "PAY-001" };
        payment.PaymentId.Should().Be("PAY-001");
    }

    [Fact]
    public void PaymentMeans_PaymentId_ThrowsOnEmpty()
    {
        var payment = new PaymentMeans();
        Action act = () => payment.PaymentId = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void PaymentMeans_PayeeFinancialAccount_CanBeSet()
    {
        var payment = new PaymentMeans { PayeeFinancialAccount = new { Id = "ACC-001" } };
        payment.PayeeFinancialAccount.Should().NotBeNull();
    }

    #endregion

    #region BillingReference Tests

    [Fact]
    public void BillingReference_Id_CanBeSet()
    {
        var billingRef = new BillingReference { Id = "INV-001" };
        billingRef.Id.Should().Be("INV-001");
    }

    [Fact]
    public void BillingReference_Id_ThrowsOnEmpty()
    {
        var billingRef = new BillingReference();
        Action act = () => billingRef.Id = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void BillingReference_Id_AcceptsNull()
    {
        var billingRef = new BillingReference { Id = "INV-001" };
        billingRef.Id = null;
        billingRef.Id.Should().BeNull();
    }

    #endregion

    #region AdditionalDocumentReference Tests

    [Fact]
    public void AdditionalDocumentReference_AllProperties_CanBeSet()
    {
        var docRef = new AdditionalDocumentReference
        {
            Id = "ICV",
            UUID = "10",
            DocumentType = "Invoice Counter",
            DocumentTypeCode = 130,
            DocumentDescription = "Counter Value",
            Attachment = new Attachment { Base64Content = "SGVsbG8=" }
        };

        docRef.Id.Should().Be("ICV");
        docRef.UUID.Should().Be("10");
        docRef.DocumentType.Should().Be("Invoice Counter");
        docRef.DocumentTypeCode.Should().Be(130);
        docRef.DocumentDescription.Should().Be("Counter Value");
        docRef.Attachment.Should().NotBeNull();
    }

    #endregion

    #region Delivery Tests

    [Fact]
    public void Delivery_AllProperties_CanBeSet()
    {
        var delivery = new Delivery
        {
            ActualDeliveryDate = new DateOnly(2024, 1, 15),
            LatestDeliveryDate = new DateOnly(2024, 1, 20),
            DeliveryLocation = new Address { CityName = "Riyadh" }
        };

        delivery.ActualDeliveryDate.Should().Be(new DateOnly(2024, 1, 15));
        delivery.LatestDeliveryDate.Should().Be(new DateOnly(2024, 1, 20));
        delivery.DeliveryLocation.Should().NotBeNull();
        delivery.DeliveryLocation!.CityName.Should().Be("Riyadh");
    }

    #endregion
}
