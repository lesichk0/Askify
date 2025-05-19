using System;
using System.Collections.Generic;
using System.Linq;

namespace Askify.BusinessLogicLayer.Helpers
{
    public class PaginationHelper<T>
    {
        public IEnumerable<T> Items { get; }
        public int TotalCount { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public int TotalPages { get; }
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;

        public PaginationHelper(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            Items = items;
        }

        public static PaginationHelper<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PaginationHelper<T>(items, count, pageNumber, pageSize);
        }
    }
}
