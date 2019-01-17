﻿using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Plato.Discuss.Moderation.ViewModels;
using Plato.Internal.Abstractions.Extensions;
using Plato.Internal.Features.Abstractions;
using Plato.Internal.Hosting.Abstractions;
using Plato.Internal.Layout.ViewProviders;
using Plato.Internal.Layout.ModelBinding;
using Plato.Internal.Models.Users;
using Plato.Internal.Navigation;
using Plato.Internal.Security.Abstractions;
using Plato.Internal.Stores.Abstractions.Users;
using Plato.Moderation.Models;
using Plato.Moderation.Stores;
using Plato.WebApi.Models;

namespace Plato.Discuss.Moderation.ViewProviders
{
    public class AdminViewProvider : BaseViewProvider<Moderator>
    {

        private readonly IContextFacade _contextFacade;
        private readonly IPermissionsManager<ModeratorPermission> _permissionsManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IModeratorStore<Moderator> _moderatorStore;
        private readonly IPlatoUserStore<User> _userStore;
        private readonly HttpRequest _request;
        private readonly IFeatureFacade _featureFacade;


        private readonly IStringLocalizer T;

        public AdminViewProvider(
            IContextFacade contextFacade,
            IPermissionsManager<ModeratorPermission> permissionsManager,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor,
            IModeratorStore<Moderator> moderatorStore, 
            IPlatoUserStore<User> userStore,
            IStringLocalizer<AdminViewProvider> stringLocalizer,
            IFeatureFacade featureFacade)
        {
            _contextFacade = contextFacade;
            _permissionsManager = permissionsManager;
            _authorizationService = authorizationService;
            _moderatorStore = moderatorStore;
            _userStore = userStore;
            _featureFacade = featureFacade;
            _request = httpContextAccessor.HttpContext.Request;

            T = stringLocalizer;
        }

        #region "Implementation"

        public override Task<IViewProviderResult> BuildIndexAsync(Moderator moderator, IViewProviderContext updater)
        {

            var filterOptions = new FilterOptions();

            var pagerOptions = new PagerOptions();
            //pagerOptions.Page = GetPageIndex(updater);

            var viewModel = new ModeratorIndexViewModel()
            {
                FilterOpts = filterOptions,
                PagerOpts = pagerOptions
            };

            return Task.FromResult(Views(
                View<ModeratorIndexViewModel>("Admin.Index.Header", model => viewModel).Zone("header").Order(1),
                View<ModeratorIndexViewModel>("Admin.Index.Tools", model => viewModel).Zone("tools").Order(1),
                View<ModeratorIndexViewModel>("Admin.Index.Content", model => viewModel).Zone("content").Order(1)
            ));

        }

        public override Task<IViewProviderResult> BuildDisplayAsync(Moderator oldModerator, IViewProviderContext updater)
        {
            return Task.FromResult(default(IViewProviderResult));
        }

        public override async Task<IViewProviderResult> BuildEditAsync(Moderator moderator, IViewProviderContext updater)
        {

            // Serialize tagIt model 
            var users = "";
            if (moderator.UserId > 0)
            {
                users = new List<UserApiResult>()
                {
                    new UserApiResult()
                    {
                        Id = moderator.User.Id,
                        DisplayName = moderator.User.DisplayName,
                        UserName = moderator.User.UserName,
                    }
                }.Serialize();
            }

            var viewModel = new EditModeratorViewModel
            {
                Users = users,
                Moderator =  moderator,
                IsNewModerator = moderator.UserId == 0,
                EnabledPermissions = await GetEnabledModeratorPermissionsAsync(moderator),
                CategorizedPermissions = await _permissionsManager.GetCategorizedPermissionsAsync()
            };

            return Views(
                View<EditModeratorViewModel>("Admin.Edit.Header", model => viewModel).Zone("header").Order(1),
                View<EditModeratorViewModel>("Admin.Edit.Content", model => viewModel).Zone("content").Order(1),
                View<EditModeratorViewModel>("Admin.Edit.Actions", model => viewModel).Zone("actions").Order(1),
                View<EditModeratorViewModel>("Admin.Edit.Footer", model => viewModel).Zone("footer").Order(1)
            );
        }
        
        public override async Task<bool> ValidateModelAsync(Moderator moderator, IUpdateModel updater)
        {
            var valid = await updater.TryUpdateModelAsync(new EditModeratorViewModel()
            {
                UserId = moderator.UserId
            });

            return valid;
        }

        public override async Task ComposeTypeAsync(Moderator moderator, IUpdateModel updater)
        {

            var model = new EditModeratorViewModel
            {
                UserId = moderator.UserId
            };

            await updater.TryUpdateModelAsync(model);

            if (updater.ModelState.IsValid)
            {
                moderator.UserId = model.UserId;
                moderator.Claims = GetPostedClaims();
            }

        }
        
        public override async Task<IViewProviderResult> BuildUpdateAsync(Moderator model, IViewProviderContext context)
        {
            
            var moderator = await _moderatorStore.GetByIdAsync(model.Id);
            if (moderator == null)
            {
                return await BuildIndexAsync(model, context);
            }
          
            // Validate model
            if (await ValidateModelAsync(model, context.Updater))
            {
                // Update claims
                moderator.Claims = GetPostedClaims();
                await _moderatorStore.UpdateAsync(moderator);
                
            }
      
            return await BuildEditAsync(moderator, context);


        }

        #endregion

        #region "Private Methods"

        IList<ModeratorClaim> GetPostedClaims()
        {
            // Build a list of claims to add or update
            var moderatorClaims = new List<ModeratorClaim>();
            foreach (var key in _request.Form.Keys)
            {
                if (key.StartsWith("Checkbox.") && _request.Form[key] == "true")
                {
                    var permissionName = key.Substring("Checkbox.".Length);
                    moderatorClaims.Add(new ModeratorClaim { ClaimType = ModeratorPermission.ClaimType, ClaimValue = permissionName });
                }
            }

            return moderatorClaims;

        }

        async Task<IEnumerable<string>> GetEnabledModeratorPermissionsAsync(Moderator moderator)
        {

            // We can only obtain enabled permissions for existing roles
            // Return an empty list for new roles to avoid additional null checks
            if (moderator.Id == 0)
            {
                return new List<string>();
            }

            // If the role is anonymous set the authtype to
            // null to ensure IsAuthenticated is set to false
            var authType = moderator.CategoryId != 0
                ? "UserAuthType"
                : null;

            // Dummy identity
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, moderator.User.UserName)
            }, authType);

            // Dummy principal
            var principal = new ClaimsPrincipal(identity);

            // Permissions grouped by feature
            var categorizedPermissions = await _permissionsManager.GetCategorizedPermissionsAsync();

            // Get flat permissions list from categorized permissions
            var permissions = categorizedPermissions.SelectMany(x => x.Value);

            var result = new List<string>();
            foreach (var permission in permissions)
            {
                if (await _authorizationService.AuthorizeAsync<ModeratorPermission>(principal, moderator.CategoryId, permission))
                {
                    result.Add(permission.Name);
                }
            }

            return result;

        }
        
        #endregion

    }
    

}
