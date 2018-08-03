using System;
using System.Collections.Generic;
using System.Text;

namespace SortingHat.API.DI
{
    public interface IServices
    {
        ILogger Logger { get; }
        IDatabase DB { get; }
        HashService HashService { get; }
    }
}
