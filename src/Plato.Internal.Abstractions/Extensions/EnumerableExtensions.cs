﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Plato.Internal.Abstractions.Extensions
{
    public static class EnumerableExtensions
    {

        public static string Serialize<T>(this IEnumerable<T> iterator) 
        {
           return JsonConvert.SerializeObject(iterator);
        }

    }
    
}
