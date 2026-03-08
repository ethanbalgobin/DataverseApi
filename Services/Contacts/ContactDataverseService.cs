using DataverseAPI.Models;
using DataverseAPI.Models.ContactModels;
using Microsoft.AspNetCore.Server.Kestrel.Transport.NamedPipes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Text.Json;

namespace DataverseAPI.Services.Contacts
{
    public class ContactDataverseService : IContactDataverseService
    {
        private readonly DataverseSettings _settings;
        private readonly ILogger<ContactDataverseService> _logger;
        private readonly ServiceClient _serviceClient;

        public ContactDataverseService(IOptions<DataverseSettings> settings, ILogger<ContactDataverseService> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            var connectionString = $"AuthType=ClientSecret;Url={_settings.DataverseUrl};ClientId={_settings.ClientId};ClientSecret={_settings.ClientSecret};TenantId={_settings.TenantId}";

            _serviceClient = new ServiceClient(connectionString);

            if (!_serviceClient.IsReady)
            {
                _logger.LogError("Failed to connect to Dataverse {Error}", _serviceClient.LastError);
                throw new InvalidOperationException($"Failed to connect to Dataverse: {_serviceClient.LastError}");
            }
        }

        public async Task<GetContactResponse?> GetContactAsync(Guid contactId)
        {
            try
            {
                var columns = new ColumnSet(
                    "contactid", "parentcustomerid", "firstname", "lastname", "emailaddress1",
                    "gendercode", "mobilephone", "address1_line1", "address1_line2",
                    "address1_line3", "address1_city", "address1_county", "address1_country",
                    "address1_addresstypecode");

                var entity = await Task.Run(() => _serviceClient.Retrieve("contact", contactId, columns));

                if (entity is null) return null;

                return new GetContactResponse
                {
                    ContactId = entity.Id,
                    AccountId = entity.GetAttributeValue<EntityReference>("parentcustomerid")?.Id,
                    FirstName = entity.GetAttributeValue<string>("firstname") ?? string.Empty,
                    LastName = entity.GetAttributeValue<string>("lastname") ?? string.Empty,
                    EmailAddress = entity.GetAttributeValue<string>("emailaddress1") ?? string.Empty,
                    Gender = entity.FormattedValues.ContainsKey("gendercode")
                        ? entity.FormattedValues["gendercode"] : null,
                    MobilePhone = entity.GetAttributeValue<string>("mobilephone"),
                    Address1Line1 = entity.GetAttributeValue<string>("address1_line1"),
                    Address1Line2 = entity.GetAttributeValue<string>("address1_line2"),
                    Address1Line3 = entity.GetAttributeValue<string>("address1_line3"),
                    Address1City = entity.GetAttributeValue<string>("address1_city"),
                    Address1County = entity.GetAttributeValue<string>("address1_county"),
                    Address1Country = entity.GetAttributeValue<string>("address1_country"),
                    Address1Type = entity.FormattedValues.ContainsKey("address1_addresstypecode")
                        ? entity.FormattedValues["address1_addresstypecode"] : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contact {contactId}", contactId);
                return null;
            }
        }

        public async Task<CreateContactResponse> CreateContactAsync(CreateContactRequest request)
        {
            try
            {
                var contact = new Entity("contact")
                {
                    ["parentcustomerid"] = request.AccountId.HasValue ? new EntityReference("account", request.AccountId.Value) : null,
                    ["firstname"] = request.FirstName,
                    ["lastname"] = request.LastName,
                    ["emailaddress1"] = request.EmailAddress,
                    ["fullname"] = $"{request.FirstName} {request.LastName}",
                    ["gendercode"] = new OptionSetValue(request.Gender),
                    ["mobilephone"] = request?.MobilePhone,
                    ["address1_line1"] = request?.Address1Line1,
                    ["address1_line2"] = request?.Address1Line2,
                    ["address1_line3"] = request?.Address1Line3,
                    ["address1_city"] = request?.Address1City,
                    ["address1_county"] = request?.Address1County,
                    ["address1_country"] = request?.Address1Country,
                    ["address1_addresstypecode"] = new OptionSetValue(request.Address1Type)
                };

                var contactId = await Task.Run(() => _serviceClient.Create(contact));

                return new CreateContactResponse
                {
                    Success = true,
                    ContactId = contactId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contact");
                return new CreateContactResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<UpdateContactResponse> UpdateContactAsync(UpdateContactRequest request)
        {
            try
            {
                var contact = new Entity("contact", request.ContactId);

                if (request.AccountId.HasValue)
                    contact["parentcustomerid"] = new EntityReference("account", request.AccountId.Value);

                if (request.FirstName is not null)
                    contact["firstname"] = request.FirstName;

                if (request.LastName is not null)
                    contact["lastname"] = request.LastName;

                if (request.EmailAddress is not null)
                    contact["emailaddress1"] = request.EmailAddress;

                if (request.Gender.HasValue)
                    contact["gendercode"] = new OptionSetValue(request.Gender.Value);

                if (request.MobilePhone is not null)
                    contact["mobilephone"] = request.MobilePhone;

                if (request.Address1Line1 is not null)
                    contact["address1_line1"] = request.Address1Line1;

                if (request.Address1Line2 is not null)
                    contact["address1_line2"] = request.Address1Line2;

                if (request.Address1Line3 is not null)
                    contact["address1_line3"] = request.Address1Line3;

                if (request.Address1City is not null)
                    contact["address1_city"] = request.Address1City;

                if (request.Address1County is not null)
                    contact["address1_county"] = request.Address1County;

                if (request.Address1Country is not null)
                    contact["address1_country"] = request.Address1Country;

                if (request.Address1Type.HasValue)
                    contact["address1_addresstypecode"] = new OptionSetValue(request.Address1Type.Value);

                if (request.FirstName is not null || request.LastName is not null)
                {
                    var existing = await GetContactAsync(request.ContactId);
                    if (existing is not null)
                    {
                        var firstname = request.FirstName ?? existing.FirstName;
                        var lastname = request.LastName ?? existing.LastName;
                        contact["fullname"] = $"{firstname} {lastname}";
                    }
                }

                await Task.Run(() => _serviceClient.Update(contact));

                return new UpdateContactResponse { Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contact {ContactId}", request.ContactId);
                return new UpdateContactResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<DeleteContactResponse> DeleteContactAsync(Guid contactId)
        {
            try
            {
                await Task.Run(() => _serviceClient.Delete("contact", contactId));

                return new DeleteContactResponse { Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contact {ContactId}", contactId);
                return new DeleteContactResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
