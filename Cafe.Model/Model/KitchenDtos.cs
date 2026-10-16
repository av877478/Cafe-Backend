using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cafe.Model.Model
{
    public class KitchenBoardDto
    {
        public List<OrderDto> NewOrders { get; set; } = new();
        public List<OrderDto> PreparingOrders { get; set; } = new();
        public List<OrderDto> ReadyOrders { get; set; } = new();

        public int NewCount => NewOrders.Count;
        public int PreparingCount => PreparingOrders.Count;
        public int ReadyCount => ReadyOrders.Count;
        public int TotalActiveCount => NewOrders.Count + PreparingOrders.Count + ReadyOrders.Count;
    }

    public class UpdateKitchenStatusDto
    {
        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = string.Empty;
    }
}
