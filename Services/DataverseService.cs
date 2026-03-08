using ContactFunction.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace ContactFunction.Services
{
    public class DataverseService : IDataverseService
    {
        private readonly DataverseSettings _settings;
        private readonly ILogger<DataverseService> _logger;
        private readonly ServiceClient _serviceClient;

        public DataverseService(IOptions<DataverseSettings> settings, ILogger<DataverseService> logger)
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
    }
}
