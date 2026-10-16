using System.ComponentModel.DataAnnotations;

namespace Cafe.Model.Model
{
    public class TableDto
    {
        public string Id { get; set; } = string.Empty;
        public int Number { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; } = "Available";
    }

    public class CreateTableDto
    {
        [Required(ErrorMessage = "Table number is required.")]
        [Range(1, 9999, ErrorMessage = "Table number must be at least 1.")]
        public int Number { get; set; }

        [Required(ErrorMessage = "Table capacity is required.")]
        [Range(1, 100, ErrorMessage = "Capacity must be between 1 and 100.")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Table status is required.")]
        [RegularExpression("^(Available|Occupied|Reserved|Out of Service)$", 
            ErrorMessage = "Status must be 'Available', 'Occupied', 'Reserved', or 'Out of Service'.")]
        public string Status { get; set; } = "Available";
    }

    public class UpdateTableDto
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Table number is required.")]
        [Range(1, 9999, ErrorMessage = "Table number must be at least 1.")]
        public int Number { get; set; }

        [Required(ErrorMessage = "Table capacity is required.")]
        [Range(1, 100, ErrorMessage = "Capacity must be between 1 and 100.")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Table status is required.")]
        [RegularExpression("^(Available|Occupied|Reserved|Out of Service)$", 
            ErrorMessage = "Status must be 'Available', 'Occupied', 'Reserved', or 'Out of Service'.")]
        public string Status { get; set; } = "Available";
    }

    public class UpdateTableStatusDto
    {
        [Required(ErrorMessage = "Table status is required.")]
        [RegularExpression("^(Available|Occupied|Reserved|Out of Service)$", 
            ErrorMessage = "Status must be 'Available', 'Occupied', 'Reserved', or 'Out of Service'.")]
        public string Status { get; set; } = "Available";
    }
}
