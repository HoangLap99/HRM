﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HRM.Api.Models
{
    public class DynamicObject : System.Dynamic.IDynamicMetaObjectProvider
    {

        private IDictionary<string, object> dynamicProperties = new Dictionary<string, object>();

        #region System.Dynamic.IDynamicMetaObjectProvider implementation
        public System.Dynamic.DynamicMetaObject GetMetaObject(Expression expression)
        {
            return new SerializableDynamicMetaObject(expression,
                System.Dynamic.BindingRestrictions.GetInstanceRestriction(expression, this), this);
        }
        #endregion

        #region Helper methods for dynamic meta object support
        internal object setValue(string name, object value)
        {
            if (dynamicProperties.ContainsKey(name))
            {
                dynamicProperties[name] = value;
            }
            else
            {
                dynamicProperties.Add(name, value);
            }
            return value;
        }

        internal object getValue(string name)
        {
            object value;
            if (!dynamicProperties.TryGetValue(name, out value))
            {
                value = null;
            }
            return value;
        }

        internal IEnumerable<string> getDynamicMemberNames()
        {
            return dynamicProperties.Keys;
        }
        public void UpdateDynamic(string name, object value)
        {
            if (dynamicProperties.ContainsKey(name))
            {
                dynamicProperties[name] = value;
            }
            else
            {
                dynamicProperties.Add(name, value);
            }
        }
        #endregion
    }

    public class SerializableDynamicMetaObject : System.Dynamic.DynamicMetaObject
    {
        Type objType;

        public SerializableDynamicMetaObject(Expression expression, System.Dynamic.BindingRestrictions restrictions, object value)
            : base(expression, restrictions, value)
        {
            objType = value.GetType();
        }

        public override System.Dynamic.DynamicMetaObject BindGetMember(System.Dynamic.GetMemberBinder binder)
        {
            var self = this.Expression;
            var dynObj = (DynamicObject)this.Value;
            var keyExpr = Expression.Constant(binder.Name);
            var getMethod = objType.GetMethod("getValue", BindingFlags.NonPublic | BindingFlags.Instance);
            var target = Expression.Call(Expression.Convert(self, objType),
                                         getMethod,
                                         keyExpr);
            return new System.Dynamic.DynamicMetaObject(target,
                System.Dynamic.BindingRestrictions.GetTypeRestriction(self, objType));
        }

        public override System.Dynamic.DynamicMetaObject BindSetMember(System.Dynamic.SetMemberBinder binder, System.Dynamic.DynamicMetaObject value)
        {
            var self = this.Expression;
            var keyExpr = Expression.Constant(binder.Name);
            var valueExpr = Expression.Convert(value.Expression, typeof(object));
            var setMethod = objType.GetMethod("setValue", BindingFlags.NonPublic | BindingFlags.Instance);
            var target = Expression.Call(Expression.Convert(self, objType),
            setMethod,
            keyExpr,
            valueExpr);
            return new System.Dynamic.DynamicMetaObject(target,
                System.Dynamic.BindingRestrictions.GetTypeRestriction(self, objType));
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var dynObj = (DynamicObject)this.Value;
            return dynObj.getDynamicMemberNames();
        }
    }
}

