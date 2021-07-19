using System;
using Newtonsoft.Json;

namespace Application.DTOs
{
    public class PaypalDTO
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("payper")]
        public Payer Payer{ get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class Payer
    {
        [JsonProperty("address")]
        public string Address { get; set; }
        [JsonProperty("email_address")]
        public string Email_Address { get; set; }
        [JsonProperty("payer_id")]
        public string Payer_Id { get; set; }
        [JsonProperty("purchase_units")]
        public Purchase Purchase { get; set; }
    }

    public class Purchase
    {
        [JsonProperty("amount")]
        public Amount Amount { get; set; }
    }

    public class Amount
    {
        [JsonProperty("currency_code")]
        public string Currency { get; set; }
        [JsonProperty("value")]
        public int Value { get; set; }
    }
}