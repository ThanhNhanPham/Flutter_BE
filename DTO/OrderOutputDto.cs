namespace FoodOdering_BE.DTO
{
    public class OrderOutputDto
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime OrderTime { get; set; }
        public string? PhoneNumber { get; set; }
        //Thêm trường sdt

        public List<OrderDetailOutputDto> OrderDetails { get; set; }
    }

}
