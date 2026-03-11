using System;

namespace Archivarius.Storage
{
    public class FilePath : Path
    {
        public DirPath Parent => _parent!;
        
        public FilePath(DirPath parent, string name)
            :base(parent, name, false)
        {
            if (name.Contains("/") || string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException();
            }
        }
        
        public bool TryGetRelativeTo(DirPath path, out FilePath? relativePath)
        {
            if (FullName.StartsWith(path.FullName) && FullName.Length >= path.FullName.Length && FullName[path.FullName.Length] == '/')
            {
                relativePath = PathFactory.BuildFile(FullName.Substring(path.FullName.Length));
                return true;
            }

            relativePath = null;
            return false;
        }
    }
}