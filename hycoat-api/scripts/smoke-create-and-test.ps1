$base='http://127.0.0.1:5099'
$results = [System.Collections.Generic.List[object]]::new()

function Add-Result($name,$ok,$details){
  $results.Add([pscustomobject]@{Test=$name;Ok=$ok;Details=$details})
}

function Post-Json($path,$body){
  Invoke-RestMethod -Uri "$base$path" -Method Post -ContentType 'application/json' -Body ($body | ConvertTo-Json -Depth 8)
}

$pngPath = Join-Path $env:TEMP 'hycoat-smoke-upload.png'
$pngB64 = 'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO8B6QAAAABJRU5ErkJggg=='
[IO.File]::WriteAllBytes($pngPath,[Convert]::FromBase64String($pngB64))
$pdfPath = Join-Path $env:TEMP 'hycoat-smoke-file.pdf'
[IO.File]::WriteAllBytes($pdfPath,[Text.Encoding]::ASCII.GetBytes('%PDF-1.4`n%Smoke`n'))

$stamp = [DateTime]::UtcNow.ToString('yyyyMMddHHmmss')

# 1) Customer
$customerBody = @{
  name = "Smoke Customer $stamp"
  shortName = "SMK$($stamp.Substring($stamp.Length-4))"
  city = 'Bangalore'
  state = 'Karnataka'
  contactPerson = 'Smoke User'
  phone = '9999999999'
  email = "smoke+$stamp@example.com"
}
$customerResp = Post-Json '/api/customers' $customerBody
$customerId = [int]$customerResp.data.id
Add-Result 'Create customer' ($customerId -gt 0) "customerId=$customerId"

# 2) Section profile
$sectionBody = @{
  sectionNumber = "SMK-SEC-$stamp"
  type = 'Box'
  perimeterMM = 120.5
  heightMM = 20
  widthMM = 15
  thicknessMM = 1.2
}
$sectionResp = Post-Json '/api/section-profiles' $sectionBody
$sectionId = [int]$sectionResp.data.id
Add-Result 'Create section profile' ($sectionId -gt 0) "sectionId=$sectionId"

# 3) Material inward (for photo upload)
$miBody = @{
  date = [DateTime]::UtcNow.ToString('o')
  customerId = $customerId
  workOrderId = $null
  customerDCNumber = "SMK-DC-$stamp"
  customerDCDate = [DateTime]::UtcNow.ToString('o')
  vehicleNumber = 'KA01AA1234'
  unloadingLocation = 'Dock 1'
  processTypeId = 1
  powderColorIds = @()
  notes = 'Smoke test row'
  lines = @(
    @{
      sectionProfileId = $sectionId
      lengthMM = 1000
      qtyAsPerDC = 5
      qtyReceived = 5
      weightKg = 10
      remarks = 'OK'
    }
  )
}
$miResp = Post-Json '/api/material-inwards' $miBody
$miId = [int]$miResp.data.id
Add-Result 'Create material inward' ($miId -gt 0) "materialInwardId=$miId"

# 4) Upload material inward photo
try {
  $raw = & curl.exe -s -X POST -F "files=@$pngPath;type=image/png" "$base/api/material-inwards/$miId/photos"
  $upMi = $raw | ConvertFrom-Json
  $stored = $upMi.data[0].storedPath
  Add-Result 'Upload material inward photo' ($stored -like 'https://hycoatdevstcixco4d.blob.core.windows.net/*') "storedPath=$stored"
} catch {
  Add-Result 'Upload material inward photo' $false $_.Exception.Message
}

# 5) Generic file upload
try {
  $raw2 = & curl.exe -s -X POST -F "file=@$pdfPath;type=application/pdf" -F "entityType=MaterialInward" -F "entityId=$miId" -F "category=SmokeTest" "$base/api/files/upload"
  $upFile = $raw2 | ConvertFrom-Json
  $downloadUrl = $upFile.data.downloadUrl
  Add-Result 'Upload generic file' ($downloadUrl -like '/api/files/*') "downloadUrl=$downloadUrl"
} catch {
  Add-Result 'Upload generic file' $false $_.Exception.Message
}

# 6) Upload section drawing
try {
  $raw3 = & curl.exe -s -X POST -F "file=@$pdfPath;type=application/pdf" "$base/api/section-profiles/$sectionId/upload-drawing"
  $upSec = $raw3 | ConvertFrom-Json
  $url = $upSec.data
  Add-Result 'Upload section drawing' ($url -like 'https://hycoatdevstcixco4d.blob.core.windows.net/*') "drawingUrl=$url"
} catch {
  Add-Result 'Upload section drawing' $false $_.Exception.Message
}

# 7) Create quotation and generate PDF (blob persisted)
$quoteBody = @{
  date = [DateTime]::UtcNow.ToString('o')
  inquiryId = $null
  customerId = $customerId
  validityDays = 30
  notes = 'Smoke quotation'
  lineItems = @(
    @{ processTypeId = 1; description = 'Powder Coating'; ratePerSFT = 12.5; warrantyYears = 2; micronRange = '60-80' }
  )
}
$quoteResp = Post-Json '/api/quotations' $quoteBody
$quoteId = [int]$quoteResp.data.id
try {
  $qpdf = Invoke-WebRequest -Uri "$base/api/quotations/$quoteId/pdf" -Method Get -TimeoutSec 120
  $qdetail = Invoke-RestMethod -Uri "$base/api/quotations/$quoteId" -Method Get -TimeoutSec 120
  $qUrl = $qdetail.data.fileUrl
  $ok = ($qpdf.StatusCode -eq 200 -and $qpdf.Headers['Content-Type'] -like 'application/pdf*' -and $qUrl -like 'https://hycoatdevstcixco4d.blob.core.windows.net/*')
  Add-Result 'Quotation PDF flow' $ok "status=$($qpdf.StatusCode); fileUrl=$qUrl"
} catch {
  Add-Result 'Quotation PDF flow' $false $_.Exception.Message
}

# 8) Create proforma invoice and generate PDF (blob persisted)
$piBody = @{
  date = [DateTime]::UtcNow.ToString('o')
  customerId = $customerId
  quotationId = $quoteId
  packingCharges = 0
  transportCharges = 0
  isInterState = $false
  notes = 'Smoke PI'
  lineItems = @(
    @{ sectionProfileId = $sectionId; lengthMM = 1000; quantity = 10; ratePerSFT = 15 }
  )
}
$piResp = Post-Json '/api/proforma-invoices' $piBody
$piId = [int]$piResp.data.id
try {
  $pipdf = Invoke-WebRequest -Uri "$base/api/proforma-invoices/$piId/pdf" -Method Get -TimeoutSec 120
  $pidetail = Invoke-RestMethod -Uri "$base/api/proforma-invoices/$piId" -Method Get -TimeoutSec 120
  $piUrl = $pidetail.data.fileUrl
  $ok2 = ($pipdf.StatusCode -eq 200 -and $pipdf.Headers['Content-Type'] -like 'application/pdf*' -and $piUrl -like 'https://hycoatdevstcixco4d.blob.core.windows.net/*')
  Add-Result 'Proforma Invoice PDF flow' $ok2 "status=$($pipdf.StatusCode); fileUrl=$piUrl"
} catch {
  Add-Result 'Proforma Invoice PDF flow' $false $_.Exception.Message
}

# 9) Invoice endpoint behavior snapshot
$invList = Invoke-RestMethod -Uri "$base/api/invoices?page=1&pageSize=1" -Method Get -TimeoutSec 60
if($invList.data.items -and $invList.data.items.Count -gt 0){
  $invId = [int]$invList.data.items[0].id
  try {
    $invPdf = Invoke-WebRequest -Uri "$base/api/invoices/$invId/pdf" -Method Get -TimeoutSec 120
    $invDetail = Invoke-RestMethod -Uri "$base/api/invoices/$invId" -Method Get -TimeoutSec 120
    Add-Result 'Invoice PDF current behavior' ($invPdf.StatusCode -eq 200) "status=$($invPdf.StatusCode); fileUrl=$($invDetail.data.fileUrl)"
  } catch {
    Add-Result 'Invoice PDF current behavior' $false $_.Exception.Message
  }
} else {
  Add-Result 'Invoice PDF current behavior' $false 'No invoice row exists in AzureDev for runtime behavior probe'
}

$results | ConvertTo-Json -Depth 6