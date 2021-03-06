﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PlatoCore.Layout.Views.Abstractions
{
    public interface ICompiledView
    {

        ViewContext ViewContext { get; set; }

        ICompiledView Contextualize(ViewContext viewContext);

        Task<IHtmlContent> InvokeAsync();

    }

}
