# Contributing to Zatca.EInvoice

First off, thank you for considering contributing to Zatca.EInvoice! It's people like you that make this library better for everyone.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Setup](#development-setup)
- [Pull Request Process](#pull-request-process)
- [Style Guidelines](#style-guidelines)
- [Community](#community)

## Code of Conduct

This project and everyone participating in it is governed by our [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Git
- Your favorite IDE (Visual Studio, VS Code with C# extension, or JetBrains Rider)

### Finding Issues to Work On

- Look for issues labeled [`good first issue`](../../issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22) for beginner-friendly tasks
- Issues labeled [`help wanted`](../../issues?q=is%3Aissue+is%3Aopen+label%3A%22help+wanted%22) are open for community contributions
- Feel free to ask questions on any issue before starting work

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check existing issues to avoid duplicates. When you create a bug report, include as many details as possible using our [bug report template](.github/ISSUE_TEMPLATE/bug_report.md).

**Great bug reports include:**

- A clear and descriptive title
- Steps to reproduce the behavior
- Expected behavior vs actual behavior
- Code samples or test cases demonstrating the issue
- Your environment details (.NET version, OS, etc.)

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, use our [feature request template](.github/ISSUE_TEMPLATE/feature_request.md) and include:

- A clear and descriptive title
- Detailed description of the proposed functionality
- Explanation of why this enhancement would be useful
- Examples of how it would be used

### Code Contributions

1. Fork the repository
2. Create a feature branch from `main`
3. Make your changes
4. Write or update tests as needed
5. Ensure all tests pass
6. Submit a pull request

## Development Setup

1. **Clone your fork:**
   ```bash
   git clone https://github.com/YOUR_USERNAME/Zatca.EInvoice.git
   cd Zatca.EInvoice
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Build the solution:**
   ```bash
   dotnet build
   ```

4. **Run tests:**
   ```bash
   dotnet test
   ```

5. **Check code formatting:**
   ```bash
   dotnet format --verify-no-changes
   ```

6. **Fix formatting issues:**
   ```bash
   dotnet format
   ```

## Pull Request Process

1. **Create a feature branch:**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes** following our style guidelines

3. **Write tests** for any new functionality

4. **Ensure all tests pass:**
   ```bash
   dotnet test
   ```

5. **Format your code:**
   ```bash
   dotnet format
   ```

6. **Commit your changes** with a clear commit message:
   ```bash
   git commit -m "Add feature: description of your changes"
   ```

7. **Push to your fork:**
   ```bash
   git push origin feature/your-feature-name
   ```

8. **Open a Pull Request** against the `main` branch

### Pull Request Requirements

- [ ] All tests pass
- [ ] Code follows the style guidelines
- [ ] New code has appropriate test coverage
- [ ] Documentation is updated if needed
- [ ] Commit messages are clear and descriptive

### Review Process

1. A maintainer will review your PR
2. Address any requested changes
3. Once approved, a maintainer will merge your PR

## Style Guidelines

### C# Code Style

We follow the standard .NET coding conventions with some additional guidelines:

- Use `var` when the type is obvious from the right side
- Use meaningful names for variables, methods, and classes
- Keep methods focused and small (single responsibility)
- Use async/await for I/O operations
- Prefer immutability where practical
- Use nullable reference types (`#nullable enable`)

### Code Formatting

We use `dotnet format` with the default .editorconfig settings. Run formatting before committing:

```bash
dotnet format
```

### Documentation

- Use XML documentation comments for public APIs
- Keep comments meaningful and up-to-date
- Update README and docs when adding new features

### Commit Messages

- Use the present tense ("Add feature" not "Added feature")
- Use the imperative mood ("Move cursor to..." not "Moves cursor to...")
- Limit the first line to 72 characters or less
- Reference issues and pull requests when relevant

**Good examples:**
- `Add support for credit note invoices`
- `Fix XML signature validation for large invoices`
- `Update dependencies to latest versions`

## Testing Guidelines

- Write unit tests for new functionality
- Ensure existing tests still pass
- Aim for high code coverage on critical paths
- Use descriptive test names that explain the scenario

```csharp
[Fact]
public void GenerateInvoice_WithValidData_ReturnsSignedXml()
{
    // Arrange
    // Act
    // Assert
}
```

## Questions?

Feel free to open an issue with the `question` label or reach out to the maintainers.

Thank you for contributing!
