# Release Process

This document describes the branching strategy and release process for Zatca.EInvoice.

## Branches

| Branch | Purpose | Version Label | Example |
|--------|---------|---------------|---------|
| `develop` | Active development (default) | `alpha` | `1.0.0-alpha.5` |
| `stable` | Production releases | (none) | `1.0.0` |
| `release/*` | Release candidates | `rc` | `1.0.0-rc.1` |
| `feature/*` | Feature development | `alpha` | `1.1.0-alpha.2` |
| `hotfix/*` | Emergency fixes | `beta` | `1.0.1-beta.1` |

## Version Numbering

This project follows [Semantic Versioning](https://semver.org/):

- **Major (X.0.0)**: Breaking changes
- **Minor (X.Y.0)**: New features, backward compatible
- **Patch (X.Y.Z)**: Bug fixes, backward compatible

## How to Release

### Alpha Release (from develop)

Alpha releases are created directly from the `develop` branch for early testing:

```bash
# Ensure develop is up to date
git checkout develop
git pull origin develop

# Create and push tag
git tag v1.0.0-alpha.2
git push origin v1.0.0-alpha.2
```

The release workflow will automatically:
1. Build and test the package
2. Publish to NuGet.org
3. Publish to GitHub Packages

### Stable Release

Stable releases are created from the `stable` branch:

```bash
# Merge develop into stable
git checkout stable
git pull origin stable
git merge develop

# Create and push tag
git tag v1.0.0
git push origin stable --tags
```

### Release Candidate (Optional)

For larger releases, you may want to create a release candidate first:

```bash
# Create release branch from develop
git checkout develop
git checkout -b release/1.0.0
git push origin release/1.0.0
# Tag will produce: 1.0.0-rc.1

# After testing, merge to stable
git checkout stable
git merge release/1.0.0
git tag v1.0.0
git push origin stable --tags

# Merge back to develop
git checkout develop
git merge release/1.0.0
git push origin develop

# Delete release branch
git branch -d release/1.0.0
git push origin --delete release/1.0.0
```

### Hotfix

For critical production fixes:

```bash
# Create hotfix branch from stable
git checkout stable
git checkout -b hotfix/critical-bug

# Make fixes and commit
git commit -m "fix: critical bug description"
git push origin hotfix/critical-bug

# After PR is merged to stable, create tag
git checkout stable
git pull origin stable
git tag v1.0.1
git push origin stable --tags

# Merge hotfix to develop
git checkout develop
git merge stable
git push origin develop
```

## What Happens When You Push a Tag

When a tag matching `v*.*.*` is pushed:

1. **GitHub Actions** detects the tag push
2. **GitVersion** determines the semantic version from the tag
3. **Build** compiles the project with the version
4. **Test** runs all unit tests
5. **Pack** creates the NuGet package
6. **Publish** uploads to NuGet.org and GitHub Packages

## Environment Requirements

The release workflow requires:

- **Environment**: `nuget` (configured in GitHub repository settings)
- **Secret**: `NUGET_USER` - NuGet.org username for OIDC authentication

## Troubleshooting

### Release workflow didn't trigger

- Ensure the tag matches the pattern `v*.*.*` (e.g., `v1.0.0`, `v1.0.0-alpha.1`)
- Check that you pushed the tag: `git push origin <tag-name>`

### Version mismatch

- GitVersion uses the tag to determine the version
- The tag must match the expected format for your branch
- Run `dotnet-gitversion` locally to verify the expected version

### Package already exists

- The workflow uses `--skip-duplicate` to handle this gracefully
- If you need to republish, increment the version number
