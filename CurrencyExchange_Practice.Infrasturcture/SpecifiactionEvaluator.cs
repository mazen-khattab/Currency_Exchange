using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CurrencyExchange_Practice.Core.Entities;
using CurrencyExchange_Practice.Core.Specification;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange_Practice.Infrasturcture
{
    public class SpecifiactionEvaluator<T> where T : BaseEntity
    {
        public static IQueryable<T> GetQuery(IQueryable<T> query, ISpecification<T> spec)
        {
            query = query.AsQueryable();

            if (spec.Criteria is { })
            {
                query = query.Where(spec.Criteria);
            }

            query = spec.Includes.Aggregate(query, (currentQuery, includeQuery) => currentQuery.Include(includeQuery));

            if (!spec.Tracked)
            {
                query.AsNoTracking();
            }

            return query;
        }
    }
}
