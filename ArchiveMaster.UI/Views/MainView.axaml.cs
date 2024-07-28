﻿using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib.Avalonia.Dialogs;
using FzLib.Avalonia.Messages;
using ArchiveMaster.Messages;
using ArchiveMaster.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArchiveMaster.Configs;
using ArchiveMaster.UI;
using System.Reflection;
using Avalonia.Media.Imaging;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Styling;

namespace ArchiveMaster.Views;

public partial class MainView : UserControl
{
    private CancellationTokenSource loadingToken = null;

    public MainView()
    {
        InitializeModules();
        AppConfig.Instance.Load();
        InitializeComponent();
        RegisterMessages();
    }

    private List<ToolPanelGroupInfo> views = new List<ToolPanelGroupInfo>();

    private void InitializeModules()
    {
        string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string[] dllFiles = Directory.GetFiles(currentDirectory, "ArchiveMaster.Module.*.dll");

        foreach (string dllFile in dllFiles)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(dllFile);
                var moduleInitializerTypes = assembly.GetTypes()
                    .Where(t => typeof(IModuleInitializer).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                var initializerTypes = moduleInitializerTypes.ToList();
                if (!initializerTypes.Any())
                {
                    throw new Exception($"在程序集 {dllFile} 中未找到实现 IModuleInitializer 接口的类。");
                }

                foreach (var type in initializerTypes)
                {
                    IModuleInitializer moduleInitializer = (IModuleInitializer)Activator.CreateInstance(type);
                    if (moduleInitializer == null)
                    {
                        throw new Exception($"模块不存在实现了{nameof(IModuleInitializer)}的类");
                    }

                    if (moduleInitializer.Configs != null)
                    {
                        foreach (var config in moduleInitializer.Configs)
                        {
                            AppConfig.RegisterConfig(config.Type, config.Key);
                        }
                    }

                    if (moduleInitializer.Views != null)
                    {
                        views.Add(moduleInitializer.Views);
                    }

                    moduleInitializer.RegisterMessages(this);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"加载程序集 {dllFile} 时出错: {ex.Message}");
            }
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DataContextProperty)
        {
            foreach (var view in views)
            {
                (DataContext as MainViewModel)?.PanelGroups.Add(view);
            }
        }
    }

    private void RegisterMessages()
    {
        this.RegisterDialogHostMessage();
        this.RegisterGetClipboardMessage();
        this.RegisterGetStorageProviderMessage();
        this.RegisterCommonDialogMessage();
        WeakReferenceMessenger.Default.Register<LoadingMessage>(this, (_, m) =>
        {
            if (m.IsVisible)
            {
                loadingToken ??= LoadingOverlay.ShowLoading(this);
            }
            else
            {
                if (loadingToken != null)
                {
                    loadingToken.Cancel();
                    loadingToken = null;
                }
            }
        });
    }
}