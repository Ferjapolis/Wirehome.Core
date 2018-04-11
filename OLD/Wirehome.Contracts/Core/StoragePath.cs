﻿using System.IO;

namespace Wirehome.Contracts.Core
{
    public static class StoragePath
    {
        public static void Initialize(string storageRoot, string localStateRoot)
        {
            StorageRoot = storageRoot;
            AppRoot = Path.Combine(localStateRoot, "App");
            ManagementAppRoot = Path.Combine(localStateRoot, "ManagementApp");
            ScriptsRoot = Path.Combine(localStateRoot, "Scripts");
        }

        public static string StorageRoot { get; private set; }

        public static string AppRoot { get; private set; } 

        public static string ManagementAppRoot { get; private set; }

        public static string ScriptsRoot { get; private set; }
    }
}
