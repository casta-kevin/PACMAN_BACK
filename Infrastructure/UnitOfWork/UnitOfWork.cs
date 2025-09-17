using Microsoft.EntityFrameworkCore.Storage;
using Domain.Repositories;
using Domain.UnitOfWork;
using Infrastructure.Persistence;
using Infrastructure.Repository;

namespace Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PacmanDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        // Lazy initialization of repositories
        private IPlayerRepository? _playerRepository;
        private IGameSessionRepository? _gameSessionRepository;

        public UnitOfWork(PacmanDbContext context)
        {
            _context = context;
        }

        public IPlayerRepository PlayerRepository
        {
            get
            {
                _playerRepository ??= new PlayerRepository(_context);
                return _playerRepository;
            }
        }

        public IGameSessionRepository GameSessionRepository
        {
            get
            {
                _gameSessionRepository ??= new GameSessionRepository(_context);
                return _gameSessionRepository;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            try
            {
                await _transaction.CommitAsync();
            }
            catch
            {
                await _transaction.RollbackAsync();
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
                _disposed = true;
            }
        }
    }
}