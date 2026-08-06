namespace ShipIt.Core.Deployment.Presentation;

public static class FargateDisplayHelper
{
    public static string GetCpuDisplayName(FargateCpu cpu)
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

    public static string GetMemoryDisplayName(FargateMemory memory)
    {
        var gb = (int)memory / 1024.0;

        return $"{gb:G} GB";
    }
}