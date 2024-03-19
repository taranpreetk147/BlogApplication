namespace BlogApplication.Model
{
    public class DisplayProducts
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public byte[] ImageURL { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}
