using System;
using UnityEngine;

namespace ArcaneOnyx.ScriptableObjectDatabase
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ScriptableItemDatabaseSelector : PropertyAttribute
    {
        public Type DatabaseType;

        public ScriptableItemDatabaseSelector(Type databaseType)
        {
            DatabaseType = databaseType;
        }
    }
}

