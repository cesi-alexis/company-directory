# Configuration
$testProjectPath = "../APITests/APITests.csproj"

# Ex�cuter les tests
Write-Host "Running tests..."
dotnet test $testProjectPath

Write-Host "Tests completed."
