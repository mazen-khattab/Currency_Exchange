using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CurrencyExchange_Practice.Core.Entities;

namespace CurrencyExchange_Practice.Core.Specification
{
    public interface ISpecification<T> where T : BaseEntity
    {
        Expression<Func<T, bool>> Criteria { get; }
        List<Expression<Func<T, object>>> Includes { get; }
        bool Tracked { get; set; }
    }
}
