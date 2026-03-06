using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Actuarius.Memory;
using Archivarius.Storage.Remote;
using Archivarius.Storage.Test.StorageBackend;
using Pontifex.Transports.Direct;
using Scriba;
using Scriba.Consumers;

namespace Archivarius.Storage.Test
{
    [TestFixture]
    public class IStorageBackend_Test
    {
        [Test]
        public async Task Test_AsyncFile()
        {
            var commands = CommandsGenerator.Generate(10000, 123);
            Tester<IStorageBackend> tester = new();
            var tempDir = System.IO.Path.GetTempPath() + "IStorageBackend_Test_7234289";
            if (System.IO.Directory.Exists(tempDir))
            {
                System.IO.Directory.Delete(tempDir, true);
            }
            System.IO.Directory.CreateDirectory(tempDir);
            FsStorageBackend backend = new(tempDir);
            InMemoryStorageBackend etalon = new();
            backend.ThrowExceptions = false;
            etalon.ThrowExceptions = false;
            var res = await tester.Run(commands, backend, etalon);
            Assert.That(res, Is.True);
        }
        
        [Test]
        public async Task Test_AsyncCompressed()
        {
            var commands = CommandsGenerator.Generate(10000, 123);
            Tester<IStorageBackend> tester = new();

            InMemoryStorageBackend inMemoryBackend = new InMemoryStorageBackend();
            CompressedStorageBackend backend = new(inMemoryBackend);
            InMemoryStorageBackend etalon = new();
            
            backend.ThrowExceptions = false;
            etalon.ThrowExceptions = false;
            
            var res = await tester.Run(commands, backend, etalon);
            Assert.That(res, Is.True);
        }
        
        [Test]
        public async Task Test_Remote()
        {
            Log.AddConsumer(new ConsoleConsumer());
            ILogger logger = new StaticLogger();
            
            var commands = CommandsGenerator.Generate(3000, 123).ToArray();


            AckRawDirectServer directServer = new AckRawDirectServer("test", logger, MemoryRental.Shared);
            ISyncStorageBackend back = new InMemorySyncStorageBackend();
            back = new ThrottledSyncStorageBackend(back, 1);
            RemoteStorageBackendServer backend = new RemoteStorageBackendServer(back);
            backend.Setup(directServer);
            
            IStorageBackend etalon = new InMemoryStorageBackend();
            etalon.ThrowExceptions = false;
            
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 5; ++i)
            {
                int idx = i;
                tasks.Add(Task.Run(async () =>
                {
                    Tester<IStorageBackend> tester = new();
                    
                    AckRawDirectClient directClient = new AckRawDirectClient("test", logger, MemoryRental.Shared);
                    var remoteBackend = RemoteStorageBackendClient.Construct(DirPath.Root.Dir(idx.ToString()), directClient) ?? throw new Exception();
                    remoteBackend.ThrowExceptions = false;
                    
                    try
                    {
                        var res = await tester.Run(commands, remoteBackend, etalon.SubDirectory(DirPath.Root.Dir(idx.ToString())));
                        Assert.That(res, Is.True);
                    }
                    finally
                    {
                        remoteBackend.GracefulShutdown(1000);
                    }
                }));
            }
            
            await Task.WhenAll(tasks);
            Assert.Charlie();
        }
    }
}