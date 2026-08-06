using ShipIt.Core.Deployment;
using ShipIt.Core.Deployment.Presentation;
using ShipIt.Core.Services;

namespace ShipIt.Infrastructure.Services;

public class DeploymentOptionsService : IDeploymentOptionsService
{
    public DeploymentOptionsResponse GetDeploymentOptions()
    {
        return new DeploymentOptionsResponse
    {
        CpuOptions = FargateOptions.Options
            .Select(option => new FargateCpuOptionDto
            {
                Cpu = (int)option.Cpu,
                DisplayName = FargateDisplayHelper.GetCpuDisplayName(option.Cpu),

                SupportedMemory = option.SupportedMemory
                    .Select(memory => new FargateMemoryOptionDto
                    {
                        Memory = (int)memory,
                        DisplayName = FargateDisplayHelper.GetMemoryDisplayName(memory)
                    })
                    .ToList()
            })
            .ToList()
    };
    }

    private static string GetCpuDisplayName(FargateCpu cpu)
    {
        return cpu switch
        {
            FargateCpu.Cpu256 => "0.25 vCPU",
            FargateCpu.Cpu512 => "0.5 vCPU",
            FargateCpu.Cpu1024 => "1 vCPU",
            FargateCpu.Cpu2048 => "2 vCPU",
            FargateCpu.Cpu4096 => "4 vCPU",
            FargateCpu.Cpu8192 => "8 vCPU",
            FargateCpu.Cpu16384 => "16 vCPU",
            _ => cpu.ToString()
        };
    }

    private static string GetMemoryDisplayName(FargateMemory memory)
    {
        return $"{(int)memory / 1024.0:G} GB";
    }
}