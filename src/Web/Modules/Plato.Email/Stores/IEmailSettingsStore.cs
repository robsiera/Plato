﻿using PlatoCore.Stores.Abstractions;

namespace Plato.Email.Stores
{
    public interface IEmailSettingsStore<T> : ISettingsStore<T> where T : class
    {
    }

}
