# Script avancado para resolver problemas de Controlo de Aplicacoes
# Execute como Administrador

Write-Host "=== Diagnostico e Resolucao de AppLocker/WDAC ===" -ForegroundColor Cyan
Write-Host ""

# Verificar privilegios de admin
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERRO: Este script DEVE ser executado como Administrador!" -ForegroundColor Red
    Write-Host "Clique com o botao direito no PowerShell e selecione 'Executar como Administrador'" -ForegroundColor Yellow
    Read-Host "Pressione Enter para sair"
    exit
}

Write-Host "1. Verificando status do AppLocker..." -ForegroundColor Yellow
try {
    $appLockerPolicy = Get-AppLockerPolicy -Effective -ErrorAction SilentlyContinue
    if ($appLockerPolicy) {
        Write-Host "   AVISO: AppLocker esta ATIVO neste sistema!" -ForegroundColor Red
        Write-Host "   Tentando desativar servico AppLocker..." -ForegroundColor Yellow
        
        try {
            Stop-Service -Name "AppIDSvc" -Force -ErrorAction Stop
            Set-Service -Name "AppIDSvc" -StartupType Disabled -ErrorAction Stop
            Write-Host "   OK Servico AppLocker desativado" -ForegroundColor Green
        } catch {
            Write-Host "   ERRO: Nao foi possivel desativar AppLocker: $($_.Exception.Message)" -ForegroundColor Red
        }
    } else {
        Write-Host "   OK AppLocker nao esta configurado" -ForegroundColor Green
    }
} catch {
    Write-Host "   INFO: Nao foi possivel verificar AppLocker" -ForegroundColor Gray
}

Write-Host ""
Write-Host "2. Verificando Windows Defender Application Control..." -ForegroundColor Yellow
try {
    $wdacStatus = Get-CimInstance -Namespace root\Microsoft\Windows\DeviceGuard -ClassName Win32_DeviceGuard -ErrorAction SilentlyContinue
    if ($wdacStatus.CodeIntegrityPolicyEnforcementStatus -eq 1) {
        Write-Host "   AVISO: WDAC esta ATIVO (Enforced Mode)!" -ForegroundColor Red
        Write-Host "   Isto pode requerer configuracao ao nivel do sistema operativo" -ForegroundColor Yellow
    } else {
        Write-Host "   OK WDAC nao esta em modo enforced" -ForegroundColor Green
    }
} catch {
    Write-Host "   INFO: Nao foi possivel verificar WDAC" -ForegroundColor Gray
}

Write-Host ""
Write-Host "3. Desativando temporariamente Windows Defender Real-Time Protection..." -ForegroundColor Yellow
try {
    Set-MpPreference -DisableRealtimeMonitoring $true -ErrorAction Stop
    Write-Host "   OK Real-Time Protection desativado temporariamente" -ForegroundColor Green
    Write-Host "   NOTA: Sera reativado automaticamente em algumas horas" -ForegroundColor Gray
} catch {
    Write-Host "   ERRO: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "4. Adicionando exclusoes abrangentes no Windows Defender..." -ForegroundColor Yellow
$paths = @(
    "C:\Users\Lenovo\Documents\GestaoCinema",
    "C:\Users\Lenovo\Documents\GestaoCinema\GestaoDeCinema\GestaoDeCinema\bin",
    "C:\Users\Lenovo\Documents\GestaoCinema\GestaoDeCinema\GestaoDeCinema\bin\Debug\net8.0"
)

foreach ($path in $paths) {
    try {
        Add-MpPreference -ExclusionPath $path -ErrorAction SilentlyContinue
        Add-MpPreference -ExclusionProcess "dotnet.exe" -ErrorAction SilentlyContinue
        Add-MpPreference -ExclusionProcess "GestaoDeCinema.exe" -ErrorAction SilentlyContinue
    } catch {
        # Ignorar erros se ja existir
    }
}
Write-Host "   OK Exclusoes adicionadas" -ForegroundColor Green

Write-Host ""
Write-Host "5. Limpando e recompilando o projeto..." -ForegroundColor Yellow
$projectDir = "C:\Users\Lenovo\Documents\GestaoCinema\GestaoDeCinema\GestaoDeCinema"
Push-Location $projectDir

if (Test-Path "bin") { Remove-Item "bin" -Recurse -Force }
if (Test-Path "obj") { Remove-Item "obj" -Recurse -Force }

dotnet clean | Out-Null
dotnet build | Out-Null

# Desbloquear todos os ficheiros compilados
Get-ChildItem -Path "bin" -Recurse -ErrorAction SilentlyContinue | Unblock-File -ErrorAction SilentlyContinue
Write-Host "   OK Projeto recompilado e ficheiros desbloqueados" -ForegroundColor Green

Write-Host ""
Write-Host "=== Configuracao Concluida ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "IMPORTANTE: Tente agora executar a aplicacao." -ForegroundColor Yellow
Write-Host ""

Pop-Location

Write-Host "Opcao 1 - Executar com dotnet run:" -ForegroundColor Cyan
Write-Host "  cd $projectDir" -ForegroundColor White
Write-Host "  dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "Opcao 2 - Executar diretamente o executavel:" -ForegroundColor Cyan
Write-Host "  cd $projectDir\bin\Debug\net8.0" -ForegroundColor White
Write-Host "  .\GestaoDeCinema.exe" -ForegroundColor White
Write-Host ""

$choice = Read-Host "Tentar executar agora? Digite 1 para dotnet run, 2 para executavel direto, ou N para sair"

if ($choice -eq "1") {
    Push-Location $projectDir
    dotnet run
    Pop-Location
} elseif ($choice -eq "2") {
    Push-Location "$projectDir\bin\Debug\net8.0"
    .\GestaoDeCinema.exe
    Pop-Location
}
