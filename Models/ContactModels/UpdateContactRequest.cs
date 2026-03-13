using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.ComponentModel.DataAnnotations;

namespace DataverseAPI.Models.ContactModels;

public class UpdateContactRequest
{
    [OpenApiProperty(Description = "The GUID of the contact record in Dataverse")]
    [Required]
    public Guid ContactId { get; set; }

    [OpenApiProperty(Nullable = true)]
    public Guid? AccountId { get; set; }

    [OpenApiProperty(Nullable = true)]
    public string? FirstName { get; set; }

    [OpenApiProperty(Nullable = true)]
    public string? LastName { get; set; }

    [OpenApiProperty(Nullable = true)]
    public string? EmailAddress { get; set; }

    [OpenApiProperty(Nullable = true)]
    public int? Gender { get; set; }

    [OpenApiProperty(Nullable = true)]
    public string? MobilePhone { get; set; }

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
    public int? Address1Type { get; set; }
}

public class UpdateContactResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}