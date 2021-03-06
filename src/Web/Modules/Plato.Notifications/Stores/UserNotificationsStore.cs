﻿using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlatoCore.Cache.Abstractions;
using PlatoCore.Data.Abstractions;
using PlatoCore.Notifications.Abstractions;
using Plato.Notifications.Repositories;

namespace Plato.Notifications.Stores
{
    
    public class UserNotificationsStore : IUserNotificationsStore<UserNotification>
    {

        private readonly IUserNotificationsRepository<UserNotification> _userNotificationsRepository;
        private readonly ILogger<UserNotificationsStore> _logger;
        private readonly IDbQueryConfiguration _dbQuery;
        private readonly ICacheManager _cacheManager;

        public UserNotificationsStore(
            IUserNotificationsRepository<UserNotification> userNotificationsRepository,
            ILogger<UserNotificationsStore> logger,
            IDbQueryConfiguration dbQuery,
            ICacheManager cacheManager)
        {
            _userNotificationsRepository = userNotificationsRepository;
            _cacheManager = cacheManager;
            _logger = logger;
            _dbQuery = dbQuery;
        }

        public async Task<UserNotification> CreateAsync(UserNotification model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (model.Id > 0)
            {
                throw new ArgumentOutOfRangeException(nameof(model.Id));
            }

            if (model.UserId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(model.UserId));
            }
            
            var result = await _userNotificationsRepository.InsertUpdateAsync(model);
            if (result != null)
            {
                CancelTokens(result);
            }

            return result;
        }

        public async Task<UserNotification> UpdateAsync(UserNotification model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (model.Id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(model.Id));
            }

            if (model.UserId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(model.UserId));
            }
            
            var result = await _userNotificationsRepository.InsertUpdateAsync(model);
            if (result != null)
            {
                CancelTokens(result);
            }

            return result;
        }
        
        public async Task<bool> DeleteAsync(UserNotification model)
        {
            var success = await _userNotificationsRepository.DeleteAsync(model.Id);
            if (success)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Deleted mention role for userId '{0}' with id {1}",
                        model.UserId, model.Id);
                }

                CancelTokens(model);

            }

            return success;
        }
        
        public async Task<UserNotification> GetByIdAsync(int id)
        {
            var token = _cacheManager.GetOrCreateToken(this.GetType(), id);
            return await _cacheManager.GetOrCreateAsync(token,
                async (cacheEntry) => await _userNotificationsRepository.SelectByIdAsync(id));

        }

        public IQuery<UserNotification> QueryAsync()
        {
            var query = new UserNotificationsQuery(this);
            return _dbQuery.ConfigureQuery<UserNotification>(query); ;
        }
        
        public async Task<IPagedResults<UserNotification>> SelectAsync(IDbDataParameter[] dbParams)
        {
            var token = _cacheManager.GetOrCreateToken(this.GetType(), dbParams.Select(p => p.Value).ToArray());
            return await _cacheManager.GetOrCreateAsync(token, 
                async (cacheEntry) => await _userNotificationsRepository.SelectAsync(dbParams));
        }

        public async Task<bool> UpdateReadDateAsync(int userId, DateTimeOffset? readDate)
        {
            var success = await _userNotificationsRepository.UpdateReadDateAsync(userId, readDate);
            if (success)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Updating ReadDate userId '{0}' to {1}",
                        userId, readDate.ToString());
                }
                _cacheManager.CancelTokens(this.GetType());
            }

            return success;
        }

        public void CancelTokens(UserNotification model = null)
        {
            _cacheManager.CancelTokens(this.GetType());
        }

    }

}
