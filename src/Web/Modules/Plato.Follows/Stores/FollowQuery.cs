﻿using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Plato.Follows.Models;
using PlatoCore.Data.Abstractions;
using PlatoCore.Stores.Abstractions;

namespace Plato.Follows.Stores
{

    #region "FollowQuery"

    public class FollowQuery : DefaultQuery<Models.Follow>
    {

        private readonly IQueryableStore<Models.Follow> _store;

        public FollowQuery(IQueryableStore<Models.Follow> store)
        {
            _store = store;
        }

        public FollowQueryParams Params { get; set; }

        public override IQuery<Models.Follow> Select<T>(Action<T> configure)
        {
            var defaultParams = new T();
            configure(defaultParams);
            Params = (FollowQueryParams)Convert.ChangeType(defaultParams, typeof(FollowQueryParams));
            return this;
        }

        public override async Task<IPagedResults<Models.Follow>> ToList()
        {

            var builder = new FollowQueryBuilder(this);
            var populateSql = builder.BuildSqlPopulate();
            var countSql = builder.BuildSqlCount();
            var name = Params.Name.Value ?? string.Empty;

            return await _store.SelectAsync(
                new IDbDataParameter[]
                {
                    new DbParam("PageIndex", DbType.Int32, PageIndex),
                    new DbParam("PageSize", DbType.Int32, PageSize),
                    new DbParam("SqlPopulate", DbType.String, populateSql),
                    new DbParam("SqlCount", DbType.String, countSql),
                    new DbParam("Name", DbType.String, name)
                });

        }
        
    }

    #endregion

    #region "FollowQueryParams"

    public class FollowQueryParams
    {
        
        private WhereInt _id;
        private WhereInt _thingId;
        private WhereString _name;
        private WhereInt _createdUserId;

        public WhereInt Id
        {
            get => _id ?? (_id = new WhereInt());
            set => _id = value;
        }

        public WhereInt ThingId
        {
            get => _thingId ?? (_thingId = new WhereInt());
            set => _thingId = value;
        }
        
        public WhereString Name
        {
            get => _name ?? (_name = new WhereString());
            set => _name = value;
        }

        public WhereInt CreatedUserId
        {
            get => _createdUserId ?? (_createdUserId = new WhereInt());
            set => _createdUserId = value;
        }

    }

    #endregion

    #region "FollowQueryBuilder"

    public class FollowQueryBuilder : IQueryBuilder
    {

        #region "Constructor"

        private readonly string _followsTableName;
        private readonly string _usersTableName;

        private readonly FollowQuery _query;

        public FollowQueryBuilder(FollowQuery query)
        {
            _query = query;
            _followsTableName = GetTableNameWithPrefix("Follows");
            _usersTableName = GetTableNameWithPrefix("Users");

        }

        #endregion

        #region "Implementation"
        
        public string BuildSqlPopulate()
        {
            var whereClause = BuildWhereClause();
            var orderBy = BuildOrderBy();
            var sb = new StringBuilder();
            sb.Append("SELECT ")
                .Append(BuildPopulateSelect())
                .Append(" FROM ")
                .Append(BuildTables());
            if (!string.IsNullOrEmpty(whereClause))
                sb.Append(" WHERE (").Append(whereClause).Append(")");
            // Order only if we have something to order by
            sb.Append(" ORDER BY ").Append(!string.IsNullOrEmpty(orderBy)
                ? orderBy
                : "(SELECT NULL)");
            // Limit results only if we have a specific page size
            if (!_query.IsDefaultPageSize)
                sb.Append(" OFFSET @RowIndex ROWS FETCH NEXT @PageSize ROWS ONLY;");
            return sb.ToString();
        }

        public string BuildSqlCount()
        {
            if (!_query.CountTotal)
                return string.Empty;
            var whereClause = BuildWhereClause();
            var sb = new StringBuilder();
            sb.Append("SELECT COUNT(f.Id) FROM ")
                .Append(BuildTables());
            if (!string.IsNullOrEmpty(whereClause))
                sb.Append(" WHERE (").Append(whereClause).Append(")");
            return sb.ToString();
        }

        #endregion

        #region "Private Methods"

        private string BuildPopulateSelect()
        {
            var sb = new StringBuilder();
            sb.Append("f.*, ")
                .Append("u.Email, ")
                .Append("u.UserName, ")
                .Append("u.DisplayName, ")
                .Append("u.NormalizedUserName");
            return sb.ToString();

        }

        private string BuildTables()
        {
            var sb = new StringBuilder();
            sb.Append(_followsTableName)
                .Append(" f WITH (nolock) LEFT OUTER JOIN ")
                .Append(_usersTableName)
                .Append(" u ON f.CreatedUserId = u.Id");
            return sb.ToString();
        }

        private string GetTableNameWithPrefix(string tableName)
        {
            return !string.IsNullOrEmpty(_query.Options.TablePrefix)
                ? _query.Options.TablePrefix + tableName
                : tableName;
        }

        private string BuildWhereClause()
        {
            var sb = new StringBuilder();

            // Id
            if (_query.Params.Id.Value > -1)
            {
                if (!string.IsNullOrEmpty(sb.ToString()))
                    sb.Append(_query.Params.Id.Operator);
                sb.Append(_query.Params.Id.ToSqlString("f.Id"));
            }

            // ThingId
            if (_query.Params.ThingId.Value > -1)
            {
                if (!string.IsNullOrEmpty(sb.ToString()))
                    sb.Append(_query.Params.ThingId.Operator);
                sb.Append(_query.Params.ThingId.ToSqlString("f.ThingId"));
            }

            // Name
            if (!String.IsNullOrEmpty(_query.Params.Name.Value))
            {
                if (!string.IsNullOrEmpty(sb.ToString()))
                    sb.Append(_query.Params.Name.Operator);
                sb.Append(_query.Params.Name.ToSqlString("f.[Name]", "Name"));
            }
        
            // CreatedUserId
            if (_query.Params.CreatedUserId.Value > -1)
            {
                if (!string.IsNullOrEmpty(sb.ToString()))
                    sb.Append(_query.Params.CreatedUserId.Operator);
                sb.Append(_query.Params.CreatedUserId.ToSqlString("f.CreatedUserId"));
            }

            return sb.ToString();

        }

        private string GetQualifiedColumnName(string columnName)
        {
            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            return columnName.IndexOf('.') >= 0
                ? columnName
                : "f." + columnName;
        }

        private string BuildOrderBy()
        {
            if (_query.SortColumns.Count == 0) return null;
            var sb = new StringBuilder();
            var i = 0;
            foreach (var sortColumn in _query.SortColumns)
            {
                sb.Append(GetQualifiedColumnName(sortColumn.Key));
                if (sortColumn.Value != OrderBy.Asc)
                    sb.Append(" DESC");
                if (i < _query.SortColumns.Count - 1)
                    sb.Append(", ");
                i += 1;
            }
            return sb.ToString();
        }

        #endregion

    }

    #endregion

}
