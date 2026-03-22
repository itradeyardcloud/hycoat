# HyCoat ERP — Build Execution Sequence

This document defines the exact order in which requirement files should be executed (assigned to an agent). Each step references a requirement file in `hycoat-knowledge/requirements/`.

---

## How to Use

1. Pick the next step from the sequence below
2. Assign the corresponding requirement MD file to an agent
3. Wait for completion, verify acceptance criteria
4. Move to the next step

> Steps marked with `||` can run **in parallel** if you have multiple agents. Otherwise, execute top to bottom.

---

## Phase 0 — Foundation

API track (A) and UI track (B) can run in parallel.

| Step | File | Description | Depends On |
|------|------|-------------|------------|
| 1a | `00-foundation/00-api-restructuring.md` | Rename namespace to HycoatApi, NuGet packages, folder structure, BaseEntity, middleware | — |
| 1b | `00-foundation/03-ui-restructuring.md` | npm deps, remove boilerplate, MUI theme, axios interceptor, Zustand stores, utils | — `\|\|` 1a |
| 2a | `00-foundation/01-database-schema.md` | All ~40 entity models, AppDbContext, EF configurations, initial migration | 1a |
| 2b | `00-foundation/04-pwa-setup.md` | vite-plugin-pwa, manifest.json, workbox caching, offline fallback, service worker | 1b `\|\|` 2a |
| 3a | `00-foundation/02-auth-system.md` | ASP.NET Identity, JWT + refresh tokens, auth endpoints, role seeding, permissions | 2a |
| 3b | `00-foundation/05-app-shell-layout.md` | DashboardLayout, AuthLayout, Sidebar, BottomNav, route guards, complete route tree, shared components | 2b `\|\|` 3a |

**Checkpoint:** API runs with auth endpoints working. UI shows login page and empty shell with sidebar navigation.

---

## Phase 1 — Masters

| Step | File | Description | Depends On |
|------|------|-------------|------------|
| 4a | `01-masters/00-master-data-api.md` | CRUD for Customer, SectionProfile, PowderColor, Vendor, ProcessType, ProductionUnit | 3a |
| 4b | `01-masters/01-master-data-ui.md` | List pages, forms, search, pagination, mobile card views for all 6 master entities | 3b + 4a |

**Checkpoint:** Can create customers, section profiles, powder colors, vendors, process types via UI.

---

## Phase 2 — Sales (Sequential)

| Step | File | Description | Depends On |
|------|------|-------------|------------|
| 5 | `02-sales/00-inquiry.md` | Inquiry CRUD, status flow (New→Quoted→Won→Lost), stat cards, auto-numbering INQ-YYYY-NNN | 4a + 4b |
| 6 | `02-sales/01-quotation.md` | Quotation with line items, QuestPDF generation, ProcessType rate auto-fill | 5 |
| 7 | `02-sales/02-proforma-invoice.md` | PI with area calculation (SFT formula), GST, PDF with annexure, live total calculation | 6 |
| 8 | `02-sales/03-work-order.md` | Work Order as central hub entity, timeline endpoint, linked documents, status lifecycle | 7 |

**Checkpoint:** Full sales cycle works: Inquiry → Quotation → PI → Work Order. PDFs generate correctly.

---

## Phase 3 — Material Inward (Sequential)

| Step | File | Description | Depends On |
|------|------|-------------|------------|
| 9 | `03-material-inward/00-material-inward.md` | Inward register, photo upload, discrepancy tracking, WO status update | 8 |
| 10 | `03-material-inward/01-incoming-inspection.md` | QA checklist per line (watermark/scratch/dent/dimensional), buffing, overall status | 9 |

**Checkpoint:** Material receipt logged with photos. QA incoming inspection records pass/fail per line.

---

## Phase 4 — PPC (Sequential)

| Step | File | Description | Depends On |
|------|------|-------------|------------|
| 11 | `04-ppc/00-production-work-order.md` | PWO with production time calc engine, surface area calc, shift allocation, basket plan | 10 |
| 12 | `04-ppc/01-production-schedule.md` | Week calendar/planner view, drag-and-drop scheduling, shift allocation, mobile list | 11 |

**Checkpoint:** PWOs created with time estimates. Weekly schedule visible in calendar view.

---

## Phase 5 — Production (Parallel OK)

| Step | File | Description | Depends On |
|------|------|-------------|------------|
| 13a | `05-production/00-pretreatment-logging.md` | Basket-level logging, 10-tank template, QA titration readings, tablet-optimized form | 12 |
| 13b | `05-production/01-coating-production-logging.md` | Shift log, conveyor speed, oven temp, powder batch, hourly photo upload | 12 `\|\|` 13a |

**Checkpoint:** Production operators can log pretreatment baskets and coating parameters from tablet.

---

## Phase 6 — Quality (Sequential)

| Step | File | Description | Depends On |
|------|------|-------------|------------|
| 14 | `06-quality/00-in-process-inspection.md` | DFT 10-point readings, adhesion/MEK/shade tests, panel tests (4hr), DFT trend chart | 13a + 13b |
| 15 | `06-quality/01-final-inspection-test-cert.md` | AQL sampling, final inspection form, Test Certificate PDF generation (QuestPDF) | 14 |

**Checkpoint:** QA inspectors record in-process readings. Final inspection triggers TC PDF generation.

---

## Phase 7 — Dispatch (Sequential)

| Step | File | Description | Depends On |
|------|------|-------------|------------|
| 16 | `07-dispatch/00-packing-delivery-challan.md` | Packing list, Delivery Challan PDF, loading photos, status flow | 15 |
| 17 | `07-dispatch/01-invoice-generation.md` | Invoice with annexure (area calc), GST, PDF, document bundle email to customer | 16 |

**Checkpoint:** Full dispatch flow: Packing → DC → Invoice. Customer receives email with all documents.

---

## Phase 8 — Purchase (Can start after Step 12)

| Step | File | Description | Depends On |
|------|------|-------------|------------|
| 18 | `08-purchase/00-powder-procurement.md` | Indent → PO → GRN flow, powder stock ledger, reorder alerts, PO PDF | 12 (parallel with Phases 5–7) |

**Checkpoint:** Powder procurement cycle works. Stock updates on GRN. Low-stock alerts visible.

---

## Phase 9 — Dashboards & Reports (Parallel OK)

| Step | File | Description | Depends On |
|------|------|-------------|------------|
| 19a | `09-dashboards-reports/00-dashboards.md` | Admin + 7 department dashboards, KPI cards, recharts, period selector, auto-refresh | 17 + 18 |
| 19b | `09-dashboards-reports/01-reports.md` | Order tracker, production throughput, quality summary, dispatch register, Excel export | 17 + 18 `\|\|` 19a |

**Checkpoint:** Dashboards show live KPIs. Reports exportable to Excel.

---

## Phase 10 — Notifications & Audit (Parallel OK)

| Step | File | Description | Depends On |
|------|------|-------------|------------|
| 20a | `10-notifications/00-notification-system.md` | SignalR real-time, in-app bell, PWA push, notification triggers across all modules | 19a + 19b |
| 20b | `10-notifications/01-audit-trail.md` | EF Core SaveChanges interceptor, audit log viewer, file management service | 3a (can start earlier) `\|\|` 20a |

**Checkpoint:** Real-time notifications working. All changes auto-logged in audit trail.

---

## Single-Agent Sequential Order (Copy-Paste Ready)

If executing one at a time, follow this exact order:

```
 1. 00-foundation/00-api-restructuring.md
 2. 00-foundation/01-database-schema.md
 3. 00-foundation/02-auth-system.md
 4. 00-foundation/03-ui-restructuring.md
 5. 00-foundation/04-pwa-setup.md
 6. 00-foundation/05-app-shell-layout.md
 7. 01-masters/00-master-data-api.md
 8. 01-masters/01-master-data-ui.md
 9. 02-sales/00-inquiry.md
10. 02-sales/01-quotation.md
11. 02-sales/02-proforma-invoice.md
12. 02-sales/03-work-order.md
13. 03-material-inward/00-material-inward.md
14. 03-material-inward/01-incoming-inspection.md
15. 04-ppc/00-production-work-order.md
16. 04-ppc/01-production-schedule.md
17. 05-production/00-pretreatment-logging.md
18. 05-production/01-coating-production-logging.md
19. 06-quality/00-in-process-inspection.md
20. 06-quality/01-final-inspection-test-cert.md
21. 07-dispatch/00-packing-delivery-challan.md
22. 07-dispatch/01-invoice-generation.md
23. 08-purchase/00-powder-procurement.md
24. 09-dashboards-reports/00-dashboards.md
25. 09-dashboards-reports/01-reports.md
26. 10-notifications/00-notification-system.md
27. 10-notifications/01-audit-trail.md
```

---

## Parallelization Summary

For maximum speed with **2 agents**, run API + UI tracks in parallel during foundation:

```
Agent 1 (API):  1a → 2a → 3a → 4a → 5 → 6 → 7 → 8 → 9 → 10 → 11 → 12 → 18
Agent 2 (UI):   1b → 2b → 3b → 4b ──────────────────────────────────────────────
                                    (then joins Agent 1's sequence after step 4b)
```

After Phase 1, both agents work the same sequential chain (5–27) since each feature has both API + UI in one file.

---

## Quick Reference

| Phase | Files | Count |
|-------|-------|-------|
| 0 — Foundation | 00-api-restructuring, 01-database-schema, 02-auth-system, 03-ui-restructuring, 04-pwa-setup, 05-app-shell-layout | 6 |
| 1 — Masters | 00-master-data-api, 01-master-data-ui | 2 |
| 2 — Sales | 00-inquiry, 01-quotation, 02-proforma-invoice, 03-work-order | 4 |
| 3 — Material Inward | 00-material-inward, 01-incoming-inspection | 2 |
| 4 — PPC | 00-production-work-order, 01-production-schedule | 2 |
| 5 — Production | 00-pretreatment-logging, 01-coating-production-logging | 2 |
| 6 — Quality | 00-in-process-inspection, 01-final-inspection-test-cert | 2 |
| 7 — Dispatch | 00-packing-delivery-challan, 01-invoice-generation | 2 |
| 8 — Purchase | 00-powder-procurement | 1 |
| 9 — Dashboards | 00-dashboards, 01-reports | 2 |
| 10 — Notifications | 00-notification-system, 01-audit-trail | 2 |
| **Total** | | **27** |
