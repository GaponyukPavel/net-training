using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Reflection.Tasks
{
    public static class CommonTasks
    {

        /// <summary>
        /// Returns the lists of public and obsolete classes for specified assembly.
        /// Please take attention: classes (not interfaces, not structs)
        /// </summary>
        /// <param name="assemblyName">name of assembly</param>
        /// <returns>List of public but obsolete classes</returns>
        public static IEnumerable<string> GetPublicObsoleteClasses(string assemblyName)
        {
            List<String> result = new List<string>();
            Assembly assembly = Assembly.Load(assemblyName);
            Type[] assemblyTypes = assembly.GetTypes();
            foreach (Type type in assemblyTypes)
            {
                if (!type.IsClass)
                    continue;
                var attr = type.GetCustomAttribute(typeof(ObsoleteAttribute));
                if (attr != null)
                    result.Add(type.Name);
            }
            return result;
        }

        /// <summary>
        /// Returns the value for required property path
        /// </summary>
        /// <example>
        ///  1) 
        ///  string value = instance.GetPropertyValue("Property1")
        ///  The result should be equal to invoking statically
        ///  string value = instance.Property1;
        ///  2) 
        ///  string name = instance.GetPropertyValue("Property1.Property2.FirstName")
        ///  The result should be equal to invoking statically
        ///  string name = instance.Property1.Property2.FirstName;
        /// </example>
        /// <typeparam name="T">property type</typeparam>
        /// <param name="obj">source object to get property from</param>
        /// <param name="propertyPath">dot-separated property path</param>
        /// <returns>property value of obj for required propertyPath</returns>
        public static T GetPropertyValue<T>(this object obj, string propertyPath)
        {
            T result;
            object curentObj = obj;
            Type type = obj.GetType();
            string[] Path = propertyPath.Split('.');
            for (int i = 0; i < Path.Length - 1; i++)
                curentObj = type.GetProperty(Path[i]).GetValue(curentObj);
            PropertyInfo propertyInfo = type.GetProperty(Path[Path.Length - 1]);
            result = (T)propertyInfo.GetValue(curentObj);
            return result;
        }


        /// <summary>
        /// Assign the value to the required property path
        /// </summary>
        /// <example>
        ///  1)
        ///  instance.SetPropertyValue("Property1", value);
        ///  The result should be equal to invoking statically
        ///  instance.Property1 = value;
        ///  2)
        ///  instance.SetPropertyValue("Property1.Property2.FirstName", value);
        ///  The result should be equal to invoking statically
        ///  instance.Property1.Property2.FirstName = value;
        /// </example>
        /// <param name="obj">source object to set property to</param>
        /// <param name="propertyPath">dot-separated property path</param>
        /// <param name="value">assigned value</param>
        public static void SetPropertyValue(this object obj, string propertyPath, object value)
        {
            object curentObj = obj;
            string[] Path = propertyPath.Split('.');
            for (int i = 0; i < Path.Length - 1; i++)
                curentObj = curentObj.GetType().GetProperty(Path[i]).GetValue(curentObj);
            PropertyInfo property =  curentObj.GetType().BaseType.GetProperty(Path[Path.Length - 1]);
            property.GetSetMethod(true).Invoke(curentObj, new object[] { value });
        }


    }
}
