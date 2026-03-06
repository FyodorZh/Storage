using System;

namespace Archivarius.Storage.Remote
{
    public interface IRemoteReadOnlyStorageBackend : IReadOnlyStorageBackend
    {
        void GracefulShutdown(int timeoutMs = 1000);
    }
    
    public interface IRemoteStorageBackend : IRemoteReadOnlyStorageBackend, IStorageBackend
    {
    }
}