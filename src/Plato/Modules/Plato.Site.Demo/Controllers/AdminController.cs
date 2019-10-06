﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Plato.Site.Demo.Models;
using Plato.Site.Demo.ViewModels;
using Plato.Internal.Layout;
using Plato.Internal.Layout.Alerts;
using Plato.Internal.Layout.ModelBinding;
using Plato.Internal.Navigation;
using Plato.Internal.Layout.ViewProviders;
using Plato.Internal.Navigation.Abstractions;
using Plato.Internal.Security.Abstractions;
using Plato.Site.Demo.Services;
using Plato.Internal.Models.Users;
using Plato.Users.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;

namespace Plato.Site.Demo.Controllers
{

    public class AdminController : Controller, IUpdateModel
    {
    
        private readonly IViewProviderManager<DemoSettings> _viewProvider;
        private readonly IAuthorizationService _authorizationService;
        private readonly IPlatoUserManager<User> _platoUserManager;
        private readonly IBreadCrumbManager _breadCrumbManager;
        private readonly ISampleEntitiesService _sampleEntitiesService;
        private readonly ISampleUsersService _sampleUsersService;

        private readonly IAlerter _alerter;

        public IHtmlLocalizer T { get; }

        public IStringLocalizer S { get; }
        
        public AdminController(
            IHtmlLocalizer<AdminController> htmlLocalizer,
            IStringLocalizer<AdminController> stringLocalizer,
            IViewProviderManager<DemoSettings> viewProvider,
            ISampleEntitiesService sampleEntitiesService,
            IAuthorizationService authorizationService,
             IPlatoUserManager<User> platoUserManager,
             ISampleUsersService sampleUsersService,
            IBreadCrumbManager breadCrumbManager,
            IAlerter alerter)
        {

            _sampleEntitiesService = sampleEntitiesService;
            _authorizationService = authorizationService;
            _sampleUsersService = sampleUsersService;
            _breadCrumbManager = breadCrumbManager;
            _platoUserManager = platoUserManager;
            _viewProvider = viewProvider;            
            _alerter = alerter;

            T = htmlLocalizer;
            S = stringLocalizer;

        }
        
        public async Task<IActionResult> Index()
        {

            // Ensure we have permission
            //if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditTwitterSettings))
            //{
            //    return Unauthorized();
            //}
            
            _breadCrumbManager.Configure(builder =>
            {
                builder.Add(S["Home"], home => home
                    .Action("Index", "Admin", "Plato.Admin")
                    .LocalNav()
                ).Add(S["Settings"], channels => channels
                    .Action("Index", "Admin", "Plato.Settings")
                    .LocalNav()
                ).Add(S["Demo"]);
            });

            // Return view
            return View((LayoutViewModel)await _viewProvider.ProvideEditAsync(new DemoSettings(), this));

        }
        
        [HttpPost, ValidateAntiForgeryToken, ActionName(nameof(Index))]
        public async Task<IActionResult> IndexPost(DemoSettingsViewModel viewModel)
        {

            // Ensure we have permission
            //if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditTwitterSettings))
            //{
            //    return Unauthorized();
            //}

            // Execute view providers ProvideUpdateAsync method
            await _viewProvider.ProvideUpdateAsync(new DemoSettings(), this);

            // Add alert
            _alerter.Success(T["Settings Updated Successfully!"]);

            // Reidrect to success
            return RedirectToAction(nameof(Index));
            
        }

        // Entities

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> InstallEntities()
        {

            var result = await _sampleEntitiesService.InstallAsync();
            if (result.Succeeded)
            {

                // Add alert
                _alerter.Success(T["Entities Added Successfully!"]);

                // Redirect to success
                return RedirectToAction(nameof(Index));

            }

            // If we reach this point something went wrong
            foreach (var error in result.Errors)
            {
                // Add errors
                _alerter.Danger(T[error.Description]);
            }

            // And redirect to display
            return RedirectToAction(nameof(Index));

        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UninstallEntities()
        {

            var result = await _sampleEntitiesService.UninstallAsync();
            if (result.Succeeded)
            {

                // Add alert
                _alerter.Success(T["Entities Removed Successfully!"]);

                // Redirect to success
                return RedirectToAction(nameof(Index));

            }

            // If we reach this point something went wrong
            foreach (var error in result.Errors)
            {
                // Add errors
                _alerter.Danger(T[error.Description]);
            }

            // And redirect to display
            return RedirectToAction(nameof(Index));

        }

        // Users

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> InstallUsers()
        {

            var result = await _sampleUsersService.InstallAsync();
            if (result.Succeeded)
            {

                // Add alert
                _alerter.Success(T["Sample Users Added Successfully!"]);

                // Redirect to success
                return RedirectToAction(nameof(Index));

            }

            // If we reach this point something went wrong
            foreach (var error in result.Errors)
            {
                // Add errors
                _alerter.Danger(T[error.Description]);
            }

            // And redirect to display
            return RedirectToAction(nameof(Index));

        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UninstallUsers()
        {

            var result = await _sampleUsersService.UninstallAsync();
            if (result.Succeeded)
            {

                // Add alert
                _alerter.Success(T["Sample Users Removed Successfully!"]);

                // Redirect to success
                return RedirectToAction(nameof(Index));

            }

            // If we reach this point something went wrong
            foreach (var error in result.Errors)
            {
                // Add errors
                _alerter.Danger(T[error.Description]);
            }

            // And redirect to display
            return RedirectToAction(nameof(Index));

        }

    }

}