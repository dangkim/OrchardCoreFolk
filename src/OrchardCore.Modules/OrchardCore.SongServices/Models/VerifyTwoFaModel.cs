using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.SongServices.Models;
public class VerifyTwoFaModel
{
    public string UserId { get; set; }
    public string ConfirmEmailCode { get; set; }
    public string TwoFaCode { get; set; }
}
