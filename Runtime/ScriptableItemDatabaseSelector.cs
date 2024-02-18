using System;
using UnityEngine;

namespace Platinio.ScriptableObjectDatabase
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ScriptableItemDatabaseSelector : PropertyAttribute
    {
        public Type DatabaseType;
        public string DatabaseName;

        public ScriptableItemDatabaseSelector(Type databaseType, string name = null)
        {
            DatabaseType = databaseType;
            DatabaseName = name;
        }
    }
}

