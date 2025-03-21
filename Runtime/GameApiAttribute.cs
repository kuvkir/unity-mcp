using System;

namespace UnityMCP.Runtime
{
    [AttributeUsage(AttributeTargets.Method)]
    public class GameApiAttribute : Attribute
    {
        public string CommandName { get; }
        
        public GameApiAttribute(string commandName)
        {
            CommandName = commandName;
        }
    }
} 