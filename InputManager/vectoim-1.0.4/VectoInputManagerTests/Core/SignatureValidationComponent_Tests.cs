using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using VectoInputManager.Core;
using VectoObjects;
using VectoObjects.Helpers;

namespace VectoInputManager.Tests.Core;

public class SignatureValidationComponent_Tests
{
    private static string ComponentsPath => Path.Combine(TestUtils.InputPath, "Components");

    [Test]
    public void Auto_ValidSig_Succeeds()
    {
        // Auto mode with valid signature should re-sign and produce valid output.
        var processor = CreateProcessor();
        var component = processor.LoadComponent(Path.Combine(ComponentsPath, "GearBox-ValidSig.xml"));

        var (skipSigning, signatureOverride) = processor.EnforceSignatureAction(
            component, wasModified: false, isXmlInput: true, BrokenSignatureAction.Auto);

        var reloaded = SaveAndReload(component, skipSigning, signatureOverride);

        var sig = reloaded.GetSignature();
        sig.IsEmptySignature().Should().BeFalse("Output should have a valid signature");
    }

    [Test]
    public void Auto_EmptySig_ReSigns()
    {
        // Auto mode with empty signature should re-sign and produce valid output.
        var processor = CreateProcessor();
        var component = processor.LoadComponent(Path.Combine(ComponentsPath, "Axle-NoSig.xml"));

        var (skipSigning, signatureOverride) = processor.EnforceSignatureAction(
            component, wasModified: false, isXmlInput: true, BrokenSignatureAction.Auto);

        var reloaded = SaveAndReload(component, skipSigning, signatureOverride);

        var sig = reloaded.GetSignature();
        sig.IsEmptySignature().Should().BeFalse("Re-signed output should have a valid signature");
    }

    [Test]
    public void Auto_CorruptedSig_ReSigns()
    {
        // Auto mode with corrupted signature should re-sign and produce valid output.
        var processor = CreateProcessor();
        var component = processor.LoadComponent(Path.Combine(ComponentsPath, "FuelCell-CorruptedSig.xml"));

        var (skipSigning, signatureOverride) = processor.EnforceSignatureAction(
            component, wasModified: false, isXmlInput: true, BrokenSignatureAction.Auto);

        var reloaded = SaveAndReload(component, skipSigning, signatureOverride);

        var sig = reloaded.GetSignature();
        sig.IsEmptySignature().Should().BeFalse("Re-signed output should have a valid signature");
    }

    [Test]
    public void Auto_Modified_ValidSig_ProducesEmptySignature()
    {
        // Auto mode with modified component (originally valid sig) should produce empty signature.
        var processor = CreateProcessor();
        var component = processor.LoadComponent(Path.Combine(ComponentsPath, "GearBox-ValidSig.xml"));

        var (skipSigning, signatureOverride) = processor.EnforceSignatureAction(
            component, wasModified: true, isXmlInput: true, BrokenSignatureAction.Auto);

        var reloaded = SaveAndReload(component, skipSigning, signatureOverride);

        var sig = reloaded.GetSignature();
        sig.IsEmptySignature().Should().BeTrue("Modified component should have empty signature in auto mode");
    }

    [Test]
    public void Auto_Modified_EmptySig_ProducesEmptySignature()
    {
        // Auto mode with modified component (originally empty sig) should produce empty signature.
        var processor = CreateProcessor();
        var component = processor.LoadComponent(Path.Combine(ComponentsPath, "Axle-NoSig.xml"));

        var (skipSigning, signatureOverride) = processor.EnforceSignatureAction(
            component, wasModified: true, isXmlInput: true, BrokenSignatureAction.Auto);

        var reloaded = SaveAndReload(component, skipSigning, signatureOverride);

        var sig = reloaded.GetSignature();
        sig.IsEmptySignature().Should().BeTrue("Modified component should have empty signature in auto mode");
    }

    [Test]
    public void Auto_MismatchSig_ReturnsError()
    {
        // Auto mode with mismatched signature should block processing.
        var processor = CreateProcessor();
        var component = processor.LoadComponent(Path.Combine(ComponentsPath, "GearBox-MismatchSig.xml"));

        Action act = () => processor.EnforceSignatureAction(
            component, wasModified: false, isXmlInput: true, BrokenSignatureAction.Auto);

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Auto_Modified_MismatchSig_ProducesEmptySignature()
    {
        // Auto mode with modified component (originally mismatched sig) should produce empty signature.
        // Modification takes precedence over mismatch blocking.
        var processor = CreateProcessor();
        var component = processor.LoadComponent(Path.Combine(ComponentsPath, "GearBox-MismatchSig.xml"));

        var (skipSigning, signatureOverride) = processor.EnforceSignatureAction(
            component, wasModified: true, isXmlInput: true, BrokenSignatureAction.Auto);

        var reloaded = SaveAndReload(component, skipSigning, signatureOverride);

        var sig = reloaded.GetSignature();
        sig.IsEmptySignature().Should().BeTrue("Modified component should have empty signature in auto mode");
    }

    [Test]
    public void Block_CorruptedSig_ReturnsError()
    {
        var processor = CreateProcessor();
        var component = processor.LoadComponent(Path.Combine(ComponentsPath, "FuelCell-CorruptedSig.xml"));

        Action act = () => processor.EnforceSignatureAction(
            component, wasModified: false, isXmlInput: true, BrokenSignatureAction.Block);

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Block_ValidSig_Succeeds()
    {
        var processor = CreateProcessor();
        var component = processor.LoadComponent(Path.Combine(ComponentsPath, "GearBox-ValidSig.xml"));

        var (skipSigning, signatureOverride) = processor.EnforceSignatureAction(
            component, wasModified: false, isXmlInput: true, BrokenSignatureAction.Block);

        var reloaded = SaveAndReload(component, skipSigning, signatureOverride);

        var sig = reloaded.GetSignature();
        sig.IsEmptySignature().Should().BeFalse("Output should have a valid signature");
    }

    [Test]
    public void Block_Modified_ReturnsError()
    {
        var processor = CreateProcessor();
        var component = processor.LoadComponent(Path.Combine(ComponentsPath, "GearBox-ValidSig.xml"));

        Action act = () => processor.EnforceSignatureAction(
            component, wasModified: true, isXmlInput: true, BrokenSignatureAction.Block);

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void ReSign_CorruptedSig_Succeeds()
    {
        var processor = CreateProcessor();
        var component = processor.LoadComponent(Path.Combine(ComponentsPath, "FuelCell-CorruptedSig.xml"));

        var (skipSigning, signatureOverride) = processor.EnforceSignatureAction(
            component, wasModified: false, isXmlInput: true, BrokenSignatureAction.ReSign);

        var reloaded = SaveAndReload(component, skipSigning, signatureOverride);

        var sig = reloaded.GetSignature();
        sig.IsEmptySignature().Should().BeFalse("Re-signed output should have a valid signature");
    }

    [Test]
    public void Clear_ProducesEmptySignature()
    {
        var processor = CreateProcessor();
        var component = processor.LoadComponent(Path.Combine(ComponentsPath, "GearBox-ValidSig.xml"));

        var (skipSigning, signatureOverride) = processor.EnforceSignatureAction(
            component, wasModified: false, isXmlInput: true, BrokenSignatureAction.Clear);

        var reloaded = SaveAndReload(component, skipSigning, signatureOverride);

        var sig = reloaded.GetSignature();
        sig.IsEmptySignature().Should().BeTrue("Clear should produce empty signature");
    }

    [Test]
    public void Keep_CorruptedSig_PreservesOriginal()
    {
        var processor = CreateProcessor();
        var component = processor.LoadComponent(Path.Combine(ComponentsPath, "FuelCell-CorruptedSig.xml"));

        var (skipSigning, signatureOverride) = processor.EnforceSignatureAction(
            component, wasModified: false, isXmlInput: true, BrokenSignatureAction.Keep);

        var reloaded = SaveAndReload(component, skipSigning, signatureOverride);

        var sig = reloaded.GetSignature();
        sig.IsEmptySignature().Should().BeFalse("Kept signature should not be empty");
    }

    [Test]
    public void Sign_EmptySig_SignsComponent()
    {
        var processor = CreateProcessor();
        var component = processor.LoadComponent(Path.Combine(ComponentsPath, "Axle-NoSig.xml"));

        var (skipSigning, signatureOverride) = processor.EnforceSignatureAction(
            component, wasModified: false, isXmlInput: true, BrokenSignatureAction.Sign);

        var reloaded = SaveAndReload(component, skipSigning, signatureOverride);

        var sig = reloaded.GetSignature();
        sig.IsEmptySignature().Should().BeFalse("Signed output should have a valid signature");
    }

    [Test]
    public void Sign_CorruptedSig_ReturnsError()
    {
        var processor = CreateProcessor();
        var component = processor.LoadComponent(Path.Combine(ComponentsPath, "FuelCell-CorruptedSig.xml"));

        Action act = () => processor.EnforceSignatureAction(
            component, wasModified: false, isXmlInput: true, BrokenSignatureAction.Sign);

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Sign_Modified_EmptySig_Succeeds()
    {
        var processor = CreateProcessor();
        var component = processor.LoadComponent(Path.Combine(ComponentsPath, "Axle-NoSig.xml"));

        var (skipSigning, signatureOverride) = processor.EnforceSignatureAction(
            component, wasModified: true, isXmlInput: true, BrokenSignatureAction.Sign);

        var reloaded = SaveAndReload(component, skipSigning, signatureOverride);

        var sig = reloaded.GetSignature();
        sig.IsEmptySignature().Should().BeFalse("Signed output should have a valid signature");
    }

    [Test]
    public void Sign_Modified_ValidSig_ReturnsError()
    {
        var processor = CreateProcessor();
        var component = processor.LoadComponent(Path.Combine(ComponentsPath, "GearBox-ValidSig.xml"));

        Action act = () => processor.EnforceSignatureAction(
            component, wasModified: true, isXmlInput: true, BrokenSignatureAction.Sign);

        act.Should().Throw<InvalidOperationException>();
    }

    private static ComponentProcessor CreateProcessor()
    {
        var provider = TestUtils.TestServiceProvider();
        return provider.GetRequiredService<ComponentProcessor>();
    }

    private IComponentDeclaration SaveAndReload(
        IComponentDeclaration component,
        bool skipSigning,
        SignatureType_v2_0? signatureOverride,
        [CallerMemberName] string memberName = "")
    {
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(SignatureValidationComponent_Tests));
        var outFile = Path.Combine(outDir, memberName + ".xml");

        var upgraded = TypeUtils.UpgradeComponent(component);
        var vInputDeclaration = TypeUtils.CreateVectoInputDeclaration();
        vInputDeclaration.SetComponentInVectoInputDeclaration(upgraded);

        using (var fs = File.Create(outFile))
        {
            if (!skipSigning)
            {
                XmlUtils.HashAndSerialize(fs, vInputDeclaration, null);
            }
            else
            {
                upgraded.Signature = signatureOverride!;
                var serializer = new XmlSerializer(typeof(VectoInputDeclaration_v2_2));
                serializer.SerializeFixed(fs, vInputDeclaration);
            }
        }

        TestUtils.AssertValidateComponentXML(outFile);
        return TestUtils.LoadComponentFromXml(outFile);
    }
}
