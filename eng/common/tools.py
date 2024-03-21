#!/usr/bin/python3

import os
import json
import stat
import subprocess
import shutil
import urllib.request
from argparse import Namespace
from typing import Union
from typing import List
from typing import TypeVar

import visual_studio


# Projects to build.
projects: List[str] = []

# CI mode - set to true on CI server for PR validation build or official build.
ci: bool = False

# Build configuration. Common values include 'Debug' and 'Release', but the repository may use other names.
configuration: str = "Debug"

# Set to true to opt out of outputting binary log while running in CI.
exclude_ci_binary_log: bool = False

# Set to true to output binary log from msbuild. Note that emitting binary log slows down the build.
binary_log: bool = False

# True to restore toolsets and dependencies.
restore: bool = True

# True to build the projects.
build: bool = False

# True to rebuild (clean + build) the projects.
rebuild: bool = False

# True to run the test projects.
test: bool = False

# True to package build outputs into NuGet packages.
pack: bool = False

# True to publish output artifacts (e.g.: packages, symbols).
publish: bool = False

# True to clean the build artifacts.
clean: bool = False

# Adjusts msbuild verbosity level.
verbosity: str = "minimal"

# Set to true to reuse msbuild nodes. Recommended to not reuse on CI.
node_reuse: bool = True

# Configures warning treatment in msbuild.
warn_as_error: bool = True

# Specifies which msbuild engine to use for build: 'vs', 'dotnet' or unspecified (determined based on presence of tools.vs in global.json).
msbuild_engine: Union[str, None] = None

# True to attempt using .NET Core already that meets requirements specified in global.json
# installed on the machine instead of downloading one.
use_installed_dotnet_cli: bool = True

# Enable repos to use a particular version of the on-line dotnet-install scripts.
#    default URL: https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh
dotnet_install_script_version: str = "v1"

# True to use global NuGet cache instead of restoring packages to repository-local directory.
use_global_nuget_cache: bool = True

# True to exclude prerelease versions Visual Studio during build.
exclude_prerelease_vs: bool = False

# True if the build is a product build.
product_build: bool = False

# Name of the local NuGet
push_nupkgs_local: Union[str, None] = None

repo_root: str
eng_root: str
artifacts_dir: str
toolset_dir: str
tools_dir: str
log_dir: str
temp_dir: str

global_json: Namespace


def init(args: Namespace) -> None:
    global projects, ci, configuration, exclude_ci_binary_log, binary_log, restore, build, rebuild, test, pack, publish, clean, verbosity, node_reuse, warn_as_error, msbuild_engine, use_global_nuget_cache, exclude_prerelease_vs, product_build, push_nupkgs_local, repo_root, eng_root, artifacts_dir, toolset_dir, tools_dir, log_dir, temp_dir, global_json

    # Initialize variables if they aren't already defined.
    projects = args.projects.split(";") if args.projects else []
    ci = _get_value_or_default(args.ci, False)
    configuration = _get_value_or_default(args.configuration, "Debug")
    exclude_ci_binary_log = _get_value_or_default(args.excludeCIBinarylog, False)
    binary_log = _get_value_or_default(args.binaryLog, ci and not exclude_ci_binary_log)
    restore = _get_value_or_default(args.restore, True)
    build = _get_value_or_default(args.build, False)
    rebuild = _get_value_or_default(args.rebuild, False)
    test = _get_value_or_default(args.test, False)
    pack = _get_value_or_default(args.pack, False)
    publish = _get_value_or_default(args.publish, False)
    clean = _get_value_or_default(args.clean, False)
    product_build = _get_value_or_default(args.productBuild, False)
    push_nupkgs_local = _get_value_or_default(args.pushNupkgsLocal, None)
    verbosity = _get_value_or_default(args.verbosity, "minimal")
    node_reuse = _get_value_or_default(args.nodeReuse, not ci)
    warn_as_error = _get_value_or_default(args.warnAsError, True)
    use_global_nuget_cache = not ci
    if os.name == "nt":
        msbuild_engine = _get_value_or_default(args.msbuildEngine, None)
        exclude_prerelease_vs = _get_value_or_default(args.excludePrereleaseVS, False)

    if product_build:
        # A product build also implies build, restore, and pack.
        build = True
        restore = True
        pack = True

        # Default configuration for product builds should be 'Release'.
        if not args.configuration:
            configuration = "Release"

    if push_nupkgs_local:
        # A local NuGet feed publishing directory also implies publish.
        publish = True

        # Ensure the path is absolute.
        push_nupkgs_local = os.path.abspath(push_nupkgs_local)

    # Initialize variables for common directories.
    script_dir = os.path.dirname(__file__)
    repo_root = os.path.abspath(os.path.join(script_dir, os.pardir, os.pardir)) + os.path.sep
    eng_root = os.path.abspath(os.path.join(script_dir, os.pardir))
    artifacts_dir = os.path.join(repo_root, "artifacts")
    toolset_dir = os.path.join(artifacts_dir, "toolset")
    tools_dir = os.path.join(repo_root, ".tools")
    log_dir = os.path.join(artifacts_dir, "log", configuration)
    temp_dir = os.path.join(artifacts_dir, "tmp", configuration)

    # HOME may not be defined in some scenarios, but it is required by NuGet.
    if not os.getenv("HOME"):
        os.environ["HOME"] = os.path.join(artifacts_dir, ".home")
        os.makedirs(os.environ["HOME"], exist_ok=True)

    os.makedirs(toolset_dir, exist_ok=True)
    os.makedirs(temp_dir, exist_ok=True)
    os.makedirs(log_dir, exist_ok=True)

    global_json_file = os.path.join(repo_root, "global.json")
    with open(global_json_file) as f:
        global_json = json.load(f, object_hook=lambda x: Namespace(**x))


T = TypeVar("T")
def _get_value_or_default(value: Union[T, None], default: T) -> T:
    if value is None:
        return default

    return value


_dotnet_install_dir: str = ""

def initialize_dotnet_cli(install: bool = True) -> str:
    global _dotnet_install_dir
    if _dotnet_install_dir:
        return _dotnet_install_dir

    # Don't resolve runtime, shared framework, or SDK from other locations to ensure build determinism.
    os.environ["DOTNET_MULTILEVEL_LOOKUP"] = "0"

    # Disable first run since we want to control all package sources.
    os.environ["DOTNET_NOLOGO"] = "1"

    # Disable telemetry.
    os.environ["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1"

    if os.name != "nt":
        # LTTNG is the logging infrastructure used by Core CLR. Need this variable set
        # so it doesn't output warnings to the console.
        os.environ["LTTNG_HOME"] = os.environ["HOME"]

    # Find the first path on PATH that contains the dotnet CLI.
    if (use_installed_dotnet_cli and not ("runtimes" in global_json.tools) and not os.getenv("DOTNET_INSTALL_DIR")):
        dotnet_path = shutil.which("dotnet")
        dotnet_path = os.path.realpath(dotnet_path)
        os.environ["DOTNET_INSTALL_DIR"] = os.path.dirname(dotnet_path)

    dotnet_sdk_version = global_json.tools.dotnet

    # Use dotnet installation specified in DOTNET_INSTALL_DIR if it contains the required SDK version,
    # otherwise install the dotnet CLI and SDK to repo local .dotnet directory to avoid potential permission issues.
    if not ("runtimes" in global_json.tools) and os.getenv("DOTNET_INSTALL_DIR") and os.path.isdir(os.path.join(os.getenv("DOTNET_INSTALL_DIR"), "sdk", dotnet_sdk_version)):
        dotnet_root = os.environ["DOTNET_INSTALL_DIR"]
    else:
        dotnet_root = os.path.join(repo_root, ".dotnet")

        os.environ["DOTNET_INSTALL_DIR"] = dotnet_root

        if not os.path.isdir(os.path.join(os.environ["DOTNET_INSTALL_DIR"], "sdk", dotnet_sdk_version)):
            if (install):
                install_dotnet_sdk(dotnet_root, dotnet_sdk_version)
            else:
                pipeline_write_error("InitializeToolset", f"Unable to find dotnet with SDK version: {dotnet_sdk_version}")
                exit(1)

    # Add dotnet to PATH. This prevents any bare invocation of dotnet in custom
    # build steps from using anything other than what we've downloaded.
    path = os.environ["PATH"]
    os.environ["PATH"] = f"{dotnet_root}{os.pathsep}{path}"

    _dotnet_install_dir = dotnet_root
    return _dotnet_install_dir


def install_dotnet_sdk(dotnet_root: str, version: str, architecture: str = "", no_path: bool = False) -> bool:
    return install_dotnet(dotnet_root, version, architecture, "", os.name != "nt", no_path)


def install_dotnet(dotnet_root: str, version: str, architecture: str = "", runtime: str = "", skip_non_versioned_files = False, no_path: bool = False) -> bool:
    dotnet_version_label = f"'sdk v{version}'"

    if runtime != "" and runtime != "sdk":
        runtime_path = dotnet_root
        runtime_path = os.path.join(runtime_path, "shared")
        if runtime == "dotnet":
            runtime_path = os.path.join(runtime_path, "Microsoft.NETCore.App")
        if runtime == "aspnetcore":
            runtime_path = os.path.join(runtime_path, "Microsoft.AspNetCore.App")
        if runtime == "windowsdesktop":
            runtime_path = os.path.join(runtime_path, "Microsoft.WindowsDesktop.App")
        runtime_path = os.path.join(runtime_path, version)

        dotnet_version_label = f"runtime toolset '{runtime}/{architecture} v{version}'"

        if os.path.exists(runtime_path):
            print(f"{dotnet_version_label} already installed.", flush=True)
            return True

    install_script = get_dotnet_install_script(dotnet_root)
    install_parameters = {
        "version": version,
        "install_dir": dotnet_root,
    }

    if architecture and architecture != "unset":
        install_parameters["architecture"] = architecture
    if runtime and runtime != "sdk":
        install_parameters["runtime"] = runtime
    if skip_non_versioned_files:
        install_parameters["skip_non_versioned_files"] = skip_non_versioned_files
    if no_path:
        install_parameters["no_path"] = True

    variations: List[dict] = []
    variations.append(install_parameters)

    dotnet_builds = install_parameters.copy()
    dotnet_builds["azure_feed"] = "https://dotnetbuilds.azureedge.net/public"
    variations.append(dotnet_builds)

    install_success = False

    for variation in variations:
        variation_name = variation.get("azure_feed", "public location")
        print(f"Attempting to install {dotnet_version_label} from {variation_name}.", flush=True)

        # Convert variation parameters to script arguments.
        args = []
        args += ["--version" if os.name != "nt" else "-Version", variation["version"]]
        args += ["--install-dir" if os.name != "nt" else "-InstallDir", variation["install_dir"]]
        if variation.get("architecture"):
            args += ["--architecture" if os.name != "nt" else "-Architecture", variation["architecture"]]
        if variation.get("runtime"):
            args += ["--runtime" if os.name != "nt" else "-Runtime", variation["runtime"]]
        if variation.get("skip_non_versioned_files"):
            args += ["--skip-non-versioned-files" if os.name != "nt" else "-SkipNonVersionedFiles"]
        if variation.get("no_path"):
            args += ["--no-path" if os.name != "nt" else "-NoPath"]
        if variation.get("azure_feed"):
            args += ["--azure-feed" if os.name != "nt" else "-AzureFeed", variation["azure_feed"]]

        if os.name != "nt":
            exit_code = subprocess.call([install_script, *args])
        else:
            exit_code = subprocess.call(["powershell.exe", install_script, *args])
        if exit_code == 0:
            install_success = True
            break

        print(f"Failed to install {dotnet_version_label} from {variation_name}.", flush=True)

    if not install_success:
        pipeline_write_error("InitializeToolset", f"Failed to install {dotnet_version_label} from any of the specified locations.")

    return install_success


def get_dotnet_install_script(dotnet_root: str) -> str:
    install_script_name = "dotnet-install.sh" if os.name != "nt" else "dotnet-install.ps1"
    install_script = os.path.join(dotnet_root, install_script_name)
    install_script_url = f"https://dotnet.microsoft.com/download/dotnet/scripts/{dotnet_install_script_version}/{install_script_name}"

    if not os.path.exists(install_script):
        os.makedirs(dotnet_root, exist_ok=True)

        print(f"Downloading '{install_script_url}'", flush=True)

        urllib.request.urlretrieve(install_script_url, install_script)

        # Ensure the script has executable permissions.
        st = os.stat(install_script)
        os.chmod(install_script, st.st_mode | stat.S_IEXEC)

    return install_script


class BuildTool:
    path: str
    command: str
    tool: str
    exclude_prerelease_vs: bool

    def __init__(self, path: str, command: str, tool: str, exclude_prerelease_vs: bool = False):
        self.path = path
        self.command = command
        self.tool = tool
        self.exclude_prerelease_vs = exclude_prerelease_vs

_build_tool: Union[BuildTool, None] = None

def initialize_build_tool() -> BuildTool:
    if os.name == "nt":
        return initialize_build_tool_windows()
    else:
        return initialize_build_tool_unix()


def initialize_build_tool_unix() -> BuildTool:
    global _build_tool
    if _build_tool:
            return _build_tool

    dotnet_root = initialize_dotnet_cli(restore)

    _build_tool = BuildTool(
        path = f"{dotnet_root}/dotnet",
        command = "msbuild",
        tool = "dotnet",
    )
    return _build_tool


_msbuild_engine: str = ""

def initialize_build_tool_windows() -> BuildTool:
    global _build_tool
    if _build_tool:
        # If the requested msbuild parameters do not match, clear the cached variable.
        if _build_tool.exclude_prerelease_vs != exclude_prerelease_vs:
            _build_tool = None
            visual_studio._msbuild_exe = None
        else:
            return _build_tool

    global _msbuild_engine
    if not _msbuild_engine:
        _msbuild_engine = _get_default_msbuild_engine()

    # Initialize dotnet CLI if listed in 'tools'.
    dotnet_root = None
    if "dotnet" in global_json.tools:
        dotnet_root = initialize_dotnet_cli(restore)

    if _msbuild_engine == "dotnet":
        if not dotnet_root:
            pipeline_write_error("InitializeToolset", "/global.json must specify 'tools.dotnet'.")
            exit(1)
        dotnet_path = os.path.join(dotnet_root, "dotnet.exe")

        _build_tool = BuildTool(
            path = dotnet_path,
            command = "msbuild",
            tool = "dotnet",
        )
    elif _msbuild_engine == "vs":
        try:
            msbuild_path = visual_studio.initialize_visual_studio_msbuild(restore)
        except Exception as e:
            pipeline_write_error("InitializeToolset", e)
            exit(1)

        _build_tool = BuildTool(
            path = msbuild_path,
            command = "",
            tool = "vs",
            exclude_prerelease_vs = exclude_prerelease_vs,
        )
    else:
        pipeline_write_error("InitializeToolset", f"Unexpected value of -msbuildEngine: '{msbuild_engine}'.")
        exit(1)

    return _build_tool


def _get_default_msbuild_engine() -> str:
    # Presence of tools.vs indicates the repo needs to build using VS msbuild on Windows.
    if "vs" in global_json.tools:
        return "vs"
    elif "dotnet" in global_json.tools:
        return "dotnet"

    pipeline_write_error("InitializeToolset", "-msbuildEngine must be specified, or /global.json must specify 'tools.dotnet' or 'tools.vs'.")
    return ""


def initialize_toolset() -> str:
    current_dir = os.path.dirname(os.path.abspath(__file__))
    return os.path.join(current_dir, "tools", "Build.proj")


# Executes msbuild (or 'dotnet msbuild') with arguments passed to the function.
# The arguments are automatically quoted.
# Terminates the script if the build fails.
def msbuild(args: List[str]) -> None:
    build_tool = initialize_build_tool()

    if ci:
        if build_tool.tool == "dotnet":
            # If CI flag is set, turn on special environment variables for improved NuGet client retry logic.
            print("Setting NUGET enhanced retry environment variables.", flush=True)

            os.environ["NUGET_ENABLE_ENHANCED_HTTP_RETRY"] = "true"
            os.environ["NUGET_ENHANCED_MAX_NETWORK_TRY_COUNT"] = "6"
            os.environ["NUGET_ENHANCED_NETWORK_RETRY_DELAY_MILLISECONDS"] = "1000"
            os.environ["NUGET_RETRY_HTTP_429"] = "true"

            os.environ["NUGET_PLUGIN_HANDSHAKE_TIMEOUT_IN_SECONDS"] = "20"
            os.environ["NUGET_PLUGIN_REQUEST_TIMEOUT_IN_SECONDS"] = "20"

        if not binary_log and not exclude_ci_binary_log:
            pipeline_write_error("Build", "Binary log must be enabled in CI build, or explicitly opted-out from with the -noBinaryLog switch.")
            exit(1)

        if node_reuse:
            pipeline_write_error("Build", "Node reuse must be disabled in CI build.")
            exit(1)

    build_args = [
        "/maxCpuCount",
        "/nologo",
        "/consoleLoggerParameters:Summary",
        f"/verbosity:{verbosity}",
        f"/nodeReuse:{node_reuse}",
        f"/p:ContinuousIntegrationBuild={ci}",
        f"/p:TreatWarningsAsErrors={warn_as_error}",
    ]

    if warn_as_error:
        build_args.append("/warnAsError")

    if push_nupkgs_local:
        build_args.append(f"/p:OutputBlobFeedDir={push_nupkgs_local}")

    exit_code = subprocess.call([build_tool.tool, build_tool.command, *build_args, *args])
    if exit_code != 0:
        # We should not pipeline_write_error here because that message shows up in the build summary
        # The build already logged an error, that's the reason it failed. Producing an error here only adds noise.
        print(f"Build failed with exit code {exit_code}. Check errors above.", flush=True)
        exit(exit_code)


# Print an error in GitHub Actions pipeline.
def pipeline_write_error(title: str, value: str):
    if ci:
        print(f"::error title={title}::{value}", flush=True)
