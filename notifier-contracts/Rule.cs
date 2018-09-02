using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GitLabNotifier;

namespace notifier_console_core
{
    public class Rule
    {
        public static IMessageRule For(string ruleName, IDictionary<string, string> ruleOptions)
        {
            var allTypesEnumerator = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes());

            var ruleType = allTypesEnumerator
                .FirstOrDefault(t => typeof(IMessageRule).IsAssignableFrom(t) && t.Name == ruleName);

            if (ruleType == null)
            {
                Console.WriteLine($"Cannot initialize requested rule \"{ruleName}\", error: rule not found.");
                return null;
            }

            var factoryInterfaceType = typeof(IRuleFactory<>).MakeGenericType(ruleType);

            var factoryType = allTypesEnumerator.FirstOrDefault(t => factoryInterfaceType.IsAssignableFrom(t));

            if (factoryType == null)
            {
                Console.WriteLine($"Cannot initialize requested rule \"{ruleName}\", error: factory not found.");
                return null;
            }

            var methodInfo = factoryType.GetMethod(nameof(IRuleFactory<object>.Create));

            try
            {
                var ruleObject = methodInfo.Invoke(
                    Create(factoryType),
                    new object[] {ruleOptions});

                return ruleObject as IMessageRule;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Cannot initialize requested rule \"{ruleName}\", error: {e.Message}");
            }

            return null;
        }

        public static IMessageRule For(string ruleName, IDictionary<string, IDictionary<string, string>> rulesConfig) => For(ruleName, rulesConfig[ruleName]);

        static object Create(Type typeToCreate)
        {
            var ctor = typeToCreate.GetConstructors().First();

            var exnew = Expression.New(ctor);
            var lambda = Expression.Lambda<Func<object>>(exnew);
            var compiled = lambda.Compile();
            return compiled();
        }
    }
}