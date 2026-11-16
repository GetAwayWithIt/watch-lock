# Draws the Watch Lock logo in code: PNGs at every size + a multi-size app.ico
# Usage: powershell -ExecutionPolicy Bypass -File tools\draw-icon.ps1
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $PSScriptRoot
$pngDir = Join-Path $root "assets\logo"
New-Item -ItemType Directory -Force $pngDir | Out-Null

function RoundedRect([single]$x, [single]$y, [single]$w, [single]$h, [single]$r) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $r * 2
    $p.AddArc($x, $y, $d, $d, 180, 90)
    $p.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $p.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
    $p.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $p.CloseFigure()
    return $p
}

function Draw([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'
    $g.PixelOffsetMode = 'HighQuality'
    $g.Clear([System.Drawing.Color]::Transparent)
    $s = $size / 256.0
    $g.ScaleTransform($s, $s)

    # Background: soft rounded square with a warm blue gradient
    $top = [System.Drawing.Color]::FromArgb(255, 108, 150, 255)
    $bottom = [System.Drawing.Color]::FromArgb(255, 74, 99, 224)
    $bg = RoundedRect 12 12 232 232 60
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush (New-Object System.Drawing.PointF 0, 12), (New-Object System.Drawing.PointF 0, 244), $top, $bottom
    $g.FillPath($brush, $bg)

    $white = [System.Drawing.Color]::FromArgb(255, 255, 255, 255)

    # Shackle (thick, round caps)
    $pen = New-Object System.Drawing.Pen $white, 24
    $pen.StartCap = 'Round'; $pen.EndCap = 'Round'
    $shackle = New-Object System.Drawing.Drawing2D.GraphicsPath
    $shackle.AddLine(90, 122, 90, 98)
    $shackle.AddArc(90, 60, 76, 76, 180, 180)
    $shackle.AddLine(166, 98, 166, 122)
    $g.DrawPath($pen, $shackle)

    # Lock body
    $body = RoundedRect 60 112 136 104 28
    $g.FillPath((New-Object System.Drawing.SolidBrush $white), $body)

    # Keyhole (background color)
    $hole = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 86, 118, 236))
    $g.FillEllipse($hole, 114, 140, 28, 28)
    $g.FillPath($hole, (RoundedRect 120 154 16 36 8))

    $g.Dispose()
    return $bmp
}

$sizes = 16, 20, 24, 32, 40, 48, 64, 128, 256
$pngs = @{}
foreach ($sz in $sizes) {
    $bmp = Draw $sz
    $path = Join-Path $pngDir "logo-$sz.png"
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    $pngs[$sz] = [System.IO.File]::ReadAllBytes($path)
}

# ICO file (PNG-compressed entries)
$icoPath = Join-Path $root "src\WatchLock\app.ico"
$ms = New-Object System.IO.MemoryStream
$w = New-Object System.IO.BinaryWriter $ms
$w.Write([uint16]0); $w.Write([uint16]1); $w.Write([uint16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
foreach ($sz in $sizes) {
    $data = $pngs[$sz]
    $edge = if ($sz -ge 256) { 0 } else { $sz }
    $w.Write([byte]$edge); $w.Write([byte]$edge); $w.Write([byte]0); $w.Write([byte]0)
    $w.Write([uint16]1); $w.Write([uint16]32)
    $w.Write([uint32]$data.Length); $w.Write([uint32]$offset)
    $offset += $data.Length
}
foreach ($sz in $sizes) { $w.Write($pngs[$sz]) }
$w.Flush()
[System.IO.File]::WriteAllBytes($icoPath, $ms.ToArray())
"Done: $icoPath"
