using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Entities
{
    public class Payment
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Email { get; set; } = default!;
        public PaymentStatus Status { get; set; }
    }
}
