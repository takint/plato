﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Plato.Internal.Models.Shell;
using Plato.Internal.Features.Abstractions;
using Plato.Internal.Hosting.Abstractions;
using Plato.Internal.Layout.ViewProviders;
using Plato.Internal.Assets.Abstractions;
using Plato.Internal.Badges.Abstractions;
using Plato.Internal.Messaging.Abstractions;
using Plato.Internal.Models.Badges;
using Plato.Internal.Models.Users;
using Plato.Internal.Navigation.Abstractions;
using Plato.Internal.Security.Abstractions;
using Plato.Internal.Tasks.Abstractions;
using Plato.Docs.Handlers;
using Plato.Docs.Assets;
using Plato.Docs.Badges;
using Plato.Docs.Models;
using Plato.Docs.Navigation;
using Plato.Docs.Services;
using Plato.Docs.Subscribers;
using Plato.Docs.Tasks;
using Plato.Docs.ViewProviders;
using Plato.Entities.Repositories;
using Plato.Entities.Services;
using Plato.Entities.Stores;
using Plato.Entities.Subscribers;
using Plato.Docs.NotificationTypes;
using Plato.Docs.Notifications;
using Plato.Entities.Models;
using Plato.Internal.Notifications;
using Plato.Internal.Notifications.Abstractions;

namespace Plato.Docs
{
    public class Startup : StartupBase
    {
        private readonly IShellSettings _shellSettings;

        public Startup(IShellSettings shellSettings)
        {
            _shellSettings = shellSettings;
        }

        public override void ConfigureServices(IServiceCollection services)
        {

            // Feature installation event handler
            services.AddScoped<IFeatureEventHandler, FeatureEventHandler>();

            // Register navigation provider
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<INavigationProvider, SiteMenu>();
            services.AddScoped<INavigationProvider, SearchMenu>();
            services.AddScoped<INavigationProvider, DocMenu>();
            services.AddScoped<INavigationProvider, DocCommentMenu>();

            // Stores
            services.AddScoped<IEntityRepository<Doc>, EntityRepository<Doc>>();
            services.AddScoped<IEntityStore<Doc>, EntityStore<Doc>>();
            services.AddScoped<IEntityManager<Doc>, EntityManager<Doc>>();

            services.AddScoped<IEntityReplyRepository<DocComment>, EntityReplyRepository<DocComment>>();
            services.AddScoped<IEntityReplyStore<DocComment>, EntityReplyStore<DocComment>>();
            services.AddScoped<IEntityReplyManager<DocComment>, EntityReplyManager<DocComment>>();

            // Register data access
            services.AddScoped<IPostManager<Doc>, TopicManager>();
            services.AddScoped<IPostManager<DocComment>, ReplyManager>();
            
            // Services
            services.AddScoped<IEntityService<Doc>, EntityService<Doc>>();
            services.AddScoped<IEntityReplyService<DocComment>, EntityReplyService<DocComment>>();

            // View incrementer
            services.AddScoped<IEntityViewIncrementer<Doc>, EntityViewIncrementer<Doc>>();
            
            // Register permissions provider
            services.AddScoped<IPermissionsProvider<Permission>, Permissions>();

            // Register client resources
            services.AddScoped<IAssetProvider, AssetProvider>();
         
            // Register admin view providers
            services.AddScoped<IViewProviderManager<AdminIndex>, ViewProviderManager<AdminIndex>>();
            services.AddScoped<IViewProvider<AdminIndex>, AdminViewProvider>();

            // Register view providers
            services.AddScoped<IViewProviderManager<Doc>, ViewProviderManager<Doc>>();
            services.AddScoped<IViewProvider<Doc>, DocViewProvider>();
            services.AddScoped<IViewProviderManager<DocComment>, ViewProviderManager<DocComment>>();
            services.AddScoped<IViewProvider<DocComment>, DocCommentViewProvider>();

            // Add profile views
            services.AddScoped<IViewProviderManager<Profile>, ViewProviderManager<Profile>>();
            services.AddScoped<IViewProvider<Profile>, ProfileViewProvider>();
            
            // Add user views
            services.AddScoped<IViewProviderManager<UserIndex>, ViewProviderManager<UserIndex>>();
            services.AddScoped<IViewProvider<UserIndex>, UserViewProvider>();

            // Register message broker subscribers
            services.AddScoped<IBrokerSubscriber, TopicSubscriber<Doc>>();
            services.AddScoped<IBrokerSubscriber, ReplySubscriber<DocComment>>();
            services.AddScoped<IBrokerSubscriber, EntityReplySubscriber<DocComment>>();

            // Badge providers
            services.AddScoped<IBadgesProvider<Badge>, TopicBadges>();
            services.AddScoped<IBadgesProvider<Badge>, ReplyBadges>();

            // Background tasks
            services.AddScoped<IBackgroundTaskProvider, TopicBadgesAwarder>();
            services.AddScoped<IBackgroundTaskProvider, ReplyBadgesAwarder>();

            // Notification types
            services.AddScoped<INotificationTypeProvider, EmailNotifications>();
            services.AddScoped<INotificationTypeProvider, WebNotifications>();

            // Notification manager
            services.AddScoped<INotificationManager<ReportSubmission<Doc>>, NotificationManager<ReportSubmission<Doc>>>();
            services.AddScoped<INotificationManager<ReportSubmission<DocComment>>, NotificationManager<ReportSubmission<DocComment>>>();

            // Notification providers
            services.AddScoped<INotificationProvider<ReportSubmission<Doc>>, TopicReportWeb>();
            services.AddScoped<INotificationProvider<ReportSubmission<Doc>>, TopicReportEmail>();
            services.AddScoped<INotificationProvider<ReportSubmission<DocComment>>, ReplyReportWeb>();
            services.AddScoped<INotificationProvider<ReportSubmission<DocComment>>, ReplyReportEmail>();

            // Report entity managers
            services.AddScoped<IReportEntityManager<Doc>, ReportTopicManager>();
            services.AddScoped<IReportEntityManager<DocComment>, ReportReplyManager>();

        }

        public override void Configure(
            IApplicationBuilder app,
            IRouteBuilder routes,
            IServiceProvider serviceProvider)
        {
            
            // Index
            routes.MapAreaRoute(
                name: "Docs",
                areaName: "Plato.Docs",
                template: "docs/{pager.offset:int?}",
                defaults: new { controller = "Home", action = "Index" }
            );
            
            // Popular
            routes.MapAreaRoute(
                name: "DocsPopular",
                areaName: "Plato.Docs",
                template: "docs/popular/{pager.offset:int?}",
                defaults: new { controller = "Home", action = "Popular" }
            );

            // Entity
            routes.MapAreaRoute(
                name: "DocsEntity",
                areaName: "Plato.Docs",
                template: "docs/t/{opts.id:int}/{opts.alias}/{pager.offset:int?}",
                defaults: new { controller = "Home", action = "Display" }
            );

            // New Entity
            routes.MapAreaRoute(
                name: "DocsNew",
                areaName: "Plato.Docs",
                template: "docs/new/{channel:int?}",
                defaults: new { controller = "Home", action = "Create" }
            );

            // Edit Entity
            routes.MapAreaRoute(
                name: "DocsEdit",
                areaName: "Plato.Docs",
                template: "docs/edit/{opts.id:int?}/{opts.alias?}",
                defaults: new { controller = "Home", action = "Edit" }
            );

            // Display Reply
            routes.MapAreaRoute(
                name: "DocsDisplayReply",
                areaName: "Plato.Docs",
                template: "docs/g/{opts.id:int}/{opts.alias}/{opts.replyId:int?}",
                defaults: new { controller = "Home", action = "Reply" }
            );

            // Report 
            routes.MapAreaRoute(
                name: "DocsReport",
                areaName: "Plato.Docs",
                template: "docs/report/{opts.id:int}/{opts.alias}/{opts.replyId:int?}",
                defaults: new { controller = "Home", action = "Report" }
            );

            // User Index
            routes.MapAreaRoute(
                name: "DocsUser",
                areaName: "Plato.Docs",
                template: "u/{opts.createdByUserId:int}/{opts.alias?}/docs/{pager.offset:int?}",
                defaults: new { controller = "User", action = "Index" }
            );

        }
    }
}