namespace DataverseAPI.Models.ContactModels;

public class GetContactResponse
{
    public Guid ContactId { get; set; }
    public Guid? AccountId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public string? MobilePhone { get; set; }
    public string? Address1Line1 { get; set; }
    public string? Address1Line2 { get; set; }
    public string? Address1Line3 { get; set; }
    public string? Address1City { get; set; }
    public string? Address1County { get; set; }
    public string? Address1Country { get; set; }
    public string? Address1Type { get; set; }
}