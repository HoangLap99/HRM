using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace HRM.Api.Persistence;

public static class DatabaseFacadeExtensions
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> WritablePropsCache = new();

    /// <summary>
    /// Tuong thich Database.SqlQuery&lt;T&gt;(string sql, params object[] parameters) tu EF6 tren EF Core 8.
    /// </summary>
    public static List<T> SqlQuery<T>(this DatabaseFacade database, string sql, params object[] parameters)
    {
        ArgumentNullException.ThrowIfNull(database);
        if (string.IsNullOrWhiteSpace(sql))
        {
            return new List<T>();
        }

        var conn = database.GetDbConnection();
        var shouldClose = conn.State != ConnectionState.Open;
        if (shouldClose)
        {
            conn.Open();
        }

        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;

            var tx = database.CurrentTransaction?.GetDbTransaction();
            if (tx != null)
            {
                cmd.Transaction = tx;
            }

            if (parameters != null && parameters.Length > 0)
            {
                foreach (var p in parameters)
                {
                    if (p is DbParameter dbParam)
                    {
                        cmd.Parameters.Add(dbParam);
                    }
                    else
                    {
                        var param = cmd.CreateParameter();
                        param.ParameterName = $"@p{cmd.Parameters.Count}";
                        param.Value = p ?? DBNull.Value;
                        cmd.Parameters.Add(param);
                    }
                }
            }

            using var reader = cmd.ExecuteReader();
            var results = new List<T>();
            var targetType = typeof(T);
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            var isScalar = underlyingType.IsPrimitive
                           || underlyingType.IsEnum
                           || underlyingType == typeof(string)
                           || underlyingType == typeof(decimal)
                           || underlyingType == typeof(DateTime)
                           || underlyingType == typeof(DateTimeOffset)
                           || underlyingType == typeof(TimeSpan)
                           || underlyingType == typeof(Guid)
                           || underlyingType == typeof(byte[]);

            if (isScalar)
            {
                while (reader.Read())
                {
                    var val = reader.FieldCount > 0 ? reader.GetValue(0) : null;
                    if (val == null || val is DBNull)
                    {
                        results.Add(default!);
                    }
                    else if (underlyingType.IsEnum)
                    {
                        results.Add((T)Enum.ToObject(underlyingType, val));
                    }
                    else if (underlyingType == typeof(Guid) && val is string gs)
                    {
                        results.Add((T)(object)Guid.Parse(gs));
                    }
                    else if (underlyingType.IsInstanceOfType(val))
                    {
                        results.Add((T)val);
                    }
                    else
                    {
                        results.Add((T)Convert.ChangeType(val, underlyingType));
                    }
                }
                return results;
            }

            var props = WritablePropsCache.GetOrAdd(targetType, t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                 .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0)
                 .ToArray());

            var colMap = new Dictionary<int, PropertyInfo>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var colName = reader.GetName(i);
                var prop = props.FirstOrDefault(p => string.Equals(p.Name, colName, StringComparison.OrdinalIgnoreCase));
                if (prop != null)
                {
                    colMap[i] = prop;
                }
            }

            while (reader.Read())
            {
                var instance = Activator.CreateInstance<T>();
                foreach (var kv in colMap)
                {
                    var rawVal = reader.GetValue(kv.Key);
                    if (rawVal == null || rawVal is DBNull)
                    {
                        continue;
                    }

                    var propType = Nullable.GetUnderlyingType(kv.Value.PropertyType) ?? kv.Value.PropertyType;
                    if (propType.IsEnum)
                    {
                        kv.Value.SetValue(instance, Enum.ToObject(propType, rawVal));
                    }
                    else if (propType.IsInstanceOfType(rawVal))
                    {
                        kv.Value.SetValue(instance, rawVal);
                    }
                    else
                    {
                        kv.Value.SetValue(instance, Convert.ChangeType(rawVal, propType));
                    }
                }
                results.Add(instance);
            }

            return results;
        }
        finally
        {
            if (shouldClose && conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }
    }

    /// <summary>
    /// Tuong thich db.Set.SqlQuery(string sql, params object[] parameters) tu EF6 tren EF Core 8.
    /// </summary>
    public static IQueryable<TEntity> SqlQuery<TEntity>(this DbSet<TEntity> dbSet, string sql, params object[] parameters)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(dbSet);
        return parameters is { Length: > 0 }
            ? dbSet.FromSqlRaw(sql, parameters)
            : dbSet.FromSqlRaw(sql);
    }
}
