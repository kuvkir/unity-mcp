using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityMCP.Runtime
{
    public static class GameApiRegistry
    {
        private static Dictionary<string, (MethodInfo Method, object Target)> _apiMethods = 
            new Dictionary<string, (MethodInfo, object)>();

        public static void RegisterObject(object target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var type = target.GetType();
            
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var attribute = method.GetCustomAttribute<GameApiAttribute>(true);
                if (attribute != null)
                {
                    Debug.Log($"Registering API method: {attribute.CommandName} -> {type.Name}.{method.Name}");
                    _apiMethods[attribute.CommandName] = (method, target);
                }
            }
        }

        public static void UnregisterObject(object target)
        {
            if (target == null) return;

            var keysToRemove = new List<string>();
            foreach (var pair in _apiMethods)
            {
                if (pair.Value.Target == target)
                {
                    Debug.Log($"Unregistering API method: {pair.Key} -> {pair.Value.Method.Name}");
                    keysToRemove.Add(pair.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _apiMethods.Remove(key);
            }
        }

        public static bool TryGetMethod(string commandName, out MethodInfo method, out object target)
        {
            if (_apiMethods.TryGetValue(commandName, out var value))
            {
                method = value.Method;
                target = value.Target;
                return true;
            }

            method = null;
            target = null;
            return false;
        }

        /// <summary>
        /// Checks if a command is registered in the registry
        /// </summary>
        /// <param name="commandName">Name of the command to check</param>
        /// <returns>True if the command exists, false otherwise</returns>
        public static bool HasCommand(string commandName)
        {
            return _apiMethods.ContainsKey(commandName);
        }
        
        /// <summary>
        /// Gets all registered command names
        /// </summary>
        /// <returns>Collection of command names</returns>
        public static IEnumerable<string> GetAllCommandNames()
        {
            return _apiMethods.Keys.ToList();
        }
    }
} 