using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using ClearScriptIssue.Data;
using ClearScriptIssue.Facades;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace ClearScriptIssue.Extensions
{
    public static class ClearScriptEngineExtensions
    {
        #region Static members

        public static void AddHostObjectWithoutCheck(this V8ScriptEngine engine, string itemName, object target)
        {
            var clearScriptTypes = typeof(V8ScriptEngine).Assembly.GetTypes();

            var scriptItemType = clearScriptTypes.FirstOrDefault(t => t.Name.AreEqual("ScriptItem"));
            if (scriptItemType == null) throw new InvalidCastException("ScriptItem not found");

            var unwrapMethod = scriptItemType.GetMethod("Unwrap");
            if (unwrapMethod == null) throw new InvalidCastException("ScriptItem.Unwrap method not found");

            var v8ContextProxyType = clearScriptTypes.FirstOrDefault(t => t.Name.AreEqual("V8ContextProxy"));
            if (v8ContextProxyType == null) throw new InvalidCastException("V8ContextProxy not found");

            var addGlobalItemMethod = v8ContextProxyType.GetMethod("AddGlobalItem");
            if (addGlobalItemMethod == null) throw new InvalidCastException("V8ContextProxy.AddGlobalItem method not found");

            var engineProxyField = typeof(V8ScriptEngine).GetField("proxy", BindingFlags.Instance | BindingFlags.NonPublic);
            if (engineProxyField == null) throw new InvalidCastException("V8ScriptEngine.proxy field not found");

            var engineScriptInvokeMethod = typeof(V8ScriptEngine).GetMethod("ScriptInvoke",
                                                                            BindingFlags.Instance | BindingFlags.NonPublic,
                                                                            null,
                                                                            new[] { typeof(Action) },
                                                                            null);
            if (engineScriptInvokeMethod == null) throw new InvalidCastException("V8ScriptEngine.ScriptInvoke method not found");

            engineScriptInvokeMethod.Invoke(engine,
                                            new object[]
                                            {
                                                new Action(() =>
                                                {
                                                    var unwrappedScriptItem = unwrapMethod.Invoke(target, new object[] { });
                                                    var proxyField = engineProxyField.GetValue(engine);

                                                    addGlobalItemMethod.Invoke(proxyField, new[] { itemName, unwrappedScriptItem, false });
                                                })
                                            });
        }

        public static object FromScriptObject(this V8ScriptEngine engine, object value)
        {
            if (value is Undefined) return null;
            if (value is IOrigin valueOrigin) return valueOrigin.QueryOrigin();
            if (value is DynamicObject dynamicObject)
            {
                dynamic dynamicValue = value;
                var result = new DynamicSerializableObject();

                var isArray = engine.Script.Array.isArray(value);
                if (isArray)
                {
                    var arrayLength = dynamicValue.length;

                    var arrayValue = new List<object>();
                    for (var i = 0; i < arrayLength; i++)
                    {
                        arrayValue.Add(engine.FromScriptObject((object)dynamicValue[i]));
                    }

                    return arrayValue.ToArray();
                }

                foreach (var property in dynamicObject.GetDynamicMemberNames())
                {
                    var propertyValue = (object)dynamicValue[property];
                    if (propertyValue is Undefined) continue;
                    if (propertyValue is Delegate) continue;

                    result[property] = engine.FromScriptObject(propertyValue);
                }

                return result;
            }

            return value;
        }

        public static object ToScriptObject(this V8ScriptEngine engine, object value)
        {
            if (value is MajorObject majorObject)
            {
                return new MajorObjectFacade
                {
                    payload = majorObject.Payload
                };
            }

            if (value is Array arrayObject)
            {
                var temporaryVariableName = "__temp";
                engine.Execute($"var {temporaryVariableName} = [];");

                var result = engine.Script[temporaryVariableName];

                for (var i = 0; i < arrayObject.Length; i++)
                {
                    result[i] = engine.ToScriptObject(arrayObject.GetValue(i));
                }

                engine.Execute($"delete {temporaryVariableName};");

                return result;
            }

            if (value is DynamicSerializableObject dynamicSerializableObject)
            {
                var temporaryVariableName = "__temp";
                engine.Execute($"var {temporaryVariableName} = {{}};");

                var result = engine.Script[temporaryVariableName];

                foreach (var property in dynamicSerializableObject.Properties)
                {
                    if (property.Value is Undefined) continue;
                    result[property.Name] = engine.ToScriptObject(property.ArrayValue ?? property.Value);
                }

                engine.Execute($"delete {temporaryVariableName};");

                return result;
            }

            return value;
        }

        #endregion
    }
}