namespace OrchardCore.SimService.ApiModels
{
    public class DataSmsWareHouseTwo
    {
        public long id { get; set; }
        public string phone { get; set; }
        public string message { get; set; }
        public bool haveVoice { get; set; }
        public string audioUrl { get; set; }
        public string code { get; set; }
        public int statusOrder { get; set; }
    }

    public class SmsWareHouseTwoDto
    {
        public int status { get; set; }
        public string message { get; set; }
        public DataSmsWareHouseTwo data { get; set; }
    }
}
