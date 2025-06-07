using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Data
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = default!;
        public string Database { get; set; } = default!;
    }
}
