﻿using System.Collections.Generic;

namespace Plato.Internal.Localization.Abstractions.Models
{

    public class LocaleValues<TModel> where TModel : class
    {
        public LocaleResource Resource { get; set; }

        public IEnumerable<TModel> Values { get; set; } = new List<TModel>();

    }

}