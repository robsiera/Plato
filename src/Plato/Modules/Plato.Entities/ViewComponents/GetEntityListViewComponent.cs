﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Plato.Entities.Models;
using Plato.Entities.Services;
using Plato.Entities.ViewModels;
using Plato.Internal.Navigation;
using Plato.Internal.Navigation.Abstractions;


namespace Plato.Search.ViewComponents
{
    public class GetEntityListViewComponent : ViewComponent
    {
        
        private readonly IEntityService<Entity> _entityService;
        //private readonly ISearchSettingsStore<SearchSettings> _searchSettingsStore;

        //private SearchSettings _searchSettings;

        public GetEntityListViewComponent(
            IEntityService<Entity> entityService)
        {
            _entityService = entityService;
            //_searchSettingsStore = searchSettingsStore;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            EntityIndexOptions options,
            PagerOptions pager)
        {

            if (options == null)
            {
                options = new EntityIndexOptions();
            }

            if (pager == null)
            {
                pager = new PagerOptions();
            }

            // Get search settings
            //_searchSettings = await _searchSettingsStore.GetAsync();
            
            return View(await GetViewModel(options, pager));

        }
        
        async Task<EntityIndexViewModel<Entity>> GetViewModel(
            EntityIndexOptions options,
            PagerOptions pager)
        {

            // Get results
            var results = await _entityService
                .GetResultsAsync(options, pager);

            // Set total on pager
            pager.SetTotal(results?.Total ?? 0);
            
            // Return view model
            return new EntityIndexViewModel<Entity>()
            {
                Results = results,
                Options = options,
                Pager = pager
            }; 

        }

    }
    
}

