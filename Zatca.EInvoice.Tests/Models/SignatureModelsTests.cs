using Zatca.EInvoice.Models.Signature;

namespace Zatca.EInvoice.Tests.Models;

public class SignatureModelsTests
{
    #region UblExtension Tests

    [Fact]
    public void UblExtension_ExtensionURI_CanBeSet()
    {
        var ext = new UblExtension { ExtensionURI = "urn:oasis:names:specification:ubl:dsig:enveloped:xades" };
        ext.ExtensionURI.Should().Be("urn:oasis:names:specification:ubl:dsig:enveloped:xades");
    }

    [Fact]
    public void UblExtension_ExtensionURI_ThrowsOnEmpty()
    {
        var ext = new UblExtension();
        Action act = () => ext.ExtensionURI = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void UblExtension_ExtensionURI_AcceptsNull()
    {
        var ext = new UblExtension { ExtensionURI = "test" };
        ext.ExtensionURI = null;
        ext.ExtensionURI.Should().BeNull();
    }

    [Fact]
    public void UblExtension_ExtensionContent_CanBeSet()
    {
        var ext = new UblExtension { ExtensionContent = new ExtensionContent() };
        ext.ExtensionContent.Should().NotBeNull();
    }

    [Fact]
    public void UblExtension_Validate_ThrowsWhenExtensionURIMissing()
    {
        var ext = new UblExtension { ExtensionContent = new ExtensionContent() };
        Action act = () => ext.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*URI*required*");
    }

    [Fact]
    public void UblExtension_Validate_ThrowsWhenExtensionContentMissing()
    {
        var ext = new UblExtension { ExtensionURI = "urn:test" };
        Action act = () => ext.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*content*required*");
    }

    [Fact]
    public void UblExtension_Validate_PassesWithAllFields()
    {
        var ext = new UblExtension
        {
            ExtensionURI = "urn:test",
            ExtensionContent = new ExtensionContent()
        };
        Action act = () => ext.Validate();
        act.Should().NotThrow();
    }

    #endregion

    #region UblExtensions Tests

    [Fact]
    public void UblExtensions_Extensions_DefaultsToEmptyList()
    {
        var extensions = new UblExtensions();
        extensions.Extensions.Should().NotBeNull();
        extensions.Extensions.Should().BeEmpty();
    }

    [Fact]
    public void UblExtensions_Extensions_CanBePopulated()
    {
        var extensions = new UblExtensions();
        extensions.Extensions.Add(new UblExtension { ExtensionURI = "test" });
        extensions.Extensions.Should().HaveCount(1);
    }

    [Fact]
    public void UblExtensions_Validate_ThrowsWhenExtensionsEmpty()
    {
        var extensions = new UblExtensions();
        Action act = () => extensions.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*Extension*");
    }

    [Fact]
    public void UblExtensions_Validate_ThrowsWhenExtensionsNull()
    {
        var extensions = new UblExtensions { Extensions = null! };
        Action act = () => extensions.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*Extension*");
    }

    [Fact]
    public void UblExtensions_Validate_PassesWithExtensions()
    {
        var extensions = new UblExtensions();
        extensions.Extensions.Add(new UblExtension());
        Action act = () => extensions.Validate();
        act.Should().NotThrow();
    }

    #endregion

    #region ExtensionContent Tests

    [Fact]
    public void ExtensionContent_UblDocumentSignatures_CanBeSet()
    {
        var content = new ExtensionContent { UblDocumentSignatures = new UblDocumentSignatures() };
        content.UblDocumentSignatures.Should().NotBeNull();
    }

    [Fact]
    public void ExtensionContent_Validate_ThrowsWhenUblDocumentSignaturesMissing()
    {
        var content = new ExtensionContent();
        Action act = () => content.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*UBLDocumentSignatures*");
    }

    [Fact]
    public void ExtensionContent_Validate_PassesWithUblDocumentSignatures()
    {
        var content = new ExtensionContent { UblDocumentSignatures = new UblDocumentSignatures() };
        Action act = () => content.Validate();
        act.Should().NotThrow();
    }

    #endregion

    #region UblDocumentSignatures Tests

    [Fact]
    public void UblDocumentSignatures_SignatureInformation_CanBeSet()
    {
        var signatures = new UblDocumentSignatures { SignatureInformation = new SignatureInformation() };
        signatures.SignatureInformation.Should().NotBeNull();
    }

    [Fact]
    public void UblDocumentSignatures_Validate_ThrowsWhenSignatureInformationMissing()
    {
        var signatures = new UblDocumentSignatures();
        Action act = () => signatures.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*Signature information*");
    }

    [Fact]
    public void UblDocumentSignatures_Validate_PassesWithSignatureInformation()
    {
        var signatures = new UblDocumentSignatures { SignatureInformation = new SignatureInformation() };
        Action act = () => signatures.Validate();
        act.Should().NotThrow();
    }

    #endregion

    #region SignatureInformation Tests

    [Fact]
    public void SignatureInformation_Id_CanBeSet()
    {
        var info = new SignatureInformation { Id = "urn:oasis:names:specification:ubl:signature:1" };
        info.Id.Should().Be("urn:oasis:names:specification:ubl:signature:1");
    }

    [Fact]
    public void SignatureInformation_Id_ThrowsOnEmpty()
    {
        var info = new SignatureInformation();
        Action act = () => info.Id = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void SignatureInformation_Id_AcceptsNull()
    {
        var info = new SignatureInformation { Id = "test" };
        info.Id = null;
        info.Id.Should().BeNull();
    }

    [Fact]
    public void SignatureInformation_ReferencedSignatureID_CanBeSet()
    {
        var info = new SignatureInformation { ReferencedSignatureID = "urn:oasis:names:specification:ubl:signature:Invoice" };
        info.ReferencedSignatureID.Should().Be("urn:oasis:names:specification:ubl:signature:Invoice");
    }

    [Fact]
    public void SignatureInformation_ReferencedSignatureID_ThrowsOnEmpty()
    {
        var info = new SignatureInformation();
        Action act = () => info.ReferencedSignatureID = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void SignatureInformation_ReferencedSignatureID_AcceptsNull()
    {
        var info = new SignatureInformation { ReferencedSignatureID = "test" };
        info.ReferencedSignatureID = null;
        info.ReferencedSignatureID.Should().BeNull();
    }

    #endregion

    #region Signature Tests

    [Fact]
    public void Signature_Id_HasDefaultValue()
    {
        var signature = new Signature();
        signature.Id.Should().Be("urn:oasis:names:specification:ubl:signature:Invoice");
    }

    [Fact]
    public void Signature_SignatureMethod_HasDefaultValue()
    {
        var signature = new Signature();
        signature.SignatureMethod.Should().Be("urn:oasis:names:specification:ubl:dsig:enveloped:xades");
    }

    [Fact]
    public void Signature_Id_CanBeChanged()
    {
        var signature = new Signature { Id = "custom-signature-id" };
        signature.Id.Should().Be("custom-signature-id");
    }

    [Fact]
    public void Signature_Id_ThrowsOnEmpty()
    {
        var signature = new Signature();
        Action act = () => signature.Id = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Signature_Id_ThrowsOnWhitespace()
    {
        var signature = new Signature();
        Action act = () => signature.Id = "   ";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Signature_SignatureMethod_CanBeChanged()
    {
        var signature = new Signature { SignatureMethod = "custom-method" };
        signature.SignatureMethod.Should().Be("custom-method");
    }

    [Fact]
    public void Signature_SignatureMethod_ThrowsOnEmpty()
    {
        var signature = new Signature();
        Action act = () => signature.SignatureMethod = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Signature_SignatureMethod_ThrowsOnWhitespace()
    {
        var signature = new Signature();
        Action act = () => signature.SignatureMethod = "   ";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    #endregion
}
