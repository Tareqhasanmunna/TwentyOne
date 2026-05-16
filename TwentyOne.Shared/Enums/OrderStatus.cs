using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyOne.Shared.Enums
{
    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Shipped = 2,
        Delivered = 3,
        Cancelled = 4,
        Returned = 5
    }
}
