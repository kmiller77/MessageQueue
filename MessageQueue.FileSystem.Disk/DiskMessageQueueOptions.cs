﻿using System;
using System.IO;

namespace KM.MessageQueue.FileSystem.Disk
{
    public sealed class DiskMessageQueueOptions<TMessage>
    {
        public int? MaxQueueSize { get; set; }
        public int? MessagePartitionSize { get; set; }
        public DirectoryInfo? MessageStore { get; set; }
    }
}
