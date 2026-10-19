
using Grpc.Core;
using ModernBackendLab.Protos;

namespace ModernBackendLab.Services;

public class ProductsGrpcService : ProductsGrpc.ProductsGrpcBase
{
    public override Task<GetProductResponse> GetProduct(
        GetProductRequest request,
        ServerCallContext context)
    {
        var response = new GetProductResponse
        {
            Id = request.Id,
            Name = "Test Product"
        };

        return Task.FromResult(response);
    }
}
