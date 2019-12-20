// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGetVSExtension
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles",
        Justification = "Following pattern for VS Context Menu IDs")]
    internal static class PkgCmdIDList
    {
        public const int cmdidPowerConsole = 0x0100;
        public const int cmdidAddPackageDialog = 0x100;
        public const int cmdidAddPackageDialogForSolution = 0x200;
        public const int cmdidRestorePackages = 0x300;
        public const int cmdidUpgradeNuGetProject = 0x400;
        public const int cmdidUpgradePackagesConfig = 0x410;
        public const int cmdidSourceSettings = 0x0200;
        public const int cmdIdGeneralSettings = 0x0300;
        public const int cmdIdVisualizer = 0x0310;
        public const int cmdIdUpdatePackage = 0x500;
        public const int cmdidDebugConsole = 0x0900;
    }
}
