using ShipIt.Core.Deployment.Enums;

namespace ShipIt.Core.Deployment.Validation;

public static class FargateValidator
{
    public static bool IsValid(
        FargateCpu cpu,
        FargateMemory memory)
    {
        var option = FargateOptions.Options
            .FirstOrDefault(x => x.Cpu == cpu);

        if (option == null)
            return false;

        return option.SupportedMemory.Contains(memory);
    }
}