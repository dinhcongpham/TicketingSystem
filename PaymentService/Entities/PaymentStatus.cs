using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Entities
{
    public enum PaymentStatus
    {
        Processing,
        Paid,
        Failed,
        Refunded
    }
}
