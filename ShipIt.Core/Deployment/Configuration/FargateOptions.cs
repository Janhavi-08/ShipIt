namespace ShipIt.Core.Deployment;

public static class FargateOptions
{
    public static IReadOnlyList<FargateOption> Options =>
        new List<FargateOption>
        {
            new()
            {
                Cpu = FargateCpu.Cpu256,
                SupportedMemory =
                [
                    FargateMemory.Memory512,
                    FargateMemory.Memory1024,
                    FargateMemory.Memory2048
                ]
            },

            new()
            {
                Cpu = FargateCpu.Cpu512,
                SupportedMemory =
                [
                    FargateMemory.Memory1024,
                    FargateMemory.Memory2048,
                    FargateMemory.Memory3072,
                    FargateMemory.Memory4096
                ]
            }
        };
}