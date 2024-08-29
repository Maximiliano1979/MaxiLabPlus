using Microsoft.EntityFrameworkCore;

namespace iLabPlus.Tests
{
    public interface ISetup<TContext, T> where TContext : DbContext
    {
        void Returns<TEntity>(DbSet<TEntity> @object) where TEntity : class;
    }
}