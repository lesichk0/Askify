# Build script for Askify project
Write-Host "Building Askify solution..." -ForegroundColor Green

# Clean the solution first
dotnet clean Askify.sln
if ($LASTEXITCODE -ne 0) {
    Write-Host "Clean failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Restore packages
Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore Askify.sln
if ($LASTEXITCODE -ne 0) {
    Write-Host "Restore failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Build solution
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build Askify.sln --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Build completed successfully!" -ForegroundColor Green

# Ask if the user wants to run the backend
$runBackend = Read-Host "Do you want to run the backend API? (y/n)"
if ($runBackend -eq 'y') {
    Write-Host "Starting Askify.WebAPI..." -ForegroundColor Cyan
    Start-Process -FilePath "dotnet" -ArgumentList "run --project Askify.WebAPI/Askify.WebAPI.csproj" -NoNewWindow
}

# Ask if the user wants to run the frontend
$runFrontend = Read-Host "Do you want to run the frontend? (y/n)"
if ($runFrontend -eq 'y') {
    Write-Host "Starting frontend..." -ForegroundColor Cyan
    Set-Location frontend
    npm run dev
}
