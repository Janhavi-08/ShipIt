public class EcsTaskDefinitionInput
{
    public string FamilyName { get; set; } = null!;

    public string ImageUri { get; set; } = null!;

    public int Cpu { get; set; }

    public int Memory { get; set; }

    public int ContainerPort { get; set; }

    public string ExecutionRoleArn { get; set; } = null!;

    public List<EcsEnvironmentVariableInput>
        EnvironmentVariables { get; set; } = [];

    public List<EcsSecretInput>
        Secrets { get; set; } = [];
}

public class EcsEnvironmentVariableInput
{
    public string Name { get; set; } = null!;

    public string Value { get; set; } = null!;
}

public class EcsSecretInput
{
    public string Name { get; set; } = null!;

    public string ValueFrom { get; set; } = null!;
}