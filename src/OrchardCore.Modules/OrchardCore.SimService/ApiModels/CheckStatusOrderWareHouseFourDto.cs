namespace OrchardCore.SimService.ApiModels
{
    public class MessageStatusOrderWareHouseFourDto
    {
        public string id { get; set; }
        public string mdn { get; set; }
        public string service { get; set; }
        public string status { get; set; }
        public string state { get; set; }
        public int markup { get; set; }
        public int till_expiration { get; set; }
    }

    public class CheckStatusOrderWareHouseFourDto
    {
        public string status { get; set; }
        public MessageStatusOrderWareHouseFourDto message { get; set; }
    }
}
