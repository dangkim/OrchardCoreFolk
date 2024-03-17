using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Data;
public interface IReadOnlySession : ISession
{
    // Add any additional read-only specific methods or properties here if needed
}

