# 00-foundation/01-database-schema

## Feature ID
`00-foundation/01-database-schema`

## Feature Name
Database Schema — All Entities, Relationships & EF Core Configuration

## Dependencies
- `00-foundation/00-api-restructuring` — namespaces, BaseEntity, folder structure must exist

## Business Context
HyCoat Systems tracks the entire lifecycle of an aluminum coating order — from customer inquiry through production, quality inspection, and dispatch. The database schema must model all entities across 9 business domains. All entities inherit from `BaseEntity` (audit fields + soft delete). Entity Framework Core Code-First with Fluent API configurations.

---

## Entity Definitions

### Common / Shared

#### BaseEntity (already created in 00-api-restructuring)
Inherited by all entities below.

#### FileAttachment
```
Models/Common/FileAttachment.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| FileName | string | Required, MaxLength(255) | Original file name |
| StoredPath | string | Required, MaxLength(500) | Server path or blob URL |
| ContentType | string | MaxLength(100) | MIME type |
| FileSizeBytes | long | | |
| EntityType | string | Required, MaxLength(100) | e.g., "MaterialInward", "ProductionLog" |
| EntityId | int | Required | FK to the parent entity |
| UploadedBy | string? | MaxLength(100) | User ID |
| UploadedAt | DateTime | | Default UTC now |

#### Notification
```
Models/Common/Notification.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| Title | string | Required, MaxLength(200) | |
| Message | string | Required, MaxLength(1000) | |
| Type | string | Required, MaxLength(50) | e.g., "Info", "Warning", "Action" |
| EntityType | string? | MaxLength(100) | Related entity type |
| EntityId | int? | | Related entity ID |
| RecipientUserId | string | Required | FK to User |
| IsRead | bool | | Default false |
| ReadAt | DateTime? | | |
| CreatedAt | DateTime | | |

#### AuditLog
```
Models/Common/AuditLog.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | long | PK | Use long for high-volume |
| EntityType | string | Required, MaxLength(100) | |
| EntityId | int | Required | |
| Action | string | Required, MaxLength(20) | "Create", "Update", "Delete" |
| OldValues | string? | | JSON |
| NewValues | string? | | JSON |
| UserId | string? | | |
| UserName | string? | MaxLength(100) | |
| Timestamp | DateTime | | UTC |

---

### Identity Entities

#### AppUser (extends IdentityUser)
```
Models/Identity/AppUser.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(inherited from IdentityUser)* | | | Id (string), Email, UserName, PhoneNumber, etc. |
| FullName | string | Required, MaxLength(200) | Display name |
| Department | string | Required, MaxLength(50) | Sales, PPC, SCM, Production, QA, Purchase, Finance |
| IsActive | bool | | Default true |
| RefreshToken | string? | MaxLength(500) | Current refresh token |
| RefreshTokenExpiryTime | DateTime? | | |

**Notes:**
- Uses ASP.NET Identity's `IdentityUser` as base → gives us Id (GUID string), Email, PasswordHash, PhoneNumber, etc.
- Role management through `IdentityRole` → roles: "Admin", "Leader", "User"
- Department is a string field (not a separate entity) — simpler for claims

#### AppRole (extends IdentityRole)
```
Models/Identity/AppRole.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(inherited from IdentityRole)* | | | Id (string), Name, NormalizedName |
| Description | string? | MaxLength(200) | |

---

### Master Entities

#### Customer
```
Models/Masters/Customer.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| Name | string | Required, MaxLength(300) | Company name |
| ShortName | string? | MaxLength(50) | Abbreviation for display |
| Address | string? | MaxLength(500) | Full address |
| City | string? | MaxLength(100) | |
| State | string? | MaxLength(100) | Important for GST (CGST/SGST vs IGST) |
| Pincode | string? | MaxLength(10) | |
| GSTIN | string? | MaxLength(15) | 15-char GST number |
| ContactPerson | string? | MaxLength(200) | |
| Phone | string? | MaxLength(20) | |
| Email | string? | MaxLength(200) | |
| Notes | string? | MaxLength(1000) | |

**Relationships:** Has many Inquiries, WorkOrders, MaterialInwards, Invoices

#### SectionProfile
```
Models/Masters/SectionProfile.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| SectionNumber | string | Required, MaxLength(50), Unique | Profile code, e.g., "M-1815", "15940" |
| Type | string? | MaxLength(100) | e.g., "Al Profile", "Al Channel" |
| PerimeterMM | decimal | Precision(10,2) | Perimeter in millimeters (from drawing) |
| WeightPerMeter | decimal? | Precision(10,4) | kg per running meter |
| HeightMM | decimal? | Precision(10,2) | Profile height |
| WidthMM | decimal? | Precision(10,2) | Profile width |
| ThicknessMM | decimal? | Precision(6,2) | Wall thickness |
| DrawingFileUrl | string? | MaxLength(500) | Path to uploaded drawing |
| Notes | string? | MaxLength(1000) | |

**Key Formula:** Coating Area (SFT) = PerimeterMM × LengthMM × Qty / 92903.04

#### PowderColor
```
Models/Masters/PowderColor.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| PowderCode | string | Required, MaxLength(50) | e.g., "YW17AN" |
| ColorName | string | Required, MaxLength(100) | e.g., "RAL 9007" |
| RALCode | string? | MaxLength(20) | RAL standard code |
| Make | string? | MaxLength(100) | e.g., "AKZO", "Asian Paints" |
| VendorId | int? | FK → Vendor | Primary vendor |
| WarrantyYears | int? | | 15 or 25 typically |
| Notes | string? | MaxLength(500) | |

**Relationships:** BelongsTo Vendor (optional)

#### Vendor
```
Models/Masters/Vendor.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| Name | string | Required, MaxLength(300) | |
| VendorType | string | Required, MaxLength(50) | "Powder", "Chemical", "Consumable", "Other" |
| Address | string? | MaxLength(500) | |
| City | string? | MaxLength(100) | |
| State | string? | MaxLength(100) | |
| GSTIN | string? | MaxLength(15) | |
| ContactPerson | string? | MaxLength(200) | |
| Phone | string? | MaxLength(20) | |
| Email | string? | MaxLength(200) | |

**Relationships:** Has many PowderColors, PurchaseOrders

#### ProcessType
```
Models/Masters/ProcessType.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| Name | string | Required, MaxLength(100), Unique | "Powder Coating", "Anodizing", "Wood Effect", "Chromotizing", "PVDF", "Mill Finish" |
| DefaultRatePerSFT | decimal? | Precision(10,2) | Default rate |
| Description | string? | MaxLength(500) | |

#### ProductionUnit
```
Models/Masters/ProductionUnit.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| Name | string | Required, MaxLength(50), Unique | e.g., "Unit-1", "Unit-2" |
| TankLengthMM | decimal? | Precision(10,2) | 7200 typical |
| TankWidthMM | decimal? | Precision(10,2) | 1200 typical |
| TankHeightMM | decimal? | Precision(10,2) | 1500 typical |
| BucketLengthMM | decimal? | Precision(10,2) | 7000 typical |
| BucketWidthMM | decimal? | Precision(10,2) | 1000 typical |
| BucketHeightMM | decimal? | Precision(10,2) | 1000 typical |
| ConveyorLengthMtrs | decimal? | Precision(10,2) | |
| IsActive | bool | | Default true |

---

### Sales Entities

#### Inquiry
```
Models/Sales/Inquiry.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| InquiryNumber | string | Required, MaxLength(30), Unique | Auto-generated: INQ-YYYY-NNN |
| Date | DateTime | Required | |
| CustomerId | int | FK → Customer | |
| ProjectName | string? | MaxLength(300) | |
| Source | string | Required, MaxLength(30) | "Email", "Phone", "WhatsApp", "Walk-in", "Tender" |
| ContactPerson | string? | MaxLength(200) | Customer-side contact |
| ContactEmail | string? | MaxLength(200) | |
| ContactPhone | string? | MaxLength(20) | |
| ProcessTypeId | int? | FK → ProcessType | |
| Description | string? | MaxLength(2000) | |
| Status | string | Required, MaxLength(30) | See status flow below |
| AssignedToUserId | string? | FK → AppUser | Sales person assigned |

**Status Flow:** `New` → `QuotationSent` → `BOMReceived` → `PISent` → `Confirmed` → `Closed` → `Lost`

**Relationships:** BelongsTo Customer, ProcessType, User. Has many Quotations.

#### Quotation
```
Models/Sales/Quotation.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| QuotationNumber | string | Required, MaxLength(30), Unique | Auto: QTN-YYYY-NNN |
| Date | DateTime | Required | |
| InquiryId | int? | FK → Inquiry | Can exist without inquiry |
| CustomerId | int | FK → Customer | |
| ValidityDays | int | | Default 30 |
| Status | string | Required, MaxLength(30) | "Draft", "Sent", "Accepted", "Rejected", "Expired" |
| Notes | string? | MaxLength(2000) | |
| FileUrl | string? | MaxLength(500) | Generated PDF path |
| PreparedByUserId | string? | FK → AppUser | |

**Relationships:** Has many QuotationLineItems

#### QuotationLineItem
```
Models/Sales/QuotationLineItem.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| QuotationId | int | FK → Quotation | |
| ProcessTypeId | int | FK → ProcessType | |
| Description | string? | MaxLength(500) | Additional notes |
| RatePerSFT | decimal | Precision(10,2) | |
| WarrantyYears | int? | | 15 or 25 |
| MicronRange | string? | MaxLength(30) | e.g., "60-80" |

#### ProformaInvoice
```
Models/Sales/ProformaInvoice.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| PINumber | string | Required, MaxLength(30), Unique | Auto: PI-YYYY-NNN |
| Date | DateTime | Required | |
| CustomerId | int | FK → Customer | |
| QuotationId | int? | FK → Quotation | |
| CustomerAddress | string? | MaxLength(500) | Snapshot for document |
| CustomerGSTIN | string? | MaxLength(15) | Snapshot |
| SubTotal | decimal | Precision(14,2) | Sum of line amounts |
| PackingCharges | decimal | Precision(10,2) | Default 0 |
| TransportCharges | decimal | Precision(10,2) | Default 0 |
| TaxableAmount | decimal | Precision(14,2) | SubTotal + Packing + Transport |
| CGSTRate | decimal | Precision(5,2) | 9% |
| CGSTAmount | decimal | Precision(12,2) | |
| SGSTRate | decimal | Precision(5,2) | 9% |
| SGSTAmount | decimal | Precision(12,2) | |
| IGSTRate | decimal | Precision(5,2) | 18% (for inter-state) |
| IGSTAmount | decimal | Precision(12,2) | |
| GrandTotal | decimal | Precision(14,2) | |
| IsInterState | bool | | Determines CGST+SGST vs IGST |
| Status | string | Required, MaxLength(30) | "Draft", "Sent", "Approved", "Rejected" |
| FileUrl | string? | MaxLength(500) | Generated PDF |
| Notes | string? | MaxLength(2000) | |
| PreparedByUserId | string? | FK → AppUser | |

**Relationships:** Has many PILineItems

#### PILineItem
```
Models/Sales/PILineItem.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| ProformaInvoiceId | int | FK → ProformaInvoice | |
| SectionProfileId | int | FK → SectionProfile | |
| SectionNumber | string | MaxLength(50) | Snapshot from profile |
| LengthMM | decimal | Precision(10,2) | In millimeters |
| Quantity | int | | Number of pieces |
| PerimeterMM | decimal | Precision(10,2) | From SectionProfile master |
| AreaSqMtr | decimal | Precision(12,4) | Calculated: Perimeter × Length × Qty |
| AreaSFT | decimal | Precision(12,2) | Calculated: AreaSqMtr converted |
| RatePerSFT | decimal | Precision(10,2) | |
| Amount | decimal | Precision(14,2) | AreaSFT × RatePerSFT |

**Formula:** `AreaSFT = (PerimeterMM × LengthMM × Quantity) / 92903.04`

#### WorkOrder
```
Models/Sales/WorkOrder.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| WONumber | string | Required, MaxLength(30), Unique | Auto: WO-YYYY-NNN |
| Date | DateTime | Required | |
| CustomerId | int | FK → Customer | |
| ProformaInvoiceId | int? | FK → ProformaInvoice | |
| ProjectName | string? | MaxLength(300) | |
| ProcessTypeId | int | FK → ProcessType | |
| PowderColorId | int? | FK → PowderColor | |
| DispatchDate | DateTime? | | Expected dispatch |
| Status | string | Required, MaxLength(30) | See status flow |
| Notes | string? | MaxLength(2000) | |

**Status Flow:** `Created` → `MaterialAwaited` → `MaterialReceived` → `InProduction` → `QAComplete` → `Dispatched` → `Invoiced` → `Closed`

**Relationships:** Has many MaterialInwards, ProductionWorkOrders, DeliveryChallans, Invoices

---

### Material Inward Entities

#### MaterialInward
```
Models/MaterialInward/MaterialInward.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| InwardNumber | string | Required, MaxLength(30), Unique | Auto: INW-YYYY-NNN |
| Date | DateTime | Required | |
| CustomerId | int | FK → Customer | |
| WorkOrderId | int? | FK → WorkOrder | |
| CustomerDCNumber | string? | MaxLength(100) | Customer's delivery challan no |
| CustomerDCDate | DateTime? | | |
| VehicleNumber | string? | MaxLength(20) | |
| UnloadingLocation | string? | MaxLength(100) | |
| ProcessTypeId | int? | FK → ProcessType | |
| PowderColorId | int? | FK → PowderColor | |
| ReceivedByUserId | string? | FK → AppUser | |
| Status | string | Required, MaxLength(30) | "Received", "InspectionPending", "Inspected", "Stored" |
| Notes | string? | MaxLength(2000) | |

**Relationships:** Has many MaterialInwardLines, IncomingInspections

#### MaterialInwardLine
```
Models/MaterialInward/MaterialInwardLine.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| MaterialInwardId | int | FK → MaterialInward | |
| SectionProfileId | int | FK → SectionProfile | |
| LengthMM | decimal | Precision(10,2) | |
| QtyAsPerDC | int | | Qty on customer's DC |
| QtyReceived | int | | Actual physical count |
| WeightKg | decimal? | Precision(12,2) | |
| Discrepancy | int | | Computed: QtyReceived - QtyAsPerDC |
| Remarks | string? | MaxLength(500) | |

#### IncomingInspection
```
Models/MaterialInward/IncomingInspection.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| InspectionNumber | string | Required, MaxLength(30), Unique | Auto: IIR-YYYY-NNN |
| Date | DateTime | Required | |
| MaterialInwardId | int | FK → MaterialInward | |
| InspectedByUserId | string? | FK → AppUser | |
| OverallStatus | string | Required, MaxLength(20) | "Pass", "Fail", "Conditional" |
| Remarks | string? | MaxLength(2000) | |

**Relationships:** Has many IncomingInspectionLines

#### IncomingInspectionLine
```
Models/MaterialInward/IncomingInspectionLine.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| IncomingInspectionId | int | FK → IncomingInspection | |
| MaterialInwardLineId | int | FK → MaterialInwardLine | |
| WatermarkOk | bool? | | null = not checked |
| ScratchOk | bool? | | |
| DentOk | bool? | | |
| DimensionalCheckOk | bool? | | |
| BuffingRequired | bool | | Default false |
| BuffingCharge | decimal? | Precision(10,2) | |
| Status | string | Required, MaxLength(20) | "Pass", "Fail", "Conditional" |
| Remarks | string? | MaxLength(500) | |

---

### Planning Entities

#### ProductionWorkOrder
```
Models/Planning/ProductionWorkOrder.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| PWONumber | string | Required, MaxLength(30), Unique | Auto: PWO-YYYY-NNN |
| Date | DateTime | Required | |
| WorkOrderId | int | FK → WorkOrder | |
| CustomerId | int | FK → Customer | |
| ProcessTypeId | int | FK → ProcessType | |
| PowderColorId | int? | FK → PowderColor | |
| ProductionUnitId | int | FK → ProductionUnit | |
| PowderCode | string? | MaxLength(50) | Snapshot |
| ColorName | string? | MaxLength(100) | Snapshot |
| PreTreatmentTimeHrs | decimal? | Precision(8,2) | Calculated |
| PostTreatmentTimeHrs | decimal? | Precision(8,2) | Calculated |
| TotalTimeHrs | decimal? | Precision(8,2) | Pre + Post |
| ShiftAllocation | string? | MaxLength(20) | "Day", "Night", "Both" |
| StartDate | DateTime? | | |
| DispatchDate | DateTime? | | Target |
| PackingType | string? | MaxLength(100) | e.g., "MERO TAPE" |
| SpecialInstructions | string? | MaxLength(2000) | |
| Status | string | Required, MaxLength(30) | "Created", "Scheduled", "InProgress", "Complete" |

**Relationships:** Has many PWOLineItems, PretreatmentLogs, ProductionLogs, InProcessInspections

#### PWOLineItem
```
Models/Planning/PWOLineItem.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| ProductionWorkOrderId | int | FK → ProductionWorkOrder | |
| SectionProfileId | int | FK → SectionProfile | |
| CustomerDCNo | string? | MaxLength(50) | Reference |
| Quantity | int | | |
| LengthMM | decimal | Precision(10,2) | |
| PerimeterMM | decimal | Precision(10,2) | |
| UnitSurfaceAreaSqMtr | decimal | Precision(10,4) | |
| TotalSurfaceAreaSqft | decimal | Precision(12,2) | |
| SpecialInstructions | string? | MaxLength(500) | e.g., "MILL FINISH" |

#### ProductionSchedule
```
Models/Planning/ProductionSchedule.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| Date | DateTime | Required | Schedule date |
| Shift | string | Required, MaxLength(10) | "Day", "Night" |
| ProductionWorkOrderId | int | FK → ProductionWorkOrder | |
| ProductionUnitId | int | FK → ProductionUnit | |
| SortOrder | int | | Display ordering within day |
| Status | string | Required, MaxLength(20) | "Planned", "InProgress", "Completed", "Cancelled" |
| Notes | string? | MaxLength(500) | |

#### ProductionTimeCalc
```
Models/Planning/ProductionTimeCalc.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| PWOLineItemId | int | FK → PWOLineItem | |
| ThicknessMM | decimal? | Precision(6,2) | |
| WidthMM | decimal? | Precision(10,2) | |
| HeightMM | decimal? | Precision(10,2) | |
| SpecificWeight | decimal? | Precision(8,4) | 2.71 for aluminum |
| WeightPerMtr | decimal? | Precision(10,4) | kg/m |
| TotalWeightKg | decimal? | Precision(12,2) | |
| LoadsRequired | int? | | Pre-treatment basket loads |
| TotalTimePreTreatMins | decimal? | Precision(10,2) | |
| ConveyorSpeedMtrPerMin | decimal? | Precision(8,2) | Based on thickness |
| JigLengthMM | decimal? | Precision(10,2) | |
| GapBetweenJigsMM | decimal? | Precision(10,2) | 500 typical |
| TotalConveyorDistanceMtrs | decimal? | Precision(12,2) | |
| TotalTimePostTreatMins | decimal? | Precision(10,2) | |

---

### Production Entities

#### PretreatmentLog
```
Models/Production/PretreatmentLog.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| Date | DateTime | Required | |
| Shift | string | Required, MaxLength(10) | "Day", "Night" |
| ProductionWorkOrderId | int | FK → ProductionWorkOrder | |
| BasketNumber | int | | Sequential per PWO |
| StartTime | TimeSpan? | | |
| EndTime | TimeSpan? | | |
| EtchTimeMins | decimal? | Precision(6,2) | Per QA instruction |
| OperatorUserId | string? | FK → AppUser | |
| QASignOffUserId | string? | FK → AppUser | |
| Remarks | string? | MaxLength(1000) | |

**Relationships:** Has many PretreatmentTankReadings

#### PretreatmentTankReading
```
Models/Production/PretreatmentTankReading.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| PretreatmentLogId | int | FK → PretreatmentLog | |
| TankName | string | Required, MaxLength(50) | "Degreasing", "Etching", "Chromating", etc. |
| Concentration | decimal? | Precision(8,4) | Points |
| Temperature | decimal? | Precision(6,2) | °C |
| ChemicalAdded | string? | MaxLength(200) | What was added |
| ChemicalQty | decimal? | Precision(8,2) | Amount added |

#### ProductionLog
```
Models/Production/ProductionLog.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| Date | DateTime | Required | |
| Shift | string | Required, MaxLength(10) | |
| ProductionWorkOrderId | int | FK → ProductionWorkOrder | |
| ConveyorSpeedMtrPerMin | decimal? | Precision(8,2) | |
| OvenTemperature | decimal? | Precision(6,2) | °C |
| PowderBatchNo | string? | MaxLength(50) | |
| SupervisorUserId | string? | FK → AppUser | |
| Remarks | string? | MaxLength(1000) | |

**Relationships:** Has many ProductionPhotos

#### ProductionPhoto
```
Models/Production/ProductionPhoto.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| ProductionLogId | int | FK → ProductionLog | |
| PhotoUrl | string | Required, MaxLength(500) | |
| CapturedAt | DateTime | | Timestamp of capture |
| UploadedByUserId | string? | FK → AppUser | |
| Description | string? | MaxLength(200) | |

---

### Quality Entities

#### InProcessInspection
```
Models/Quality/InProcessInspection.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| Date | DateTime | Required | |
| Time | TimeSpan | Required | Time of inspection |
| ProductionWorkOrderId | int | FK → ProductionWorkOrder | |
| InspectorUserId | string? | FK → AppUser | |
| Remarks | string? | MaxLength(1000) | |

**Relationships:** Has many InProcessDFTReadings, InProcessTestResults

#### InProcessDFTReading
```
Models/Quality/InProcessDFTReading.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| InProcessInspectionId | int | FK → InProcessInspection | |
| SectionProfileId | int? | FK → SectionProfile | |
| S1 | decimal? | Precision(6,2) | Micron reading point 1 |
| S2 | decimal? | Precision(6,2) | |
| S3 | decimal? | Precision(6,2) | |
| S4 | decimal? | Precision(6,2) | |
| S5 | decimal? | Precision(6,2) | |
| S6 | decimal? | Precision(6,2) | |
| S7 | decimal? | Precision(6,2) | |
| S8 | decimal? | Precision(6,2) | |
| S9 | decimal? | Precision(6,2) | |
| S10 | decimal? | Precision(6,2) | |
| MinReading | decimal? | Precision(6,2) | Computed |
| MaxReading | decimal? | Precision(6,2) | Computed |
| AvgReading | decimal? | Precision(6,2) | Computed |
| IsWithinSpec | bool | | True if all 60–80 microns |

#### InProcessTestResult
```
Models/Quality/InProcessTestResult.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| InProcessInspectionId | int | FK → InProcessInspection | |
| TestType | string | Required, MaxLength(30) | "DryFilmThickness", "Polymerisation", "Adhesion", "ShadeMatch", "GlossLevel" |
| InstrumentName | string? | MaxLength(100) | |
| InstrumentMake | string? | MaxLength(100) | |
| InstrumentModel | string? | MaxLength(100) | |
| CalibrationDate | DateTime? | | |
| ReferenceStandard | string? | MaxLength(100) | e.g., "AAMA-2604&2605" |
| StandardLimit | string? | MaxLength(100) | e.g., "30 Rubs" |
| Result | string | Required, MaxLength(200) | Test outcome value |
| Status | string | Required, MaxLength(10) | "Pass", "Fail" |
| Remarks | string? | MaxLength(500) | |

#### PanelTest
```
Models/Quality/PanelTest.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| Date | DateTime | Required | |
| ProductionWorkOrderId | int | FK → ProductionWorkOrder | |
| BoilingWaterResult | string? | MaxLength(100) | |
| BoilingWaterStatus | string? | MaxLength(10) | "Pass"/"Fail" |
| ImpactTestResult | string? | MaxLength(100) | |
| ImpactTestStatus | string? | MaxLength(10) | |
| ConicalMandrelResult | string? | MaxLength(100) | |
| ConicalMandrelStatus | string? | MaxLength(10) | |
| PencilHardnessResult | string? | MaxLength(100) | |
| PencilHardnessStatus | string? | MaxLength(10) | |
| InspectorUserId | string? | FK → AppUser | |
| Remarks | string? | MaxLength(1000) | |

#### FinalInspection
```
Models/Quality/FinalInspection.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| InspectionNumber | string | Required, MaxLength(30), Unique | Auto: FIR-YYYY-NNN |
| Date | DateTime | Required | |
| ProductionWorkOrderId | int | FK → ProductionWorkOrder | |
| LotQuantity | int | | Total pieces in lot |
| SampledQuantity | int | | Pieces inspected (AQL) |
| VisualCheckStatus | string? | MaxLength(10) | "Pass"/"Fail" |
| DFTRecheckStatus | string? | MaxLength(10) | |
| ShadeMatchFinalStatus | string? | MaxLength(10) | |
| OverallStatus | string | Required, MaxLength(20) | "Approved", "Rejected", "Rework" |
| InspectorUserId | string? | FK → AppUser | |
| Remarks | string? | MaxLength(2000) | |

**Relationships:** Has one TestCertificate

#### TestCertificate
```
Models/Quality/TestCertificate.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| CertificateNumber | string | Required, MaxLength(30), Unique | Auto: TC-YYYY-NNN |
| Date | DateTime | Required | |
| FinalInspectionId | int | FK → FinalInspection | |
| CustomerId | int | FK → Customer | |
| WorkOrderId | int | FK → WorkOrder | |
| ProductCode | string? | MaxLength(50) | RAL code |
| ProjectName | string? | MaxLength(300) | |
| LotQuantity | int | | |
| Warranty | string? | MaxLength(20) | "15 Years", "25 Years" |
| SubstrateResult | string? | MaxLength(100) | e.g., "Aluminium Profiles" |
| BakingTempResult | string? | MaxLength(50) | e.g., "200°C" |
| BakingTimeResult | string? | MaxLength(50) | e.g., "10 Minutes" |
| ColorResult | string? | MaxLength(50) | "Close to standard" |
| DFTResult | string? | MaxLength(50) | e.g., "76.2+/- 2.5micron" |
| MEKResult | string? | MaxLength(50) | "Pass" |
| CrossCutResult | string? | MaxLength(50) | "No peel up" |
| ConicalMandrelResult | string? | MaxLength(50) | "No Crack" |
| BoilingWaterResult | string? | MaxLength(50) | "No peel up" |
| FileUrl | string? | MaxLength(500) | PDF |

---

### Dispatch Entities

#### PackingList
```
Models/Dispatch/PackingList.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| Date | DateTime | Required | |
| ProductionWorkOrderId | int | FK → ProductionWorkOrder | |
| WorkOrderId | int | FK → WorkOrder | |
| PackingType | string? | MaxLength(100) | e.g., "MERO TAPE", "Poly wrap" |
| BundleCount | int? | | Total bundles |
| PreparedByUserId | string? | FK → AppUser | |
| Notes | string? | MaxLength(1000) | |

**Relationships:** Has many PackingListLines

#### PackingListLine
```
Models/Dispatch/PackingListLine.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| PackingListId | int | FK → PackingList | |
| SectionProfileId | int | FK → SectionProfile | |
| Quantity | int | | |
| LengthMM | decimal | Precision(10,2) | |
| BundleNumber | int? | | |
| Remarks | string? | MaxLength(300) | |

#### DeliveryChallan
```
Models/Dispatch/DeliveryChallan.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| DCNumber | string | Required, MaxLength(30), Unique | Auto: DC-YYYY-NNN |
| Date | DateTime | Required | |
| WorkOrderId | int | FK → WorkOrder | |
| CustomerId | int | FK → Customer | |
| CustomerAddress | string? | MaxLength(500) | Snapshot |
| CustomerGSTIN | string? | MaxLength(15) | Snapshot |
| VehicleNumber | string? | MaxLength(20) | |
| DriverName | string? | MaxLength(100) | |
| LRNumber | string? | MaxLength(50) | Lorry receipt |
| MaterialValueApprox | decimal? | Precision(14,2) | |
| Status | string | Required, MaxLength(20) | "Created", "Dispatched", "Delivered" |
| Notes | string? | MaxLength(1000) | |

**Relationships:** Has many DCLineItems

#### DCLineItem
```
Models/Dispatch/DCLineItem.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| DeliveryChallanId | int | FK → DeliveryChallan | |
| SectionProfileId | int | FK → SectionProfile | |
| LengthMM | decimal | Precision(10,2) | |
| Quantity | int | | |
| CustomerDCRef | string? | MaxLength(50) | Reference to customer's original DC |
| Remarks | string? | MaxLength(300) | |

#### Invoice
```
Models/Dispatch/Invoice.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| InvoiceNumber | string | Required, MaxLength(30), Unique | Auto: INV-YYYY-NNN |
| Date | DateTime | Required | |
| CustomerId | int | FK → Customer | |
| WorkOrderId | int | FK → WorkOrder | |
| DeliveryChallanId | int? | FK → DeliveryChallan | |
| CustomerName | string? | MaxLength(300) | Snapshot |
| CustomerAddress | string? | MaxLength(500) | |
| CustomerGSTIN | string? | MaxLength(15) | |
| OurGSTIN | string | MaxLength(15) | HyCoat's GSTIN |
| HSNSACCode | string? | MaxLength(20) | |
| SubTotal | decimal | Precision(14,2) | |
| PackingCharges | decimal | Precision(10,2) | |
| TransportCharges | decimal | Precision(10,2) | |
| TaxableAmount | decimal | Precision(14,2) | |
| CGSTRate | decimal | Precision(5,2) | |
| CGSTAmount | decimal | Precision(12,2) | |
| SGSTRate | decimal | Precision(5,2) | |
| SGSTAmount | decimal | Precision(12,2) | |
| IGSTRate | decimal | Precision(5,2) | |
| IGSTAmount | decimal | Precision(12,2) | |
| GrandTotal | decimal | Precision(14,2) | |
| RoundOff | decimal | Precision(6,2) | |
| AmountInWords | string? | MaxLength(300) | |
| IsInterState | bool | | |
| PaymentTerms | string? | MaxLength(500) | |
| BankName | string? | MaxLength(100) | |
| BankAccountNo | string? | MaxLength(30) | |
| BankIFSC | string? | MaxLength(15) | |
| Status | string | Required, MaxLength(20) | "Draft", "Finalized", "Sent", "Paid" |
| FileUrl | string? | MaxLength(500) | PDF |

**Relationships:** Has many InvoiceLineItems

#### InvoiceLineItem
```
Models/Dispatch/InvoiceLineItem.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| InvoiceId | int | FK → Invoice | |
| SectionProfileId | int | FK → SectionProfile | |
| SectionNumber | string? | MaxLength(50) | Snapshot |
| DCNumber | string? | MaxLength(30) | Delivery challan reference |
| Color | string? | MaxLength(50) | |
| MicronRange | string? | MaxLength(20) | "60-80" |
| PerimeterMM | decimal | Precision(10,2) | |
| LengthMM | decimal | Precision(10,2) | |
| Quantity | int | | |
| AreaSFT | decimal | Precision(12,2) | Calculated |
| RatePerSFT | decimal | Precision(10,2) | |
| Amount | decimal | Precision(14,2) | |

---

### Purchase Entities

#### PowderIndent
```
Models/Purchase/PowderIndent.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| IndentNumber | string | Required, MaxLength(30), Unique | Auto: IND-YYYY-NNN |
| Date | DateTime | Required | |
| ProductionWorkOrderId | int? | FK → ProductionWorkOrder | |
| RequestedByUserId | string? | FK → AppUser | |
| Status | string | Required, MaxLength(20) | "Requested", "Approved", "Ordered", "Received" |
| Notes | string? | MaxLength(1000) | |

**Relationships:** Has many PowderIndentLines

#### PowderIndentLine
```
Models/Purchase/PowderIndentLine.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| PowderIndentId | int | FK → PowderIndent | |
| PowderColorId | int | FK → PowderColor | |
| RequiredQtyKg | decimal | Precision(10,2) | |

#### PurchaseOrder
```
Models/Purchase/PurchaseOrder.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| PONumber | string | Required, MaxLength(30), Unique | Auto: PO-YYYY-NNN |
| Date | DateTime | Required | |
| VendorId | int | FK → Vendor | |
| PowderIndentId | int? | FK → PowderIndent | |
| Status | string | Required, MaxLength(20) | "Draft", "Sent", "PartiallyReceived", "Received", "Cancelled" |
| Notes | string? | MaxLength(1000) | |

**Relationships:** Has many POLineItems

#### POLineItem
```
Models/Purchase/POLineItem.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| PurchaseOrderId | int | FK → PurchaseOrder | |
| PowderColorId | int | FK → PowderColor | |
| QtyKg | decimal | Precision(10,2) | |
| RatePerKg | decimal | Precision(10,2) | |
| Amount | decimal | Precision(12,2) | QtyKg × RatePerKg |
| RequiredByDate | DateTime? | | |

#### GoodsReceivedNote
```
Models/Purchase/GoodsReceivedNote.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| *(BaseEntity)* | | | |
| GRNNumber | string | Required, MaxLength(30), Unique | Auto: GRN-YYYY-NNN |
| Date | DateTime | Required | |
| PurchaseOrderId | int | FK → PurchaseOrder | |
| ReceivedByUserId | string? | FK → AppUser | |
| Notes | string? | MaxLength(1000) | |

**Relationships:** Has many GRNLineItems

#### GRNLineItem
```
Models/Purchase/GRNLineItem.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| GoodsReceivedNoteId | int | FK → GoodsReceivedNote | |
| PowderColorId | int | FK → PowderColor | |
| QtyReceivedKg | decimal | Precision(10,2) | |
| BatchCode | string? | MaxLength(50) | |
| MfgDate | DateTime? | | |
| ExpiryDate | DateTime? | | |

#### PowderStock
```
Models/Purchase/PowderStock.cs
```
| Field | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK | |
| PowderColorId | int | FK → PowderColor, Unique | One record per powder |
| CurrentStockKg | decimal | Precision(12,2) | Running balance |
| ReorderLevelKg | decimal? | Precision(10,2) | Alert threshold |
| LastUpdated | DateTime | | |

---

## Relationships Summary

```
Customer ──< Inquiry ──< Quotation
Customer ──< WorkOrder
Customer ──< MaterialInward
Customer ──< Invoice

Inquiry ──< Quotation ──< ProformaInvoice ──< WorkOrder
ProformaInvoice ──< PILineItem >── SectionProfile
Quotation ──< QuotationLineItem >── ProcessType

WorkOrder ──< MaterialInward ──< MaterialInwardLine >── SectionProfile
MaterialInward ──< IncomingInspection ──< IncomingInspectionLine >── MaterialInwardLine

WorkOrder ──< ProductionWorkOrder ──< PWOLineItem >── SectionProfile
ProductionWorkOrder ──< ProductionSchedule
ProductionWorkOrder ──< PretreatmentLog ──< PretreatmentTankReading
ProductionWorkOrder ──< ProductionLog ──< ProductionPhoto
ProductionWorkOrder ──< InProcessInspection ──< InProcessDFTReading
ProductionWorkOrder ──< InProcessInspection ──< InProcessTestResult
ProductionWorkOrder ──< PanelTest
ProductionWorkOrder ──< FinalInspection ──< TestCertificate

WorkOrder ──< DeliveryChallan ──< DCLineItem
WorkOrder ──< Invoice ──< InvoiceLineItem

PowderColor >── Vendor
PowderIndent ──< PowderIndentLine >── PowderColor
PurchaseOrder ──< POLineItem >── PowderColor
GoodsReceivedNote ──< GRNLineItem >── PowderColor
PowderStock >── PowderColor
```

## EF Core Configuration

### AppDbContext Updates
Register all entities as DbSets in `Data/AppDbContext.cs`. Configure with Fluent API in individual configuration files under `Data/Configurations/`.

### Global Query Filter
Apply soft-delete filter globally:
```csharp
// In OnModelCreating
foreach (var entityType in modelBuilder.Model.GetEntityTypes())
{
    if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
    {
        modelBuilder.Entity(entityType.ClrType)
            .HasQueryFilter(/* e => !e.IsDeleted */);
    }
}
```

### Seed Data
Seed the following on initial migration:
1. **Roles:** "Admin", "Leader", "User"
2. **Default Admin User:** admin@hycoat.com / Admin@123 (change on first login)
3. **Process Types:** "Powder Coating", "Anodizing", "Wood Effect", "Chromotizing", "PVDF", "Mill Finish"
4. **Production Units:** "Unit-1", "Unit-2"

### Migration Command
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Acceptance Criteria
1. All entity classes compile without errors under their respective `Models/` subdirectories
2. `AppDbContext` has DbSets for all entities (~40+)
3. EF Configuration files exist under `Data/Configurations/` for entities needing custom config
4. `dotnet ef migrations add InitialCreate` succeeds without errors
5. `dotnet ef database update` creates all tables in SQL Server
6. Seed data is inserted: roles, admin user, process types, production units
7. Foreign key relationships are correct — navigation properties work
8. Soft-delete global query filter is applied
9. Unique constraints are enforced (all auto-number fields, SectionNumber, etc.)
10. Decimal precision is set correctly for monetary (14,2) and measurement (10,2) fields

## Reference
- **WORKFLOWS.md:** All sections — entity fields are derived from "Key Data Fields" in each workflow
- **Image Slides:** Slide 9 (Material Inward), Slide 10 (PI), Slide 14 (Production Order), Slide 15 (Process Route Card), Slide 20 (Incoming Inspection), Slide 22 (Final Inspection), Slide 24 (Test Certificate), Slide 25 (DC), Slide 26 (Invoice)
