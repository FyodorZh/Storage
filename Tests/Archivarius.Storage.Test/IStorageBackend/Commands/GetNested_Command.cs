using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Archivarius.Storage.Test.StorageBackend
{
    public class GetNested_Command : StorageBackendTestCommand<GetNested_Command.PathArray>
    {
        private readonly DirPath _path;
        private readonly bool _recursive;
        
        public GetNested_Command(DirPath path, bool recursive)
        {
            _path = path;
            _recursive = recursive;
        }
        
        protected override async Task<PathArray> InvokeOnSubject(IStorageBackend subject)
        {
            return new PathArray() { Paths = new List<FilePath>(await subject.GetNested(_path, _recursive)) };
        }
        
        public override string ToString()
        {
            return "GetSubPath(" + _path + ", " + _recursive + ")";
        }
        
        public struct PathArray : IEquatable<PathArray>
        {
            public List<FilePath> Paths = [];
            public PathArray() { }

            public bool Equals(PathArray other)
            {
                return Paths.SequenceEqual(other.Paths);
            }
        }
    }
}