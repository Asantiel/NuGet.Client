// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Protocol.Core.Types;

namespace NuGet.Protocol
{
    public class PackageSearchResourceV3Provider : ResourceProvider
    {
        public PackageSearchResourceV3Provider()
            : base(typeof(PackageSearchResource), nameof(PackageSearchResourceV3Provider), nameof(PackageSearchResourceV2FeedProvider))
        {
        }

        public override async Task<Tuple<bool, INuGetResource>> TryCreate(SourceRepository source, IProtocolDiagnostics protocolDiagnostics, CancellationToken token)
        {
            PackageSearchResourceV3 curResource = null;
            var serviceIndex = await source.GetResourceAsync<ServiceIndexResourceV3>(protocolDiagnostics, token);

            if (serviceIndex != null)
            {
                var rawSearch = await source.GetResourceAsync<RawSearchResourceV3>(protocolDiagnostics, token);

                curResource = new PackageSearchResourceV3(rawSearch);
            }

            return new Tuple<bool, INuGetResource>(curResource != null, curResource);
        }
    }
}
