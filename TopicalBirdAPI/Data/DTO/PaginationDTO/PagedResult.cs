namespace TopicalBirdAPI.Data.DTO.PaginationDTO
{
    public class PagedResult<T>
    {
        public PaginationMetadata Pagination { get; set; }
        public IEnumerable<T> Items { get; set; }
    }
}
