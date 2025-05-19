using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CurrencyExchange_Practice.Core.Entities;

namespace CurrencyExchange_Practice.Core.Specification
{
    public abstract class BaseSpecification<T> : ISpecification<T> where T : BaseEntity
    {
        public BaseSpecification(Expression<Func<T, bool>>? criteria)
        {
            Criteria = criteria;
        }
        public Expression<Func<T, bool>>? Criteria {get;}

        public List<Expression<Func<T, object>>> Includes { get; } = new();
        public bool Tracked { get; set; }
    }
}
