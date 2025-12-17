using Xunit;
using Zatca.EInvoice.Helpers;
using Zatca.EInvoice.Exceptions;

namespace Zatca.EInvoice.Tests.Helpers;

/// <summary>
/// Tests for Storage helper class with automatic cleanup.
/// </summary>
public class StorageTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly Storage _storage;
    private readonly List<string> _filesToCleanup;

    public StorageTests()
    {
        // Create a unique temp directory for each test run
        _tempDirectory = Path.Combine(Path.GetTempPath(), "ZatcaStorageTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        _storage = new Storage(_tempDirectory);
        _filesToCleanup = new List<string>();
    }

    [Fact]
    public void TestWriteCreatesFile()
    {
        // Arrange
        var fileName = "test-write.txt";
        var content = "Hello, ZATCA!";
        _filesToCleanup.Add(fileName);

        // Act
        _storage.Write(fileName, content);

        // Assert
        var fullPath = Path.Combine(_tempDirectory, fileName);
        Assert.True(File.Exists(fullPath));

        var actualContent = File.ReadAllText(fullPath);
        Assert.Equal(content, actualContent);
    }

    [Fact]
    public void TestAppendAppendsToFile()
    {
        // Arrange
        var fileName = "test-append.txt";
        var initialContent = "First line\n";
        var appendContent = "Second line\n";
        _filesToCleanup.Add(fileName);

        // Act
        _storage.Write(fileName, initialContent);
        _storage.Append(fileName, appendContent);

        // Assert
        var fullPath = Path.Combine(_tempDirectory, fileName);
        var actualContent = File.ReadAllText(fullPath);
        Assert.Equal(initialContent + appendContent, actualContent);
    }

    [Fact]
    public void TestAppendCreatesFileIfNotExists()
    {
        // Arrange
        var fileName = "test-append-new.txt";
        var content = "New content via append";
        _filesToCleanup.Add(fileName);

        // Act
        _storage.Append(fileName, content);

        // Assert
        var fullPath = Path.Combine(_tempDirectory, fileName);
        Assert.True(File.Exists(fullPath));

        var actualContent = File.ReadAllText(fullPath);
        Assert.Equal(content, actualContent);
    }

    [Fact]
    public void TestReadReturnsContent()
    {
        // Arrange
        var fileName = "test-read.txt";
        var content = "Content to read";
        _filesToCleanup.Add(fileName);

        var fullPath = Path.Combine(_tempDirectory, fileName);
        File.WriteAllText(fullPath, content);

        // Act
        var actualContent = _storage.Read(fileName);

        // Assert
        Assert.Equal(content, actualContent);
    }

    [Fact]
    public void TestReadThrowsExceptionIfFileNotFound()
    {
        // Arrange
        var fileName = "non-existent-file.txt";

        // Act & Assert
        var exception = Assert.Throws<ZatcaStorageException>(() =>
            _storage.Read(fileName));

        Assert.Contains("File not found", exception.Message);
        Assert.NotNull(exception.Context);
        Assert.True(exception.Context.ContainsKey("path"));
    }

    [Fact]
    public void TestExistsReturnsTrueForExistingFile()
    {
        // Arrange
        var fileName = "test-exists.txt";
        _filesToCleanup.Add(fileName);

        var fullPath = Path.Combine(_tempDirectory, fileName);
        File.WriteAllText(fullPath, "exists");

        // Act
        var exists = _storage.Exists(fileName);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public void TestExistsReturnsFalseForNonExistingFile()
    {
        // Arrange
        var fileName = "does-not-exist.txt";

        // Act
        var exists = _storage.Exists(fileName);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public void TestDeleteRemovesFile()
    {
        // Arrange
        var fileName = "test-delete.txt";
        var fullPath = Path.Combine(_tempDirectory, fileName);
        File.WriteAllText(fullPath, "to be deleted");

        // Act
        _storage.Delete(fileName);

        // Assert
        Assert.False(File.Exists(fullPath));
    }

    [Fact]
    public void TestDeleteDoesNotThrowIfFileDoesNotExist()
    {
        // Arrange
        var fileName = "non-existent-delete.txt";

        // Act & Assert - Should not throw
        _storage.Delete(fileName);
    }

    [Fact]
    public void TestWriteCreatesNestedDirectories()
    {
        // Arrange
        var fileName = "nested/path/to/file.txt";
        var content = "nested content";
        _filesToCleanup.Add(fileName);

        // Act
        _storage.Write(fileName, content);

        // Assert
        var fullPath = Path.Combine(_tempDirectory, fileName);
        Assert.True(File.Exists(fullPath));

        var actualContent = File.ReadAllText(fullPath);
        Assert.Equal(content, actualContent);
    }

    [Fact]
    public void TestWriteOverwritesExistingFile()
    {
        // Arrange
        var fileName = "test-overwrite.txt";
        var initialContent = "initial";
        var newContent = "overwritten";
        _filesToCleanup.Add(fileName);

        // Act
        _storage.Write(fileName, initialContent);
        _storage.Write(fileName, newContent);

        // Assert
        var fullPath = Path.Combine(_tempDirectory, fileName);
        var actualContent = File.ReadAllText(fullPath);
        Assert.Equal(newContent, actualContent);
    }

    [Fact]
    public void TestStorageWithoutBasePath()
    {
        // Arrange
        var storage = new Storage(); // No base path
        var fullPath = Path.Combine(_tempDirectory, "absolute-path-test.txt");
        var content = "absolute path content";

        // Act
        storage.Write(fullPath, content);

        // Assert
        Assert.True(File.Exists(fullPath));
        var actualContent = storage.Read(fullPath);
        Assert.Equal(content, actualContent);

        // Cleanup
        File.Delete(fullPath);
    }

    [Fact]
    public void TestReadWithUtf8Encoding()
    {
        // Arrange
        var fileName = "test-utf8.txt";
        var content = "Arabic: مرحبا، Chinese: 你好، Emoji: 😊";
        _filesToCleanup.Add(fileName);

        // Act
        _storage.Write(fileName, content);
        var actualContent = _storage.Read(fileName);

        // Assert
        Assert.Equal(content, actualContent);
    }

    [Fact]
    public void TestWriteThrowsOnNullOrEmptyPath()
    {
        // Act & Assert - null path
        Assert.Throws<ArgumentNullException>(() =>
            _storage.Write(null, "content"));

        // Act & Assert - empty path
        Assert.Throws<ArgumentNullException>(() =>
            _storage.Write(string.Empty, "content"));
    }

    [Fact]
    public void TestMultipleAppends()
    {
        // Arrange
        var fileName = "test-multiple-appends.txt";
        _filesToCleanup.Add(fileName);

        // Act
        _storage.Write(fileName, "Line 1\n");
        _storage.Append(fileName, "Line 2\n");
        _storage.Append(fileName, "Line 3\n");
        _storage.Append(fileName, "Line 4\n");

        // Assert
        var content = _storage.Read(fileName);
        Assert.Equal("Line 1\nLine 2\nLine 3\nLine 4\n", content);
    }

    public void Dispose()
    {
        // Clean up all test files and directories
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
