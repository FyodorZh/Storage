using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Archivarius.Storage
{
    public class ThrottledSyncStorageBackend : ISyncStorageBackend
    {
        private readonly ISyncStorageBackend _core;
        private readonly Semaphore _semaphore;

        public ThrottledSyncStorageBackend(ISyncStorageBackend core, int maxConcurrentOperations = 10)
        {
            _core = core ?? throw new ArgumentNullException(nameof(core));
            _semaphore = new Semaphore(maxConcurrentOperations, maxConcurrentOperations);
        }

        public event Action<Exception> OnError
        {
            add => _core.OnError += value;
            remove => _core.OnError -= value;
        }

        public bool ThrowExceptions
        {
            get => _core.ThrowExceptions;
            set => _core.ThrowExceptions = value;
        }

        public bool Read<TParam>(FilePath path, TParam param, Action<Stream, TParam> reader)
        {
            _semaphore.WaitOne();
            try
            {
                return _core.Read(path, param, reader);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public bool IsExists(FilePath path)
        {
            _semaphore.WaitOne();
            try
            {
                return _core.IsExists(path);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public IReadOnlyList<FilePath> GetSubPaths(DirPath path)
        {
            _semaphore.WaitOne();
            try
            {
                return _core.GetSubPaths(path);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public bool Write<TParam>(FilePath path, TParam param, Action<Stream, TParam> writer)
        {
            _semaphore.WaitOne();
            try
            {
                return _core.Write(path, param, writer);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public bool Erase(FilePath path)
        {
            _semaphore.WaitOne();
            try
            {
                return _core.Erase(path);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
