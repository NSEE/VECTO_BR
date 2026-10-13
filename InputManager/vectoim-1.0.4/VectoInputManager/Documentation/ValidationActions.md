# Signature Validation Actions

This document describes the signature validation behavior for VECTO component declarations. When processing vehicle components, different actions can be taken based on the signature state and the chosen validation strategy.

## Overview

Component signatures ensure the integrity and authenticity of vehicle component data. During processing, components can have four signature states:

- **Empty**: No signature present
- **Corrupted**: Signature exists but is malformed (e.g., missing or mismatched id)
- **Mismatch**: Signature exists but doesn't match the component data (hash mismatch)
- **Valid**: Signature exists and correctly validates the component data

## Validation Actions

Six different actions are available to handle components based on their signature state:

### Auto (default)

**Use Case**: Balanced approach for development and integration workflows
**User Profile**: General users who want sensible defaults

Automatically handles signatures based on their state: keeps empty signatures as-is (allowing development flexibility), blocks mismatched signatures (protecting integrity), and preserves valid signatures. This provides a good balance between safety and flexibility.

### Sign

**Use Case**: Initial component creation and certification
**User Profile**: OEMs/TAAs creating and testing components for certification

Signs components that lack signatures, but blocks components with mismatched signatures. Also blocks modification of components with valid signatures.

### Block

**Use Case**: Quality assurance for production/certification workflows
**User Profile**: TAAs integrating existing certified components into vehicles

Strictest validation mode - rejects any component with signature issues. Also blocks modification of components with valid signatures.

### Keep

**Use Case**: Preserving existing component data during integration
**User Profile**: TAAs integrating existing components into vehicles

Maintains components as-is, including invalid signatures. When components are modified, the original signature is preserved (resulting in a mismatched signature).

### Re-sign

**Use Case**: Updating or fixing component signatures
**User Profile**: Engineers experimenting freely (no certification required)

Forces recalculation of all signatures, regardless of current state. Allows modification of components with valid signatures.

### Clear

**Use Case**: Development and testing workflows
**User Profile**: Engineers experimenting freely (no certification required)

Clears all signatures, resulting in empty signatures for all components. Allows modification of components with valid signatures.

## Behavior Matrix

The following table shows the action taken for each combination of component signature state and validation action:

| Input Signature    | Auto (def.) | Sign        | Block | Keep    | Re-sign | Clear   |
| ------------------ | ----------- | ----------- | ----- | ------- | ------- | ------- |
| Empty or Corrupted | ▪ 🔲 ℹ️     | 🔒 ✅ ℹ️    | 🔴    | ▪ 🔲 ⚠️ | 🔒 ✅ ℹ️| ▬ 🔲 ⚠️ |
| Mismatch           | 🔴          | 🔴          | 🔴    | ▪ ✖️ ⚠️ | 🔒 ✅ ⚠️| ▬ 🔲 ⚠️ |
| Valid              | ▪ ✅        | ▪ ✅        | ▪ ✅  | ▪ ✅    | ▪ ✅    | ▬ 🔲 ⚠️ |

**Actions**:

- 🔴 = Block processing (throw exception)
- ▪ = Do nothing (keep component as-is)
- 🔒 = (Re)sign component (calculate new signature)
- ▬ = Empty signature (`DigestValue` is empty)

**Output Signature**:

- ✅ = Valid signature
- ✖️ = Signature doesn't match the component data (hash mismatch)
- 🔲 = Empty signature

**Logging**:

- ℹ️ = Info message
- ⚠️ = Warning message

## Command-Line Usage

When using the VECTO Input Manager command-line tool:

```bash
# Auto mode: keep empty signatures, block mismatched (default)
vectoim job input.xml -o output.xml

# Sign missing signatures, block on invalid
vectoim job -b sign input.xml -o output.xml

# Block on any signature issues
vectoim job -b block input.xml -o output.xml

# Keep all signatures as-is with warnings
vectoim job -b keep input.xml -o output.xml

# Force re-signing of all components
vectoim job -b re-sign input.xml -o output.xml

# Clear all signatures
vectoim job -b clear input.xml -o output.xml
```

## Best Practices

### For General Users

**Auto (default)**: Balanced approach for most workflows

1. **Default Workflow**: Use `auto` action
   - Keeps empty signatures for components in development
   - Protects against mismatched signatures
   - Preserves valid signatures

### For TAAs (Type Approval Authorities)

**Block/Keep**: Used by TAAs integrating existing components into vehicles

1. **Integrating Certified Components**: Use `block` or `keep` action
   - `Block`: Ensures only valid components are accepted
   - `Keep`: Allows documenting issues without blocking workflow

### For OEMs (Original Equipment Manufacturers)

**Sign**: Used by OEMs/TAAs creating (& then testing) components to certify

1. **Component Development**: Use `sign` action

   - Signs components as they're created
   - Protects against modifying already-validated components

2. **Testing/Validation**: Use `block` action

   - Ensures only properly signed components enter certification

### For Engineers/Developers

**Re-sign/Clear**: Used by engineers experimenting freely (no certification)

1. **Development/Testing**: Use `clear` or `re-sign` action

   - `Clear`: Remove signatures for easier data comparison
   - `Re-sign`: Update signatures after data modifications

2. **Debugging**: Use `keep` action

   - Preserves all original signatures
   - Logs issues without blocking processing

## Notes

1. If an XML vehicle job input file contains a component with a corrupted, empty or mismatched signature, and this component is replaced with an input XML component file that has a valid signature, then no error or warning is raised from any validation action.

2. Input files may also be JSON component files. In this case, `sign`, `block`, `keep` and `re-sign` will all sign the component. `clear` will output an empty signature.

3. If components of the output vehicle job are further modified via the `--modify` option, or via arbitrary JSON files, or via the interactive mode, then `sign` and `block` will block the modification if the original component had a valid signature. `keep` will preserve the original signature (which becomes mismatched). `re-sign` and `clear` allow modifications and will update/clear signatures accordingly.

4. The `clear` action will clear all signatures, even from components with a correct signature.

5. For JSON input vehicle job files, all actions except `clear` will sign all components found in the input vehicle. If however input XML component files are used to replace components in the vehicle job, then the behavior is as described above for XML files.

6. Similar to the above, JSON standard value input files will always result in signed components for all actions except `clear`, which will result in empty signatures.

7. The `keep` action preserves the original signature from the input file exactly as-is in the output, even if it's invalid.

8. Component signatures without an `id` attribute in the `Data` field, or if the `id` attribute is different from the URI referenced in the signature, the validation will return `Corrupted`.

9. Scaffolding components (components with only structural data) created in interactive mode are always assigned an empty signature during serialization, regardless of the validation action.
