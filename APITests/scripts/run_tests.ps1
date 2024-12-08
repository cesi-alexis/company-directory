# Configuration
$testProjectPath = "../APITests/APITests.csproj"

# Exécuter les tests
Write-Host "Running tests..."
dotnet test $testProjectPath

Write-Host "Tests completed."
