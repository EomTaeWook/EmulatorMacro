﻿using Macro.Models;
using System;
using System.Diagnostics;
using System.Windows;
using Unity;
using Utils;
using Utils.Document;

namespace Macro
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                Process(s, ex.ExceptionObject as Exception);
            };
            AppDomain.CurrentDomain.FirstChanceException += (s, ex) =>
            {
                Process(s, ex.Exception);
            };

            Init();
            base.OnStartup(e);
        }
        private void Init()
        {
            DependenciesResolved();
            var path = Environment.CurrentDirectory;
#if DEBUG
            path = Environment.CurrentDirectory + "/../../../Datas/";
#endif
            Singleton<LabelDocument>.Instance.Init(path);
        }
        private void DependenciesResolved()
        {
            var config = JsonHelper.Load<Config>($@"{ Environment.CurrentDirectory}\config.json");

            var container = Singleton<UnityContainer>.Instance;
            container.RegisterInstance<IConfig>(config);
        }
        private void Process(object sender, Exception ex)
        {
            //Debug.Assert(false, ex.Message);
            LogHelper.Warning(ex.Message);
        }
    }
}