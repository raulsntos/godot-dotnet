#!/usr/bin/python3

import os
import shutil
import subprocess
import json
import urllib.request
from typing import Union

import tools


_msbuild_exe: Union[str, None] = None

# Locates Visual Studio MSBuild installation.
# The preference order for MSBuild to use is as follows:
#
#   1. MSBuild from an active VS command prompt
#   2. MSBuild from a compatible VS installation
#
# Returns full path to msbuild.exe.
# Throws on failure.
def initialize_visual_studio_msbuild(install: bool, vs_requirements: dict = {}) -> str:
    if os.name != "nt":
        raise Exception("Cannot initialize Visual Studio on non-Windows.")

    global _msbuild_exe
    if _msbuild_exe:
        return _msbuild_exe

    # Minimum VS version to require.
    vs_min_version_required = "17.7"

    if not vs_requirements:
        if "vs" in tools.global_json.tools:
            vs_requirements = { "version": tools.global_json.tools.vs }
        else:
            vs_requirements = { "version": vs_min_version_required }

    vs_min_version = vs_requirements["version"] if vs_requirements else vs_min_version_required

    # Try msbuild command available in the environment.
    if os.getenv("VSINSTALLDIR"):
        msbuild_cmd = shutil.which("msbuild.exe")
        if msbuild_cmd:
            msbuild_version = _get_version_from_file(msbuild_cmd)

            if _parse_version(msbuild_version) >= _parse_version(vs_min_version):
                _msbuild_exe = msbuild_cmd
                return _msbuild_exe

            # Report error - the developer environment is initialized with incompatible VS version.
            vs_version = os.getenv("VisualStudioVersion")
            raise Exception(f"Developer Command Prompt for VS {vs_version} is not recent enough. Please upgrade to {vs_min_version} or build from a plain CMD window.")

    # Locate Visual Studio installation.
    vs_info = locate_visual_studio(vs_requirements)
    if vs_info:
        # Ensure vs_install_dir has trailing slash.
        vs_install_dir = os.path.join(vs_info["installation_path"])
        vs_major_version = vs_info["installation_version"].split(".")[0]

        _initialize_visual_studio_environment_variables(vs_install_dir, vs_major_version)
    else:
        raise Exception("Unable to find Visual Studio that has required version and components installed.")

    msbuild_version_dir = f"{vs_major_version}.0" if vs_major_version < 16 else "Current"

    bin_folder = os.path.join(vs_install_dir, "MSBuild", msbuild_version_dir, "Bin")
    prefer_64bit = vs_requirements["prefer_64bit"] if "prefer_64bit" in vs_requirements else False
    if prefer_64bit and os.path.exists(os.path.join(bin_folder, "amd64")):
        _msbuild_exe = os.path.join(bin_folder, "amd64", "msbuild.exe")
    else:
        _msbuild_exe = os.path.join(bin_folder, "msbuild.exe")

    return _msbuild_exe


def _initialize_visual_studio_environment_variables(vs_install_dir: str, vs_major_version: str) -> None:
    os.environ["VSINSTALLDIR"] = vs_install_dir
    os.environ[f"VS{vs_major_version}0COMNTOOLS"] = os.path.join(vs_install_dir, "Common7", "Tools", "")

    vs_sdk_install_dir = os.path.join(vs_install_dir, "VSSDK", "")
    if os.path.exists(vs_sdk_install_dir):
        os.environ[f"VSSDK{vs_major_version}0Install"] = vs_sdk_install_dir
        os.environ["VSSDKInstall"] = vs_sdk_install_dir


# Locates Visual Studio instance that meets the minimal requirements specified by tools.vs object in global.json.
#
# The following properties of tools.vs are recognized:
#   "version": "{major}.{minor}"
#       Two part minimal VS version, e.g. "15.9", "16.0", etc.
#   "components": ["componentId1", "componentId2", ...]
#       Array of ids of workload components that must be available in the VS instance.
#       See e.g. https://docs.microsoft.com/en-us/visualstudio/install/workload-component-id-vs-enterprise?view=vs-2017
#
# Returns JSON describing the located VS instance (same format as returned by vswhere),
# or $null if no instance meeting the requirements is found on the machine.
def locate_visual_studio(vs_requirements: dict = {}) -> Union[dict, None]:
    if os.name != "nt":
        raise Exception("Cannot run vswhere on non-Windows.")

    if "vswhere" in tools.global_json.tools:
        vswhere_version = tools.global_json.tools.vswhere
    else:
        vswhere_version = "2.5.2"

    vswhere_dir = os.path.join(tools.tools_dir, "vswhere", vswhere_version)
    vswhere_exe = os.path.join(vswhere_dir, "vswhere.exe")

    if not os.path.exists(vswhere_exe):
        os.mkdir(vswhere_dir)
        print("Downloading vswhere", flush=True)
        urllib.request.urlretrieve(f"https://netcorenativeassets.blob.core.windows.net/resource-packages/external/windows/vswhere/{vswhere_version}/vswhere.exe", vswhere_exe)

    if not vs_requirements:
        vs_requirements = tools.global_json.tools.vs

    args = ["-latest", "-format", "json", "-requires", "Microsoft.Component.MSBuild", "-products", "*"]

    if not tools.exclude_prerelease_vs:
        args.append("-prerelease")

    if "version" in vs_requirements:
        args += ["-version", vs_requirements["version"]]

    if "components" in vs_requirements:
        for component in vs_requirements["components"]:
            args += ["-requires", component]

    vswhere_process = subprocess.run([vswhere_exe, *args])
    if vswhere_process.returncode != 0:
        return None

    vs_info = json.loads(vswhere_process.stdout)

    # Use first matching instance.
    return vs_info[0]


def _get_version_from_file(file_path: str) -> str:
    import win32api
    from win32api import LOWORD, HIWORD
    info = win32api.GetFileVersionInfo(file_path, "\\")
    ms = info["ProductVersionMS"]
    ls = info["ProductVersionLS"]
    return f"{HIWORD(ms)}.{LOWORD(ms)}.{HIWORD(ls)}.{LOWORD(ls)}"


class Version:
    major: int
    minor: int
    patch: int
    build: int

    def __init__(self, major: int, minor: int, patch: int = 0, build: int = 0):
        self.major = major
        self.minor = minor
        self.patch = patch
        self.build = build

    def __eq__(self, other) -> bool:
        self.major == other.major and self.minor == other.minor and self.patch == other.patch and self.build == other.build
        return True

    def __ne__(self, other) -> bool:
        return not self.__eq__(other)

    def __gt__(self, other) -> bool:
        if self.major == other.major:
            if self.minor == other.minor:
                if self.patch == other.patch:
                    return self.build > other.build
                else:
                    return self.patch > other.patch
            else:
                return self.minor > other.patch
        else:
            return self.major > other.major
    
    def __ge__(self, other) -> bool:
        return self.__gt__(other) or self.__eq__(other)

    def __lt__(self, other) -> bool:
        return not self.__gt__(other) and not self.__eq__(other)

    def __le__(self, other) -> bool:
        return self.__lt__(other) or self.__eq__(other)

def _parse_version(version_str: str) -> Version:
    parts = version_str.split(".")
    major = int(parts[0])
    minor = int(parts[1]) if len(parts) > 1 else 0
    patch = int(parts[2]) if len(parts) > 2 else 0
    build = int(parts[3]) if len(parts) > 3 else 0
    return Version(major, minor, patch, build)
