namespace DataverseAPI.Models.ContactModels;

public class UpdateContactRequest
{
    public Guid ContactId { get; set; }
    public Guid? AccountId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmailAddress { get; set; }
    public int? Gender { get; set; }
    public string? MobilePhone { get; set; }
    public string? Address1Line1 { get; set; }
    public string? Address1Line2 { get; set; }
    public string? Address1Line3 { get; set; }
    public string? Address1City { get; set; }
    public string? Address1County { get; set; }
    public string? Address1Country { get; set; }
    public int? Address1Type { get; set; }
}

public class UpdateContactResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}