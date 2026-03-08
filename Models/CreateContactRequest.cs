namespace ContactFunction.Models
{
    public class CreateContactRequest
    {
        public Guid? AccountId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public int Gender { get; set; } = 3;
        public string MobilePhone { get; set; } = string.Empty;
        public string Address1Line1 { get; set; } = string.Empty;
        public string Address1Line2 { get; set; } = string.Empty;
        public string Address1Line3 { get; set; } = string.Empty;
        public string Address1City { get; set; } = string.Empty;
        public string Address1County { get; set; } = string.Empty;
        public string Address1Country { get; set; } = string.Empty;
        public int Address1Type { get; set; } = 4;
    }

    public class CreateContactResponse
    {
        public bool Success { get; set; }
        public Guid ContactId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
