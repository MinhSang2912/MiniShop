using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniShop.Core.Entities
{
    public class OrderItem
    {
        public string ProductId { get; set; } = default!;
        public int Qty { get; set; }
        public decimal Price { get; set; }
    }
}
