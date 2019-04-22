﻿using System.Collections.Generic;

namespace Plato.Internal.Models.Metrics
{

    public class AggregatedResult<T>  
    {

        public ICollection<AggregatedCount<T>> Data { get; set; } = new List<AggregatedCount<T>>();

        public int Total()
        {

            var output = 0;
            foreach (var item in Data)
            {
                output += item.Count;
            }

            return output;

        }
        
    }
    
}
