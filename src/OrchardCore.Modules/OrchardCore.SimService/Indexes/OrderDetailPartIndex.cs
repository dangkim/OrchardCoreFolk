using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.SimService.Models;
using YesSql.Indexes;

namespace OrchardCore.SimService.Indexes
{
    public class OrderDetailPartIndex : MapIndex
    {
        public string ContentItemId { get; set; }
        public long OrderId { get; set; }
        public int InventoryId { get; set; }
        public string Phone { get; set; }
        public string Operator { get; set; }
        public string Product { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created_at { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string Category { get; set; }
    }

    public class OrderDetailPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context) =>
            context.For<OrderDetailPartIndex>().Map(contentItem =>
            {
                var orderDetailPart = contentItem.As<OrderDetailPart>();

                return orderDetailPart == null
                    ? null
                    : new OrderDetailPartIndex
                    {
                        ContentItemId = contentItem.ContentItemId,
                        InventoryId = orderDetailPart.InventoryId,
                        OrderId = orderDetailPart.OrderId,
                        Phone = orderDetailPart.Phone,
                        Operator = orderDetailPart.Operator,
                        Product = orderDetailPart.Product,
                        Price = orderDetailPart.Price,
                        Status = orderDetailPart.Status,
                        Expires = orderDetailPart.Expires,
                        Created_at = orderDetailPart.Created_at,
                        Country = orderDetailPart.Country,
                        Email = orderDetailPart.Email,
                        UserId = orderDetailPart.UserId,
                        UserName = orderDetailPart.UserName,
                        Category = orderDetailPart.Category
                    };
            });
    }
}
