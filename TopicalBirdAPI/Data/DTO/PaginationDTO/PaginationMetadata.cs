namespace TopicalBirdAPI.Data.DTO.PaginationDTO
{
    public class PaginationMetadata
    {
        public int PageNumber { get; set; }
        public int Limit { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
