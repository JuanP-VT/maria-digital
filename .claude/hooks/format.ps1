# PostToolUse hook: format files Claude just edited. Never blocks (always exits 0).
$ErrorActionPreference = 'SilentlyContinue'
$payload = [Console]::In.ReadToEnd() | ConvertFrom-Json
$file = $payload.tool_input.file_path
if (-not $file -or -not (Test-Path -LiteralPath $file)) { exit 0 }

$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$full = (Resolve-Path -LiteralPath $file).Path
$rel = $full.Substring($root.Length).TrimStart([char]92, '/').Replace([string][char]92, '/')

# CSharpier: only the Tests project, matching the CI check (older app code predates CSharpier).
if ($rel -like 'backend/GuadalupeDigital.Tests/*.cs') {
	Push-Location (Join-Path $root 'backend')
	dotnet csharpier format $full | Out-Null
	Pop-Location
}
# ESLint: frontend source, excluding generated shadcn components.
elseif ($rel -match '^frontend/src/.*\.tsx?$' -and $rel -notlike 'frontend/src/components/ui/*') {
	Push-Location (Join-Path $root 'frontend')
	npx eslint --fix $full | Out-Null
	Pop-Location
}
exit 0
