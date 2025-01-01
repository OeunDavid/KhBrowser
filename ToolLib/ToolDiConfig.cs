using Ninject;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ToolLib.Data;
using ToolLib.Http;
using ToolLib.Tool;

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

            Bind<IImageHelper>().To<ImageHelper>();
            Bind<IAdbCommand>().To<AdbCommand>();
            Bind<IAdbTask>().To<AdbTask>();
            Bind<IDataDao>().To<DataDao>();
            Bind<IAccountDao>().To<AccountDao>();
            Bind<IDeviceAccountDao>().To<DeviceAccountDao>();
            Bind<IActivityLogDao>().To<ActivityLogDao>();
            Bind<IDeviceDao>().To<DeviceDao>();
            Bind<IDeviceInfo>().To<DeviceInfo>();
            Bind<IFacebookTool>().To<FacebookTool>();
            Bind<IFacebookLiteTool>().To<FacebookLiteTool>();
            Bind<IStoreDao>().To<StoreDao>();
            Bind<IHttpHelper>().To<HttpHelper>();
            Bind<ITwoFactorRequest>().To<TwoFactorRequest>();
            Bind<IViewParser>().To<ViewParser>();
            Bind<IConfigDao>().To<ConfigDao>();
            Bind<IGroupDevicesDao>().To<GroupDevicesDao>();
        }
    }
}
