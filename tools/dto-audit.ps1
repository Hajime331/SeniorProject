param(
  [string]$Root = ".",
  [string]$OutDir = "./dto-audit"
)

New-Item -Force -ItemType Directory -Path $OutDir | Out-Null

# 期望存在的新 DTO 類別（依你拆分後的規劃，可再微調）
$expected = @(
  # Common
  "PagedResultDto","PagingQuery","OptionDto",
  # Tags
  "TagDto","TagQueryDto",
  # Videos
  "TrainingVideoCreateDto","TrainingVideoUpdateDto","TrainingVideoQueryDto",
  "TrainingVideoListItemDto","TrainingVideoDetailDto",
  # TrainingSets
  "TrainingSetCreateDto","TrainingSetUpdateDto","TrainingSetQueryDto",
  "TrainingSetListItemDto","TrainingSetDetailDto",
  "TrainingSetItemCreateDto","TrainingSetItemUpdateDto","TrainingSetItemDto",
  # Sessions
  "TrainingSessionCreateDto","TrainingSessionCompleteDto","TrainingSessionQueryDto",
  "TrainingSessionListItemDto","TrainingSessionDetailDto",
  "TrainingSessionItemDto","TrainingSessionItemUpdateDto"
)

# 可能殘留的舊名
$legacy = @(
  "TrainingVideoDto",
  "TrainingSetListDto"
)

# 掃 DTO 目錄，列出 class -> file
$dtoFiles = Get-ChildItem -Recurse -File "$Root/Meow.Shared/Dtos/*.cs"
$classes = @()
foreach ($f in $dtoFiles) {
  $content = Get-Content $f.FullName -Raw
  foreach ($m in [regex]::Matches($content, "class\s+([A-Za-z0-9_]+)")) {
    $classes += [pscustomobject]@{ Class=$m.Groups[1].Value; File=$f.FullName.Replace($Root+"\","") }
  }
}
$classes | Sort-Object Class | Export-Csv -NoTypeInformation -Encoding UTF8 "$OutDir/00-dto-classes.csv"

# 檢查缺少/殘留
$present = $classes.Class | Sort-Object -Unique
$missing = $expected | Where-Object { $_ -notin $present }
$legacyFound = $legacy | Where-Object { $_ -in $present }

$report = @()
$report += "== EXPECTED MISSING =="
$report += ($missing | ForEach-Object { "  - $_" })
$report += ""
$report += "== LEGACY STILL PRESENT =="
$report += ($legacyFound | ForEach-Object { "  - $_" })
$report | Set-Content -Encoding UTF8 "$OutDir/01-dto-presence.txt"

# 逐一檢查關鍵 DTO 是否含必要欄位（很鬆的 regex 檢查）
function HasProp($text, $prop){ return $text -match "(public\s+[^\s]+\s+$prop\b)"; }

$checks = @(
  @{File="Meow.Shared/Dtos/TrainingSets/TrainingSetCreateDto.cs"; Required=@("Name","BodyPart","Equipment","Items","TagIds")},
  @{File="Meow.Shared/Dtos/TrainingSets/TrainingSetItemCreateDto.cs"; Required=@("VideoId")},
  @{File="Meow.Shared/Dtos/TrainingSets/TrainingSetDetailDto.cs"; Required=@("Items")},
  @{File="Meow.Shared/Dtos/Videos/TrainingVideoUpdateDto.cs"; Required=@("VideoId","Title","BodyPart","Url","DurationSec","Status","TagIds")},
  @{File="Meow.Shared/Dtos/Videos/TrainingVideoCreateDto.cs"; Required=@("Title","BodyPart","Url","DurationSec","Status","TagIds")},
  @{File="Meow.Shared/Dtos/Videos/TrainingVideoQueryDto.cs"; Required=@("Status","BodyPart","TagId","Page","PageSize")},
  @{File="Meow.Shared/Dtos/TrainingSets/TrainingSetUpdateDto.cs"; Required=@("SetId","Name","BodyPart","Equipment","Items","TagIds")},
  @{File="Meow.Shared/Dtos/TrainingSessions/TrainingSessionCompleteDto.cs"; Required=@("EndedAt","CaloriesBurned","PointsAwarded","Notes")}
)

$missingProps = @()
foreach ($c in $checks) {
  $p = Join-Path $Root $c.File
  if (-not (Test-Path $p)) { $missingProps += "MISSING FILE: $($c.File)"; continue }
  $t = Get-Content $p -Raw
  foreach ($r in $c.Required) {
    if (-not (HasProp $t $r)) { $missingProps += "$($c.File) -> missing property '$r'" }
  }
}
$missingProps | Set-Content -Encoding UTF8 "$OutDir/02-dto-required-props.txt"

# 掃整個解決方案找舊 DTO 引用點
$targets = @("Meow.Api","Meow.Web","Meow.Shared")
foreach ($name in ($legacy + @("TrainingVideoDto","TrainingSetListDto"))) {
  foreach ($t in $targets) {
    $outfile = "$OutDir/10-refs-$name-$t.txt"
    rg --hidden --case-sensitive --line-number --glob "!bin/**" --glob "!obj/**" "$name\b" "$Root/$t" | Set-Content -Encoding UTF8 $outfile
  }
}

# 可能需要調整的 using 命名空間（供檢查）
$nsToCheck = @(
  "Meow.Shared.Dtos.Videos",
  "Meow.Shared.Dtos.TrainingSets",
  "Meow.Shared.Dtos.TrainingSessions"
)
foreach ($ns in $nsToCheck) {
  $outfile = "$OutDir/20-usings-$ns.txt"
  rg --hidden --line-number --glob "!bin/**" --glob "!obj/**" "using\s+$ns;" "$Root" | Set-Content -Encoding UTF8 $outfile
}

# 抓 Controller 的回傳型別（容易漏改）
rg --hidden --line-number --glob "!bin/**" --glob "!obj/**" "ActionResult<[^>]+>" "$Root/Meow.Api" `
  | Set-Content -Encoding UTF8 "$OutDir/30-controller-return-types.txt"

Write-Host "DTO Audit done. See folder: $OutDir"
