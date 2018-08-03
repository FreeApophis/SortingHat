using System;
using System.Collections.Generic;
using System.Text;

namespace SortingHat.API.DI
{
    public interface IDatabase
    {
        void Setup();
        void TearDown();
    }
}
