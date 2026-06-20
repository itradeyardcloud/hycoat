$base='http://127.0.0.1:5099'
$results = [System.Collections.Generic.List[object]]::new()

function Add-Result($name,$ok,$details){
  $results.Add([pscustomobject]@{Test=$name;Ok=$ok;Details=$details})
}

function Get-FirstId($path){
  try {
    $resp = Invoke-RestMethod -Uri "$base$path" -Method Get -TimeoutSec 60
    if($resp.data.items -and $resp.data.items.Count -gt 0){ return [int]$resp.data.items[0].id }
    return $null
  } catch {
    return $null
  }
}

$pngPath = Join-Path $env:TEMP 'hycoat-smoke-upload.png'
$pngB64 = 'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO8B6QAAAABJRU5ErkJggg=='
[IO.File]::WriteAllBytes($pngPath,[Convert]::FromBase64String($pngB64))

$miId = Get-FirstId '/api/material-inwards?page=1&pageSize=1'
if($miId){
  try {
    $up = Invoke-RestMethod -Uri "$base/api/material-inwards/$miId/photos" -Method Post -Form @{ files = Get-Item $pngPath } -TimeoutSec 120
    $stored = $up.data[0].storedPath
    $isBlob = ($stored -like 'https://hycoatdevstcixco4d.blob.core.windows.net/*')
    Add-Result 'MaterialInward photo upload' $isBlob "storedPath=$stored"
  } catch {
    Add-Result 'MaterialInward photo upload' $false $_.Exception.Message
  }
} else {
  Add-Result 'MaterialInward photo upload' $false 'No material inward row found to test against'
}

if($miId){
  try {
    $fileUp = Invoke-RestMethod -Uri "$base/api/files/upload" -Method Post -Form @{ file = Get-Item $pngPath; entityType='MaterialInward'; entityId="$miId"; category='SmokeTest' } -TimeoutSec 120
    $stored2 = $fileUp.data.storedPath
    $isBlob2 = ($stored2 -like 'https://hycoatdevstcixco4d.blob.core.windows.net/*')
    Add-Result 'Generic files upload' $isBlob2 "storedPath=$stored2"
  } catch {
    Add-Result 'Generic files upload' $false $_.Exception.Message
  }
}

$quoteId = Get-FirstId '/api/quotations?page=1&pageSize=1'
if($quoteId){
  try {
    $pdf = Invoke-WebRequest -Uri "$base/api/quotations/$quoteId/pdf" -Method Get -TimeoutSec 120
    Add-Result 'Quotation PDF endpoint' ($pdf.StatusCode -eq 200 -and $pdf.Headers['Content-Type'] -like 'application/pdf*') "status=$($pdf.StatusCode); type=$($pdf.Headers['Content-Type'])"
  } catch {
    Add-Result 'Quotation PDF endpoint' $false $_.Exception.Message
  }
} else {
  Add-Result 'Quotation PDF endpoint' $false 'No quotation row found'
}

$piId = Get-FirstId '/api/proforma-invoices?page=1&pageSize=1'
if($piId){
  try {
    $pdf2 = Invoke-WebRequest -Uri "$base/api/proforma-invoices/$piId/pdf" -Method Get -TimeoutSec 120
    Add-Result 'Proforma Invoice PDF endpoint' ($pdf2.StatusCode -eq 200 -and $pdf2.Headers['Content-Type'] -like 'application/pdf*') "status=$($pdf2.StatusCode); type=$($pdf2.Headers['Content-Type'])"
  } catch {
    Add-Result 'Proforma Invoice PDF endpoint' $false $_.Exception.Message
  }
} else {
  Add-Result 'Proforma Invoice PDF endpoint' $false 'No proforma invoice row found'
}

$tcId = Get-FirstId '/api/test-certificates?page=1&pageSize=1'
if($tcId){
  try {
    $gen = Invoke-WebRequest -Uri "$base/api/test-certificates/$tcId/generate-pdf" -Method Post -TimeoutSec 120
    $dl = Invoke-WebRequest -Uri "$base/api/test-certificates/$tcId/pdf" -Method Get -TimeoutSec 120
    Add-Result 'Test Certificate PDF generate/download' ($gen.StatusCode -eq 200 -and $dl.StatusCode -eq 200 -and $dl.Headers['Content-Type'] -like 'application/pdf*') "gen=$($gen.StatusCode); dl=$($dl.StatusCode); type=$($dl.Headers['Content-Type'])"
  } catch {
    Add-Result 'Test Certificate PDF generate/download' $false $_.Exception.Message
  }
} else {
  Add-Result 'Test Certificate PDF generate/download' $false 'No test certificate row found'
}

$invId = Get-FirstId '/api/invoices?page=1&pageSize=1'
if($invId){
  try {
    $invPdf = Invoke-WebRequest -Uri "$base/api/invoices/$invId/pdf" -Method Get -TimeoutSec 120
    Add-Result 'Invoice PDF endpoint' ($invPdf.StatusCode -eq 200 -and $invPdf.Headers['Content-Type'] -like 'application/pdf*') "status=$($invPdf.StatusCode); type=$($invPdf.Headers['Content-Type'])"
  } catch {
    Add-Result 'Invoice PDF endpoint' $false $_.Exception.Message
  }
} else {
  Add-Result 'Invoice PDF endpoint' $false 'No invoice row found'
}

$results | ConvertTo-Json -Depth 5