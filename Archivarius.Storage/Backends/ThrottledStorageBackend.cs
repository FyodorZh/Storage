using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Archivarius.Storage
{
    public class ThrottledStorageBackend : IStorageBackend
    {
        private readonly IStorageBackend _core;
        private readonly SemaphoreSlim _semaphore;

        public ThrottledStorageBackend(IStorageBackend core, int maxConcurrentOperations = 10)
        {
            _core = core ?? throw new ArgumentNullException(nameof(core));
            _semaphore = new SemaphoreSlim(maxConcurrentOperations, maxConcurrentOperations);
        }

        public event Action<Exception>? OnError
        {
            add => _core.OnError += value;
            remove => _core.OnError -= value;
        }

        public bool ThrowExceptions
        {
            get => _core.ThrowExceptions;
            set => _core.ThrowExceptions = value;
        }

        public async Task<bool> Read<TParam>(FilePath path, TParam param, Func<Stream, TParam, Task> reader)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _core.Read(path, param, reader);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> IsExists(FilePath path)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _core.IsExists(path);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IReadOnlyCollection<FilePath>> GetSubPaths(DirPath path)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _core.GetSubPaths(path);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> Write<TParam>(FilePath path, TParam param, Func<Stream, TParam, Task> writer)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _core.Write(path, param, writer);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> Erase(FilePath path)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _core.Erase(path);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}