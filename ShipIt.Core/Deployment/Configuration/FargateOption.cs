namespace ShipIt.Core.Deployment;

public class FargateOption
{
    public FargateCpu Cpu { get; set; }

    public IReadOnlyList<FargateMemory> SupportedMemory { get; set; }
        = [];
}