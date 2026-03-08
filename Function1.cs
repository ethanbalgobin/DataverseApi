using System.Text.Json;
using ContactFunction.Models;
using ContactFunction.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ContactFunction;

public class CreateContact
{
    private readonly ILogger<CreateContact> _logger;
    private readonly IDataverseService _dataverseService;

    public CreateContact(
        ILogger<CreateContact> logger,
        IDataverseService dataverseService)
    {
        _logger = logger;
        _dataverseService = dataverseService;
    }

    [Function("CreateContact")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        _logger.LogInformation("CreateContact function triggered");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return new BadRequestObjectResult(new { error = "Request body is required" });
            }

            var contactRequest = JsonSerializer.Deserialize<CreateContactRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (contactRequest is null)
            {
                return new BadRequestObjectResult(new { error = "Invalid request body" });
            }

            var validationErrors = ValidateRequest(contactRequest);
            if (validationErrors.Count > 0)
            {
                return new BadRequestObjectResult(new { errors = validationErrors });
            }

            var result = await _dataverseService.CreateContactAsync(contactRequest);

            if (result.Success)
            {
                return new OkObjectResult(result);
            }
            else
            {
                return new ObjectResult(new { error = result.ErrorMessage })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse request body");
            return new BadRequestObjectResult(new { error = "Invalid JSON format" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in CreateContact");
            return new ObjectResult(new { error = "An error occurred" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    private static List<string> ValidateRequest(CreateContactRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.FirstName))
            errors.Add("First name is required");

        if (string.IsNullOrWhiteSpace(request.LastName))
            errors.Add("Last name is required");

        if (string.IsNullOrWhiteSpace(request.EmailAddress))
            errors.Add("Email address is required");

        return errors;
    }
}