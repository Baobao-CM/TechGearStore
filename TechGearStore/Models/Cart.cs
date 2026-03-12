namespace TechGearStore.Models
{
    public class Cart
    {
        public int CartId { get; set; }

        public int UserId { get; set; }

        public ICollection<CartItem>? CartItems { get; set; }
    }
}