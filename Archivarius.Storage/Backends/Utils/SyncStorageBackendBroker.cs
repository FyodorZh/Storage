using System;
using System.Collections.Generic;

namespace Archivarius.Storage
{
    public class SyncStorageBackendBroker
    {
        private readonly IReadOnlySyncStorageBackend _reader;
        private readonly ISyncStorageBackend? _writer;
        private readonly HashSet<string> _writePaths = new();

        public SyncStorageBackendBroker(IReadOnlySyncStorageBackend core)
        {
            _reader = core;
        }
        
        public SyncStorageBackendBroker(ISyncStorageBackend core)
        {
            _reader = _writer = core;
        }

        public IAccessor GetReader(DirPath path)
        {
            return new Accessor(this, path, _reader.SubDirectory(path));
        }

        public IAccessor? GetWriter(DirPath path)
        {
            if (_writer == null)
            {
                return null;
            }
            lock (_writePaths)
            {
                var pathStr = path.FullName + "/";
                foreach (var lockedPath in _writePaths)
                {
                    if (pathStr.StartsWith(lockedPath))
                    {
                        return null;
                    }
                }

                _writePaths.Add(pathStr);
                return new Accessor(this, path, _writer.SubDirectory(path));
            }
        }

        private bool FreeWriter(DirPath path)
        {
            lock (_writePaths)
            {
                return _writePaths.Remove(path.FullName + "/");
            }
        }
        
        public interface IAccessor : IDisposable
        {
            DirPath Path { get; }
            IReadOnlySyncStorageBackend? Reader { get; }
            ISyncStorageBackend? Writer { get; }
        }

        private class Accessor : IAccessor
        {
            private readonly SyncStorageBackendBroker _broker;
            
            public DirPath Path { get; }
            
            public IReadOnlySyncStorageBackend? Reader { get; private set; }
            public ISyncStorageBackend? Writer { get; private set; }
            
            public Accessor(SyncStorageBackendBroker owner, DirPath path, IReadOnlySyncStorageBackend reader)
            {
                _broker = owner;
                Path = path;
                Reader = reader;
            }

            public Accessor(SyncStorageBackendBroker owner, DirPath path, ISyncStorageBackend writer)
            {
                _broker = owner;
                Path = path;
                Reader = writer;
                Writer = writer;
            }

            public void Dispose()
            {
                if (Writer != null)
                {
                    Writer = null;
                    _broker.FreeWriter(Path);
                }

                Reader = null;
            }
        }
    }
}