
namespace ModernBackendLab.DTOs;

public sealed record ProductResponse(
    int Id,
    string Name,
    decimal Price,
    byte[] RowVersion);