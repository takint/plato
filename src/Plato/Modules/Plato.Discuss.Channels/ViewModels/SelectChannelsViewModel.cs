﻿using System.Collections.Generic;
using Plato.Discuss.Channels.Models;

namespace Plato.Discuss.Channels.ViewModels
{
    public class SelectChannelsViewModel
    {
        
        public IList<Selection<Channel>> SelectedChannels { get; set; }

        public string HtmlName { get; set; }

    }

    public class Selection<T>
    {

        public bool IsSelected { get; set; }

        public T Value { get; set; }

    }


}