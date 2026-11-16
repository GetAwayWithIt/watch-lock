# Ekran Kilidi logosunu kodla çizer: her boyutta PNG + çok boyutlu app.ico
# Kullanım: powershell -ExecutionPolicy Bypass -File tools\ikon-ciz.ps1
Add-Type -AssemblyName System.Drawing

$kok = Split-Path -Parent $PSScriptRoot
$pngKlasor = Join-Path $kok "assets\logo"
New-Item -ItemType Directory -Force $pngKlasor | Out-Null

function Yuvarlak([single]$x, [single]$y, [single]$w, [single]$h, [single]$r) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $r * 2
    $p.AddArc($x, $y, $d, $d, 180, 90)
    $p.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $p.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
    $p.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $p.CloseFigure()
    return $p
}

function Ciz([int]$boyut) {
    $bmp = New-Object System.Drawing.Bitmap $boyut, $boyut, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'
    $g.PixelOffsetMode = 'HighQuality'
    $g.Clear([System.Drawing.Color]::Transparent)
    $s = $boyut / 256.0
    $g.ScaleTransform($s, $s)

    # Arka plan: yumuşak köşeli kare, sıcak mavi geçiş
    $ust = [System.Drawing.Color]::FromArgb(255, 108, 150, 255)
    $alt = [System.Drawing.Color]::FromArgb(255, 74, 99, 224)
    $bg = Yuvarlak 12 12 232 232 60
    $firca = New-Object System.Drawing.Drawing2D.LinearGradientBrush (New-Object System.Drawing.PointF 0, 12), (New-Object System.Drawing.PointF 0, 244), $ust, $alt
    $g.FillPath($firca, $bg)

    $beyaz = [System.Drawing.Color]::FromArgb(255, 255, 255, 255)

    # Kilit halkası (kalın, yuvarlak uçlu)
    $kalem = New-Object System.Drawing.Pen $beyaz, 24
    $kalem.StartCap = 'Round'; $kalem.EndCap = 'Round'
    $halka = New-Object System.Drawing.Drawing2D.GraphicsPath
    $halka.AddLine(90, 122, 90, 98)
    $halka.AddArc(90, 60, 76, 76, 180, 180)
    $halka.AddLine(166, 98, 166, 122)
    $g.DrawPath($kalem, $halka)

    # Kilit gövdesi
    $govde = Yuvarlak 60 112 136 104 28
    $g.FillPath((New-Object System.Drawing.SolidBrush $beyaz), $govde)

    # Anahtar deliği (arka plan rengi)
    $delik = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 86, 118, 236))
    $g.FillEllipse($delik, 114, 140, 28, 28)
    $g.FillPath($delik, (Yuvarlak 120 154 16 36 8))

    $g.Dispose()
    return $bmp
}

$boyutlar = 16, 20, 24, 32, 40, 48, 64, 128, 256
$pngler = @{}
foreach ($b in $boyutlar) {
    $bmp = Ciz $b
    $yol = Join-Path $pngKlasor "logo-$b.png"
    $bmp.Save($yol, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    $pngler[$b] = [System.IO.File]::ReadAllBytes($yol)
}

# ICO dosyası (PNG sıkıştırmalı girişler)
$icoYol = Join-Path $kok "src\EkranKilidi\app.ico"
$ms = New-Object System.IO.MemoryStream
$w = New-Object System.IO.BinaryWriter $ms
$w.Write([uint16]0); $w.Write([uint16]1); $w.Write([uint16]$boyutlar.Count)
$ofset = 6 + 16 * $boyutlar.Count
foreach ($b in $boyutlar) {
    $veri = $pngler[$b]
    $kenar = if ($b -ge 256) { 0 } else { $b }
    $w.Write([byte]$kenar); $w.Write([byte]$kenar); $w.Write([byte]0); $w.Write([byte]0)
    $w.Write([uint16]1); $w.Write([uint16]32)
    $w.Write([uint32]$veri.Length); $w.Write([uint32]$ofset)
    $ofset += $veri.Length
}
foreach ($b in $boyutlar) { $w.Write($pngler[$b]) }
$w.Flush()
[System.IO.File]::WriteAllBytes($icoYol, $ms.ToArray())
"Tamam: $icoYol"
