namespace ModernBackendLab.DTOs;

public sealed record UpdateProductRequest(
    string Name,
    decimal Price,
    byte[] RowVersion);