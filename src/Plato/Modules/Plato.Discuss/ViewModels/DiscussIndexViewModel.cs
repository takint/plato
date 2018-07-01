﻿using Plato.Entities.Models;
using Plato.Internal.Data.Abstractions;
using Plato.Internal.Navigation;

namespace Plato.Discuss.ViewModels
{
    public class DiscussIndexViewModel
    {

        public DiscussIndexViewModel()
        {

        }

        public DiscussIndexViewModel(
            IPagedResults<Entity> results,
            FilterOptions filterOptions,
            PagerOptions pagerOptions)
        {
            this.Results = results;
            this.FilterOpts = filterOptions;
            this.PagerOpts = pagerOptions;
            this.PagerOpts.SetTotal(results?.Total ?? 0);
        }


        public IPagedResults<Entity> Results { get; set; }

        public NewEntityViewModel NewEntityViewModel { get; set; }

        public PagerOptions PagerOpts { get; set; }


        public FilterOptions FilterOpts { get; set; }

    }

    public class FilterOptions
    {
        public string Search { get; set; }

        public EntityOrder Order { get; set; }

    }

    public class NewEntityViewModel
    {

        public string Title { get; set; }

        public string Message { get; set; }


    }

    public enum EntityOrder
    {
        Username,
        Email
    }




}
