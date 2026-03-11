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
    [OpenApiOperation(operationId: "GetAccount", tags: ["Accounts"], Summary = "Get an account by ID", Description = "Retrieves an account record from Dataverse by its unique identifier.")]
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

    [Function("CreateAccount")]
    [OpenApiOperation(operationId: "CreateAccount", tags: ["Accounts"], Summary = "Create a new account", Description = "Creates a new account record in Dataverse.")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(CreateAccountRequest), Required = true, Description = "The account details to create.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CreateAccountResponse), Description = "The account was created successfully.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Invalid request body or validation errors.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "An error occurred while creating the account.")]
    public async Task<IActionResult> CreateAccount(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "accounts")] HttpRequest req)
    {
        _logger.LogInformation("CreateAccount function triggered");
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(requestBody)){
                return new BadRequestObjectResult(new { error = "Request body is required" });
            }

            var accountRequest = JsonSerializer.Deserialize<CreateAccountRequest>(requestBody, _jsonOptions);

            if (accountRequest is null)
            {
                return new BadRequestObjectResult(new ErrorResponse{  Error = "Invalid request body" });
            }

            var validationErrors = ValidateRequest(accountRequest);
            if (validationErrors.Count > 0)
            {
                return new BadRequestObjectResult(new ErrorResponse { Error = "Missing required fields", Errors = validationErrors });
            }

            var result = await _dataverseService.CreateAccountAsync(accountRequest);

            if (result.Success)
            {
                return new OkObjectResult(result);
            }
            else
            {
                return new ObjectResult(new ErrorResponse { Error = result.ErrorMessage })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse request body");
            return new BadRequestObjectResult(new ErrorResponse { Error = "Invalid JSON format" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in CreateAccount");
            return new ObjectResult(new ErrorResponse { Error = "An error occurred" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    private static List<string> ValidateRequest(CreateAccountRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.AccountName))
            errors.Add("Account name is required");

        if (string.IsNullOrWhiteSpace(request.EmailAddress))
            errors.Add("Email address is required");

        if (string.IsNullOrWhiteSpace(request.MainPhone))
            errors.Add("Main phone number is required");

        return errors;
    }

}