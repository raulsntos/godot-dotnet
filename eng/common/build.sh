#!/usr/bin/env bash

# Stop script if unbound variable found (use ${var:-} if intentional)
set -u

# Stop script if command returns non-zero exit code.
# Prevents hidden errors caused by missing error code propagation.
set -e

usage()
{
  echo "Common settings:"
  echo "  --configuration <value>    Build configuration: 'Debug' or 'Release' (short: -c)"
  echo "  --verbosity <value>        MSBuild verbosity: q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic] (short: -v)"
  echo "  --binaryLog                Create MSBuild binary log (short: -bl)"
  echo "  --help                     Print help and exit (short: -h)"
  echo ""

  echo "Actions:"
  echo "  --restore                  Restore dependencies (short: -r)"
  echo "  --build                    Build solution (short: -b)"
  echo "  --rebuild                  Rebuild solution"
  echo "  --test                     Run all unit tests in the solution (short: -t)"
  echo "  --generate                 Generate Godot bindings (short: -g)"
  echo "  --pack                     Package build outputs into NuGet packages"
  echo "  --publish                  Publish artifacts (e.g. packages, symbols)"
  echo "  --clean                    Clean the solution"
  echo "  --productBuild             Build the solution in the way it will be built for distribution (short: -pb)"
  echo "                             Will additionally trigger the following actions: --restore, --build, --pack"
  echo "                             If --configuration is not set explicitly, will also set it to 'Release'"
  echo "  --pushNupkgsLocal          Local NuGet feed directory to publish assets to"
  echo "                             Will additionally trigger the following actions: --publish"
  echo ""

  echo "Advanced settings:"
  echo "  --projects <value>         Semi-colon delimited list of sln/proj's to build. Globbing is supported (*.sln)"
  echo "  --ci                       Set when running on CI server"
  echo "  --excludeCIBinarylog       Don't output binary log (short: -nobl)"
  echo "  --nodeReuse <value>        Sets nodereuse msbuild parameter ('true' or 'false')"
  echo "  --warnAsError <value>      Sets warnaserror msbuild parameter ('true' or 'false')"
  echo ""
  echo "Command line arguments not listed above are passed thru to msbuild."
  echo "Arguments can also be passed in with a single hyphen."
}

source="${BASH_SOURCE[0]}"

# resolve $source until the file is no longer a symlink
while [[ -h "$source" ]]; do
  scriptroot="$( cd -P "$( dirname "$source" )" && pwd )"
  source="$(readlink "$source")"
  # if $source was a relative symlink, we need to resolve it relative to the path where the
  # symlink file was located
  [[ $source != /* ]] && source="$scriptroot/$source"
done
scriptroot="$( cd -P "$( dirname "$source" )" && pwd )"

properties=''
while [[ $# > 0 ]]; do
  opt="$(echo "${1/#--/-}" | tr "[:upper:]" "[:lower:]")"
  case "$opt" in
    -help|-h)
      usage
      exit 0
      ;;
    -clean)
      clean=true
      ;;
    -configuration|-c)
      configuration=$2
      shift
      ;;
    -verbosity|-v)
      verbosity=$2
      shift
      ;;
    -binarylog|-bl)
      binary_log=true
      ;;
    -excludecibinarylog|-nobl)
      exclude_ci_binary_log=true
      ;;
    -pipelineslog|-pl)
      pipelines_log=true
      ;;
    -restore|-r)
      restore=true
      ;;
    -build|-b)
      build=true
      ;;
    -rebuild)
      rebuild=true
      ;;
    -pack)
      pack=true
      ;;
    -publish)
      publish=true
      ;;
    -productBuild|-pb)
      product_build=true
      ;;
    -pushNupkgsLocal)
      push_nupkgs_local=$2
      shift
      ;;
    -test|-t)
      test=true
      ;;
    -generate|-g)
      generate=true
      ;;
    -projects)
      projects=$2
      shift
      ;;
    -ci)
      ci=true
      ;;
    -warnaserror)
      warn_as_error=$2
      shift
      ;;
    -nodereuse)
      node_reuse=$2
      shift
      ;;
    *)
      properties="$properties $1"
      ;;
  esac

  shift
done

args=("$scriptroot/build.py")

if [[ -n "${configuration:-}" ]]; then
  args+=("--configuration=$configuration")
fi
if [[ -n "${verbosity:-}" ]]; then
  args+=("--verbosity=$verbosity")
fi
if [[ -n "${binary_log:-}" ]]; then
  args+=("--binaryLog")
fi
if [[ -n "${restore:-}" ]]; then
  args+=("--restore")
fi
if [[ -n "${build:-}" ]]; then
  args+=("--build")
fi
if [[ -n "${rebuild:-}" ]]; then
  args+=("--rebuild")
fi
if [[ -n "${test:-}" ]]; then
  args+=("--test")
fi
if [[ -n "${generate:-}" ]]; then
  args+=("--generate")
fi
if [[ -n "${pack:-}" ]]; then
  args+=("--pack")
fi
if [[ -n "${publish:-}" ]]; then
  args+=("--publish")
fi
if [[ -n "${clean:-}" ]]; then
  args+=("--clean")
fi
if [[ -n "${product_build:-}" ]]; then
  args+=("--productBuild")
fi
if [[ -n "${push_nupkgs_local:-}" ]]; then
  args+=("--pushNupkgsLocal=$push_nupkgs_local")
fi
if [[ -n "${projects:-}" ]]; then
  args+=("--projects=$projects")
fi
if [[ -n "${ci:-}" ]]; then
  args+=("--ci")
fi
if [[ -n "${exclude_ci_binary_log:-}" ]]; then
  args+=("--excludeCIBinarylog")
fi
if [[ -n "${node_reuse:-}" ]]; then
  args+=("--nodeReuse=$node_reuse")
fi
if [[ -n "${warn_as_error:-}" ]]; then
  args+=("--warnAsError=$warn_as_error")
fi
if [[ -n "$properties" ]]; then
  args+=("$properties")
fi

python ${args[@]}
