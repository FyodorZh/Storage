namespace Archivarius.Storage.Remote
{
    public enum CommandType
    {
        Read = 0, IsExists, GetNested, Write, Erase
    }

    public static class CommandType_Ext
    {
        private static string[] _hash = {
            nameof(CommandType.Read),
            nameof(CommandType.IsExists),
            nameof(CommandType.GetNested),
            nameof(CommandType.Write),
            nameof(CommandType.Erase)
        };

        public static string ToString2(this CommandType type)
        {
            return _hash[(int)type];
        }
    }
}