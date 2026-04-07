# VECTO_BR

VECTO repository cloned from:
https://code.europa.eu/

## Branching Strategy

- `master`: unmodified European (EU) base code
- `brazil`: Brazilian adaptations and features

## Development Workflow

- Always branch **from `brazil`**
- Create feature/fix branches:
  - `feature/...`
  - `fix/...`
- Merge changes into `brazil` via PR (even if self-reviewed)

## Synchronization with EU

- Changes from `master` should be **periodically merged into `brazil`**
- Resolve conflicts in favor of maintaining Brazilian requirements
- Avoid modifying `master` directly

## Design Principles

- Keep the EU base intact whenever possible
- Prefer **extending** behavior rather than modifying core logic
- Isolate Brazilian-specific logic into dedicated modules/components

## Regional Behavior

A global flag determines execution context. Avoid introducing conditionals everywhere, try to group changes.

```csharp
if (BRAZILIAN_VERSION) {
    opts = [N2];
} else {
    opts = [N1, N2, M3];
}
