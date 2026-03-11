using DataverseAPI.Models;
using DataverseAPI.Models.AccountModels;
using DataverseAPI.Services.Contacts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Security.Cryptography;
using System.ServiceModel;

namespace DataverseAPI.Services.Account
{
    public class AccountDataverseService : IAccountDataverseService
    {
        private readonly DataverseSettings _settings;
        private readonly ILogger<AccountDataverseService> _logger;
        private readonly IContactDataverseService _contactService;
        private readonly ServiceClient _serviceClient;

        public AccountDataverseService(IOptions<DataverseSettings> settings, ILogger<AccountDataverseService> logger, IContactDataverseService contactService)
        {
            _settings = settings.Value;
            _logger = logger;
            _contactService = contactService;

            var connectionString = $"AuthType=ClientSecret;Url={_settings.DataverseUrl};ClientId={_settings.ClientId};ClientSecret={_settings.ClientSecret};TenantId={_settings.TenantId}";

            _serviceClient = new ServiceClient(connectionString);

            if (!_serviceClient.IsReady)
            {
                _logger.LogError("Failed to connect to Dataverse {Error}", _serviceClient.LastError);
                throw new InvalidOperationException($"Failed to connect to Dataverse: {_serviceClient.LastError}");
            }
        }

        public async Task<GetAccountResponse?> GetAccountAsync(Guid accountId)
        {
            try
            {
                var columns = new ColumnSet(
                    "accountid", "parentaccountid", "name", "accountnumber", "accountratingcode",
                    "address1_name", "address1_line1", "address1_line2", "address1_line3", "address1_city",
                    "address1_county", "address1_country", "address1_addresstypecode", "createdon", "description",
                    "statuscode", "primarycontactid");

                var entity = await Task.Run(() => _serviceClient.Retrieve("account", accountId, columns));
                
                if (entity is null) return null;

                var primaryContact = await _contactService.GetContactAsync(entity.GetAttributeValue<EntityReference>("primarycontactid").Id);


                return new GetAccountResponse
                {
                    AccountId = entity.Id,
                    ParentAccount = entity.GetAttributeValue<EntityReference>("parentaccountid")?.Id,
                    AccountName = entity.GetAttributeValue<string>("name"),
                    AccountNumber = entity.GetAttributeValue<string>("accountnumber"),
                    AccountRating = entity.FormattedValues.ContainsKey("accountratingcode")
                        ? entity.FormattedValues["accountratingcode"] : null,
                    Address1Name = entity.GetAttributeValue<string>("address1_name"),
                    Address1Line1 = entity.GetAttributeValue<string>("address1_line1"),
                    Address1Line2 = entity.GetAttributeValue<string>("address1_line2"),
                    Address1Line3 = entity.GetAttributeValue<string>("address1_line3"),
                    Address1City = entity.GetAttributeValue<string>("address1_city"),
                    Address1County = entity.GetAttributeValue<string>("address1_county"),
                    Address1Country = entity.GetAttributeValue<string>("address1_country"),
                    Address1Type = entity.FormattedValues.ContainsKey("address1_addresstypecode")
                        ? entity.FormattedValues["address1_addresstypecode"] : null,
                    CreatedOn = entity.GetAttributeValue<DateTime?>("createdon")?.ToString("dd-MM-yyyy hh:mm tt"),
                    Description = entity.GetAttributeValue<string>("description"),
                    StatusCode = entity.FormattedValues.ContainsKey("statuscode")
                        ? entity.FormattedValues["statuscode"] : null,
                    PrimaryContact = new PrimaryContact
                    {
                        ContactId = primaryContact?.ContactId,
                        ContactName = $"{primaryContact?.FirstName} {primaryContact?.LastName}",
                        ContactEmail = primaryContact?.EmailAddress,
                    }
                };
            }
            catch (FaultException<OrganizationServiceFault>)
            {
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account {accountId}", accountId);
                return null;
            }
        }

        public async Task<CreateAccountResponse> CreateAccountAsync(CreateAccountRequest request)
        {
            try
            {
                var account = new Entity("account")
                {
                    ["name"] = request.AccountName,
                    ["accountnumber"] = RandomNumberGenerator.GetInt32(100000, 100000000).ToString(),
                    ["accountratingcode"] = new OptionSetValue(request.AccountRating),
                    ["address1_addresstypecode"] = new OptionSetValue(request.Address1Type),
                    ["emailaddress1"] = request.EmailAddress,
                    ["telephone1"] = request.MainPhone,
                    ["websiteurl"] = request?.Website,
                    ["address1_name"] = request?.Address1Name,
                    ["address1_line1"] = request?.Address1Line1,
                    ["address1_line2"] = request?.Address1Line2,
                    ["address1_line3"] = request?.Address1Line3,
                    ["address1_city"] = request?.Address1City,
                    ["address1_county"] = request?.Address1County,
                    ["address1_country"] = request?.Address1Country,
                    ["description"] = request?.Description,
                    ["primarycontactid"] = request?.PrimaryContactId != null ? new EntityReference("contact", request.PrimaryContactId.Value) : null,
                    ["parentaccountid"] = request?.ParentAccountId != null ? new EntityReference("account", request.ParentAccountId.Value) : null
                };

                var accountId = await Task.Run(() => _serviceClient.Create(account));

                return new CreateAccountResponse
                {
                    Success = true,
                    AccountId = accountId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account");
                return new CreateAccountResponse
                {
                    Success = false,
                    ErrorMessage = "An error occurred."
                };
            }
        }
    }
}
