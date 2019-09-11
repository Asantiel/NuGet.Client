// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;

namespace NuGet.Protocol
{
    /// <summary>
    /// Provides the download metatdata for a given package from a V3 server endpoint.
    /// </summary>
    public class DownloadResourceV3 : DownloadResource
    {
        private readonly RegistrationResourceV3 _regResource;
        private readonly HttpSource _client;
        private readonly string _packageBaseAddressUrl;

        /// <summary>
        /// Download packages using the download url found in the registration resource.
        /// </summary>
        public DownloadResourceV3(HttpSource client, RegistrationResourceV3 regResource)
            : this(client)
        {
            if (regResource == null)
            {
                throw new ArgumentNullException(nameof(regResource));
            }

            _regResource = regResource;
        }

        /// <summary>
        /// Download packages using the package base address container resource.
        /// </summary>
        public DownloadResourceV3(HttpSource client, string packageBaseAddress)
            : this(client)
        {
            if (packageBaseAddress == null)
            {
                throw new ArgumentNullException(nameof(packageBaseAddress));
            }

            _packageBaseAddressUrl = packageBaseAddress.TrimEnd('/');
        }

        private DownloadResourceV3(HttpSource client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _client = client;
        }

        /// <summary>
        /// Get the download url of the package.
        /// 1. If the identity is a SourcePackageDependencyInfo the SourcePackageDependencyInfo.DownloadUri is used.
        /// 2. A url will be constructed for the flat container location if the source has that resource.
        /// 3. The download url will be found in the registration blob as a fallback.
        /// </summary>
        private async Task<Uri> GetDownloadUrl(PackageIdentity identity, ILogger log, IProtocolDiagnostics protocolDiagnostics, CancellationToken token)
        {
            Uri downloadUri = null;
            var sourcePackage = identity as SourcePackageDependencyInfo;

            if (sourcePackage?.DownloadUri != null)
            {
                // Read the already provided url
                downloadUri = sourcePackage?.DownloadUri;
            }
            else if (_packageBaseAddressUrl != null)
            {
                // Construct the url
                var id = identity.Id.ToLowerInvariant();
                var version = identity.Version.ToNormalizedString().ToLowerInvariant();

                var url = $"{_packageBaseAddressUrl}/{id}/{version}/{id}.{version}.nupkg";
                downloadUri = new Uri(url);
            }
            else if (_regResource != null)
            {
                using (var sourceCacheContext = new SourceCacheContext())
                {
                    // Read the url from the registration information
                    var blob = await _regResource.GetPackageMetadata(identity, sourceCacheContext, log, protocolDiagnostics, token);

                    if (blob != null
                        && blob["packageContent"] != null)
                    {
                        downloadUri = new Uri(blob["packageContent"].ToString());
                    }
                }
            }

            return downloadUri;
        }

        [Obsolete("Use the overload with " + nameof(IProtocolDiagnostics) + ". Use " + nameof(NullProtocolDiagnostics) + " if no diagnostics are needed")]
        public override Task<DownloadResourceResult> GetDownloadResourceResultAsync(
            PackageIdentity identity,
            PackageDownloadContext downloadContext,
            string globalPackagesFolder,
            ILogger logger,
            CancellationToken token)
        {
            return GetDownloadResourceResultAsync(
                identity,
                downloadContext,
                globalPackagesFolder,
                logger,
                NullProtocolDiagnostics.Instance,
                token);
        }

        public override async Task<DownloadResourceResult> GetDownloadResourceResultAsync(
            PackageIdentity identity,
            PackageDownloadContext downloadContext,
            string globalPackagesFolder,
            ILogger logger,
            IProtocolDiagnostics protocolDiagnostics,
            CancellationToken token)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            if (downloadContext == null)
            {
                throw new ArgumentNullException(nameof(downloadContext));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            var uri = await GetDownloadUrl(identity, logger, protocolDiagnostics, token);

            if (uri != null)
            {
                return await GetDownloadResultUtility.GetDownloadResultAsync(
                    _client,
                    identity,
                    uri,
                    downloadContext,
                    globalPackagesFolder,
                    logger,
                    protocolDiagnostics,
                    token);
            }

            return new DownloadResourceResult(DownloadResourceResultStatus.NotFound);
        }
    }
}
