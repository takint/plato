﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Plato.Internal.Hosting.Abstractions;
using Plato.StopForumSpam.Client.Services;

namespace Plato.StopForumSpam.Client
{
    public class Startup : StartupBase
    {
        
        public Startup()
        {
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            // StopForumSpam services
            services.AddScoped<IStopForumSpamClient, StopForumSpamClient>();
            services.AddScoped<ISpamFrequencies, SpamFrequencies>();
        }

        public override void Configure(
            IApplicationBuilder app,
            IRouteBuilder routes,
            IServiceProvider serviceProvider)
        {
        }
    }
}