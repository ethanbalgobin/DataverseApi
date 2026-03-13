using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.ComponentModel.DataAnnotations;

namespace DataverseAPI.Models.ContactModels
{
    public class CreateContactRequest
    {
        [OpenApiProperty(Nullable = true)]
        public Guid? AccountId { get; set; }

        [OpenApiProperty(Description = "The first name of the contact")]
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [OpenApiProperty(Description = "The last name of the contact")]
        [Required]
        public string LastName { get; set; } = string.Empty;

        [OpenApiProperty(Description = "The email address of the contact")]
        [Required]
        public string EmailAddress { get; set; } = string.Empty;

        [OpenApiProperty(Nullable = true)]
        public int Gender { get; set; } = 3;

        [OpenApiProperty(Nullable = true)]
        public string MobilePhone { get; set; } = string.Empty;

        [OpenApiProperty(Nullable = true)]
        public string Address1Line1 { get; set; } = string.Empty;

        [OpenApiProperty(Nullable = true)]
        public string Address1Line2 { get; set; } = string.Empty;

        [OpenApiProperty(Nullable = true)]
        public string Address1Line3 { get; set; } = string.Empty;

        [OpenApiProperty(Nullable = true)]
        public string Address1City { get; set; } = string.Empty;

        [OpenApiProperty(Nullable = true)]
        public string Address1County { get; set; } = string.Empty;

        [OpenApiProperty(Nullable = true)]
        public string Address1Country { get; set; } = string.Empty;

        [OpenApiProperty(Nullable = true)]
        public int Address1Type { get; set; } = 4;
    }

    public class CreateContactResponse
    {
        public bool Success { get; set; }
        public Guid ContactId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
