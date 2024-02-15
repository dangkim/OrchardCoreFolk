namespace OrchardCore.SimService.ApiModels
{
    public class Data
    {
        public int id { get; set; }
        public string phone { get; set; }
        public string message { get; set; }
        public bool haveVoice { get; set; }
        public string audioUrl { get; set; }
        public string code { get; set; }
        public int statusOrder { get; set; }
    }

    public class CheckOrderWareHouseTwoDto
    {
        public int status { get; set; }
        public string message { get; set; }
        public Data data { get; set; }
    }
}
