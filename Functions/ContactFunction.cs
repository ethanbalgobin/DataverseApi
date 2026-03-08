using System.Text.Json;
using DataverseAPI.Models.ContactModels;
using DataverseAPI.Services.Contacts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DataverseAPI.Functions;

public class ContactFunction
{
    private readonly ILogger<ContactFunction> _logger;
    private readonly IContactDataverseService _dataverseService;

    public ContactFunction(
        ILogger<ContactFunction> logger,
        IContactDataverseService dataverseService)
    {
        _logger = logger;
        _dataverseService = dataverseService;
    }

    [Function("GetContact")]
    public async Task<IActionResult> GetContact(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "contacts/{id:guid}")] HttpRequest req,
        Guid id)
    {
        _logger.LogInformation("GetContact function triggred for {ContactId}", id);

        var contact = await _dataverseService.GetContactAsync(id);

        if (contact == null)
        {
            return new NotFoundObjectResult(new { error = "Contact not found" });
        }

        return new OkObjectResult(contact);
    }

    [Function("CreateContact")]
    public async Task<IActionResult> CreateContact(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "contacts")] HttpRequest req)
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