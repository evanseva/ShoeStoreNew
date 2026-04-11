using System;

namespace ShoeStore
{
    public class Order
    {
        public int Id { get; set; }
        public string Article { get; set; }
        public string Status { get; set; }
        public string PickupAddress { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? IssueDate { get; set; }
    }
}