using System;
using System.Text;
using Actuarius.Memory;
using Pontifex.Abstractions.Servers;
using Pontifex.Api;
using Pontifex.StopReasons;
using Pontifex.Utils;

namespace Archivarius.Storage.Remote
{
    public class RemoteStorageBackendServer
    {
        private readonly SyncStorageBackendBroker _storageBroker;

        private IAckRawServer? _transport;

        public event Action<IServerSideStorageApiInstance>? Started;
        public event Action<IServerSideStorageApiInstance>? Stopped;

        public RemoteStorageBackendServer(ISyncStorageBackend storage)
        {
            _storageBroker = new SyncStorageBackendBroker(storage);
        }
        
        public RemoteStorageBackendServer(IReadOnlySyncStorageBackend storage)
        {
            _storageBroker = new SyncStorageBackendBroker(storage);
        }
        
        public bool Setup(IAckRawServer transport)
        {
            _transport = transport;
            transport.Init(new ServerSideApiFactory<RemoteStorageApi>(
                ackData =>
                {
                    if (!ackData.TryPopFirst(out bool writable) || !ackData.TryPopFirst(out IMultiRefReadOnlyByteArray? pathBytes))
                    {
                        return null;
                    }
                    using var pathBytesDisposer = pathBytes.AsDisposable();
                    var pathString = Encoding.UTF8.GetString(pathBytes.ReadOnlyArray, pathBytes.Offset, pathBytes.Count);
                    var path = PathFactory.BuildDir(pathString);

                    SyncStorageBackendBroker.IAccessor? accessor = writable ? _storageBroker.GetWriter(path) : _storageBroker.GetReader(path);

                    if (accessor == null)
                    {
                        transport.Log.w($"Failed to get write access to path: {path}");
                        return null;
                    }

                    var res = new ServerSideStorageApiInstance(accessor, transport.Memory, transport.Log);
                    res.ApiStarted += i => Started?.Invoke((ServerSideStorageApiInstance)i);
                    res.ApiStopped += i => Stopped?.Invoke((ServerSideStorageApiInstance)i);
                    return res;
                }));
            return transport.Start(_ => { });
        }

        public void Stop(string? reason = null)
        {
            _transport?.Stop(new UserIntention("RemoteStorageBackendServer", reason ?? ""));
        }
    }
}