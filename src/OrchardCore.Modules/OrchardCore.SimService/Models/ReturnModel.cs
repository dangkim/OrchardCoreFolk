using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.SimService.Models;
public class ReturnModel
{
    public string Error { get; set; }
    public int ErrorCode { get; set; }
    public string Value { get; set; }
}

public enum PurchaseProfileErrorCode
{
    Success = 0,
    NoBalance = 1,
}

public enum GoogleLoginErrorCode
{
    Success = 0,
    ErrorFromExternal = 1,
    CouldNotGetExternalLoginInfo = 2,
    UnableToLoadUser = 3,
    CouldNotCreateUserProfilePart = 4,
}
