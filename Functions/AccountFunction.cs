using DataverseAPI.Models;
using DataverseAPI.Models.AccountModels;
using DataverseAPI.Models.ContactModels;
using DataverseAPI.Services.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Text.Json;

namespace DataverseAPI.Functions;

public class AccountFunction
{
    private readonly ILogger<AccountFunction> _logger;
    private readonly IAccountDataverseService _dataverseService;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AccountFunction(
        ILogger<AccountFunction> logger,
        IAccountDataverseService dataverseService)
    {
        _logger = logger;
        _dataverseService = dataverseService;
    }

    [Function("GetAccount")]
    [OpenApiOperation(operationId: "GetAccount", tags: ["Account"], Summary = "Get an account by ID", Description = "Retrieves an account record from Dataverse by its unique identifier.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The unique identifier of the account.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(GetAccountResponse), Description = "The account was found.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Account not found.")]

    public async Task<IActionResult> GetAccount(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "accounts/{id:guid}")] HttpRequest req,
        Guid id)
    {
        _logger.LogInformation("GetAccount function triggered for {accountId}", id);

        var account = await _dataverseService.GetAccountAsync(id);

        if (account is null)
        {
            return new NotFoundObjectResult(new ErrorResponse { Error = "Account not found" });
        }

        return new OkObjectResult(account);
    }
}