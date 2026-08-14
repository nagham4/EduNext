param(
    [switch]$Foreground
)

$ErrorActionPreference = "Stop"
Set-Location -LiteralPath $PSScriptRoot

$port = 5001
$venvDir = Join-Path $PSScriptRoot ".venv"
$venvPython = Join-Path $venvDir "Scripts\python.exe"
$requirements = Join-Path $PSScriptRoot "requirements.txt"
$envFile = Join-Path $PSScriptRoot ".env"
$outLogFile = Join-Path $PSScriptRoot "chatbot.host.out.log"
$errLogFile = Join-Path $PSScriptRoot "chatbot.host.err.log"

function Test-PortOpen($port) {
    $client = [System.Net.Sockets.TcpClient]::new()
    try {
        $connect = $client.BeginConnect("127.0.0.1", $port, $null, $null)
        if (-not $connect.AsyncWaitHandle.WaitOne(500)) {
            return $false
        }

        $client.EndConnect($connect)
        return $true
    } catch {
        return $false
    } finally {
        $client.Close()
    }
}

function Test-PythonVersion($python) {
    if (-not $python) {
        return $false
    }

    try {
        $version = & $python -c "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')" 2>$null
        return $version -in @("3.11", "3.12", "3.13")
    } catch {
        return $false
    }
}

function Find-Python {
    if (Test-Path -LiteralPath $venvPython) {
        return $venvPython
    }

    $localUvPython = Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot ".uv-python") `
        -Recurse `
        -Filter "python.exe" `
        -ErrorAction SilentlyContinue |
        Select-Object -First 1

    if ($localUvPython -and (Test-PythonVersion $localUvPython.FullName)) {
        return $localUvPython.FullName
    }

    if ($env:PYTHON_EXE -and (Test-PythonVersion $env:PYTHON_EXE)) {
        return $env:PYTHON_EXE
    }

    if (Get-Command py -ErrorAction SilentlyContinue) {
        foreach ($version in @("3.12", "3.11", "3.13")) {
            try {
                & py "-$version" -c "import sys" 2>$null
                if ($LASTEXITCODE -eq 0) {
                    return "py -$version"
                }
            } catch {
            }
        }
    }

    if ((Get-Command python -ErrorAction SilentlyContinue) -and (Test-PythonVersion "python")) {
        return "python"
    }

    return $null
}

function Invoke-Python($pythonCommand, [string[]]$arguments) {
    if ($pythonCommand -like "py -*") {
        $parts = $pythonCommand.Split(" ", 2)
        & $parts[0] $parts[1] @arguments
    } else {
        & $pythonCommand @arguments
    }
}

function Ensure-EnvFile {
    if (-not (Test-Path -LiteralPath $envFile)) {
        Write-Host "Missing Gemini API key."
        Write-Host "Create this file:"
        Write-Host "  $envFile"
        Write-Host "Then add:"
        Write-Host "  GEMINI_API_KEY=your-gemini-api-key"
        exit 1
    }

    $keyLine = Get-Content -LiteralPath $envFile |
        Where-Object { $_ -match '^\s*GEMINI_API_KEY\s*=' } |
        Select-Object -First 1

    if (-not $keyLine -or $keyLine -match '^\s*GEMINI_API_KEY\s*=\s*$') {
        Write-Host "GEMINI_API_KEY is missing or empty in:"
        Write-Host "  $envFile"
        exit 1
    }
}

function Ensure-Venv {
    if (Test-Path -LiteralPath $venvPython) {
        return $venvPython
    }

    $python = Find-Python
    $uv = Get-Command uv -ErrorAction SilentlyContinue

    Write-Host "Creating Python virtual environment..."

    if ($python) {
        Invoke-Python $python @("-m", "venv", $venvDir)
    } elseif ($uv) {
        $env:UV_CACHE_DIR = Join-Path $PSScriptRoot ".uv-cache"
        $env:UV_PYTHON_INSTALL_DIR = Join-Path $PSScriptRoot ".uv-python"
        & $uv.Source venv --python 3.12 $venvDir
    } else {
        Write-Host "Python 3.11/3.12/3.13 is not available, and uv was not found."
        Write-Host "Install Python 3.12, then run this command again."
        exit 1
    }

    if (-not (Test-Path -LiteralPath $venvPython)) {
        Write-Host "Failed to create virtual environment:"
        Write-Host "  $venvDir"
        exit 1
    }

    return $venvPython
}

function Ensure-Dependencies($python) {
    $importCheck = @'
import fastapi
import uvicorn
import google.genai
import faiss
import sentence_transformers
import pypdf
import fitz
'@

    & $python -c $importCheck
    if ($LASTEXITCODE -eq 0) {
        return
    }

    $stamp = Join-Path $venvDir ".requirements.stamp"
    $requirementsHash = (Get-FileHash -LiteralPath $requirements -Algorithm SHA256).Hash
    $installedHash = if (Test-Path -LiteralPath $stamp) {
        Get-Content -LiteralPath $stamp -ErrorAction SilentlyContinue
    } else {
        ""
    }

    if ($installedHash -eq $requirementsHash) {
        return
    }

    Write-Host "Installing AI Python dependencies. This may take a few minutes the first time..."
    & $python -m pip install --upgrade pip
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to upgrade pip. Check your internet connection or install dependencies manually:"
        Write-Host "  $python -m pip install -r $requirements"
        exit $LASTEXITCODE
    }

    & $python -m pip install -r $requirements
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to install AI dependencies. Check your internet connection or install dependencies manually:"
        Write-Host "  $python -m pip install -r $requirements"
        exit $LASTEXITCODE
    }

    Set-Content -LiteralPath $stamp -Value $requirementsHash
}

Ensure-EnvFile

if (Test-PortOpen $port) {
    Write-Host "AI chatbot is already running:"
    Write-Host "  http://127.0.0.1:$port/docs"
    Write-Host "  http://127.0.0.1:$port/health"

    if ($Foreground) {
        Write-Host ""
        Write-Host "Watching logs. Press Ctrl+C to stop watching."
        if (Test-Path -LiteralPath $errLogFile) {
            Get-Content -LiteralPath $errLogFile -Tail 40 -Wait
        } elseif (Test-Path -LiteralPath $outLogFile) {
            Get-Content -LiteralPath $outLogFile -Tail 40 -Wait
        }
    }

    exit 0
}

$python = Ensure-Venv
Ensure-Dependencies $python

if ($Foreground) {
    Write-Host "Starting AI chatbot in this terminal:"
    Write-Host "  http://127.0.0.1:$port/docs"
    Write-Host "Requests will appear in this terminal."
    Write-Host "Press Ctrl+C to stop it."
    Write-Host ""
    & $python run_local.py
    exit $LASTEXITCODE
}

$process = Start-Process -FilePath $python `
    -ArgumentList @("run_local.py") `
    -WorkingDirectory $PSScriptRoot `
    -RedirectStandardOutput $outLogFile `
    -RedirectStandardError $errLogFile `
    -WindowStyle Hidden `
    -PassThru

Write-Host "Starting AI chatbot on http://127.0.0.1:$port ..."

for ($attempt = 1; $attempt -le 30; $attempt++) {
    Start-Sleep -Milliseconds 500

    if (Test-PortOpen $port) {
        Write-Host "AI chatbot is running:"
        Write-Host "  http://127.0.0.1:$port/docs"
        Write-Host "  http://127.0.0.1:$port/health"
        Write-Host "Log files:"
        Write-Host "  $outLogFile"
        Write-Host "  $errLogFile"
        exit 0
    }

    if ($process.HasExited) {
        Write-Host "AI chatbot stopped before it could start."
        Write-Host "Log files:"
        Write-Host "  $outLogFile"
        Write-Host "  $errLogFile"
        if (Test-Path -LiteralPath $outLogFile) {
            Write-Host ""
            Get-Content -LiteralPath $outLogFile -Tail 40
        }
        if (Test-Path -LiteralPath $errLogFile) {
            Write-Host ""
            Get-Content -LiteralPath $errLogFile -Tail 40
        }
        exit $process.ExitCode
    }
}

Write-Host "AI chatbot is still starting in the background."
Write-Host "Check shortly:"
Write-Host "  http://127.0.0.1:$port/docs"
Write-Host "Log files:"
Write-Host "  $outLogFile"
Write-Host "  $errLogFile"
