using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using System;

namespace OrchardCore.SongServices.ContentParts;

public class StaffPartViewModel
{
    public string Nickname { get; set; }
    public string AvatarId { get; set; }
    public string Operator { get; set; }
    public string Team { get; set; }
    public string FullName { get; set; }
    public string UserName { get; set; }
    public string BookmarkedReportContentItemIds { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; }
    public string CustomNickname { get; set; }
    public string StaffId { get; set; }
    public DateTime? Birthday { get; set; }
    public DateTime? DateTime { get; set; }

    [BindNever]
    public StaffPart StaffPart { get; set; }
}
