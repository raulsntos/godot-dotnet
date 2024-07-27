[CmdletBinding(PositionalBinding = $false)]
Param(
  [string][Alias('c')] $configuration = $null,
  [string] $platform = $null,
  [string] $projects,
  [string][Alias('v')] $verbosity = "minimal",
  [string] $msbuildEngine = $null,
  [bool] $warnAsError = $true,
  [bool] $nodeReuse = $true,
  [switch][Alias('r')] $restore,
  [switch][Alias('b')] $build,
  [switch] $rebuild,
  [switch][Alias('t')] $test,
  [switch][Alias('g')] $generate,
  [switch] $pack,
  [switch] $publish,
  [switch] $clean,
  [switch][Alias('pb')] $productBuild,
  [string] $pushNupkgsLocal = $null,
  [switch][Alias('bl')] $binaryLog,
  [switch][Alias('nobl')] $excludeCIBinarylog,
  [switch] $ci,
  [switch] $excludePrereleaseVS,
  [switch] $help,
  [Parameter(ValueFromRemainingArguments = $true)][String[]] $properties
)

# Unset 'Platform' environment variable to avoid unwanted collision in InstallDotNetCore.targets file
# some computer has this env var defined (e.g. Some HP)
if ($env:Platform) {
  $env:Platform = ""
}
function Print-Usage() {
  Write-Host "Common settings:"
  Write-Host "  -configuration <value>  Build configuration: 'Debug' or 'Release' (short: -c)"
  Write-Host "  -platform <value>       Platform configuration: 'x86', 'x64' or any valid Platform value to pass to msbuild"
  Write-Host "  -verbosity <value>      MSBuild verbosity: q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic] (short: -v)"
  Write-Host "  -binaryLog              Output binary log (short: -bl)"
  Write-Host "  -help                   Print help and exit"
  Write-Host ""

  Write-Host "Actions:"
  Write-Host "  -restore                Restore dependencies (short: -r)"
  Write-Host "  -build                  Build solution (short: -b)"
  Write-Host "  -rebuild                Rebuild solution"
  Write-Host "  -test                   Run all unit tests in the solution (short: -t)"
  Write-Host "  -generate               Generate Godot bindings (short: -g)"
  Write-Host "  -pack                   Package build outputs into NuGet packages"
  Write-Host "  -clean                  Clean the solution"
  Write-Host "  -publish                Publish artifacts (e.g. packages, symbols)"
  Write-Host "  -productBuild           Build the solution in the way it will be built for distribution (short: -pb)"
  Write-Host "                          Will additionally trigger the following actions: -restore, -build, -pack"
  Write-Host "                          If -configuration is not set explicitly, will also set it to 'Release'"
  Write-Host ""

  Write-Host "Advanced settings:"
  Write-Host "  -projects <value>       Semi-colon delimited list of sln/proj's to build. Globbing is supported (*.sln)"
  Write-Host "  -ci                     Set when running on CI server"
  Write-Host "  -excludeCIBinarylog     Don't output binary log (short: -nobl)"
  Write-Host "  -nodeReuse <value>      Sets nodereuse msbuild parameter ('true' or 'false')"
  Write-Host "  -warnAsError <value>    Sets warnaserror msbuild parameter ('true' or 'false')"
  Write-Host "  -msbuildEngine <value>  MSBuild engine to use to run build ('dotnet', 'vs', or unspecified)."
  Write-Host "  -excludePrereleaseVS    Set to exclude build engines in prerelease versions of Visual Studio"
  Write-Host ""

  Write-Host "Command line arguments not listed above are passed thru to msbuild."
  Write-Host "The above arguments can be shortened as much as to be unambiguous (e.g. -co for configuration, -t for test, etc.)."
}

[string[]] $_args = @()

if ($configuration) {
  $_args += @("--configuration=$configuration")
}
if ($platform) {
  $_args += @("--platform=$platform")
}
if ($verbosity) {
  $_args += @("--verbosity=$verbosity")
}
if ($binaryLog) {
  $_args += @("--binaryLog")
}
if ($restore) {
  $_args += @("--restore")
}
if ($build) {
  $_args += @("--build")
}
if ($rebuild) {
  $_args += @("--rebuild")
}
if ($test) {
  $_args += @("--test")
}
if ($generate) {
  $_args += @("--generate")
}
if ($pack) {
  $_args += @("--pack")
}
if ($publish) {
  $_args += @("--publish")
}
if ($clean) {
  $_args += @("--clean")
}
if ($productBuild) {
  $_args += @("--productBuild")
}
if ($pushNupkgsLocal) {
  $_args += @("--pushNupkgsLocal=$pushNupkgsLocal")
}
if ($projects) {
  $_args += @("--projects=$projects")
}
if ($ci) {
  $_args += @("--ci")
}
if ($excludeCIBinarylog) {
  $_args += @("--excludeCIBinarylog")
}
if (!$ci) {
  $_args += @("--nodeReuse=$nodeReuse")
}
$_args += @("--warnAsError=$warnAsError")
if ($msbuildEngine) {
  $_args += @("--msbuildEngine=$msbuildEngine")
}
if ($excludePrereleaseVS) {
  $_args += @("--excludePrereleaseVS=$excludePrereleaseVS")
}
if ($properties) {
  $_args += @($properties)
}

try {
  & python $PSScriptRoot/build.py @_args
  if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
  }
}
catch {
  exit 1
}

exit 0
