using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace ComponentAsService.Specs
{
    public static class TestServerBuilder
    {
        public static TestServer RunningServerUsingStartup<TStartup>(Assembly mvcApplicationAssembly = null, string webProjectPhysicalPath = null,
            string environmentName = "Development",
            FeatureCollection featureCollection = null)
        {
            mvcApplicationAssembly = mvcApplicationAssembly ?? typeof (TStartup).GetTypeInfo().Assembly;
            webProjectPhysicalPath = webProjectPhysicalPath 
                                     ?? GuessWebProjectPathFromAssemblyName(mvcApplicationAssembly);

            return new TestServer(
                new WebHostBuilder()
                    .UseContentRoot(webProjectPhysicalPath)
                    .ConfigureServices(services => InitializeServices(services, mvcApplicationAssembly))
                    .UseEnvironment(environmentName)
                    .UseStartup(typeof (TStartup)), featureCollection ?? new FeatureCollection());
        }

        internal static void InitializeServices(IServiceCollection services, Assembly assembly)
        {
            ApplicationPartManager implementationInstance = new ApplicationPartManager();
            implementationInstance.ApplicationParts.Add(new AssemblyPart(assembly));
            implementationInstance.FeatureProviders.Add(new ControllerFeatureProvider());
            implementationInstance.FeatureProviders.Add(new ViewComponentFeatureProvider());
            services.AddSingleton(implementationInstance);
        }

        public static string GuessWebProjectPathFromAssemblyName(Assembly startupAssembly, string projectFilePattern = "*.*proj")
        {
            string name = startupAssembly.GetName().Name;
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo directoryInfo = new DirectoryInfo(baseDirectory);
            while (!directoryInfo.GetFileSystemInfos("*.sln").Any())
            {
                directoryInfo = directoryInfo.Parent;
                if (directoryInfo.Parent == null)
                    throw new Exception(string.Format("Solution root could not be located using application root {0}.", baseDirectory));
            }
            FileSystemInfo fileSystemInfo = directoryInfo.EnumerateDirectories("*", SearchOption.TopDirectoryOnly).SelectMany(d => (IEnumerable<FileSystemInfo>) d.GetFileSystemInfos(projectFilePattern)).FirstOrDefault(p => p.Name == name + p.Extension);
            if (fileSystemInfo == null)
                throw new ArgumentException(string.Format("Failed to find a Project file {0} whose name matched the Startup classes' AssemblyName {1} under solution directory {2}", projectFilePattern, name, directoryInfo.FullName), directoryInfo.FullName);
            return Path.GetDirectoryName(fileSystemInfo.FullName);
        }
    }
}