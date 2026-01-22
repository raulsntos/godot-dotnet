#!/usr/bin/python3

"""
This is the main entry-point for the build script, the build.sh and build.ps1
scripts are just wrappers that call this script. The reason is to avoid code
duplication for the different platforms.

The build.sh and build.ps1 are still kept as separate scripts because I wanted
to keep the same argument parsing behavior that matches the Arcade SDK and
I couldn't find a way to achieve the same behavior with Python directly.
"""

import os
import shutil
import sys
import signal
import argparse
from argparse import Namespace
from typing import List
from typing import Tuple

import tools


# Silence traceback on Ctrl-C.
signal.signal(signal.SIGINT, lambda x, y: sys.exit(1))


def _parse_args() -> Tuple[Namespace, List[str]]:
    parser = argparse.ArgumentParser(add_help=False)

    # Common settings.
    parser.add_argument("--configuration", "-c")
    if os.name == "nt":
        parser.add_argument("--platform")
    parser.add_argument("--verbosity", "-v")
    parser.add_argument("--binaryLog", "-bl", action="store_true", default=None)
    parser.add_argument("--help", "-h", action="store_true", default=None)

    # Actions.
    parser.add_argument("--restore", "-r", action="store_true", default=None)
    parser.add_argument("--build", "-b", action="store_true", default=None)
    parser.add_argument("--rebuild", action="store_true", default=None)
    parser.add_argument("--test", "-t", action="store_true", default=None)
    parser.add_argument("--generate", "-g", action="store_true", default=None)
    parser.add_argument("--pack", action="store_true", default=None)
    parser.add_argument("--publish", action="store_true", default=None)
    parser.add_argument("--clean", action="store_true", default=None)
    parser.add_argument("--productBuild", "-pb", action="store_true", default=None)
    parser.add_argument("--pushNupkgsLocal")

    # Advanced settings.
    parser.add_argument("--projects")
    parser.add_argument("--ci", action="store_true", default=None)
    parser.add_argument("--excludeCIBinarylog", "-nobl", action="store_true", default=None)
    parser.add_argument("--nodeReuse", type=lambda x: (str(x).lower() == "true"))
    parser.add_argument("--warnAsError", type=lambda x: (str(x).lower() == "true"))
    if os.name == "nt":
        parser.add_argument("--msbuildEngine")
        parser.add_argument("--excludePrereleaseVS")

    return parser.parse_known_args()


def build(unknown_args: List[str]):
    toolset = tools.initialize_toolset()

    build_args = []

    if tools.projects:
        # Resolve relative project paths into full paths.
        projects = tools.projects
        projects = list(map(os.path.abspath, projects))
        projects_str = ";".join(projects)
        build_args.append(f"/p:Projects={projects_str}")

    if tools.binary_log:
        build_args.append(f'/bl:"{tools.log_dir}/Build.binlog"')

    build_args += [
        f"/p:Configuration={tools.configuration}",
        f"/p:RepoRoot={tools.repo_root}",
        f"/p:Restore={tools.restore}",
        f"/p:Build={tools.build}",
        f"/p:Rebuild={tools.rebuild}",
        f"/p:Test={tools.test}",
        f"/p:GenerateGodotBindings={tools.generate}",
        f"/p:Pack={tools.pack}",
        f"/p:Publish={tools.publish}",
        f"/p:ProductBuild={tools.product_build}",
    ]

    tools.msbuild([toolset, *build_args, *unknown_args])


def clean():
    if os.path.exists(tools.artifacts_dir):
        shutil.rmtree(tools.artifacts_dir)
        print("Artifacts directory deleted.", flush=True)
    exit(0)


def main():
    args, unknown_args = _parse_args()
    tools.init(args)

    if args.clean:
        clean()

    build(unknown_args)


if __name__ == "__main__":
    main()
