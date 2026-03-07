using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Archivarius.Storage.Remote
{
    public static class IRemoteStorageBackend_Ext
    {
        public static IRemoteReadOnlyStorageBackend Wrap(this IRemoteReadOnlyStorageBackend storage, Func<IReadOnlyStorageBackend, IReadOnlyStorageBackend> wrapper)
        {
            if (storage is RemoteReadOnlyStorageBackendWrapper rw)
            {
                return new RemoteReadOnlyStorageBackendWrapper(rw.Core, wrapper.Invoke(rw.Wrapped));
            }
            return new RemoteReadOnlyStorageBackendWrapper(storage, wrapper.Invoke(storage));
        }
        
        public static IRemoteStorageBackend Wrap(this IRemoteStorageBackend storage, Func<IStorageBackend, IStorageBackend> wrapper)
        {
            if (storage is RemoteStorageBackendWrapper ww)
            {
                return new RemoteStorageBackendWrapper(ww.Core, wrapper.Invoke(ww.Wrapped));
            }
            return new RemoteStorageBackendWrapper(storage, wrapper.Invoke(storage));
        }
        
        private class RemoteReadOnlyStorageBackendWrapper : IRemoteReadOnlyStorageBackend
        {
            private readonly IRemoteReadOnlyStorageBackend _core;
            private readonly IReadOnlyStorageBackend _wrapped;
            
            public IRemoteReadOnlyStorageBackend Core => _core;
            public IReadOnlyStorageBackend Wrapped => _wrapped;
            
            public RemoteReadOnlyStorageBackendWrapper(IRemoteReadOnlyStorageBackend core, IReadOnlyStorageBackend wrapped)
            {
                _core = core;
                _wrapped = wrapped;
            }

            public event Action<Exception> OnError
            {
                add => _wrapped.OnError += value;
                remove => _wrapped.OnError -= value;
            }

            public bool ThrowExceptions
            {
                get => _wrapped.ThrowExceptions;
                set => _wrapped.ThrowExceptions = value;
            }

            public Task<bool> Read<TParam>(FilePath path, TParam param, Func<Stream, TParam, Task> reader)
            {
                return _wrapped.Read(path, param, reader);
            }

            public Task<bool> IsExists(FilePath path)
            {
                return _wrapped.IsExists(path);
            }

            public Task<IReadOnlyCollection<FilePath>> GetNested(DirPath path, bool recursive)
            {
                return _wrapped.GetNested(path, recursive);
            }

            public void GracefulShutdown(int timeoutMs)
            {
                _core.GracefulShutdown(timeoutMs);
            }
        }

        private class RemoteStorageBackendWrapper : RemoteReadOnlyStorageBackendWrapper, IRemoteStorageBackend
        {
            private readonly IRemoteStorageBackend _core;
            private readonly IStorageBackend _wrapped;
            
            public new IRemoteStorageBackend Core => _core;
            public new IStorageBackend Wrapped => _wrapped;

            public RemoteStorageBackendWrapper(IRemoteStorageBackend core, IStorageBackend wrapped)
                :base(core, wrapped)
            {
                _core = core;
                _wrapped = wrapped;
            }

            public Task<bool> Write<TParam>(FilePath path, TParam param, Func<Stream, TParam, Task> writer)
            {
                return _wrapped.Write(path, param, writer);
            }

            public Task<bool> Erase(FilePath path)
            {
                return _wrapped.Erase(path);
            }
        }
    }
}