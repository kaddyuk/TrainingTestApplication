
namespace TrainingTestApplication.Models
{
    public record PartNumber
    {
        public int ID { get; init; }
        public string PartNo { get; init; } = string.Empty;
        public string PartDescription { get; set; } = string.Empty;
        public string? Model { get; init; }
        public string PartClassification { get; init; } = string.Empty;
        public bool? IsRotable { get; init; }
        public string UnitOfMeasure { get; init; } = string.Empty;
        public int StockCount { get; init; }
        public DateTime RecordTimeStampCreated { get; init; }

    }
}
