using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace LegalSystem.DataAccess.Repositories.Base
{
    public class Repository<TEntity> where TEntity : class
    {
        protected readonly ApplicationDbContext context;

        public Repository(ApplicationDbContext context)
        {
            this.context = context;
        }


        public Task<List<TEntity>> GetAllAsync()
        {
            return context.Set<TEntity>().ToListAsync();
        }
        public Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return context.Set<TEntity>().Where(predicate).AsNoTracking().ToListAsync();
        }
        public virtual IQueryable<TEntity> GetAllQuerable(Expression<Func<TEntity, bool>>? predicate = null)
        {
            return context.Set<TEntity>();
        }


        public ValueTask<TEntity?> GetByIdAsync(long id)
        {
            return context.Set<TEntity>().FindAsync(id);

        }
        public Task<TEntity?> GetByPredicateNoTrackingAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return context.Set<TEntity>().Where(predicate).AsNoTracking().FirstOrDefaultAsync();
        }

        
        public ValueTask<EntityEntry<TEntity>> AddAsync(TEntity entity)
        {
            return context.Set<TEntity>().AddAsync(entity);
        }
        public Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            return context.Set<TEntity>().AddRangeAsync(entities);
        }


        public void Update(TEntity entity)
        {
            if (context.Entry(entity).State != EntityState.Added)
            {
                context.Set<TEntity>().Attach(entity);
                context.Entry(entity).State = EntityState.Modified;
            }
        }

        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            context.Set<TEntity>().UpdateRange(entities);
        }


        public void Delete(TEntity entity)
        {
            context.Set<TEntity>().Remove(entity);
        }
        public void DeleteRange(IEnumerable<TEntity> entities)
        {
            context.Set<TEntity>().RemoveRange(entities);
        }

    }
}
