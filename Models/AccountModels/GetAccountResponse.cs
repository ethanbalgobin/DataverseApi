namespace DataverseAPI.Models.AccountModels
{
    public class GetAccountResponse
    {
        public Guid AccountId { get; set; }
        public Guid? ParentAccount { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string? AccountNumber { get; set; }
        public string? AccountRating { get; set; }
        public string? Address1Name { get; set; }
        public string? Address1Line1 { get; set; }
        public string? Address1Line2 { get; set; }
        public string? Address1Line3 { get; set; }
        public string? Address1City { get; set; }
        public string? Address1County { get; set; }
        public string? Address1Country { get; set; }
        public string? Address1Type { get; set; }
        public string? CreatedOn { get; set; }
        public string? Description { get; set; }
        public string? StatusCode { get; set; }
        public PrimaryContact? PrimaryContact { get; set; }
    }

    public class PrimaryContact
    {
        public Guid? ContactId { get; set; }
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
    }
}
