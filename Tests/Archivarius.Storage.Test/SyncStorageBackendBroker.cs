namespace Archivarius.Storage.Test
{
    [TestFixture]
    public class SyncStorageBackendBrokerTest
    {
        [Test]
        public void Test()
        {
            InMemorySyncStorageBackend core = new();
            SyncStorageBackendBroker broker = new(core);

            SyncStorageBackendBroker.IAccessor? a1, a2, a3, a4, a5, a6, a7;
            
            Assert.That(a1 = broker.GetWriter(PathFactory.BuildDir("/dir1")), Is.Not.Null);
            Assert.That(a1?.Writer, Is.Not.Null);
            Assert.That(a2 = broker.GetWriter(PathFactory.BuildDir("/dir2")), Is.Not.Null);
            Assert.That(a2?.Writer, Is.Not.Null);
            Assert.That(a3 = broker.GetWriter(PathFactory.BuildDir("/dir3/dir")), Is.Not.Null);
            Assert.That(a3?.Writer, Is.Not.Null);
            Assert.That(a4 = broker.GetWriter(PathFactory.BuildDir("/dir2/dir")), Is.Null);
            Assert.That(a5 = broker.GetWriter(PathFactory.BuildDir("/dir1")), Is.Null);
            Assert.That(a6 = broker.GetWriter(PathFactory.BuildDir("/dir3/d")), Is.Not.Null);
            Assert.That(a6?.Writer, Is.Not.Null);
            a2!.Dispose();
            Assert.That(a7 = broker.GetWriter(PathFactory.BuildDir("/dir2/dir")), Is.Not.Null);
            Assert.That(a7?.Writer, Is.Not.Null);
        }
    }
}