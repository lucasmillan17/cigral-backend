$testPath = "C:\Users\lucas\OneDrive\Documentos\programming_proyects\CIGRALBack\CigralBackend.Tests\CigralBackend.Tests.csproj"
Write-Host "Running tests..."
dotnet test $testPath 2>&1 | Select-String -Pattern "Resumen|error|correcto|Superado|Total" | Select-Object -Last 10
