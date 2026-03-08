using ContactFunction.Models;

namespace ContactFunction.Services
{
    public interface IDataverseService
    {
        Task<CreateContactResponse> CreateContactAsync(CreateContactRequest request);
    }
}
