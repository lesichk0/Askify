﻿namespace Askify.BusinessLogicLayer.DTO.Pagination
{
    public class PaginationParams
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public const int MaxPageSize = 50;

        public int GetSafePageSize()
        {
            return PageSize > MaxPageSize ? MaxPageSize : PageSize;
        }
    }
}
