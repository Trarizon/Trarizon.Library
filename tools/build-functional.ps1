param(
    [string]$GeneratorProj = "./src/Trarizon.Library.Functional.Generators/Trarizon.Library.Functional.Generators.csproj",
    [string]$RuntimeProj = "./src/Trarizon.Library.Functional/Trarizon.Library.Functional.csproj"
)

Write-Host "=== Release Start ==="

# # 读取版本号
# [xml]$genXml = Get-Content $GeneratorProj
# $oldVersion = $genXml.Project.PropertyGroup.Version

# Write-Host "当前版本: $oldVersion"

# # 自动递增 suffix 版本号 (e.g. 1.0.5-alpha.6 -> 1.0.5-alpha.7)
# if ($oldVersion -match '^(.+-\w+\.)(\d+)$') {
#     $prefix = $Matches[1]
#     $suffix = [int]$Matches[2] + 1
#     $newVersion = "$prefix$suffix"
# }
# else {
#     throw "版本号格式不正确，期望格式: X.Y.Z-suffix.N, 实际: $oldVersion"
# }

# Write-Host "新版本: $newVersion"

# # 写回 Generator 的 csproj
# $genXml.Project.PropertyGroup.Version = $newVersion
# $genXml.Save($GeneratorProj)

# # 同步更新 Runtime 的 csproj
# [xml]$rtXml = Get-Content $RuntimeProj
# $rtXml.Project.PropertyGroup.Version = $newVersion
# $rtXml.Save($RuntimeProj)

# Write-Host "版本号已更新"

# 构建源生成器
dotnet build $GeneratorProj -c Release
if ($LASTEXITCODE -ne 0) { throw "Generators project build failed." }

# 构建 runtime 库
dotnet pack $RuntimeProj -c Release
if ($LASTEXITCODE -ne 0) { throw "Runtime project pack failed." }

Write-Host "=== Release End ==="