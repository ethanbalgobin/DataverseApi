using DataverseAPI.Models.ContactModels;
using DataverseAPI.Models;
using DataverseAPI.Services.Contacts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Text.Json;

namespace DataverseAPI.Functions;

public class ContactFunction
{
    private readonly ILogger<ContactFunction> _logger;
    private readonly IContactDataverseService _dataverseService;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ContactFunction(
        ILogger<ContactFunction> logger,
        IContactDataverseService dataverseService)
    {
        _logger = logger;
        _dataverseService = dataverseService;
    }

    [Function("GetContact")]
    [OpenApiOperation(operationId: "GetContact", tags: ["Contacts"], Summary = "Get a contact by ID", Description = "Retrieves a contact record from Dataverse by its unique identifier.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The unique identifier of the contact.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(GetContactResponse), Description = "The contact was found.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Contact not found.")]
    public async Task<IActionResult> GetContact(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "contacts/{id:guid}")] HttpRequest req,
        Guid id)
    {
        _logger.LogInformation("GetContact function triggred for {ContactId}", id);

        var contact = await _dataverseService.GetContactAsync(id);

        if (contact == null)
        {
            return new NotFoundObjectResult(new ErrorResponse{ Error = "Contact not found" });
        }

        return new OkObjectResult(contact);
    }

    [Function("CreateContact")]
    [OpenApiOperation(operationId: "CreateContact", tags: ["Contacts"], Summary = "Create a new contact", Description = "Creates a new contact record in Dataverse.")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(CreateContactRequest), Required = true, Description = "The contact details to create.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CreateContactResponse), Description = "The contact was created successfully.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Invalid request body or validation errors.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "An error occurred while creating the contact.")]
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

            var contactRequest = JsonSerializer.Deserialize<CreateContactRequest>(requestBody, _jsonOptions);

            if (contactRequest is null)
            {
                return new BadRequestObjectResult(new ErrorResponse{ Error = "Invalid request body" });
            }

            var validationErrors = ValidateRequest(contactRequest);
            if (validationErrors.Count > 0)
            {
                return new BadRequestObjectResult(new ErrorResponse{ Error = "Missing required fields", Errors = validationErrors });
            }

            var result = await _dataverseService.CreateContactAsync(contactRequest);

            if (result.Success)
            {
                return new OkObjectResult(result);
            }
            else
            {
                return new ObjectResult(new ErrorResponse{ Error = result.ErrorMessage })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse request body");
            return new BadRequestObjectResult(new ErrorResponse{ Error = "Invalid JSON format" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in CreateContact");
            return new ObjectResult(new ErrorResponse{ Error = "An error occurred" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    [Function("UpdateContact")]
    [OpenApiOperation(operationId: "UpdateContact", tags: ["Contacts"], Summary = "Update an existing contact", Description = "Partially updates a contact record in Dataverse. Only provided fields will be updated.")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(UpdateContactRequest), Required = true, Description = "The contact fields to update. ContactId is required.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(UpdateContactResponse), Description = "The contact was updated successfully.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Invalid request body.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "An error occurred while updating the contact.")]
    public async Task<IActionResult> UpdateContact(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "contacts")] HttpRequest req)
    {
        _logger.LogInformation("UpdateContact function triggered");
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return new BadRequestObjectResult(new ErrorResponse { Error = "Request body is required" });
            }

            var updateRequest = JsonSerializer.Deserialize<UpdateContactRequest>(requestBody, _jsonOptions);

            if (updateRequest is null)
            {
                return new BadRequestObjectResult(new ErrorResponse { Error = "Invalid request body" });
            }

            var result = await _dataverseService.UpdateContactAsync(updateRequest);

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
            _logger.LogError(ex, "Unhandled exception in UpdateContact");
            return new ObjectResult(new ErrorResponse { Error = "An error occurred" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    [Function("DeleteContact")]
    [OpenApiOperation(operationId: "DeleteContact", tags: ["Contacts"], Summary = "Delete a contact", Description = "Deletes a contact record from Dataverse by its unique identifier.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The unique identifier of the contact to delete.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(DeleteContactResponse), Description = "The contact was deleted successfully.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Contact not found.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "An error occurred while deleting the contact.")]
    public async Task<IActionResult> DeleteContact(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "contacts/{id:guid}")] HttpRequest req, Guid id)
    {
        _logger.LogInformation("DeleteContact function triggered for {ContactId}", id);

        try
        {
            var result = await _dataverseService.DeleteContactAsync(id);

            if (result.Success)
            {
                return new OkObjectResult(result);
            }
            else
            {
                if (result.ErrorMessage?.Contains("Does Not Exist") == true)
                {
                    return new NotFoundObjectResult(new ErrorResponse { Error = "Contact not found" });
                }

                return new ObjectResult(new ErrorResponse { Error = result.ErrorMessage })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in DeleteContact");
            return new ObjectResult(new ErrorResponse { Error = "An error occurred" })
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