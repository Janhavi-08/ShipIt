public class FargateCpuOptionDto
{
    public int Cpu { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public List<FargateMemoryOptionDto> SupportedMemory { get; set; }
        = [];
}