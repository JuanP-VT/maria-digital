---
name: add-wizard-step
description: Add a document step to the capture wizard (doc-wizard). Use when a new document type must be captured.
---

# Add a document step to the wizard

The wizard is driven by data: components read `WIZARD_STEPS` and the draft's `slots`, so a new document is mostly config.

1. Append an entry to `WIZARD_STEPS` in `frontend/src/components/doc-wizard/config.ts` (order of the array is the order of the steps): `id`, Spanish `label`, Spanish `hintText` (match the gendered wording of the existing hints), and `required`.
2. Check how drafts are persisted (`useStudentDraft.ts`, `src/lib/idb.ts`). A draft saved before the change has fewer slots than steps; make sure loading an old draft does not crash (add or fix the migration logic and cover it with a test).
3. Check anything that counts or indexes steps: `progress.ts`, `StepHeader.tsx`, `PreviewScreen.tsx`, and tests with hard-coded step counts.
4. Add or update tests: draft loading with old data, progress counts, and the preview screen showing the new document as missing/skipped/captured.
5. Run, from `frontend/`: `npm run lint`, `npm test`, `npm run build`.
6. Follow `frontend/UI_GUIDELINES.md` for any UI change; do not touch `src/components/ui/**`.
