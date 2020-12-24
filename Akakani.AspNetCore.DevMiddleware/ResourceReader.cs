namespace Akakani.AspNetCore.DevMiddleware
{
    using System;
    using System.IO;
    using System.Reflection;


    internal static class ResourceReader
    {
        public static string Read(Type assemblyContainingType, string path)
        {
            Assembly assembly = assemblyContainingType.GetTypeInfo().Assembly;
            string embeddedResourceName = assembly.GetName().Name + path.Replace("/", ".");

            using (Stream stream = assembly.GetManifestResourceStream(embeddedResourceName))
            {
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }
}
