using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AppPlate.Core.Extensions
{
    public static class ReflectionExtensions
    {
        public static Type GetGenericType(this Type genericType, Type genericArgument)
        {
            return genericType.MakeGenericType(genericArgument);
        }

        public static void CallMethod(this object owner, string methodName, Type genericArgument, params object[] arguments)
        {
            var method = owner.GetType().GetMethods()//.GetRuntimeMethods()
                .Single(m =>
                    m.Name == methodName &&
                    m.ContainsGenericParameters &&
                    m.GetParameters().Length == arguments.Length &&
                    m.GetGenericArguments().Length == 1);

            method.MakeGenericMethod(genericArgument).Invoke(owner, arguments.Length == 0 ? null : arguments);
        }

        public static IEnumerable<Type> AllTypes(this Assembly assembly)
        {
            return assembly.GetExportedTypes();
        }

        public static IEnumerable<Type> IsNotAbstract(this IEnumerable<Type> types)
        {
            return types.Where(type => !type.IsAbstract);
        }

        public static IEnumerable<Type> ImplementsInterface<TInterface>(this IEnumerable<Type> types)
        {
            return types.Where(ImplementsInterface<TInterface>);
        }

        public static IEnumerable<Type> IsSubClassOf<TClass>(this IEnumerable<Type> types)
        {
            return types.Where(type => type.IsSubclassOf(typeof(TClass)));
        }

        public static bool ImplementsInterface<TInterface>(this Type type)
        {
            return type.ImplementsInterface(typeof(TInterface));
        }

        public static bool ImplementsInterface(this Type type, Type interfaceType)
        {
            return type .GetInterfaces().Any(i => i == interfaceType);
        }

        public static bool HasAttribute<TAttribute>(this Type type)
            where TAttribute : class
        {
            var attribute = type.GetCustomAttributes(typeof(TAttribute), inherit: true).FirstOrDefault();
            return attribute != null;
        }

        public static bool HasAttribute<TAttribute>(this Type type, Func<TAttribute, bool> filter)
            where TAttribute : Attribute
        {
            var attribute = (TAttribute)type.GetCustomAttributes(typeof(TAttribute), inherit: true).FirstOrDefault();
            return attribute != null && filter(attribute);
        }

        public static string TypeName(this object value)
        {
            if (value == null) return null;
            return value.GetType().Name;
        }

        public static string TypeFullName(this object value)
        {
            if (value == null) return null;
            return value.GetType().FullName;
        }

        public static IEnumerable<Type> GetParentTypes(this Type type, Func<Type, bool> filter)
        {
            var parentType = type.BaseType;
            while (parentType != null && filter(parentType))
            {
                yield return parentType;
                parentType = parentType.BaseType;
            }
        }
    }
}