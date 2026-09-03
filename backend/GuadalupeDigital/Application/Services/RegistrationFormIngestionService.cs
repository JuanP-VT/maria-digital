using GuadalupeDigital.Application.Abstractions.ObjectStorage;
using GuadalupeDigital.Application.DTOs.Requests;
using GuadalupeDigital.Infrastructure.Persistance;

namespace GuadalupeDigital.Application.Services;

public class RegistrationFormIngestionService(
    AppDbContext dbContext,
    IObjectStorage objectStorage,
    CancellationToken ct)
{ 
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IObjectStorage _objectStorage = objectStorage;

    public async Task<string> GetPressignedUpload(CreatePresignedPutRequest request)
    {
        // Validate 
        string key = $"registrationform/{Guid.NewGuid()}";
        TimeSpan expiresIn = TimeSpan.FromMinutes(60);
        var result = await _objectStorage.GetPresignedPutAsync(key,request.ContentType,expiresIn,ct);

        // track record in DB
        string fileName = request.Filename;
        // return
        return result;
    }
}
