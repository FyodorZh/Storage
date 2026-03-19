using System.Threading.Tasks;

namespace Archivarius.Storage
{
    public static class IChainStorage_Ext
    {
        public static async Task<T?> GetLast<T>(this IReadOnlyChainStorage<T> storage)
            where T : class, IDataStruct
        {
            var count = await storage.GetCount();
            if (count > 0)
            {
                return await storage.GetAt(count - 1);
            }

            return null;
        }
    }
}