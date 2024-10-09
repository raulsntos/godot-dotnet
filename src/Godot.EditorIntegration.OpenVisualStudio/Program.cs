using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.Marshalling;
using System.Text.RegularExpressions;
using EnvDTE;

namespace Godot.EditorIntegration.OpenVisualStudio;

internal static partial class Program
{
    [LibraryImport("ole32.dll")]
    private static partial int GetRunningObjectTable(int reserved, [MarshalAs(UnmanagedType.Interface)] out IRunningObjectTable pprot);

    [LibraryImport("ole32.dll")]
    private static partial void CreateBindCtx(int reserved, [MarshalAs(UnmanagedType.Interface)] out IBindCtx ppbc);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(nint hWnd);

    private static void ShowHelp()
    {
        Console.WriteLine("""
            Opens the file(s) in a Visual Studio instance that is editing the specified solution.
            If an existing instance for the solution is not found, a new one is created.

            Usage:
              GodotTools.OpenVisualStudio.exe solution [file[;line[;col]]...]

            Lines and columns begin at one. Zero or lower will result in an error.
            If a line is specified but a column is not, the line is selected in the text editor.
            """);
    }

    // STAThread needed, otherwise CoRegisterMessageFilter may return CO_E_NOT_SUPPORTED.
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
        {
            ShowHelp();
            return 0;
        }

        string solutionFile = NormalizePath(args[0]);

        var dte = FindInstanceEditingSolution(solutionFile);

        if (dte is null)
        {
            // Open a new instance
            dte = TryVisualStudioLaunch("VisualStudio.DTE.17.0");

            if (dte is null)
            {
                Console.Error.WriteLine("Visual Studio not found");
                return 1;
            }

            dte.UserControl = true;

            try
            {
                dte.Solution.Open(solutionFile);
            }
            catch (ArgumentException)
            {
                Console.Error.WriteLine("Solution.Open: Invalid path or file not found");
                return 1;
            }

            dte.MainWindow.Visible = true;
        }

        MessageFilter.Register();

        try
        {
            // Open files.

            foreach (string arg in args)
            {
                // Both the line number and the column begin at one.

                string[] fileArgumentParts = arg.Split(';');

                string filePath = NormalizePath(fileArgumentParts[0]);

                try
                {
                    dte.ItemOperations.OpenFile(filePath);
                }
                catch (ArgumentException)
                {
                    Console.Error.WriteLine("ItemOperations.OpenFile: Invalid path or file not found");
                    return 1;
                }

                if (fileArgumentParts.Length > 1)
                {
                    if (int.TryParse(fileArgumentParts[1], out int line))
                    {
                        var textSelection = (TextSelection)dte.ActiveDocument.Selection;

                        if (fileArgumentParts.Length > 2)
                        {
                            if (int.TryParse(fileArgumentParts[2], out int column))
                            {
                                textSelection.MoveToLineAndOffset(line, column);
                            }
                            else
                            {
                                Console.Error.WriteLine("The column part of the argument must be a valid integer");
                                return 1;
                            }
                        }
                        else
                        {
                            textSelection.GotoLine(line, Select: true);
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine("The line part of the argument must be a valid integer");
                        return 1;
                    }
                }
            }
        }
        finally
        {
            var mainWindow = dte.MainWindow;
            mainWindow.Activate();
            SetForegroundWindow(mainWindow.HWnd);

            MessageFilter.Revoke();
        }

        return 0;
    }

    private static DTE? TryVisualStudioLaunch(string version)
    {
        try
        {
            var visualStudioDteType = Type.GetTypeFromProgID(version, throwOnError: true);
            var dte = (DTE?)Activator.CreateInstance(visualStudioDteType!);

            return dte;
        }
        catch (COMException)
        {
            return null;
        }
    }

    [GeneratedRegex("!VisualStudio.DTE.17.0:[0-9]")]
    private static partial Regex VisualStudioDTERegex();

    private static DTE? FindInstanceEditingSolution(string solutionPath)
    {
        if (GetRunningObjectTable(0, out IRunningObjectTable pprot) != 0)
        {
            return null;
        }

        try
        {
            pprot.EnumRunning(out IEnumMoniker ppenumMoniker);
            ppenumMoniker.Reset();

            var moniker = new IMoniker[1];

            while (ppenumMoniker.Next(1, moniker, 0) == 0)
            {
                string ppszDisplayName;

                CreateBindCtx(0, out IBindCtx ppbc);

                try
                {
                    moniker[0].GetDisplayName(ppbc, null, out ppszDisplayName);
                }
                finally
                {
                    Marshal.ReleaseComObject(ppbc);
                }

                if (ppszDisplayName is null)
                {
                    continue;
                }

                // The digits after the colon are the process ID.
                if (!VisualStudioDTERegex().IsMatch(ppszDisplayName))
                {
                    continue;
                }

                if (pprot.GetObject(moniker[0], out object ppunkObject) == 0)
                {
                    if (ppunkObject is DTE dte && dte.Solution.FullName.Length > 0)
                    {
                        if (NormalizePath(dte.Solution.FullName) == solutionPath)
                        {
                            return dte;
                        }
                    }
                }
            }
        }
        finally
        {
            Marshal.ReleaseComObject(pprot);
        }

        return null;
    }

    private static string NormalizePath(string path)
    {
        return new Uri(Path.GetFullPath(path)).LocalPath
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .ToUpperInvariant();
    }

    #region MessageFilter. See: http: //msdn.microsoft.com/en-us/library/ms228772.aspx

    [GeneratedComClass]
    private sealed partial class MessageFilter : IOleMessageFilter
    {
        // Class containing the IOleMessageFilter thread error-handling functions.

        private static IOleMessageFilter? _oldFilter;

        // Start the filter.
        public static void Register()
        {
            IOleMessageFilter newFilter = new MessageFilter();
            int ret = CoRegisterMessageFilter(newFilter, out _oldFilter);
            if (ret != 0)
            {
                Console.Error.WriteLine($"CoRegisterMessageFilter failed with error code: {ret}");
            }
        }

        // Done with the filter, close it.
        public static void Revoke()
        {
            int ret = CoRegisterMessageFilter(_oldFilter, out _);
            if (ret != 0)
            {
                Console.Error.WriteLine($"CoRegisterMessageFilter failed with error code: {ret}");
            }
        }

        // Implement the IOleMessageFilter interface.

        // Handle incoming thread requests.
        int IOleMessageFilter.HandleInComingCall(int dwCallType, nint hTaskCaller, int dwTickCount, nint lpInterfaceInfo)
        {
            // Return the flag SERVERCALL_ISHANDLED.
            return 0;
        }

        // Thread call was rejected, so try again.
        int IOleMessageFilter.RetryRejectedCall(nint hTaskCallee, int dwTickCount, int dwRejectType)
        {
            // flag = SERVERCALL_RETRYLATER
            if (dwRejectType == 2)
            {
                // Retry the thread call immediately if return >= 0 & < 100.
                return 99;
            }

            // Too busy; cancel call.
            return -1;
        }

        int IOleMessageFilter.MessagePending(nint hTaskCallee, int dwTickCount, int dwPendingType)
        {
            // Return the flag PENDINGMSG_WAITDEFPROCESS.
            return 2;
        }

        [LibraryImport("ole32.dll")]
        private static partial int CoRegisterMessageFilter(IOleMessageFilter? newFilter, out IOleMessageFilter? oldFilter);
    }

    [GeneratedComInterface]
    [Guid("00000016-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal partial interface IOleMessageFilter
    {
        [PreserveSig]
        public int HandleInComingCall(int dwCallType, nint hTaskCaller, int dwTickCount, nint lpInterfaceInfo);

        [PreserveSig]
        public int RetryRejectedCall(nint hTaskCallee, int dwTickCount, int dwRejectType);

        [PreserveSig]
        public int MessagePending(nint hTaskCallee, int dwTickCount, int dwPendingType);
    }

    #endregion
}
