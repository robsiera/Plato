﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

using Plato.Internal.Layout.Alerts;
using Plato.Internal.Layout.ModelBinding;
using Plato.Internal.Navigation;
using Plato.Internal.Abstractions.Extensions;
using Plato.Internal.Hosting.Abstractions;
using Plato.Internal.Models.Shell;
using Plato.Search.Models;
using Plato.Search.Stores;
using Plato.Search.ViewModels;

namespace Plato.Search.Controllers
{

    public class AdminController : Controller, IUpdateModel
    {

        #region "Constructor"

        private readonly IAuthorizationService _authorizationService;
        private readonly ISearchSettingsStore<SearchSettings> _searchSettingsStore;
        private readonly IAlerter _alerter;
        private readonly IBreadCrumbManager _breadCrumbManager;
        private readonly IShellSettings _shellSettings;
        private readonly IPlatoHost _platoHost;

        public IHtmlLocalizer T { get; }

        public IStringLocalizer S { get; }
        
        public AdminController(
            IHtmlLocalizer<AdminController> htmlLocalizer,
            IStringLocalizer<AdminController> stringLocalizer,
            IAuthorizationService authorizationService,
            ISearchSettingsStore<SearchSettings> searchSettingsStore,
            IBreadCrumbManager breadCrumbManager,
            IAlerter alerter, IShellSettings shellSettings, IPlatoHost platoHost)
        {
       
            _breadCrumbManager = breadCrumbManager;
            _authorizationService = authorizationService;
            _searchSettingsStore = searchSettingsStore;
            _alerter = alerter;
            _shellSettings = shellSettings;
            _platoHost = platoHost;

            T = htmlLocalizer;
            S = stringLocalizer;

        }

        #endregion

        #region "Actions"

        public async Task<IActionResult> Index()
        {

            //if (!await _authorizationService.AuthorizeAsync(User, PermissionsProvider.ManageRoles))
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
                ).Add(S["Search Settings"]);
            });
            
            return View(await GetModel());

        }
        

        [HttpPost]
        [ActionName(nameof(Index))]
        public async Task<IActionResult> IndexPost(SearchSettingsViewModel viewModel)
        {


            //if (!await _authorizationService.AuthorizeAsync(User, PermissionsProvider.ManageRoles))
            //{
            //    return Unauthorized();
            //}
            
            if (!ModelState.IsValid)
            {
                return View(await GetModel());
            }
            
            var settings = new SearchSettings()
            {
             
            };
            
            var result = await _searchSettingsStore.SaveAsync(settings);
            if (result != null)
            {
                // Recycle shell context to ensure changes take effect
                _platoHost.RecycleShellContext(_shellSettings);
                _alerter.Success(T["Settings Updated Successfully!"]);
            }
            else
            {
                _alerter.Danger(T["A problem occurred updating the settings. Please try again!"]);
            }
            
            return RedirectToAction(nameof(Index));
            
        }
        
        #endregion

        #region "Private Methods"

        private async Task<SearchSettingsViewModel> GetModel()
        {

            var settings = await _searchSettingsStore.GetAsync();
            if (settings != null)
            {
                return new SearchSettingsViewModel()
                {
                 
                };
            }
            
            // return default settings
            return new SearchSettingsViewModel()
            {
            
            };

        }


        #endregion


    }
}
