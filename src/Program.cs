using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Windows.Win32;
using Windows.Win32.System.Threading;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

/*
sidenote: 
> This call site is reachable on: 'windows' 6.2 and later. 'PInvoke.GetProcessInformation(SafeHandle, PROCESS_INFORMATION_CLASS, void*, uint)' is only supported on: 'windows' 8.0 and later. [GetProcessInformation_hProcess]csharp(CA1416)
The platform "windows8.0" does not exist. Windows 8.0 should be parsed as "windows6.2"
*/

if (!OperatingSystem.IsWindowsVersionAtLeast(6, 2))
    throw new PlatformNotSupportedException("This application can only run on Windows 8 or higher.");

Console.WriteLine("Opening process handles...");

SafeFileHandle hProcess_QUERY = PInvoke.OpenProcess_SafeHandle(PROCESS_QUERY_INFORMATION, false, (uint)Environment.ProcessId);
if (hProcess_QUERY.IsInvalid)
    Console.Error.WriteLine($"Failed to open {nameof(hProcess_QUERY)} with permission {nameof(PROCESS_QUERY_INFORMATION)}.");

SafeFileHandle hProcess_QUERY_LIMITED = PInvoke.OpenProcess_SafeHandle(PROCESS_QUERY_LIMITED_INFORMATION, false, (uint)Environment.ProcessId);
if (hProcess_QUERY_LIMITED.IsInvalid)
    Console.Error.WriteLine($"Failed to open {nameof(hProcess_QUERY_LIMITED)} with permission {nameof(PROCESS_QUERY_LIMITED_INFORMATION)}.");

SafeFileHandle hProcess_SET = PInvoke.OpenProcess_SafeHandle(PROCESS_SET_INFORMATION, false, (uint)Environment.ProcessId);
if (hProcess_SET.IsInvalid)
    Console.Error.WriteLine($"Failed to open {nameof(hProcess_SET)} with permission {nameof(PROCESS_SET_INFORMATION)}.");

Console.WriteLine("Calling GetProcessInformation for valid handles...");

PROCESS_PROTECTION_LEVEL_INFORMATION protectionLevel = default;
bool success;

if (!hProcess_QUERY.IsInvalid)
{
    unsafe
    { success = PInvoke.GetProcessInformation(hProcess_QUERY, PROCESS_INFORMATION_CLASS.ProcessProtectionLevelInfo, &protectionLevel, (uint)Marshal.SizeOf(protectionLevel)); }

    if (!success)
        Console.Error.WriteLine($"Operation with {nameof(hProcess_QUERY)} failed. Exception: {new Win32Exception()}");
    else
        Console.WriteLine($"Operation with {nameof(hProcess_QUERY)} succeeded. Value: {protectionLevel.ProtectionLevel}");
}

if (!hProcess_QUERY_LIMITED.IsInvalid)
{
    unsafe
    { success = PInvoke.GetProcessInformation(hProcess_QUERY_LIMITED, PROCESS_INFORMATION_CLASS.ProcessProtectionLevelInfo, &protectionLevel, (uint)Marshal.SizeOf(protectionLevel)); }

    if (!success)
        Console.Error.WriteLine($"Operation with {nameof(hProcess_QUERY_LIMITED)} failed. Exception: {new Win32Exception()}");
    else
        Console.WriteLine($"Operation with {nameof(hProcess_QUERY_LIMITED)} succeeded. Value: {protectionLevel.ProtectionLevel}");
}

if (!hProcess_SET.IsInvalid)
{
    unsafe
    { success = PInvoke.GetProcessInformation(hProcess_SET, PROCESS_INFORMATION_CLASS.ProcessProtectionLevelInfo, &protectionLevel, (uint)Marshal.SizeOf(protectionLevel)); }

    if (!success)
    {
        const int ERROR_ACCESS_DENIED = 5;
        int err = Marshal.GetLastPInvokeError();
        Console.Error.WriteLine($"Operation with {nameof(hProcess_SET)} failed. {(err is ERROR_ACCESS_DENIED ? $"Error is {nameof(ERROR_ACCESS_DENIED)}; Incorrect or insufficient process access rights specified." : "")} Exception: {new Win32Exception(err)}");
    }
    else
    {
        Console.WriteLine($"Operation with {nameof(hProcess_SET)} succeeded. Value: {protectionLevel.ProtectionLevel}");
    }
}
