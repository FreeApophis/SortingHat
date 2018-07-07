using System;
using System.Collections.Generic;

namespace SortingHat.API
{
    internal class BasicFileTypeIdentifier : IdentifyFileType
    {
        private bool _valid;
        private int _offset;
        private string _signature;
        public BasicFileTypeIdentifier(string rule)
        {
            var parts = rule.Split(":");

            _valid = int.TryParse(parts[1], out _offset);
            _signature = parts[2];
        }
    }
}