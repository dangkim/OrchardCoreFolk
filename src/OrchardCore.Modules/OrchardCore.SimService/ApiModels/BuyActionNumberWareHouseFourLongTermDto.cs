namespace OrchardCore.SimService.ApiModels
{
    public class MessageBuyNumberWareHouseFourLongTerm
    {
        public int id { get; set; }
        public string mdn { get; set; }
        public string service { get; set; }
        public double price { get; set; }
        public string expires { get; set; }
    }

    public class BuyActionNumberWareHouseFourLongTermDto
    {
        public string status { get; set; }
        public MessageBuyNumberWareHouseFourLongTerm message { get; set; }
    }
}
