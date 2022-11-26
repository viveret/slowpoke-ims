using System;
using Xunit;

using slowpoke.core.Models.Diff;
using slowpoke.core.Models.Node.Docs.ReadOnlyLocal;
using SlowPokeIMS.Core.Services.Node.Docs.ReadOnly;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using SlowPokeIMS.Core.Services.Node.Docs;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace SlowPokeIMS.Core.Tests.Models.Diff;

public class DocumentDiffTests
{
    [Fact]
    public async Task DocumentDiff_Constructor_NullThrowsException()
    {
        var config = new Config();
        var broadcastProvider = new StubBroadcastProviderResolver();
        var path = "/awd.txt".AsIDocPath(config);
        var inMemoryGenericDocumentRepository = new InMemoryGenericDocumentRepository();
        var resolver = new GenericReadOnlyDocumentResolver(config, broadcastProvider, inMemoryGenericDocumentRepository);
        var oldDoc = new GenericReadOnlyDocument(resolver, broadcastProvider.MemCached, path);
        var newDoc = new GenericReadOnlyDocument(resolver, broadcastProvider.MemCached, path);

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await DocumentDiff.Create(null, null, true, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await DocumentDiff.Create(oldDoc, null, true, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await DocumentDiff.Create(null, newDoc, true, CancellationToken.None));
    }

    [Fact]
    public async Task DocumentDiff_Constructor_Default_NoChange()
    {
        var config = new Config();
        var broadcastProvider = new StubBroadcastProviderResolver();
        var path = "/awd.txt".AsIDocPath(config);
        var inMemoryGenericDocumentRepository = new InMemoryGenericDocumentRepository();
        var resolver = new GenericReadOnlyDocumentResolver(config, broadcastProvider, inMemoryGenericDocumentRepository);
        var oldDoc = new GenericReadOnlyDocument(resolver, broadcastProvider.MemCached, path);
        var newDoc = new GenericReadOnlyDocument(resolver, broadcastProvider.MemCached, path);
        
        var diff = await DocumentDiff.Create(oldDoc, newDoc, true, CancellationToken.None);
        Assert.NotNull(diff);
        Assert.False(diff.HasChanged);
        Assert.Equal(oldDoc, diff.Old);
        Assert.Equal(newDoc, diff.New);
        Assert.NotNull(diff.OriginalHash);
        Assert.NotNull(diff.NewHash);
    }

    [Fact]
    public async Task DocumentDiff_Constructor_DoesNotExistBeforeAndAfter_NoChange()
    {
        var config = new Config();
        var broadcastProvider = new StubBroadcastProviderResolver();
        var path = "/awd.txt".AsIDocPath(config);
        var inMemoryGenericDocumentRepository = new InMemoryGenericDocumentRepository();
        var resolver = new GenericReadOnlyDocumentResolver(config, broadcastProvider, inMemoryGenericDocumentRepository);
        var oldDoc = new StubReadOnlyDocument(false, path, config);
        var newDoc = new StubReadOnlyDocument(false, path, config);
        
        var diff = await DocumentDiff.Create(oldDoc, newDoc, true, CancellationToken.None);
        Assert.NotNull(diff);
        Assert.False(diff.HasChanged);
        Assert.Equal(oldDoc, diff.Old);
        Assert.Equal(newDoc, diff.New);
        Assert.NotNull(diff.OriginalHash);
        Assert.NotNull(diff.NewHash);
    }

    [Fact]
    public async Task DocumentDiff_Constructor_MovedFile_IsChange()
    {
        var config = new Config();
        var broadcastProvider = new StubBroadcastProviderResolver();
        var pathOld = "/awd.txt".AsIDocPath(config);
        var pathNew = "/awd2.txt".AsIDocPath(config);
        var inMemoryGenericDocumentRepository = new InMemoryGenericDocumentRepository();
        var resolver = new GenericReadOnlyDocumentResolver(config, broadcastProvider, inMemoryGenericDocumentRepository);
        var oldDoc = new GenericReadOnlyDocument(resolver, broadcastProvider.MemCached, pathOld);
        var newDoc = new GenericReadOnlyDocument(resolver, broadcastProvider.MemCached, pathNew);
        
        var diff = await DocumentDiff.Create(oldDoc, newDoc, true, CancellationToken.None);
        Assert.NotNull(diff);
        Assert.True(diff.HasChanged);
        Assert.Equal(oldDoc, diff.Old);
        Assert.Equal(newDoc, diff.New);
        Assert.NotNull(diff.OriginalHash);
        Assert.NotNull(diff.NewHash);
    }

    [Fact]
    public async Task DocumentDiff_Constructor_NewFile_IsChange()
    {
        var config = new Config();
        var broadcastProvider = new StubBroadcastProviderResolver();
        var pathOld = "/awd.txt".AsIDocPath(config);
        var pathNew = "/awd.txt".AsIDocPath(config);
        var inMemoryGenericDocumentRepository = new InMemoryGenericDocumentRepository();

        var guidNew = inMemoryGenericDocumentRepository.CreateFile(pathNew.PathValue);
        inMemoryGenericDocumentRepository.FileData[guidNew] = System.Text.Encoding.ASCII.GetBytes("new");

        var resolver = new GenericReadOnlyDocumentResolver(config, broadcastProvider, inMemoryGenericDocumentRepository);
        var oldDoc = new StubReadOnlyDocument(false, pathOld, config);
        var newDoc = new GenericReadOnlyDocument(resolver, broadcastProvider.MemCached, pathNew);
        
        Assert.False(await oldDoc.Exists);
        Assert.True(await newDoc.Exists);

        var diff = await DocumentDiff.Create(oldDoc, newDoc, true, CancellationToken.None);
        Assert.NotNull(diff);
        Assert.True(await diff.CreatedOrDeleted());
        Assert.False(diff.ChangedPath);
        Assert.True(diff.HasChanged);
        Assert.Equal(oldDoc, diff.Old);
        Assert.Equal(newDoc, diff.New);
        Assert.NotNull(diff.OriginalHash);
        Assert.NotNull(diff.NewHash);
    }

    [Fact]
    public async Task DocumentDiff_Constructor_DeletedFile_IsChange()
    {
        var config = new Config();
        var broadcastProvider = new StubBroadcastProviderResolver();
        var pathOld = "/awd.txt".AsIDocPath(config);
        var pathNew = "/awd.txt".AsIDocPath(config);
        var inMemoryGenericDocumentRepository = new InMemoryGenericDocumentRepository();
        
        var guidOld = inMemoryGenericDocumentRepository.CreateFile(pathOld.PathValue);
        var guidNew = inMemoryGenericDocumentRepository.CreateFile(pathNew.PathValue);
        inMemoryGenericDocumentRepository.FileData[guidOld] = System.Text.Encoding.ASCII.GetBytes("old");

        // this does not get Exists property correctly
        var resolver = new GenericReadOnlyDocumentResolver(config, broadcastProvider, inMemoryGenericDocumentRepository);
        var oldDoc = new GenericReadOnlyDocument(resolver, broadcastProvider.MemCached, pathOld);
        var newDoc = new StubReadOnlyDocument(false, pathNew, config);
        
        Assert.True(await oldDoc.Exists);
        Assert.False(await newDoc.Exists);

        var diff = await DocumentDiff.Create(oldDoc, newDoc, true, CancellationToken.None);
        Assert.NotNull(diff);
        Assert.False(diff.ChangedPath);
        Assert.True(await diff.CreatedOrDeleted());
        Assert.True(diff.HasChanged);
        Assert.Equal(oldDoc, diff.Old);
        Assert.Equal(newDoc, diff.New);
        Assert.NotNull(diff.OriginalHash);
        Assert.NotNull(diff.NewHash);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DocumentDiff_Constructor_ContentsChanged_IsChange(bool fullStreamDiff)
    {
        var config = new Config();
        var broadcastProvider = new StubBroadcastProviderResolver();
        var pathOld = "/awd.txt".AsIDocPath(config);
        var pathNew = "/awd.txt".AsIDocPath(config);
        
        var inMemoryGenericDocumentRepository = new InMemoryGenericDocumentRepository();
        var guidOld = inMemoryGenericDocumentRepository.CreateFile(pathOld.PathValue);

        inMemoryGenericDocumentRepository.FileData[guidOld] = System.Text.Encoding.ASCII.GetBytes("old");
        using var newStream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes("new"));

        var resolver = new GenericReadOnlyDocumentResolver(config, broadcastProvider, inMemoryGenericDocumentRepository);
        var oldDoc = new GenericReadOnlyDocument(resolver, broadcastProvider.MemCached, pathOld);
        var newDoc = new GenericReadOnlyDocument(resolver, broadcastProvider.MemCached, pathNew) { TmpStream = newStream };
        
        var diff = await DocumentDiff.Create(oldDoc, newDoc, fullStreamDiff, CancellationToken.None);
        Assert.NotNull(diff);
        Assert.NotNull(diff.OriginalHash);
        Assert.NotNull(diff.NewHash);
        Assert.NotEqual(diff.OriginalHash, diff.NewHash);
        Assert.False(diff.ChangedPath);
        Assert.False(await diff.CreatedOrDeleted());
        Assert.True(diff.HasChanged);
        Assert.Equal(oldDoc, diff.Old);
        Assert.Equal(newDoc, diff.New);
    }

    [Fact]
    public async Task DocumentDiff_Constructor_ContentsSame_NoChange()
    {
        var config = new Config();
        var broadcastProvider = new StubBroadcastProviderResolver();
        var pathOld = "/awd.txt".AsIDocPath(config);
        var pathNew = "/awd.txt".AsIDocPath(config);
        
        var inMemoryGenericDocumentRepository = new InMemoryGenericDocumentRepository();
        var guidOld = inMemoryGenericDocumentRepository.CreateFile(pathOld.PathValue);
        var guidNew = inMemoryGenericDocumentRepository.CreateFile(pathNew.PathValue);

        Assert.NotEqual(guidOld, guidNew);

        inMemoryGenericDocumentRepository.FileData[guidOld] = System.Text.Encoding.ASCII.GetBytes("same");
        inMemoryGenericDocumentRepository.FileData[guidNew] = System.Text.Encoding.ASCII.GetBytes("same");

        var resolver = new GenericReadOnlyDocumentResolver(config, broadcastProvider, inMemoryGenericDocumentRepository);
        var oldDoc = new GenericReadOnlyDocument(resolver, broadcastProvider.MemCached, pathOld);
        var newDoc = new GenericReadOnlyDocument(resolver, broadcastProvider.MemCached, pathNew);
        
        var diff = await DocumentDiff.Create(oldDoc, newDoc, true, CancellationToken.None);
        Assert.NotNull(diff);
        Assert.False(diff.ChangedPath);
        Assert.False(await diff.CreatedOrDeleted());
        Assert.False(diff.HasChanged);
        Assert.Equal(oldDoc, diff.Old);
        Assert.Equal(newDoc, diff.New);
        Assert.NotNull(diff.OriginalHash);
        Assert.NotNull(diff.NewHash);
    }
}