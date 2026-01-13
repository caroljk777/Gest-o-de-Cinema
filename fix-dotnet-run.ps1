# Script para resolver problemas de bloqueio de ficheiros .NET
# Execute este script como Administrador

Write-Host "=== Script de Resolucao de Problemas .NET ===" -ForegroundColor Cyan
Write-Host ""

# Diretorio do projeto
$projectPath = "C:\Users\Lenovo\Documents\GestaoCinema"
$projectDir = "C:\Users\Lenovo\Documents\GestaoCinema\GestaoDeCinema\GestaoDeCinema"

Write-Host "1. A verificar privilegios de administrador..." -ForegroundColor Yellow
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "   AVISO: Este script nao esta a ser executado como Administrador!" -ForegroundColor Red
    Write-Host "   Algumas operacoes podem falhar. Clique com o botao direito no PowerShell e selecione 'Executar como Administrador'" -ForegroundColor Red
    Write-Host ""
    $continue = Read-Host "Deseja continuar mesmo assim? (S/N)"
    if ($continue -ne "S" -and $continue -ne "s") {
        exit
    }
}

Write-Host "2. A adicionar exclusao no Windows Defender..." -ForegroundColor Yellow
try {
    Add-MpPreference -ExclusionPath $projectPath -ErrorAction Stop
    Write-Host "   OK Exclusao adicionada com sucesso!" -ForegroundColor Green
} catch {
    Write-Host "   ERRO ao adicionar exclusao: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Pode adicionar manualmente em: Windows Security > Virus and threat protection > Manage settings > Exclusions" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "3. A desbloquear todos os ficheiros do projeto..." -ForegroundColor Yellow
try {
    Get-ChildItem -Path $projectPath -Recurse -ErrorAction SilentlyContinue | Unblock-File -ErrorAction SilentlyContinue
    Write-Host "   OK Ficheiros desbloqueados!" -ForegroundColor Green
} catch {
    Write-Host "   ERRO ao desbloquear ficheiros: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "4. A limpar pastas bin e obj..." -ForegroundColor Yellow
try {
    if (Test-Path "$projectDir\bin") {
        Remove-Item -Path "$projectDir\bin" -Recurse -Force -ErrorAction Stop
        Write-Host "   OK Pasta bin removida" -ForegroundColor Green
    }
    if (Test-Path "$projectDir\obj") {
        Remove-Item -Path "$projectDir\obj" -Recurse -Force -ErrorAction Stop
        Write-Host "   OK Pasta obj removida" -ForegroundColor Green
    }
} catch {
    Write-Host "   ERRO ao limpar pastas: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "5. A executar dotnet clean..." -ForegroundColor Yellow
Push-Location $projectDir
try {
    dotnet clean | Out-Null
    Write-Host "   OK Clean executado com sucesso" -ForegroundColor Green
} catch {
    Write-Host "   ERRO ao executar clean: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "6. A executar dotnet build..." -ForegroundColor Yellow
try {
    $buildOutput = dotnet build 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   OK Build executado com sucesso" -ForegroundColor Green
    } else {
        Write-Host "   ERRO Build falhou com codigo de saida: $LASTEXITCODE" -ForegroundColor Red
        Write-Host $buildOutput
    }
} catch {
    Write-Host "   ERRO ao executar build: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "7. A desbloquear ficheiros compilados..." -ForegroundColor Yellow
try {
    Get-ChildItem -Path "$projectDir\bin" -Recurse -ErrorAction SilentlyContinue | Unblock-File -ErrorAction SilentlyContinue
    Write-Host "   OK Ficheiros compilados desbloqueados" -ForegroundColor Green
} catch {
    Write-Host "   ERRO: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Resolucao de Problemas Concluida ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Agora pode tentar executar: dotnet run" -ForegroundColor Green
Write-Host ""
Write-Host "Se o problema persistir, tente:" -ForegroundColor Yellow
Write-Host "  1. Desativar temporariamente o Windows Defender" -ForegroundColor Yellow
Write-Host "  2. Verificar se existe software de antivirus de terceiros" -ForegroundColor Yellow
Write-Host "  3. Verificar politicas de grupo corporativas (gpedit.msc)" -ForegroundColor Yellow
Write-Host ""

$runNow = Read-Host "Deseja executar dotnet run agora? (S/N)"
if ($runNow -eq "S" -or $runNow -eq "s") {
    Write-Host ""
    Write-Host "A executar dotnet run..." -ForegroundColor Cyan
    dotnet run
}

Pop-Location
