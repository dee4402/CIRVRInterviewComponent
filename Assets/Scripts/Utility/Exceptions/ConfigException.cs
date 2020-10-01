using System;

namespace UnityStandardAssets.Utility.Exceptions
{
    class ConfigException : Exception
    {
        string configType;

        public ConfigException(string filename, string msg = "There was a configuration loading error")
            : base(msg)
        {
            configType = filename;
        }
        
    }
}