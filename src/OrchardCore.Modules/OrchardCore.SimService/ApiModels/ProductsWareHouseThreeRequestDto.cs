using System.Collections.Generic;

namespace OrchardCore.SimService.ApiModels
{
    public class Result
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Cost { get; set; }
    }

    public class ProductsWareHouseThreeRequestDto
    {
        public int ResponseCode { get; set; }
        public string Msg { get; set; }
        public List<Result> Result { get; set; }
    }

}
