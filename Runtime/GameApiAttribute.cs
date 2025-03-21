using System;

namespace UnityMCP.Runtime
{
    [AttributeUsage(AttributeTargets.Method)]
    public class GameApiAttribute : Attribute
    {
        public string CommandName { get; }
        public string[] ParameterNames { get; }
        // public string Documentation { get; }

        public GameApiAttribute(string commandName, params string[] parameterNames)
        {
            CommandName = commandName;
            ParameterNames = parameterNames;
        }

        // public GameApiAttribute(string commandName, string doc, params string[] parameterNames)
        // {
        //     CommandName = commandName;
        //     Documentation = doc;
        //     ParameterNames = parameterNames;
        // }
    }
} 