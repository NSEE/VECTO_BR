using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using VectoInputManager.Core;
using VectoObjects;
using VectoObjects.Helpers;

namespace VectoInputManager.Tests.Core;

/*

| Input Signature     | auto (def.) | sign        | block    | keep         | re-sign       | clear        |
|---------------------|-------------|-------------|----------|--------------|---------------|--------------|
| Empty or Corrupted  | ▪ 🔲 ℹ️     | 🔒 ✅ ℹ️    | 🔴      | ▪ 🔲 ⚠️      | 🔒 ✅ ℹ️      | ▬ 🔲 ⚠️      |
| Mismatch            | 🔴          | 🔴          | 🔴      | ▪ ✖️ ⚠️      | 🔒 ✅ ⚠️      | ▬ 🔲 ⚠️      |
| Valid               | ▪ ✅        | ▪ ✅        | ▪ ✅    | ▪ ✅         | ▪ ✅          | ▬ 🔲 ⚠️      |

**Actions**: 
🔴 = Block processing (throw exception)
▪ = Do nothing (keep component as-is)
🔒 = (Re)sign component (calculate new signature)
▬ = Drop signature (remove signature)
  
**Output Signature**: 
✅ = Valid signature
✖️ = Signature doesn't match the component data (hash mismatch)
🔲 = Empty signature
  
**Logging**: 
ℹ️ = Info message
⚠️ = Warning message

*/

/// </summary>
public class SignatureValidation_Tests
{
    private const string NoSig_Axle = "Axle-NoSignature.xml";
    private const string NoSig_Json_Battery = "Battery-json.json";

    private const string MismatchSig_EMS = "EMS-Modification-Mismatch.xml";
    private const string MismatchSig_Tyre = "Tyre1-Mismatch.xml";

    private const string CorruptedSig_FuelCell = "FuelCell-Corrupted.xml";

    private const string ValidSig_Gearbox = "Gearbox-Valid.xml";

    private const string VehicleValid = "Vehicle-Valid.xml";
    private const string VehicleEmptySig = "Vehicle-EmptySigs.xml";
    private const string VehicleGearboxMismatch = "Vehicle-Gearbox-Mismatch.xml";

    private const string GearboxModifyJson = "Gearbox-Modify.json";

    #region Auto Action Tests

    [Test]
    public void Auto_EmptySignature_ShouldKeepEmpty()
    {
        // Arrange: Component with Empty or Corrupted signature
        // Result: ▪ 🔲 ℹ️ Keep empty signature, info logged
        var (corpus, job) = PrepareTest([VehicleValid, NoSig_Axle, NoSig_Json_Battery]);

        // Act
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Auto);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw, no errors (empty signatures are acceptable)
        var errors = corpus.HashErrors;
        errors.Should().BeEmpty();

        // Verify component signatures
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, path) in components)
        {
            if (path == "Components.Axlegear")
            {
                component.Signature.IsEmptySignature().Should().BeTrue();
                continue;
            }
            if (path == "Components.ElectricEnergyStorage.Battery[0].REESS")
            {
                component.Signature.IsEmptySignature().Should().BeTrue();
                continue;
            }

            var result = XmlUtils.ValidateComponentHashes(component);
            result.Should().Be(SignatureValidation.Valid);
        }
    }

    [Test]
    public void Auto_MismatchSignature_ShouldBlock()
    {
        // Arrange: Component with Mismatch signature
        // Result: 🔴 Block processing (throw exception)
        var (corpus, job) = PrepareTest([VehicleValid, MismatchSig_EMS, NoSig_Json_Battery]);

        // Act & Assert
        Action act = () => corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Auto);
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Auto_ValidSignature_Json_Mix()
    {
        // Arrange: Component with Valid signature
        // Result: Keeps signature on Gearbox XML, empty on the JSON
        var (corpus, job) = PrepareTest([VehicleValid, ValidSig_Gearbox, NoSig_Json_Battery]);

        // Act
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Auto);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw, no errors
        var errors = corpus.HashErrors;
        errors.Should().BeEmpty();

        // Verify all component signatures remain valid
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, path) in components)
        {
            if (path == "Components.ElectricEnergyStorage.Battery[0].REESS")
            {
                component.Signature.IsEmptySignature().Should().BeTrue();
                continue;
            }

            var result = XmlUtils.ValidateComponentHashes(component);
            result.Should().Be(SignatureValidation.Valid);
        }
    }

    [Test]
    public void Auto_ValidSignature_Modification_Test()
    {
        // Arrange: Valid component that has been modified
        // Result: 🔴 Empty Signature on the modified component
        var (corpus, job) = PrepareTest([VehicleValid, GearboxModifyJson]);

        // Act
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Auto);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw, no errors
        var errors = corpus.HashErrors;
        errors.Should().BeEmpty();

        // Verify component signatures - all should remain empty or valid
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, path) in components)
        {
            // Modified component should have empty signature
            if (path == "Components.Gearbox")
            {
                component.Signature.IsEmptySignature().Should().BeTrue();
                continue;
            }

            // Other components should be valid or empty
            var result = XmlUtils.ValidateComponentHashes(component);
            result.Should().Be(SignatureValidation.Valid);
        }
    }

    [Test]
    public void Auto_EmptySignature_Modification_Test()
    {
        // Arrange: Component with empty signature that has been modified
        // Result: ▪ 🔲 ℹ️ Keep empty signature (allow modification), info logged
        var (corpus, job) = PrepareTest([VehicleEmptySig, GearboxModifyJson]);

        // Act
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Auto);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw, no errors
        var errors = corpus.HashErrors;
        errors.Should().BeEmpty();

        // Verify component signatures - all should remain empty or valid
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, path) in components)
        {
            // Modified component should still have empty signature
            if (path == "Components.Gearbox")
            {
                component.Signature.IsEmptySignature().Should().BeTrue();
            }

            // Other components should be valid or empty
            var result = XmlUtils.ValidateComponentHashes(component);
            result.Should().Be(SignatureValidation.Empty);
        }
    }

    [Test]
    public void Auto_MismatchSignature_Modification_Test()
    {
        // Arrange: Invalid component that has been modified
        // Result: 🔴 Block processing (throw exception)
        var (corpus, job) = PrepareTest([VehicleGearboxMismatch, GearboxModifyJson]);

        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Auto);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw, no errors
        var errors = corpus.HashErrors;
        errors.Should().BeEmpty();

        // Verify component signatures - all should remain empty or valid
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, path) in components)
        {
            // Modified component should still have empty signature
            if (path == "Components.Gearbox")
            {
                component.Signature.IsEmptySignature().Should().BeTrue();
                continue;
            }

            // Other components should be valid or empty
            var result = XmlUtils.ValidateComponentHashes(component);
            result.Should().Be(SignatureValidation.Valid);
        }
    }

    #endregion

    #region Block Action Tests

    [Test]
    public void Block_EmptySignature_ShouldBlock()
    {
        // Arrange: Component with Empty or Corrupted signature
        // Result: 🔴 Block processing (throw exception)
        var (corpus, job) = PrepareTest([VehicleValid, NoSig_Axle, NoSig_Json_Battery]);

        // Act & Assert
        Action act = () => corpus.EnforceSignatureAction(
            (IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Block);
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Block_MismatchSignature_ShouldBlock()
    {
        // Arrange: Component with Mismatch signature
        // Result: 🔴 Block processing (throw exception)
        var (corpus, job) = PrepareTest([VehicleValid, MismatchSig_EMS, NoSig_Json_Battery]);

        // Act & Assert
        Action act = () => corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Block);
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Block_ValidSignature_Test()
    {
        // Arrange: Component with Valid signature
        // Result: ▪ ✅ Do nothing, output valid signature
        var (corpus, job) = PrepareTest([VehicleValid, ValidSig_Gearbox, NoSig_Json_Battery]);

        // Act
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Block);

        job = SaveAndReload(corpus, job);

        // Assert
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();
        components.Should().NotBeEmpty();

        // Verify signatures
        foreach (var (component, _) in components)
        {
            var result = XmlUtils.ValidateComponentHashes(component);
            result.Should().Be(SignatureValidation.Valid);
        }
    }

    [Test]
    public void Block_ValidSignature_Modification_Test()
    {
        // Arrange: Valid component that has been modified
        // Result: 🔴 Block processing (throw exception)
        var (corpus, job) = PrepareTest([VehicleValid, GearboxModifyJson]);

        // Act & Assert
        Action act = () => corpus.EnforceSignatureAction(
            (IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Block);
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Block_MismatchSignature_Modification_Test()
    {
        // Arrange: Invalid component that has been modified
        // Result: 🔴 Block processing (throw exception)
        var (corpus, job) = PrepareTest([VehicleGearboxMismatch, GearboxModifyJson]);

        // Act & Assert
        Action act = () => corpus.EnforceSignatureAction(
            (IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Block);
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Keep Action Tests

    [Test]
    public void Keep_EmptySignature_ShouldKeepEmptySignature()
    {
        // Arrange: Component with Empty or Corrupted signature
        // Result: ▪ 🔲 ⚠️ Do nothing, output empty signature, warning logged
        var (corpus, job) = PrepareTest([VehicleValid, NoSig_Axle, NoSig_Json_Battery]);

        // Act
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Keep);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw, warning should be logged
        var errors = corpus.HashErrors;
        errors.Count.Should().Be(1);
        errors[0].path.Should().Be("Components.Axlegear");
        errors[0].result.Should().Be(SignatureValidation.Empty);

        // Verify component signatures
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, path) in components)
        {
            if (path == "Components.Axlegear")
            {
                component.Signature.IsEmptySignature().Should().BeTrue();
                continue;
            }

            var result = XmlUtils.ValidateComponentHashes(component);
            result.Should().Be(SignatureValidation.Valid);
        }
    }

    [Test]
    public void Keep_MismatchSignature_ShouldKeepMismatchSignature()
    {
        // Arrange: Component with Mismatch signature
        // Result: ▪ ✖️ ⚠️ Do nothing, output mismatch signature, warning logged
        var (corpus, job) = PrepareTest([VehicleValid, MismatchSig_EMS, NoSig_Json_Battery]);

        // Act
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Keep);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw, warning should be logged
        var errors = corpus.HashErrors;
        errors.Count.Should().Be(1);
        errors[0].path.Should().Be("Components.ElectricMachine.ElectricMachineSystem");
        errors[0].result.Should().Be(SignatureValidation.Mismatch);

        // Verify component signatures - invalid signature is kept
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, path) in components)
        {
            if (path == "Components.ElectricMachine.ElectricMachineSystem")
            {
                component.Signature.Should().NotBeNull();
                var r = XmlUtils.ValidateComponentHashes(component);
                r.Should().Be(SignatureValidation.Mismatch);
                continue;
            }

            var result = XmlUtils.ValidateComponentHashes(component);
            result.Should().Be(SignatureValidation.Valid);
        }
    }

    [Test]
    public void Keep_VehicleMismatchSignature_ShouldKeepMismatchSignature()
    {
        // Arrange: Component with Mismatch signature
        // Result: ▪ ✖️ ⚠️ Do nothing, output mismatch signature, warning logged
        var (corpus, job) = PrepareTest([VehicleGearboxMismatch, MismatchSig_EMS]);

        // Act
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Keep);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw, warning should be logged
        var errors = corpus.HashErrors;
        errors.Count.Should().Be(2);
        errors[0].path.Should().Be("Components.ElectricMachine.ElectricMachineSystem");
        errors[0].result.Should().Be(SignatureValidation.Mismatch);
        errors[1].path.Should().Be("Components.Gearbox");
        errors[1].result.Should().Be(SignatureValidation.Mismatch);

        // Verify component signatures - invalid signature is kept
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, path) in components)
        {
            if (path == "Components.ElectricMachine.ElectricMachineSystem")
            {
                component.Signature.Should().NotBeNull();
                var r = XmlUtils.ValidateComponentHashes(component);
                r.Should().Be(SignatureValidation.Mismatch);
                continue;
            }

            if (path == "Components.Gearbox")
            {
                component.Signature.Should().NotBeNull();
                var r = XmlUtils.ValidateComponentHashes(component);
                r.Should().Be(SignatureValidation.Mismatch);
                continue;
            }

            var result = XmlUtils.ValidateComponentHashes(component);
            result.Should().Be(SignatureValidation.Valid);
        }
    }

    [Test]
    public void Keep_ValidSignature_ShouldDoNothing()
    {
        // Arrange: Component with Valid signature
        // Result: ▪ ✅ Do nothing, output valid signature
        var (corpus, job) = PrepareTest([VehicleValid, ValidSig_Gearbox, NoSig_Json_Battery]);

        // Act
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Keep);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw, no errors
        var errors = corpus.HashErrors;
        errors.Should().BeEmpty();

        // Verify all component signatures remain valid
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, _) in components)
        {
            var result = XmlUtils.ValidateComponentHashes(component);
            result.Should().Be(SignatureValidation.Valid);
        }
    }

    [Test]
    public void Keep_ValidSignature_Modification_Test()
    {
        // Arrange: Valid component that has been modified
        // Result: ▪ ✖️ ⚠️ Keep original signature (now mismatched due to modification), warning logged
        var (corpus, job) = PrepareTest([VehicleValid, GearboxModifyJson]);

        // Act
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Keep);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw, warning should be logged
        var errors = corpus.HashErrors;
        errors.Count.Should().Be(1);
        errors[0].path.Should().Be("Components.Gearbox");
        errors[0].result.Should().Be(SignatureValidation.Mismatch);

        // Verify component signatures - original signature is kept but now mismatched
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, path) in components)
        {
            if (path == "Components.Gearbox")
            {
                component.Signature.Should().NotBeNull();
                var r = XmlUtils.ValidateComponentHashes(component);
                r.Should().Be(SignatureValidation.Mismatch);
                continue;
            }

            var result = XmlUtils.ValidateComponentHashes(component);
            result.Should().Be(SignatureValidation.Valid);
        }
    }

    [Test]
    public void Keep_MismatchSignature_Modification_Test()
    {
        // Arrange: Invalid component that has been modified
        // Result: ▪ ✖️ ⚠️ Keep original signature (still mismatched), warning logged
        var (corpus, job) = PrepareTest([VehicleGearboxMismatch, GearboxModifyJson]);

        // Act
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Keep);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw, warning should be logged
        var errors = corpus.HashErrors;
        errors.Count.Should().Be(1);
        errors[0].path.Should().Be("Components.Gearbox");
        errors[0].result.Should().Be(SignatureValidation.Mismatch);

        // Verify component signatures - original mismatched signature is kept
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, path) in components)
        {
            if (path == "Components.Gearbox")
            {
                component.Signature.Should().NotBeNull();
                var r = XmlUtils.ValidateComponentHashes(component);
                r.Should().Be(SignatureValidation.Mismatch);
                continue;
            }

            var result = XmlUtils.ValidateComponentHashes(component);
            result.Should().Be(SignatureValidation.Valid);
        }
    }

    #endregion

    #region Sign Action Tests

    [Test]
    public void Sign_EmptySignature_ShouldSign()
    {
        // Arrange: Component with Empty or Corrupted signature
        // Result: 🔒 ✅ ℹ️ (Re)sign component, output valid signature, info logged
        var (corpus, job) = PrepareTest([VehicleValid, NoSig_Axle, NoSig_Json_Battery]);

        // Act
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Sign);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw, and no errors/warnings tracked (just signs silently)
        var errors = corpus.HashErrors;
        errors.Should().BeEmpty();

        // Verify component signatures - empty signature will be signed later (not validated here)
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, _) in components)
        {
            var result = XmlUtils.ValidateComponentHashes(component);
            result.Should().Be(SignatureValidation.Valid);
        }
    }

    [Test]
    public void Sign_MismatchSignature_ShouldBlock()
    {
        // Arrange: Component with Mismatch signature
        // Result: 🔴 Block processing (throw exception)
        var (corpus, job) = PrepareTest([VehicleValid, MismatchSig_EMS, NoSig_Json_Battery]);

        // Act & Assert
        Action act = () => corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Sign);
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Sign_ValidSignature_ShouldDoNothing()
    {
        // Arrange: Component with Valid signature
        // Result: ▪ ✅ Do nothing, output valid signature
        var (corpus, job) = PrepareTest([VehicleValid, ValidSig_Gearbox, NoSig_Json_Battery]);

        // Act
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Sign);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw, no errors
        var errors = corpus.HashErrors;
        errors.Should().BeEmpty();

        // Verify all component signatures remain valid
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, _) in components)
        {
            var result = XmlUtils.ValidateComponentHashes(component);
            result.Should().Be(SignatureValidation.Valid);
        }
    }

    [Test]
    public void Sign_ValidSignature_Modification_Test()
    {
        // Arrange: Valid component that has been modified
        // Result: 🔴 Block processing (throw exception)
        var (corpus, job) = PrepareTest([VehicleValid, GearboxModifyJson]);

        // Act & Assert
        Action act = () => corpus.EnforceSignatureAction(
             (IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Sign);
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Sign_EmptySignature_Modification_Test()
    {
        // Arrange: Valid component that has been modified
        // Result: 🔴 Block processing (throw exception)
        var (corpus, job) = PrepareTest([VehicleEmptySig, GearboxModifyJson]);

        // Act
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Sign);
        job = SaveAndReload(corpus, job);

        // Assert: Should not throw
        // Note: Even valid signatures will be re-signed - no errors logged for valid sigs
        var errors = corpus.HashErrors;
        errors.Should().BeEmpty();

        // Verify all component signatures are still valid at this point
        // (they will be re-signed during serialization)
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, _) in components)
        {
            var result = XmlUtils.ValidateComponentHashes(component);
            result.Should().Be(SignatureValidation.Valid);
        }
    }

    [Test]
    public void Sign_MismatchSignature_Modification_Test()
    {
        // Arrange: Invalid component that has been modified
        // Result: 🔴 Block processing (throw exception)
        var (corpus, job) = PrepareTest([VehicleGearboxMismatch, GearboxModifyJson]);

        // Act & Assert
        Action act = () => corpus.EnforceSignatureAction(
             (IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Sign);
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Re-sign Action Tests

    [Test]
    public void ReSign_EmptySignature_ShouldReSign()
    {
        // Arrange: Component with Empty or Corrupted signature
        // Result: 🔒 ✅ ℹ️ (Re)sign component, output valid signature, info logged
        var (corpus, job) = PrepareTest([VehicleValid, NoSig_Axle, NoSig_Json_Battery]);

        // Act
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.ReSign);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw, info should be logged
        var errors = corpus.HashErrors;
        errors.Count.Should().Be(1);
        errors[0].path.Should().Be("Components.Axlegear");
        errors[0].result.Should().Be(SignatureValidation.Empty);

        // Verify component signatures - signature will be re-signed during serialization
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, _) in components)
        {
            var result = XmlUtils.ValidateComponentHashes(component);
            result.Should().Be(SignatureValidation.Valid);
        }
    }

    [Test]
    public void ReSign_MismatchSignature_ShouldReSign()
    {
        // Arrange: Component with Mismatch signature
        // Result: 🔒 ✅ ⚠️ (Re)sign component, output valid signature, warning logged
        var (corpus, job) = PrepareTest([VehicleValid, MismatchSig_EMS, NoSig_Json_Battery]);

        // Act
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.ReSign);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw, info should be logged
        var errors = corpus.HashErrors;
        errors.Count.Should().Be(1);
        errors[0].path.Should().Be("Components.ElectricMachine.ElectricMachineSystem");
        errors[0].result.Should().Be(SignatureValidation.Mismatch);

        // Verify component signatures - invalid signature is kept for now, will be re-signed during serialization
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, _) in components)
        {
            var result = XmlUtils.ValidateComponentHashes(component);
            result.Should().Be(SignatureValidation.Valid);
        }
    }

    [Test]
    public void ReSign_ValidSignature_ShouldReSign()
    {
        // Arrange: Component with Valid signature
        // Result: ▪ ✅ Output valid signature
        var (corpus, job) = PrepareTest([VehicleValid, ValidSig_Gearbox, NoSig_Json_Battery]);

        // Act
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.ReSign);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw
        // Note: Even valid signatures will be re-signed - no errors logged for valid sigs
        var errors = corpus.HashErrors;
        errors.Should().BeEmpty();

        // Verify all component signatures are still valid at this point
        // (they will be re-signed during serialization)
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, _) in components)
        {
            var result = XmlUtils.ValidateComponentHashes(component);
            result.Should().Be(SignatureValidation.Valid);
        }
    }

    [Test]
    public void ReSign_ValidSignature_Modification_Test()
    {
        // Arrange: Valid component that has been modified
        // Result: ▪ ✅ Output valid signature
        var (corpus, job) = PrepareTest([VehicleValid, GearboxModifyJson]);

        // Act & Assert
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.ReSign);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw
        // Note: Even valid signatures will be re-signed - no errors logged for valid sigs
        var errors = corpus.HashErrors;
        errors.Should().BeEmpty();

        // Verify all component signatures are still valid at this point
        // (they will be re-signed during serialization)
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, _) in components)
        {
            var result = XmlUtils.ValidateComponentHashes(component);
            result.Should().Be(SignatureValidation.Valid);
        }
    }

    [Test]
    public void ReSign_MismatchSignature_Modification_Test()
    {
        // Arrange: Invalid component that has been modified
        // Result: ▪ ✅ Output valid signature
        var (corpus, job) = PrepareTest([VehicleGearboxMismatch, GearboxModifyJson]);

        // Act & Assert
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.ReSign);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw
        // Note: Even valid signatures will be re-signed - no errors logged for valid sigs
        var errors = corpus.HashErrors;
        errors.Should().BeEmpty();

        // Verify all component signatures are still valid at this point
        // (they will be re-signed during serialization)
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, _) in components)
        {
            var result = XmlUtils.ValidateComponentHashes(component);
            result.Should().Be(SignatureValidation.Valid);
        }
    }

    #endregion

    #region Clear Action Tests

    [Test]
    public void Clear_EmptySignature_ShouldClearSignature()
    {
        // Arrange: Component with Empty or Corrupted signature
        // Result: ▬ 🔲 ⚠️ Drop signature, output empty signature, warning logged
        var (corpus, job) = PrepareTest([VehicleValid, NoSig_Axle, NoSig_Json_Battery]);

        // Act
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Clear);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw, warning should be logged
        var errors = corpus.HashErrors;
        errors.Count.Should().Be(1);
        errors[0].path.Should().Be("Components.Axlegear");
        errors[0].result.Should().Be(SignatureValidation.Empty);

        // Verify signatures are cleared
        var components = TypeUtils.FindComponentDelarations((IVehicleDeclaration)job.Vehicle!, null).ToList();

        foreach (var (component, path) in components)
        {
            if (path == "Components.Axlegear")
            {
                component.Signature.IsEmptySignature().Should().BeTrue();
                continue;
            }

            component.Signature.IsEmptySignature().Should().BeTrue();
        }
    }

    [Test]
    public void Clear_MismatchSignature_ShouldClearSignature()
    {
        // Arrange: Component with Mismatch signature
        // Result: ▬ 🔲 ⚠️ Drop signature, output empty signature, warning logged
        var (corpus, job) = PrepareTest([VehicleValid, MismatchSig_EMS, NoSig_Json_Battery]);

        // Act
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Clear);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw, warning should be logged
        var errors = corpus.HashErrors;
        errors.Count.Should().Be(1);
        errors[0].path.Should().Be("Components.ElectricMachine.ElectricMachineSystem");
        errors[0].result.Should().Be(SignatureValidation.Mismatch);

        // Verify signatures are cleared
        var components = TypeUtils.FindComponentDelarations((IVehicleDeclaration)job.Vehicle!, null).ToList();

        foreach (var (component, path) in components)
        {
            if (path == "Components.ElectricMachine.ElectricMachineSystem")
            {
                component.Signature.IsEmptySignature().Should().BeTrue();
                continue;
            }

            component.Signature.IsEmptySignature().Should().BeTrue();
        }
    }

    [Test]
    public void Clear_ValidSignature_ShouldClearSignature()
    {
        // Arrange: Component with Valid signature
        // Result: ▬ 🔲 ⚠️ Drop signature, output empty signature, warning logged
        var (corpus, job) = PrepareTest([VehicleValid, ValidSig_Gearbox, NoSig_Json_Battery]);

        // Act
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Clear);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw, no errors for valid signatures
        var errors = corpus.HashErrors;
        errors.Should().BeEmpty();

        // Verify all other component signatures remain valid
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, _) in components)
        {
            component.Signature.IsEmptySignature().Should().BeTrue();
        }
    }

    [Test]
    public void Clear_ValidSignature_Modification_Test()
    {
        // Arrange: Valid component that has been modified
        // Result: ▪ ✅ Output valid signature
        var (corpus, job) = PrepareTest([VehicleValid, GearboxModifyJson]);

        // Act & Assert
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Clear);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw
        // Note: Even valid signatures will be re-signed - no errors logged for valid sigs
        var errors = corpus.HashErrors;
        errors.Should().BeEmpty();

        // Verify all other component signatures remain valid
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, _) in components)
        {
            component.Signature.IsEmptySignature().Should().BeTrue();
        }
    }

    [Test]
    public void Clear_MismatchSignature_Modification_Test()
    {
        // Arrange: Invalid component that has been modified
        // Result: ▪ ✅ Output valid signature
        var (corpus, job) = PrepareTest([VehicleGearboxMismatch, GearboxModifyJson]);

        // Act & Assert
        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!, BrokenSignatureAction.Clear);

        job = SaveAndReload(corpus, job);

        // Assert: Should not throw
        // Note: Even valid signatures will be re-signed - no errors logged for valid sigs
        var errors = corpus.HashErrors;
        errors.Should().BeEmpty();

        // Verify all other component signatures remain valid
        var components = TypeUtils.FindComponentDelarations(job.Vehicle, null).ToList();

        foreach (var (component, _) in components)
        {
            component.Signature.IsEmptySignature().Should().BeTrue();
        }
    }

    #endregion

    #region Helper Methods

    private static (CorpusProcessor corpus, VectoDeclarationJobType_v2_7 job) PrepareTest(
        string[] fileNames, [CallerMemberName] string memberName = "")
    {
        var provider = TestUtils.TestServiceProvider();
        var corpus = provider.GetRequiredService<CorpusProcessor>();

        var directory = Path.Combine(TestUtils.InputPath, "Signatures");

        // Load only the specified files
        foreach (var fileName in fileNames)
        {
            var filePath = Path.Combine(directory, fileName);
            corpus.AddCorpusFile(filePath);
        }

        corpus.Vehicle.Should().NotBeNull($"Vehicle should be loaded for test: {memberName}");

        var builder = provider.GetRequiredService<VehicleBuilder>();
        builder.TemplateObject = corpus.Vehicle;

        builder.PromptHandler = action =>
        {
            var path = action.Property;

            var corpusFound = corpus.FindFromArbitraryObjectsByPath(action);

            if (corpusFound.found)
                action.Answer = corpusFound.value;
            else
                corpusFound = corpus.FindComponentByPath(path);

            if (corpusFound.found)
                action.Answer = corpusFound.value;
            else
                action.Answer = action.TemplateValue;
        };
        builder.Run(null);

        var job = VectoXml.CreateVehicleJobDeclaration(
            builder.Object as IVehicleDeclaration) as VectoDeclarationJobType_v2_7;

        return (corpus, job)!;
    }

    private VectoDeclarationJobType_v2_7 SaveAndReload(CorpusProcessor corpus, VectoDeclarationJobType_v2_7 job,
        [CallerMemberName] string memberName = "")
    {
        var outputDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(SignatureValidation_Tests));
        var outputFile = Path.Combine(outputDir, memberName + ".xml");

        using (var stream = File.Create(outputFile))
        {
            corpus.Save(stream, job);
        }

        // Read back the output file and verify the modification
        using var outputStream = File.OpenRead(outputFile);

        VectoXml.TryGetSerializer(outputStream, out var serializer2, out _, out _);
        var newJob = (VectoDeclarationJobType_v2_7)serializer2.Deserialize(outputStream)!;
        return newJob;
    }

    #endregion
}
