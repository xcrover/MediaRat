using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ops.NetCoe.LightFrame;
using System.Net;
using MediOrg.Views;
using MediOrg;

namespace MediOrg {

    /// <summary>
    /// Bootstrapper is responsible for proper initialization on startup
    /// </summary>
    public static class Bootstrap {
        //static string ServiceEndpointPrefix = "Default";

        static bool _isInitiated;
        static object _syncLock = new object();

        /// <summary>Ensures the context.</summary>
        /// <returns></returns>
        public static IAppContext EnsureContext() {
            if (!_isInitiated) {
                lock (_syncLock) {
                    if (!_isInitiated) {
                        ServiceLocator sl = new ServiceLocator();
                        AppContext.SetContext(AppContext.CreateDefault("XC.MediOrg", sl));
                        InitLocator(sl);
                        AppContext.Current.LogTrace("Initialization completed");
                        _isInitiated = true;
                    }
                }
            }
            return AppContext.Current;
        }

        static void InitLocator(ServiceLocator locator) {
            //LogVModel log = new LogVModel();
            WinLog log = new WinLog(AppContext.Current.ApplicationTitle);
            //LogVModel log = new LogVModel();
            locator.RegisterInstance<Ops.NetCoe.LightFrame.ILog>(log);
            //locator.RegisterInstance<LogVModel>(log);
            AppContext.Current.LogTrace("Init locator");
            locator.RegisterType<IWebProxy>(() => {
                IWebProxy proxy = WebRequest.GetSystemWebProxy();
                return proxy;
            });

            locator.RegisterSingleton<Ops.NetCoe.LightFrame.IServiceLocator>(() => locator);
            //locator.RegisterInstance<VideoHelper>(new VideoHelper());
            //ViewFactory vFactory = new ViewFactory();
            //vFactory.Register<ImageProjectVModel, ImageProjectView>();
            //vFactory.Register<VideoProjectVModel, VideoProjectView>();
            //vFactory.Register<PropElementListVModel, PropElementListView>();
            //vFactory.Register<TrackGroupVModel, TrackGroupView>();
            //vFactory.Register<MediaSplitVModel, MediaSplitView>();
            //vFactory.Register<TextRqtVModel, TextRqtView>();

            ////locator.RegisterInstance<LogVModel>(log);
            //locator.RegisterInstance<ViewFactory>(vFactory);
            //var uiBus = new UIBus();
            //locator.RegisterInstance<UIBus>(uiBus);
            IUIHelper uhlp = App.Current as IUIHelper;
            if (uhlp == null) {
                string em = "Initialization failure: Class App does not implement IUIHelper interface";
                AppContext.Current.LogTechError(em, null);
                throw new TechnicalException(em);
            }
            locator.RegisterInstance<IUIHelper>(uhlp);
            //locator.RegisterInstance<ICrypto>(new Crypto());
            //locator.RegisterType<IisWebsiteVModel, IisWebsiteVModel>();
            //locator.RegisterType<DeploymentTaskVModel, DeploymentTaskVModel>();

            //ServiceEndpointPrefix = AppContext.Current.GetAppCfgItem("ServicePrefix");
            //if (ServiceEndpointPrefix == null) {
            //    ServiceEndpointPrefix = "Default";
            //    AppContext.Current.LogWarning("Configuration error: value appSettings\\@ServicePrefix is not specified. Will fall back to 'Default'.");
            //}

            
            // Create Main VModel at the end so it can use previos declarations
            //MainVModel mvm = new MainVModel();
            //locator.RegisterInstance<MainVModel>(mvm);
        }


    }
}
