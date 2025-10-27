namespace Hotel_Booking_API.Application.Common
{
    public class PaginationParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public PaginationParameters()
        {
        }

        public PaginationParameters(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber < 1 ? 1 : pageNumber;
            PageSize = pageSize < 1 ? 10 : pageSize;
        }
    }
}
