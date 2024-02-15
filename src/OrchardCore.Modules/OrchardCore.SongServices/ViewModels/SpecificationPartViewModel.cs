using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.SongServices.ContentParts;

namespace OrchardCore.SongServices.ViewModels
{
    public class SpecificationPartViewModel
    {
        public string Description { get; set; }
        public string AssignerContentItemId { get; set; }
        public string AssigneeContentItemId { get; set; }
        public string Supplement { get; set; }
        public string RootCause { get; set; }
        public bool IsPlanned { get; set; }
        public bool IsIncident { get; set; }
        public bool IsInHouse { get; set; }
        public bool IsOutsourced { get; set; }
        public string ReportStatus { get; set; }
        public string OfferContentItemId { get; set; }
        public string Behavior { get; set; }
        public string Asset { get; set; }
        public string Event { get; set; }
        public string Others { get; set; }
        public string Sender { get; set; }
        public string Writer { get; set; }
        public string Photos { get; set; }
        public string Clips { get; set; }
        public string Audio { get; set; }
        public string Files { get; set; }
        public string LocationContentItemId { get; set; }
        public DateTime? DateTime { get; set; }

        [BindNever]
        public SpecificationPart SpecificationPart { get; set; }
    }
}
