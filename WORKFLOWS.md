# Hycoat ERP — Business Workflow Plan

## Business Context
Hycoat is an **aluminum powder coating job-work** company.
Customers (e.g. extruders, fabricators) send raw aluminum sections/profiles. Hycoat applies powder coating and returns the finished material. Revenue is charged per square foot of coated area.

---

## Departments & Roles

| Department | Abbreviation | Core Responsibility |
|---|---|---|
| Sales & Business Development | Sales | Inquiry → Quotation → PI → Work Order |
| Production Planning, Controlling & Coordination | PPC | Production scheduling, work orders, capacity planning |
| Supply Chain Management / Stores | SCM | Material inward, storage, dispatch |
| Production | Prod | Pretreatment, powder coating, log sheets |
| Quality Lab | QA | Incoming, in-process, final inspection, test certificates |
| Purchase & Stores | Purchase | Powder and consumable procurement |
| HR & Admin | HR | Human resources, administration |
| Accounts & Finance | Finance | Invoicing, billing, financial records |

---

## Workflow 1 — Sales & Order Acquisition

**Owner:** Sales Department
**Trigger:** Customer inquiry (email / phone / WhatsApp)

```
CUSTOMER                          SALES TEAM
   |                                   |
   |--- Inquiry (general rate?) ------>|
   |                                   |-- Prepare Quotation
   |                                   |   (generic process rates)
   |<-- Quotation ----------------------|
   |                                   |
   |--- Price Confirmation ----------->|
   |--- BOM + Drawings ---------------->|
   |    (section no, type, length,     |
   |     quantity, coating/masking     |
   |     areas)                        |
   |                                   |-- Request drawings if missing
   |                                   |-- Calculate perimeter/area
   |                                   |-- Prepare Proforma Invoice (PI)
   |<-- PI ------------------------------|
   |                                   |
   |--- PI Approval (signed PI,        |
   |    email, or Work Order) -------->|
   |                                   |-- Order Powder from vendor
   |                                   |   (concurrent with WO receipt)
   |                                   |-- Hand off WO to Operations
```

### Steps
1. Receive inquiry — log customer name, contact, project name, inquiry date
2. Send **Quotation** — generic rates per process (powder coating, anodizing, etc.)
3. Receive **BOM** (Bill of Material) — section number, type, length, quantity
4. Receive **Drawings** — extrusion profile dimensions, perimeter, masking/coating area marking
5. Calculate area (SFT) from perimeter × length × quantity
6. Prepare and send **Proforma Invoice (PI)** — itemised by section, rate/SFT, total amount, packing, transport
7. Receive PI confirmation — signed PI scan, email, or formal **Work Order**
8. Raise **Powder Purchase Order** to vendor (concurrent)
9. Create internal **Job/Work Order** — pass to PPC & SCM

### Key Data Fields — Quotation
- Quotation number, date
- Customer name & contact
- Process type (powder coating / anodizing / wood finish)
- Rate per SFT
- Packing charges
- Transport charges
- Validity period

### Key Data Fields — BOM / Material Details
- Serial no
- Section number (profile code)
- Type of section
- Length (meters)
- Quantity (nos)
- Color / powder code
- Coating area (SFT) — calculated from drawing perimeter

### Key Data Fields — PI
- PI number, date
- Customer name, address, GSTIN
- Reference: BOM, quotation no
- Line items: section no, length, qty, area (SFT), rate/SFT, amount
- Packing charges, transport charges
- Grand total, GST breakup
- Bank details, terms & conditions

---

## Workflow 2 — Material Inward (SCM)

**Owner:** SCM / Stores
**Trigger:** Customer dispatches aluminum sections to factory

```
CUSTOMER VEHICLE ARRIVES
         |
         v
SCM: Photograph vehicle + bundles
         |
         v
Unload material
         |
         v
Fill Inward Register (manual → digital)
  - Customer name, DC number
  - Section no, length, qty (as per customer doc)
  - Qty physically received
  - Discrepancy: excess / shortage
         |
         v
QA: Incoming Material Inspection
  - Open 2–3 bundles
  - Check for watermark, scratch, dent
  - Record observations
  - Inform customer of defects (charge buffing if applicable)
         |
         v
Stick identification label on each bundle/lot
  - Customer name, DC no, quantity
         |
         v
Store material in designated area
  - PPC notified that material is ready
```

### Key Data Fields — Inward Register
- Inward number, date, time
- Customer name, DC number, vehicle number
- Section number, type, length
- Quantity as per customer document
- Quantity physically received
- Discrepancy (excess +ve / shortage -ve)
- Remarks (damage, watermark, etc.)
- Received by (SCM staff name)
- QA verified by

### Key Data Fields — Incoming Inspection Report
- Inspection number, date
- Inward reference number
- Customer name, section number
- Parameters: watermark (Y/N), scratch (Y/N), dent (Y/N), dimensional check
- Status: PASS / FAIL / CONDITIONAL
- Buffing required (Y/N), buffing charge if applicable
- Inspector name, date

---

## Workflow 3 — Production Planning (PPC)

**Owner:** PPC Department
**Trigger:** Material inward complete & confirmed; Work Order received

```
Receive Work Order + Inward confirmation
         |
         v
Retrieve drawings — calculate perimeter
  - Coating area per section (SFT)
  - Total SFT for order
         |
         v
Check powder stock (coordinate with Purchase/Stores)
  |                   |
  | Stock OK          | Stock low → Indent to Purchase
  v                   v
Proceed             Powder PO raised to vendor
         |
         v
Production Time Calculation
  - Section length
  - Basket capacity (profiles per basket)
  - Conveyor speed per section thickness
  - Pretreatment time per basket
  - Total pre-treatment hours
  - Total post-treatment (coating) hours
         |
         v
Weekly Production Schedule
  - Monday–Sunday allocation
  - Day shift / Night shift
  - Customer allocation per slot
         |
         v
Create Production Work Order (PWO)
         |
     ____|____________________________
    |           |                    |
    v           v                    v
Production    Quality              Stores/Purchase
  Team         Team                  Team
(execute)   (inspection plan)    (issue powder)
```

### Key Data Fields — Production Work Order (PWO)
- PWO number, date
- Customer name, Work Order reference
- Section number(s), length, quantity
- Powder code, color name
- Pretreatment parameters: etch time, temperature, chemical concentration guidance
- Post-treatment parameters: conveyor speed (m/min), oven temperature (°C), gun settings
- Basket plan: profiles per basket, number of baskets
- Estimated hours: pretreatment, coating
- Shift allocation (day/night), start date
- Issued by (PPC), signatures

---

## Workflow 4 — Powder Procurement (Purchase)

**Owner:** Purchase Department
**Trigger:** PPC indent or Work Order confirmation

```
Receive indent from PPC
         |
         v
Check warehouse stock
  |              |
  | Available    | Not available
  v              v
Issue to       Raise Purchase Order to powder vendor
Production       - Powder code, color, quantity (kg)
                 - Required delivery date
                 |
                 v
               Vendor delivers powder
                 |
                 v
               Goods Received Note (GRN)
               Store in warehouse
               Update stock ledger
```

### Key Data Fields — Purchase Order (Powder)
- PO number, date
- Vendor name, address, GSTIN
- Powder code, color name (RAL / custom)
- Quantity (kg)
- Rate per kg
- Required delivery date
- Our Work Order reference

---

## Workflow 5 — Pretreatment Production

**Owner:** Production (Pretreatment Operator) + QA Lab
**Trigger:** PWO issued; material and powder ready

```
QA Lab: Morning titration
  - Measure chemical concentration in each tank
  - Derive etch rate, chrome weight
  - Issue time instructions to operator
  - e.g. "Basket 1: 20 min, increase by 1 min each basket"
         |
         v
Operator loads basket with aluminum sections
         |
         v
Tank sequence:
  1. Degreasing
  2. Water rinse
  3. Etching (time per QA instruction)
  4. Water rinse
  5. Deoxidizing / De-smutting
  6. Water rinse
  7. Chrome conversion coating
  8. Water rinse
  9. DI water rinse
  10. Dry / oven dry
         |
         v
Operator records each basket in Pretreatment Log Sheet
  - Basket no, start/end time, etch time used
         |
         v
QA Lab: Evening re-titration
  - Verify etch rate still within spec
  - Add chemicals if required
```

### Key Data Fields — Pretreatment Log Sheet
- Date, shift
- PWO number, customer name
- Basket number
- Tank: concentration (point), time (minutes)
- Etch rate (mg/dm²)
- Chrome weight (mg/m²)
- Temperature readings
- Chemical additions made
- Operator name, QA sign-off

---

## Workflow 6 — Powder Coating Production

**Owner:** Production (Coating Supervisor + Operator)
**Trigger:** Pretreatment complete

```
Unload pretreat basket → conveyor loading area
         |
         v
Air spray sections (remove dust)
         |
         v
Hang sections on conveyor hooks
         |
         v
Set conveyor speed (per PWO) and oven temp (225°C std)
         |
         v
Powder booth — electrostatic spray
  - Gun voltage, current per PWO
         |
         v
Conveyor through oven
  - Curing: ~200–230°C, 10–20 min dwell
         |
         v
Cool down zone
         |
         v
Unloading
         |
         v
Supervisor: Line Inspection (immediately)
  - DFT (micron) check
  - Gloss meter reading
  - Shade match vs customer sample
  - MEK rub test (30 rubs)
         |
         v
Record in Production Log Sheet
Upload photos to app
```

### Key Data Fields — Production Log Sheet
- Date, shift, PWO number
- Customer name, section number
- Conveyor speed setting (m/min)
- Oven temperature (°C)
- Powder batch/lot number, powder code
- Supervisor name
- Hourly photo record (timestamp + photo)

---

## Workflow 7 — Quality Inspection

**Owner:** QA Department
Three stages: Incoming → In-Process → Final

### 7A. In-Process Inspection

```
Every 15–30 minutes during production:
  - DFT (micron) check on 5 points → record + photo upload
  - Gloss level measurement

Every 2 hours:
  - Adhesion test: cross-hatch (11×11) + 3M tape peel
  - MEK rub test (30 rubs)
  - Shade match

Every 4 hours (Panel Testing):
  - Panels (alloy 5005/5030) put through same pretreat + coating
  - Boiling water test: 100°C × 1 hour → adhesion check
  - Impact test: weighted ball drop (35 kg·cm)
  - Conical mandrel bend test
  - Pencil hardness test (scratch resistance)
  - Record all 9 tests on panel record
```

### 7B. Final Inspection

```
After production complete → batch sampling (AQL)
  - Visual inspection: uniformity, runs, sags, bare spots
  - DFT re-check
  - Shade match final confirmation
         |
         v
PASS → issue Test Certificate
FAIL → rework or scrap record
```

### Key Data Fields — In-Process Inspection Record
- Date, time, PWO number
- Customer name, section number
- DFT readings × 5 (min, max, avg) — every 30 min
- Gloss reading
- Adhesion result (PASS/FAIL) — every 2 hours
- MEK rub result (PASS/FAIL)
- Shade match (PASS/FAIL)
- Panel test results (if applicable)
- Inspector name, photos

### Key Data Fields — Test Certificate
- Certificate number, date
- Customer name, DC number (inward reference)
- Project name
- Section number, quantity, area (SFT)
- Powder code, color name, vendor
- Test results table: DFT, gloss, adhesion, MEK, impact, bend, pencil hardness, boiling water
- Pass/Fail per test
- Overall status: APPROVED / REJECTED
- QA Manager signature, stamp

---

## Workflow 8 — Dispatch

**Owner:** SCM / Dispatch team
**Trigger:** Final inspection PASSED; dispatch confirmed with customer

```
Final inspection approved
         |
         v
Packing:
  - Tape each section (taping machine)
  - Bundle and tie
  - Bundle count and labeling
         |
         v
Prepare dispatch documents:
  1. Delivery Challan (DC)
  2. Packing List
  3. Invoice (with area calculation)
  4. Annexure (perimeter × length × qty = SFT per line)
  5. Test Certificate
  6. E-way Bill (GST portal)
         |
         v
Physical copies: 3 × invoice set
  - 1 copy with driver (vehicle)
  - 1 copy for customer records
  - 1 copy retained by Hycoat
         |
         v
Email to customer:
  - All 6 documents in single email
         |
         v
Vehicle loading photo (app upload)
         |
         v
Update dispatch register (date, vehicle, DC no, qty)
```

### Key Data Fields — Delivery Challan
- DC number, date
- Customer name, address, GSTIN
- Our GSTIN, address
- Inward reference (customer's own DC number)
- Section number, length, quantity dispatched
- Bundle count
- Vehicle number, driver name, LR number
- Remarks

### Key Data Fields — Invoice
- Invoice number, date
- Customer name, address, GSTIN
- Our GSTIN, HSN/SAC code
- Section-wise: section no, qty, area (SFT), rate/SFT, amount
- Annexure: perimeter calculation breakdown
- Packing charges, transport charges
- Subtotal, GST (CGST + SGST or IGST), grand total
- Payment terms

---

## Workflow 9 — Customer Communication Events

Automated / semi-automated notifications at key milestones:

| Event | Notification to Customer |
|---|---|
| Material received at factory | Inward confirmation SMS/email with quantity summary |
| Discrepancy found | Alert email with exact quantities (shortage/excess) |
| Production started | Optional status update |
| Quality approved | Test certificate email (internal) |
| Dispatched | Email with all 6 documents attached |
| PI sent | Email with PI PDF |
| Work Order confirmation pending | Reminder after X days |

---

## App Module Breakdown

Based on the workflows above, the app should be organized into these modules:

### Module 1 — Masters
- Customer master (name, address, GSTIN, contact)
- Section / Profile master (section no, type, perimeter, weight/meter)
- Powder / Color master (powder code, color name, RAL, vendor)
- Vendor master
- Process master (process types, rates)
- User master (role-based: Sales, PPC, SCM, QA, Production, Purchase, Finance)

### Module 2 — Sales
- Inquiry log
- Quotation management
- Drawing Upload & perimeter calculator
- Proforma Invoice (PI) generation
- PI approval tracking
- Work Order registration

### Module 3 — Material Inward (SCM)
- Inward register (material receipt)
- Incoming inspection record (QA)
- Excess/shortage management
- Material identification & labeling
- Stock location tracking

### Module 4 — PPC
- Production time calculator (Excel logic → app)
- Weekly production schedule (drag-and-drop planner)
- Production Work Order (PWO) creation
- PWO distribution to Production, QA, Purchase

### Module 5 — Purchase
- Powder indent from PPC
- Purchase Order generation
- Goods Received Note (GRN)
- Powder stock ledger

### Module 6 — Production
- Pretreatment log sheet (tablet/mobile entry by operator)
- Production log sheet
- Photo upload every hour (timestamped)
- Process parameter checklist

### Module 7 — Quality
- Incoming inspection form
- In-process inspection record (with photo upload)
- Panel test record (every 4 hours)
- Final inspection form
- Test certificate generation (PDF)

### Module 8 — Dispatch
- Packing list creation
- Delivery Challan generation
- Invoice + Annexure generation (auto SFT calculation)
- E-way bill (reference / manual entry)
- Dispatch register
- Document bundling → single email send

### Module 9 — Reports & Dashboard
- Order-wise status tracker (inquiry → dispatched)
- Daily/weekly production throughput (SFT/kg)
- Pending work orders
- Powder consumption vs. order
- Quality failure rate
- Customer-wise history

---

## Data Flow Summary

```
Customer
  └──[Inquiry]──> Sales ──[PI]──> Customer ──[Work Order]──> Sales
                                                                |
                                                          [Job Order]
                              ┌─────────────────────────────────┤
                              v                                 v
                      SCM (Inward)                     PPC (Planning)
                              |                                 |
                         QA Incoming                    [PWO issued]
                         Inspection                      |    |    |
                              |                    Prod  QA  Purchase
                              └──[Material ready]──>|         |
                                                   Pretreat   |
                                                   Coating    |
                                                      |       |
                                                  QA In-Process
                                                  QA Final
                                                      |
                                               Test Certificate
                                                      |
                                             SCM Dispatch
                                                      |
                                               Docs → Customer
                                               Finance → Invoice
```

---

## Priority Build Order

1. **Masters** — customer, section, powder, vendor
2. **Sales** — inquiry, quotation, PI, work order
3. **SCM Inward** — material receipt, incoming inspection
4. **PPC** — production work order
5. **Production** — log sheets, photo upload
6. **Quality** — in-process, final, test certificate
7. **Dispatch** — DC, invoice, email bundle
8. **Purchase** — powder PO, GRN
9. **Dashboard & Reports**
