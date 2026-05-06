namespace SaaSPlatform.BuildingBlocks.Abstractions;

public interface IPlugin
{
    string Name { get; }

    void Register(IServiceCollection services);
}