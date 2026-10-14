$ErrorActionPreference = 'Stop'

Push-Location $PSScriptRoot
try {
    Remove-Item -Recurse -Force ./TestResults, ./coveragereport -ErrorAction SilentlyContinue

    dotnet test --collect "XPlat Code Coverage" --settings coverlet.runsettings --results-directory ./TestResults
    if ($LASTEXITCODE -ne 0) { throw "dotnet test falhou (codigo $LASTEXITCODE)." }

    reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:"Html;TextSummary"
    if ($LASTEXITCODE -ne 0) { throw "reportgenerator falhou (codigo $LASTEXITCODE)." }

    Get-Content ./coveragereport/Summary.txt -TotalCount 20
}
finally {
    Pop-Location
}
