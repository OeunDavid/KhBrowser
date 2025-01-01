
using Ninject;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ToolKHBrowser.ToolLib.Data;
using ToolKHBrowser.ViewModels;
using ToolLib;
using ToolLib.Data;
using ToolLib.Tool;
using WpfUI.ViewModels;

namespace WpfUI
{
    public class DIConfig
    {
        private static KernelBase kernelBase = new StandardKernel()
        {

        };
        static DIConfig()
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
        /// <summary>
        /// Adapter function for get object by DI Container
        /// If we want to change the DI Container in the future we just change this method.
        /// Then it will work fine.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>()
        {
            return Kernel.Get<T>();
        }
    }
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<IConfigViewModel>().ToMethod((contx) => new ConfigViewModel(ToolDiConfig.Get<IConfigDao>()));
            Bind<IStoreViewModel>().ToMethod((contx) => new StoreViewModel(ToolDiConfig.Get<IStoreDao>()));
            Bind<IFbAccountViewModel>().ToMethod((contx) => new FbAccountViewModel(ToolDiConfig.Get<IAccountDao>()));
            Bind<ILoginViewModel>().ToMethod((contx) => new LoginViewModel(ToolDiConfig.Get<IAccountDao>(), ToolDiConfig.Get<ICacheDao>()));
            Bind<IActiveViewModel>().ToMethod((contx) => new ActiveViewModel(ToolDiConfig.Get<IAccountDao>(), ToolDiConfig.Get<ICacheDao>()));

            Bind<IShareViewModel>().ToMethod((contx) => new ShareViewModel(ToolDiConfig.Get<IAccountDao>(), ToolDiConfig.Get<ICacheDao>()));
            Bind<INewsFeedViewModel>().ToMethod((contx) => new NewsFeedViewModel(ToolDiConfig.Get<IAccountDao>(), ToolDiConfig.Get<ICacheDao>()));
            Bind<IPageViewModel>().ToMethod((contx) => new PageViewModel(ToolDiConfig.Get<IAccountDao>(), ToolDiConfig.Get<ICacheDao>(),ToolDiConfig.Get<IPagesDao>()));

            Bind<IGroupViewModel>().ToMethod((contx) => new GroupViewModel(ToolDiConfig.Get<IAccountDao>(), ToolDiConfig.Get<ICacheDao>(), ToolDiConfig.Get<IGroupsDao>()));
            Bind<IFriendsViewModel>().ToMethod((contx) => new FriendsViewModel(ToolDiConfig.Get<IAccountDao>(), ToolDiConfig.Get<ICacheDao>()));
            Bind<IProfileViewModel>().ToMethod((contx) => new ProfileViewModel(ToolDiConfig.Get<IAccountDao>(), ToolDiConfig.Get<ICacheDao>()));

            Bind<IVerifyViewModel>().ToMethod((contx) => new VerifyViewModel(ToolDiConfig.Get<IAccountDao>(), ToolDiConfig.Get<ICacheDao>()));
            Bind<IClearProfileViewModel>().ToMethod((contx) => new ClearProfileViewModel(ToolDiConfig.Get<IAccountDao>(), ToolDiConfig.Get<ICacheDao>()));
            Bind<ICacheViewModel>().ToMethod((contx) => new CacheViewModel(ToolDiConfig.Get<ICacheDao>()));
        }
    }
}
