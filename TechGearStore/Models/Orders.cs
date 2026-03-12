using System.ComponentModel.DataAnnotations;

namespace TechGearStore.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public string CustomerName { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";

        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}