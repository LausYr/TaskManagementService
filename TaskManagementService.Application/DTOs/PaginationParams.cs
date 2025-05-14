namespace TaskManagementService.Application.DTOs
{
    /// <summary>
    /// Параметры пагинации для запросов
    /// </summary>
    public class PaginationParams
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;
        private int _page = 1;

        /// <summary>
        /// Номер страницы (начиная с 1)
        /// </summary>
        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        /// <summary>
        /// Размер страницы
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 1 : value);
        }
    }
}