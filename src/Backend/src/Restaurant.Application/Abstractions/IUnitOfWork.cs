﻿using System.Data.Common;

namespace Restaurant.Application.Abstractions
{
    public interface IUnitOfWork : IDisposable
    {
        public Task<DbTransaction> BeginTransactionAsync();
        public DbTransaction BeginTransaction();
        Task CommitAsync();
        void Commit();
        Task RollbackAsync();
        void Rollback();
    }
}
