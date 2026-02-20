using Ninject;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ToolKHBrowser.ToolLib.Data;
using ToolLib.Data;
using ToolLib.Http;
using ToolLib.Tool;
using ToolKHBrowser.ToolLib.Mail;

namespace ToolLib
{
    public class ToolDiConfig
    {
        private static KernelBase kernelBase = new StandardKernel()
        {

        };
         static ToolDiConfig()
        {
            kernelBase.Load(Assembly.GetExecutingAssembly());
        }
        public static KernelBase Kernel
        {
            get
            {
                return kernelBase;
            }
        }
        public static T Get<T>()
        {
            return Kernel.Get<T>();
        }
    }
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<IAdbCommand>().To<AdbCommand>();
            Bind<IDataDao>().To<DataDao>();
            Bind<IAccountDao>().To<AccountDao>();
            Bind<IHttpHelper>().To<HttpHelper>();
            Bind<IConfigDao>().To<ConfigDao>();
            Bind<IGroupsDao>().To<GroupsDao>();
            Bind<IPagesDao>().To<PagesDao>();
            Bind<IStoreDao>().To<StoreDao>();
            Bind<ICacheDao>().To<CacheDao>();
            Bind<IClientEmail>().To<ClientEmail>();
            Bind<LDPlayerTool>().ToSelf();
        }
    }
}
