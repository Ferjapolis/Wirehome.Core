﻿using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Backup;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Services.Backup
{
    [ApiServiceClass(typeof(IBackupService))]
    public class BackupService : ServiceBase, IBackupService
    {
        public event EventHandler<BackupEventArgs> CreatingBackup;
        public event EventHandler<BackupEventArgs> RestoringBackup;

        [ApiMethod]
        public void CreateBackup(IApiContext apiContext)
        {
            var backup = new JObject
            {
                ["Timestamp"] = DateTime.Now.ToString("O"),
                ["Version"] = 1
            };

            var eventArgs = new BackupEventArgs(backup);
            CreatingBackup?.Invoke(this, eventArgs);

            apiContext.Response = backup;
        }

        [ApiMethod]
        public void RestoreBackup(IApiContext apiContext)
        {
            if (apiContext.Request.Type != JTokenType.Object)
            {
                throw new NotSupportedException();
            }

            var eventArgs = new BackupEventArgs(apiContext.Request);
            RestoringBackup?.Invoke(this, eventArgs);
        }
    }
}
