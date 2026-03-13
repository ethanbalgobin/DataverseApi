using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.ComponentModel.DataAnnotations;

namespace DataverseAPI.Models.AccountModels
{
    public class UpdateAccountRequest
    {
        [OpenApiProperty(Description = "The GUID of the account record in Dataverse")]
        [Required]
        public Guid AccountId { get; set; }

        [OpenApiProperty(Nullable = true)]
        public string AccountName { get; set; } = string.Empty;

        [OpenApiProperty(Nullable = true)]
        public string EmailAddress { get; set; } = string.Empty;

        [OpenApiProperty(Nullable = true)]
        public string MainPhone { get; set; } = string.Empty;

        [OpenApiProperty(Nullable = true)]
        public int? AccountRating { get; set; } = 4;

        [OpenApiProperty(Nullable = true)]
        public string? Website { get; set; }

        [OpenApiProperty(Nullable = true)]
        public string? Address1Name { get; set; }

        [OpenApiProperty(Nullable = true)]
        public string? Address1Line1 { get; set; }

        [OpenApiProperty(Nullable = true)]
        public string? Address1Line2 { get; set; }

        [OpenApiProperty(Nullable = true)]
        public string? Address1Line3 { get; set; }

        [OpenApiProperty(Nullable = true)]
        public string? Address1City { get; set; }

        [OpenApiProperty(Nullable = true)]
        public string? Address1County { get; set; }

        [OpenApiProperty(Nullable = true)]
        public string? Address1Country { get; set; }

        [OpenApiProperty(Nullable = true)]
        public int? Address1Type { get; set; } = 3;

        [OpenApiProperty(Nullable = true)]
        public string? Description { get; set; }

        [OpenApiProperty(Description = "The GUID of the associated contact record in Dataverse.", Nullable = true)]
        public Guid? PrimaryContactId { get; set; }

        [OpenApiProperty(Description = "The GUID of the associated account record in Dataverse.", Nullable = true)]
        public Guid? ParentAccountId { get; set; }
    }

    public class UpdateAccountResponse
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
