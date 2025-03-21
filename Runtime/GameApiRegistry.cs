using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json.Linq;

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
                    Debug.Log($"=======================");
                    Debug.Log($"Registering API method: {attribute.CommandName} -> {type.Name}.{method.Name}");
                    Debug.Log($"Parameter names: {string.Join(", ", attribute.ParameterNames)}");
                    Debug.Log($"=======================");
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

        // Add a new method for executing commands with automatic parameter conversion
        public static object ExecuteCommand(string commandName, JObject @params)
        {
            if (!TryGetMethod(commandName, out var method, out var target))
            {
                throw new Exception($"Command '{commandName}' is not registered in GameApiRegistry.");
            }
            Debug.Log($"Executing command: {commandName} with params: {@params}");
            
            // Get the parameter names from the attribute
            var attribute = method.GetCustomAttribute<GameApiAttribute>();
            var parameterNames = attribute?.ParameterNames ?? Array.Empty<string>();
            
            // Get the method's parameter information
            var methodParams = method.GetParameters();
            var arguments = new object[methodParams.Length];
            
            // Map parameters from JObject to method arguments
            for (int i = 0; i < methodParams.Length; i++)
            {
                var param = methodParams[i];
                Debug.Log($"Parameter {i}: {param.Name} ({param.ParameterType.Name})");
                
                // If we have a parameter name in the attribute, use it to get the value
                string paramName = i < parameterNames.Length ? parameterNames[i] : param.Name;
                
                if (@params.ContainsKey(paramName))
                {
                    // Convert the parameter to the expected type
                    try
                    {
                        arguments[i] = @params[paramName].ToObject(param.ParameterType);
                        Debug.Log($"Converted parameter {i}: {paramName} to {arguments[i]}");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to convert parameter '{paramName}' to type '{param.ParameterType.Name}': {ex.Message}");
                    }
                }
                else if (param.IsOptional)
                {
                    // Use the default value for optional parameters
                    arguments[i] = param.DefaultValue;
                }
                else
                {
                    // If the parameter is required but not provided, throw an exception
                    throw new Exception($"Required parameter '{paramName}' not provided for command '{commandName}'");
                }
            }
            
            // Invoke the method with the converted parameters
            return method.Invoke(target, arguments);
        }
    }
} 