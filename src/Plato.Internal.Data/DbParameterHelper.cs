﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Plato.Internal.Abstractions.Extensions;
using Plato.Internal.Data.Abstractions;
using Plato.Internal.Data.Abstractions.Extensions;

namespace Plato.Internal.Data
{

    public static class DbParameterHelper
    {

        public static string CreateExecuteStoredProcedureSql(
            string procedureName,
            params object[] args)
        {
            // Execute procedure 
            var sb = new StringBuilder("; EXEC ");
            sb.Append(procedureName);

            if (args != null)
            {
                for (var i = 0; i < args.Length; i++)
                {

                    // Values can be null
                    if (args[i] != null)
                    {
                        var valueType = args[i].GetType();
                        if (valueType.IsAnonymousType())
                        {
                            var properties = valueType.GetProperties();
                            for (var x = 0; x < properties.Length; x++)
                            {
                                var property = properties[x];
                                sb.Append($" @{property.Name}");
                                if (x < properties.Length - 1)
                                    sb.Append(",");
                            }

                        }
                        else
                        {

                            sb.Append($" @{i}");
                            if (i < args.Length - 1)
                                sb.Append(",");
                        }

                    }
                    else
                    {
                        sb.Append($" @{i}");
                        if (i < args.Length - 1)
                            sb.Append(",");
                    }

                }

            }

            return sb.ToString();
        }

        public static string CreateScalarStoredProcedureSql(string procedureName, params object[] args)
        {

            // Build a collection of output parameters and there index
            IDictionary<int, IDbDataParameter> outputParams = null; ;
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] != null)
                {
                    if (args[i].GetType() == typeof(DbDataParameter))
                    {
                        if (((DbDataParameter)args[i]).Direction == ParameterDirection.Output)
                        {
                            if (outputParams == null)
                            {
                                outputParams = new Dictionary<int, IDbDataParameter>(); ;
                            }
                            outputParams.Add(i, ((IDbDataParameter)args[i]));
                        }
                    }
                }
            }

            var sb = new StringBuilder();
            if (outputParams != null)
            {
                foreach (var outputParam in outputParams)
                {
                    var name = !string.IsNullOrEmpty(outputParam.Value.ParameterName)
                        ? outputParam.Value.ParameterName
                        : outputParam.Key.ToString();
                    sb.Append($"DECLARE @{name}_out {outputParam.Value.DbTypeNormalized()};");
                }
            }

            sb.Append("EXEC ");
            sb.Append(procedureName);
            for (var i = 0; i < args.Length; i++)
            {

                if (outputParams?.ContainsKey(i) ?? false)
                {
                    var name = !string.IsNullOrEmpty(outputParams[i].ParameterName)
                        ? outputParams[i].ParameterName
                        : i.ToString();
                    sb.Append($" @{name}_out output");
                }
                else
                {
                    sb.Append($" @{i}");
                }

                if (i < args.Length - 1)
                    sb.Append(",");
            }

            sb.Append(";");

            // Return output parameters
            if (outputParams != null)
            {
                sb.Append("SELECT ");
                var i = 0;
                foreach (var outputParam in outputParams)
                {
                    var name = !string.IsNullOrEmpty(outputParam.Value.ParameterName)
                        ? outputParam.Value.ParameterName
                        : outputParam.Key.ToString();
                    sb.Append("@")
                        .Append(name)
                        .Append("_out");
                    if (i < outputParams.Count - 1)
                    {
                        sb.Append(",");
                    }
                    i++;
                }

                sb.Append(";");
            }

            return sb.ToString();
        }


        public static SqlCommand CreateSqlCommand(
            SqlConnection connection,
            string sql,
            params object[] args)
        {

            // Create the command and add parameters
            var cmd = connection.CreateCommand();
            cmd.Connection = connection;
            cmd.CommandText = sql;

            foreach (var value in args)
            {
                if (value != null)
                {
                    var valueType = value.GetType();
                    // If we have an anonymous type use the
                    // property names from the anonymous type
                    // as the parameter names for our query
                    if (valueType.IsAnonymousType())
                    {
                        var properties = valueType.GetProperties();
                        foreach (var property in properties)
                        {
                            var propValue = property.GetValue(value, null);
                            cmd.Parameters.Add(CreateParameter(
                                cmd,
                                $"@{property.Name}",
                                propValue,
                                propValue.GetType()));
                        }

                    }
                    else
                    {

                        // use params object array
                        cmd.Parameters.Add(CreateParameter(
                            cmd,
                            $"@{cmd.Parameters.Count}",
                            value,
                            valueType));
                    }

                }
                else
                {
                    // null values
                    cmd.Parameters.Add(CreateParameter(
                        cmd,
                        $"@{cmd.Parameters.Count}",
                        null));
                }

            }

            return cmd;

        }

        public static IDbDataParameter CreateParameter(IDbCommand cmd, string name, object value, Type valueType = null)
        {

            var p = cmd.CreateParameter();
            p.ParameterName = name;

            if (value == null)
            {
                p.Value = DBNull.Value;
            }
            else
            {

                if (valueType == null)
                {
                    throw new ArgumentNullException(nameof(valueType));
                }

                if (valueType == typeof(Guid))
                {
                    p.Value = value.ToString();
                    p.DbType = DbType.String;
                    p.Size = 40;
                }
                else if (valueType == typeof(byte[]))
                {
                    p.Value = value;
                    p.DbType = DbType.Binary;
                }
                else if (valueType == typeof(string))
                {
                    p.Size = Math.Max(((string)value).Length + 1, 4000); // Help query plan caching by using common size
                    p.Value = value;
                }
                else if (valueType == typeof(bool))
                {
                    p.Value = ((bool)value) ? 1 : 0;
                    p.DbType = DbType.Boolean;
                }
                else if (valueType == typeof(int))
                {
                    p.Value = ((int)value);
                }
                else if (valueType == typeof(DateTime?))
                {
                    p.Value = ((DateTime)value);
                }
                else if (valueType == typeof(DbDataParameter))
                {
                    var dbParam = (IDbDataParameter)value;
                    p.ParameterName = dbParam.ParameterName;
                    p.Value = dbParam.Value ?? DBNull.Value;
                    p.DbType = dbParam.DbType;
                    p.Direction = dbParam.Direction;
                }
                else
                {
                    p.Value = value;
                }
            }

            return p;

        }
        
    }

}