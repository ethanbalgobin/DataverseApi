using DataverseAPI.Models.ContactModels;

namespace DataverseAPI.Services.Contacts;

public interface IContactDataverseService
{
    Task<CreateContactResponse> CreateContactAsync(CreateContactRequest request);
    Task<GetContactResponse?> GetContactAsync(Guid contactId);
}
