namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 服务器监控快照
/// </summary>
public class ServerMonitorOverviewDto
{
    public DateTime CollectedAt { get; set; }
    public string HostName { get; set; } = string.Empty;
    public ServerCpuDto Cpu { get; set; } = new();
    public ServerMemoryDto Memory { get; set; } = new();
    public List<ServerDiskDto> Disks { get; set; } = [];
    public List<ServerNetworkDto> Networks { get; set; } = [];
    public List<DotNetProcessDto> DotNetProcesses { get; set; } = [];
}

/// <summary>
/// CPU信息
/// </summary>
public class ServerCpuDto
{
    public double UsagePercent { get; set; }
    public int LogicalProcessorCount { get; set; }
}

/// <summary>
/// 内存信息
/// </summary>
public class ServerMemoryDto
{
    public double TotalBytes { get; set; }
    public double UsedBytes { get; set; }
    public double FreeBytes { get; set; }
    public double UsagePercent { get; set; }
}

/// <summary>
/// 磁盘信息
/// </summary>
public class ServerDiskDto
{
    public string Name { get; set; } = string.Empty;
    public string DriveFormat { get; set; } = string.Empty;
    public double TotalBytes { get; set; }
    public double UsedBytes { get; set; }
    public double FreeBytes { get; set; }
    public double UsagePercent { get; set; }
}

/// <summary>
/// 网络信息
/// </summary>
public class ServerNetworkDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double ReceivedBytes { get; set; }
    public double SentBytes { get; set; }
}

/// <summary>
/// .NET进程信息
/// </summary>
public class DotNetProcessDto
{
    public int ProcessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public double CpuMilliseconds { get; set; }
    public double WorkingSetBytes { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
}
