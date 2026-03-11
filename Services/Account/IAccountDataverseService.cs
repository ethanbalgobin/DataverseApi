using DataverseAPI.Models.AccountModels;

namespace DataverseAPI.Services.Account
{
    public interface IAccountDataverseService
    {
        Task<GetAccountResponse?> GetAccountAsync(Guid accountId);
        Task<CreateAccountResponse> CreateAccountAsync(CreateAccountRequest request);
    }
}
