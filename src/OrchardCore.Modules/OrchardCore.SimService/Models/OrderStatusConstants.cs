using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.SimService.Models;
public static class StatusConstants
{
    public const string PENDING = "pending";
    public const string CANCELED = "canceled";
    public const string RECEIVED = "received";
    public const string BANNED = "banned";
    public const string TIMEOUT = "timeout";
    public const string FINISHED = "finished";
    public const string STATUSWAITCODE = "STATUS_WAIT_CODE";
    public const string STATUSWAITRETRY = "STATUS_WAIT_RETRY";
    public const string STATUSCANCEL = "STATUS_CANCEL";
    public const string STATUSOK = "STATUS_OK";
}
