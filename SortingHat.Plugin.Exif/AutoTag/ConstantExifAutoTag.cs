﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MetadataExtractor;
using SortingHat.API.AutoTag;

namespace SortingHat.Plugin.Exif.AutoTag
{
    [UsedImplicitly]
    public class ConstantExifAutoTag<TDirectory> : ConstantAutoTag where TDirectory : Directory
    {
        private readonly string _baseKey;
        private readonly int _directoryEntryID;
        public override string AutoTagKey => $"{_baseKey}";
        public override string Description { get; }

        public ConstantExifAutoTag(int directoryEntryID, string baseKey, string description)
        {
            _directoryEntryID = directoryEntryID;
            _baseKey = baseKey;
            Description = description;
        }

        protected override string HandleTag(System.IO.FileInfo file)
        {
            IEnumerable<Directory> directories = ImageMetadataReader.ReadMetadata(file.FullName);

            // obtain the Exif SubIFD directory
            var directory = directories.OfType<TDirectory>().FirstOrDefault();

            return directory?.GetString(_directoryEntryID);

        }
    }
}
