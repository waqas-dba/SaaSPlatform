namespace SaaSPlatform.BuildingBlocks.Abstractions;

public interface IModule
{
    string Name { get; }

    void Register(IServiceCollection services, IConfiguration config);

    void MapEndpoints(IEndpointRouteBuilder endpoints);
}