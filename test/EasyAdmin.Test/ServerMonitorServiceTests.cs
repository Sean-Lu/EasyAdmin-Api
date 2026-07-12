using EasyAdmin.Application.Services;

namespace EasyAdmin.Test;

[TestClass]
public class ServerMonitorServiceTests
{
    [TestMethod]
    public async Task GetOverviewAsync_ReturnsHostAndMetricSections()
    {
        var service = new ServerMonitorService();

        var result = await service.GetOverviewAsync();

        Assert.IsFalse(string.IsNullOrWhiteSpace(result.HostName));
        Assert.IsNotNull(result.Cpu);
        Assert.IsNotNull(result.Memory);
        Assert.IsNotNull(result.Disks);
        Assert.IsNotNull(result.Networks);
        Assert.IsNotNull(result.DotNetProcesses);
    }

    [TestMethod]
    public async Task GetOverviewAsync_ReturnsNonNegativeMetricValues()
    {
        var service = new ServerMonitorService();

        var result = await service.GetOverviewAsync();

        Assert.IsTrue(result.Cpu.UsagePercent is >= 0 and <= 100);
        Assert.IsTrue(result.Memory.TotalBytes >= 0);
        Assert.IsTrue(result.Memory.UsedBytes >= 0);
        Assert.IsTrue(result.Disks.All(disk => disk.TotalBytes >= 0 && disk.FreeBytes >= 0));
        Assert.IsTrue(result.Networks.All(network => network.ReceivedBytes >= 0 && network.SentBytes >= 0));
        Assert.IsTrue(result.DotNetProcesses.All(process => process.ProcessId > 0 && process.WorkingSetBytes >= 0));
    }
}
