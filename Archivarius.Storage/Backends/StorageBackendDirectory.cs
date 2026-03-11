using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Archivarius.Storage
{
    public class DirectoryReadOnlyStorageBackend : IReadOnlyStorageBackend
    {
        private readonly IReadOnlyStorageBackend _storage;
        protected readonly DirPath _path;
        
        public event Action<Exception>? OnError;

        public bool ThrowExceptions
        {
            get => _storage.ThrowExceptions;
            set => _storage.ThrowExceptions = value;
        }

        public DirectoryReadOnlyStorageBackend(IReadOnlyStorageBackend storage, DirPath dir)
        {
            if (storage is DirectoryReadOnlyStorageBackend roDirBackend)
            {
                _storage = roDirBackend._storage;
                _path = roDirBackend._path.Dir(dir);
            }
            else
            {
                _storage = storage;
                _path = dir;
            }
            storage.OnError += e => OnError?.Invoke(e);
        }
        
        public Task<bool> Read<TParam>(FilePath path, TParam param, Func<Stream, TParam, Task> reader)
        {
            return _storage.Read(_path.File(path), param, reader);
        }

        public Task<bool> IsExists(FilePath path)
        {
            return _storage.IsExists(_path.File(path));
        }

        public async Task<IReadOnlyList<FilePath>> GetNested(DirPath path, bool recursive)
        {
            var list = await _storage.GetNested(_path.Dir(path), recursive);
            FilePath[] res = new FilePath[list.Count];
            int pos = 0;
            foreach (var element in list)
            {
                if (!element.TryGetRelativeTo(_path, out res[pos++]!))
                {
                    throw new Exception($"Failed to extract '{_path} from '{element}'");
                }
            }

            return res;
        }
    }
    public class DirectoryStorageBackend : DirectoryReadOnlyStorageBackend, IStorageBackend
    {
        private readonly IStorageBackend _storage;

        public DirectoryStorageBackend(IStorageBackend storage, DirPath dir)
            : base((storage is DirectoryStorageBackend dirBackend1) ? dirBackend1._storage : storage,
                (storage is DirectoryStorageBackend dirBackend2) ? dirBackend2._path.Dir(dir) : dir)
        {
            _storage = (storage is DirectoryStorageBackend dirBackend) ? dirBackend._storage : storage;
        }
        
        public Task<bool> Write<TParam>(FilePath path, TParam param, Func<Stream, TParam, Task> writer)
        {
            return _storage.Write(_path.File(path), param, writer);
        }

        public Task<bool> Erase(FilePath path)
        {
            return _storage.Erase(_path.File(path));
        }
    }

    public static class DirectoryStorageBackend_Ext
    {
        public static IReadOnlyStorageBackend SubDirectory(this IReadOnlyStorageBackend storage, DirPath subDirectory)
        {
            return new DirectoryReadOnlyStorageBackend(storage, subDirectory);
        }
        
        public static IStorageBackend SubDirectory(this IStorageBackend storage, DirPath subDirectory)
        {
            return new DirectoryStorageBackend(storage, subDirectory);
        }
    }
}