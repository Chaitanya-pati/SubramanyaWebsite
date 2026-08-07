---
name: Protected Replit config
description: Platform-specific handling for updating imported Replit configuration.
---

When `.replit` needs to change, write the complete replacement to a temporary file
inside the workspace and use the platform’s validation-and-replacement callback
instead of editing `.replit` directly.

**Why:** Direct file edits to `.replit` are blocked so runtime and workflow settings
can be schema-validated before replacement.

**How to apply:** Preserve the existing TOML settings, change only the required
modules or configuration, validate the full temporary file, then configure the
managed workflow separately.