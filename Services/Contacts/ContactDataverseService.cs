using DataverseAPI.Models;
using DataverseAPI.Models.ContactModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

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
    }
}
