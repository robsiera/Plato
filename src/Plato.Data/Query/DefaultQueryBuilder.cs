﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plato.Abstractions.Extensions;

namespace Plato.Data.Query123
{
    
    public interface IQueryBuilder
    {


        string BuildSqlStartId();

        string BuildSqlPopulate();

        string BuildSqlCount();


    }
    
    public class WhereString
    {
        private string _value;
        private QueryOperator _operator = QueryOperator.And;
        private readonly StringBuilder _builder;

        public WhereString()
        {
            _builder = new StringBuilder();
        }

        public string Value => _value;

        public string Operator => _operator == QueryOperator.And ? " AND " : " OR ";

        public WhereString Or()
        {
            _operator = QueryOperator.Or;
            return this;
        }
        public WhereString And()
        {
            _operator = QueryOperator.And;
            return this;
        }

        public WhereString Equals(string value)
        {
            if (!string.IsNullOrEmpty(_builder.ToString()))
                _builder.Append(" OR ");
            _value = value;
            _builder.Append("{0} = @{0}");
            return this;
        }

        public WhereString StartsWith(string value)
        {
            if (!string.IsNullOrEmpty(_builder.ToString()))
                _builder.Append(" OR ");
            _value = value;
            _builder.Append("{0} LIKE '%' + @{0}");
            return this;
        }

        public WhereString Endsith(string value)
        {
            if (!string.IsNullOrEmpty(_builder.ToString()))
                _builder.Append(" OR ");
            _value = value;
            _builder.Append("{0} LIKE @{0} + '%'");
            return this;
        }

        public WhereString IsIn(string value, char delimiter = ',')
        {
            if (!string.IsNullOrEmpty(_builder.ToString()))
                _builder.Append(" OR ");
            _value = value;
            _builder
                .Append("{0} IN ( SELECT * FROM plato_fn_ListToTable('")
                .Append(delimiter)
                .Append("', @{0})  )");
            return this;
        }

        public WhereString Like(string value)
        {
            if (!string.IsNullOrEmpty(_builder.ToString()))
                _builder.Append(" OR ");
            _value = value;
            _builder.Append("{0} LIKE '%' + @{0}");
            return this;
        }

        public string ToSqlString(string parameterName)
        {
            return _builder.ToString().Replace("{0}", parameterName);
        }
        
    }
    
    public class WhereInt
    {
        private int _value;
        private QueryOperator _operator = QueryOperator.And;
        private readonly StringBuilder _builder;

        public WhereInt()
        {
            _builder = new StringBuilder();
        }

        public int Value => _value;

        public string Operator => _operator == QueryOperator.And ? " AND " : " OR ";

        public WhereInt Or()
        {
            _operator = QueryOperator.Or;
            return this;
        }
        public WhereInt And()
        {
            _operator = QueryOperator.And;
            return this;
        }

        public WhereInt Equals(int value)
        {
            if (!string.IsNullOrEmpty(_builder.ToString()))
                _builder.Append(" OR ");
            _value = value;
            _builder.Append("{0} = @{0}");
            return this;
        }

        public WhereInt LessThan(int value)
        {
            if (!string.IsNullOrEmpty(_builder.ToString()))
                _builder.Append(" OR ");
            _value = value;
            _builder.Append("{0} < @{0}");
            return this;
        }

        public WhereInt LessThanOrEqual(int value)
        {
            if (!string.IsNullOrEmpty(_builder.ToString()))
                _builder.Append(" OR ");
            _value = value;
            _builder.Append("{0} <= @{0}");
            return this;
        }
        
        public WhereInt GreaterThan(int value)
        {
            if (!string.IsNullOrEmpty(_builder.ToString()))
                _builder.Append(" OR ");
            _value = value;
            _builder.Append("{0} >= @{0}");
            return this;
        }

        public WhereInt GreaterThanOrEqual(int value)
        {
            if (!string.IsNullOrEmpty(_builder.ToString()))
                _builder.Append(" OR ");
            _value = value;
            _builder.Append("{0} >= @{0}");
            return this;
        }


        public WhereInt Between(int min, int max)
        {
            if (!string.IsNullOrEmpty(_builder.ToString()))
                _builder.Append(" OR ");
            _value = max;
            _builder
                .Append("{0} >= ")
                .Append(min.ToString())
                .Append(" AND ")
                .Append("{0} <= ")
                .Append(max.ToString());
            return this;
        }
        
        public WhereInt IsIn(int[] value, char delimiter = ',')
        {
            if (!string.IsNullOrEmpty(_builder.ToString()))
                _builder.Append(" OR ");
            _value = value[0];
            _builder
                .Append("{0} IN ( SELECT * FROM plato_fn_ListToTable('")
                .Append(delimiter)
                .Append("', '").
                Append(value.ToDelimitedString()).
                Append("' )");
            return this;
        }
        
        public string ToSqlString(string parameterName)
        {
            return _builder.ToString().Replace("{0}", parameterName);
        }

    }
    
    public enum QueryOperator
    {
        And,
        Or
    }



}
