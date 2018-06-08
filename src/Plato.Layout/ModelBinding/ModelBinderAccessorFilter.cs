﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Plato.Layout.ModelBinding
{
    public class ModelBinderAccessorFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.Controller as Controller;
            if (controller != null)
            {
                var modelBinderAccessor = context.HttpContext.RequestServices.GetRequiredService<IUpdateModelAccessor>();
                modelBinderAccessor.ModelUpdater = new ControllerModelUpdater(controller);
            }
        }
    }
}
