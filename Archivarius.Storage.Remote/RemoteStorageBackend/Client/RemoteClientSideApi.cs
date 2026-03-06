using System.Text;
using Actuarius.Memory;
using Pontifex.Api;
using Pontifex.Utils;
using Scriba;

namespace Archivarius.Storage.Remote
{
    internal class RemoteClientSideApi : ClientSideApi
    {
        private readonly DirPath _rootPath;
        private readonly bool _writable;

        public RemoteClientSideApi(IApiRoot api, DirPath rootPath, bool writable, IMemoryRental memoryRental, ILogger logger) 
            : base(api, memoryRental, logger)
        {
            _rootPath = rootPath;
            _writable = writable;
        }

        protected override void AppendAckData(UnionDataList ackData)
        {
            var rootPathString = _rootPath.FullName;
            var rootPathBytes = Encoding.UTF8.GetBytes(rootPathString);
            var rootPath = new StaticReadOnlyByteArray(rootPathBytes);
            ackData.PutFirst(rootPath);
            ackData.PutFirst(_writable);
            base.AppendAckData(ackData);
        }
    }
}