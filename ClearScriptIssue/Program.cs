using System;
using ClearScriptIssue.Extensions;
using ClearScriptIssue.Facades;
using Microsoft.ClearScript.V8;

namespace ClearScriptIssue
{
    internal class Program
    {
        #region Static members

        private static V8ScriptEngine CreateEngine()
        {
            var engine = new V8ScriptEngine();
            engine.AddHostType("majorObject", typeof(MajorObjectFacade));
            return engine;
        }

        private static void Main(string[] args)
        {
            object serializedComplexValue;
            object serializedSimpleNotTypedArray;
            object serializedSimpleDynamicObject;

            using (var engine = CreateEngine())
            {
                //TODO: Does not work (Issue 1)
                //var simpleDynamicObject = engine.Evaluate("{a: 'b'}");
                var simpleDynamicObject = engine.Evaluate("let test = {a: 'b'}; test;");
                serializedSimpleDynamicObject = engine.FromScriptObject(simpleDynamicObject);

                //TODO: Does not work (Issue 2)
                //var simpleNotTypedArray = engine.Evaluate("['a','b']");
                //TODO: test object is already defined. But let was used in code above (Issue 3)
                Console.WriteLine($"Test variable after first evaluate run: {engine.Script.test}");
                //var simpleNotTypedArray = engine.Evaluate("let test = ['a','b']; test;");
                var simpleNotTypedArray = engine.Evaluate("let test2 = ['a','b']; test2;");
                serializedSimpleNotTypedArray = engine.FromScriptObject(simpleNotTypedArray);

                var complexValue = engine.Evaluate("let test3 = { d: ['a','b'], c: new majorObject() }; test3;");
                serializedComplexValue = engine.FromScriptObject(complexValue);
            }

            Console.WriteLine($"Serialized dynamic object: {serializedSimpleDynamicObject}");
            Console.WriteLine($"Serialized not typed array: {serializedSimpleNotTypedArray}");
            Console.WriteLine($"Serialized complex object: {serializedComplexValue}");

            Console.WriteLine("-------------");

            using (var engine = CreateEngine())
            {
                var deserialized = engine.ToScriptObject(serializedComplexValue);

                //TODO: Throws 'Invalid host item' exception (Issue 4)
                //engine.AddHostObject("test", deserialized);

                engine.AddHostObjectWithoutCheck("test", deserialized);

                Console.WriteLine($"Getting test.d[0]: {engine.Evaluate("test.d[0]")}"); //'a' expected
                Console.WriteLine($"Serialized complex object from second engine: {engine.FromScriptObject((object)engine.Script.test)}");
            }

            Console.WriteLine("-------------");

            //TODO: Not typed array information missed (Issue 5)
            using (var engine = CreateEngine())
            using (var engine2 = CreateEngine())
            {
                var deserialized = engine2.ToScriptObject(serializedComplexValue);

                //TODO: 'Invalid host item' exception will not be fired (strange bcs of Issue 4)
                engine.AddHostObject("test", deserialized);

                //TODO: Works as expected
                Console.WriteLine($"Getting test.d[0]: {engine.Evaluate("test.d[0]")}"); //'a' expected

                //TODO: But object was transformed from array to dynamic object. To determine array engine.Script.Array.isArray(value) method was used in FromScriptObject extension method
                Console.WriteLine($"Serialized complex object from second engine: {engine.FromScriptObject((object)engine.Script.test)}");
            }

            Console.ReadLine();
        }

        #endregion
    }
}