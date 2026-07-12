using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 服务器监控服务
/// </summary>
public class ServerMonitorService : IServerMonitorService
{
    public async Task<ServerMonitorOverviewDto> GetOverviewAsync()
    {
        var cpu = await ReadCpuAsync();
        return new ServerMonitorOverviewDto
        {
            CollectedAt = DateTime.Now,
            HostName = Environment.MachineName,
            Cpu = cpu,
            Memory = ReadMemory(),
            Disks = ReadDisks(),
            Networks = ReadNetworks(),
            DotNetProcesses = ReadProcesses()
        };
    }

    private static async Task<ServerCpuDto> ReadCpuAsync()
    {
        var before = ReadCpuTimes();
        await Task.Delay(100);
        var after = ReadCpuTimes();
        var total = after.Total - before.Total;
        var idle = after.Idle - before.Idle;
        var usage = total <= 0 ? 0 : Math.Clamp((total - idle) / total * 100, 0, 100);
        return new ServerCpuDto { UsagePercent = Math.Round(usage, 2), LogicalProcessorCount = Environment.ProcessorCount };
    }

    private static (double Total, double Idle) ReadCpuTimes()
    {
        if (OperatingSystem.IsWindows() && GetSystemTimes(out var idle, out var kernel, out var user))
        {
            return (ToDouble(kernel) + ToDouble(user), ToDouble(idle));
        }

        if (OperatingSystem.IsLinux())
        {
            try
            {
                var line = File.ReadLines("/proc/stat").FirstOrDefault(value => value.StartsWith("cpu "));
                var values = line?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(double.Parse).ToArray();
                if (values is { Length: >= 4 }) return (values.Sum(), values[3]);
            }
            catch
            {
                // 读取失败时返回零，接口仍然返回其他指标
            }
        }

        return (0, 0);
    }

    private static ServerMemoryDto ReadMemory()
    {
        var total = 0d;
        var free = 0d;
        try
        {
            if (OperatingSystem.IsWindows() && GetMemoryStatus(out var status))
            {
                total = status.TotalPhys;
                free = status.AvailPhys;
            }
            else if (OperatingSystem.IsLinux())
            {
                var values = File.ReadLines("/proc/meminfo")
                    .Select(line => Regex.Match(line, "^(MemTotal|MemAvailable):\\s+(\\d+)").Groups)
                    .Where(groups => groups.Count > 2 && groups[2].Success)
                    .ToDictionary(groups => groups[1].Value, groups => double.Parse(groups[2].Value) * 1024);
                values.TryGetValue("MemTotal", out total);
                values.TryGetValue("MemAvailable", out free);
            }
        }
        catch
        {
            // 保留零值，避免单项采集失败影响接口
        }

        var used = Math.Max(0, total - free);
        return new ServerMemoryDto
        {
            TotalBytes = total,
            UsedBytes = used,
            FreeBytes = free,
            UsagePercent = total <= 0 ? 0 : Math.Round(used / total * 100, 2)
        };
    }

    private static List<ServerDiskDto> ReadDisks()
    {
        var result = new List<ServerDiskDto>();
        try
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                try
                {
                    if (!drive.IsReady || drive.DriveType != DriveType.Fixed) continue;
                    var total = Math.Max(0, drive.TotalSize);
                    var free = Math.Max(0, drive.AvailableFreeSpace);
                    result.Add(new ServerDiskDto
                    {
                        Name = drive.Name,
                        DriveFormat = drive.DriveFormat,
                        TotalBytes = total,
                        UsedBytes = Math.Max(0, total - free),
                        FreeBytes = free,
                        UsagePercent = total <= 0 ? 0 : Math.Round((double)(total - free) / total * 100, 2)
                    });
                }
                catch
                {
                    // 忽略不可读取的磁盘
                }
            }
        }
        catch
        {
            // 忽略磁盘枚举失败
        }
        return result;
    }

    private static List<ServerNetworkDto> ReadNetworks()
    {
        var result = new List<ServerNetworkDto>();
        try
        {
            foreach (var network in NetworkInterface.GetAllNetworkInterfaces())
            {
                try
                {
                    var statistics = network.GetIPStatistics();
                    result.Add(new ServerNetworkDto
                    {
                        Name = network.Name,
                        Description = network.Description,
                        Status = network.OperationalStatus.ToString(),
                        ReceivedBytes = Math.Max(0, statistics.BytesReceived),
                        SentBytes = Math.Max(0, statistics.BytesSent)
                    });
                }
                catch
                {
                    // 忽略不可读取的网卡
                }
            }
        }
        catch
        {
            // 忽略网卡枚举失败
        }
        return result;
    }

    private static List<DotNetProcessDto> ReadProcesses()
    {
        var result = new List<DotNetProcessDto>();
        try
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.Id != Environment.ProcessId && !IsDotNetProcess(process)) continue;
                    result.Add(new DotNetProcessDto
                    {
                        ProcessId = process.Id,
                        Name = process.ProcessName,
                        StartTime = TryGetStartTime(process),
                        CpuMilliseconds = Math.Max(0, process.TotalProcessorTime.TotalMilliseconds),
                        WorkingSetBytes = Math.Max(0, process.WorkingSet64),
                        ThreadCount = process.Threads.Count,
                        HandleCount = TryGetHandleCount(process)
                    });
                }
                catch
                {
                    // 权限不足或进程退出时忽略该进程
                }
                finally
                {
                    process.Dispose();
                }
            }
        }
        catch
        {
            // 忽略进程枚举失败
        }
        return result.OrderByDescending(process => process.WorkingSetBytes).ToList();
    }

    private static bool IsDotNetProcess(Process process)
    {
        try
        {
            return process.Modules.Cast<ProcessModule>().Any(module =>
                module.FileName.Contains("coreclr", StringComparison.OrdinalIgnoreCase) ||
                module.FileName.Contains("clr", StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    private static DateTime? TryGetStartTime(Process process)
    {
        try { return process.StartTime; } catch { return null; }
    }

    private static int TryGetHandleCount(Process process)
    {
        try { return process.HandleCount; } catch { return 0; }
    }

    private static double ToDouble(System.Runtime.InteropServices.ComTypes.FILETIME value)
        => ((ulong)value.dwHighDateTime << 32) + (uint)value.dwLowDateTime;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemTimes(out System.Runtime.InteropServices.ComTypes.FILETIME idleTime, out System.Runtime.InteropServices.ComTypes.FILETIME kernelTime, out System.Runtime.InteropServices.ComTypes.FILETIME userTime);

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatus
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhys;
        public ulong AvailPhys;
        public ulong TotalPageFile;
        public ulong AvailPageFile;
        public ulong TotalVirtual;
        public ulong AvailVirtual;
        public ulong AvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatus status);

    private static bool GetMemoryStatus(out MemoryStatus status)
    {
        status = new MemoryStatus { Length = (uint)Marshal.SizeOf<MemoryStatus>() };
        return GlobalMemoryStatusEx(ref status);
    }
}
