﻿using SortingHat.API.Models;

namespace SortingHat.Test.Mock
{
    internal class MockTagParser : ITagParser
    {
        public MockTagParser()
        {
        }

        public Tag? Parse(string tagString)
        {
            return null;
        }
    }
}