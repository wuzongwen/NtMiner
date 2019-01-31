﻿using NTMiner.AppSetting;
using NTMiner.Core.Impl;
using NTMiner.Language.Impl;
using NTMiner.Notifications;
using NTMiner.ServiceContracts.DataObjects;
using NTMiner.Views;
using NTMiner.Views.Ucs;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace NTMiner.Vms {
    public static class AppStatic {
        public static double MainWindowHeight {
            get {
                if (DevMode.IsDevMode) {
                    if (SystemParameters.WorkArea.Size.Height > 920) {
                        return 920;
                    }
                    return SystemParameters.WorkArea.Size.Height;
                }
                if (SystemParameters.WorkArea.Size.Height > 620) {
                    return 620;
                }
                return 520;
            }
        }

        public static double MainWindowWidth {
            get {
                if (SystemParameters.WorkArea.Size.Width > 1000) {
                    return 1000;
                }
                return 860;
            }
        }

        public static ICommand ExportServerJson { get; private set; } = new DelegateCommand(() => {
            try {
                string fileName = ServerJson.Export();
                MainWindowViewModel.Current.Manager.CreateMessage()
                    .Accent("#1751C3")
                    .Background("#333")
                    .HasBadge("Info")
                    .HasMessage($"导出成功：{fileName}")
                    .Dismiss()
                    .WithDelay(TimeSpan.FromSeconds(2))
                    .Queue();
            }
            catch (Exception e) {
                Global.Logger.ErrorDebugLine(e.Message, e);
            }
        });

        public static ICommand ExportLangJson { get; private set; } = new DelegateCommand(() => {
            try {
                string fileName = LangJson.Export();
                MainWindowViewModel.Current.Manager.CreateMessage()
                    .Accent("#1751C3")
                    .Background("#333")
                    .HasBadge("Info")
                    .HasMessage($"导出成功：{fileName}")
                    .Dismiss()
                    .WithDelay(TimeSpan.FromSeconds(2))
                    .Queue();
            }
            catch (Exception e) {
                Global.Logger.ErrorDebugLine(e.Message, e);
            }
        });

        public static string ServerJsonFileName = AssemblyInfo.ServerJsonFileName;

        public static ICommand SetServerJsonVersion { get; private set; } = new DelegateCommand(() => {
            try {
                DialogWindow.ShowDialog(message: $"您确定刷新{AssemblyInfo.ServerJsonFileName}吗？", title: "确认", onYes: () => {
                    Server.AppSettingService.SetAppSettingAsync(new AppSettingData {
                        Key = ServerJsonFileName,
                        Value = Global.GetTimestamp()
                    }, response => {
                        Execute.OnUIThread(() => {
                            if (response.IsSuccess()) {
                                ControlCenterWindowViewModel.Current.Manager.CreateMessage()
                                    .Accent("#1751C3")
                                    .Background("#333")
                                    .HasBadge("Info")
                                    .HasMessage($"刷新成功")
                                    .Dismiss()
                                    .WithDelay(TimeSpan.FromSeconds(2))
                                    .Queue();
                            }
                            else {
                                ControlCenterWindowViewModel.Current.Manager.CreateMessage()
                                    .Accent("#1751C3")
                                    .Background("Red")
                                    .HasBadge("Error")
                                    .HasMessage($"刷新失败")
                                    .Dismiss()
                                    .WithDelay(TimeSpan.FromSeconds(2))
                                    .Queue();
                            }
                        });
                    });
                }, icon: "Icon_Confirm");
            }
            catch (Exception e) {
                Global.Logger.ErrorDebugLine(e.Message, e);
            }
        });

        public static ICommand ShowLangViewItems { get; private set; } = new DelegateCommand<string>((viewId) => {
            ViewLang.ShowWindow(new ViewLangViewModel(viewId));
        });

        public static ICommand JoinQQGroup { get; private set; } = new DelegateCommand(() => {
            Process.Start("https://jq.qq.com/?_wv=1027&k=5ZPsuCk");
        });

        public static ICommand RunAsAdministrator { get; private set; } = new DelegateCommand(() => {
            AppHelper.RunAsAdministrator();
        });

        public static ICommand ShowNotificationSample { get; private set; } = new DelegateCommand(() => {
            NotificationSample.ShowWindow();
        });

        public static ICommand AppExit { get; private set; } = new DelegateCommand(() => {
            Application.Current.MainWindow.Close();
        });

        public static ICommand ShowRestartWindows { get; private set; } = new DelegateCommand(() => {
            RestartWindows.ShowDialog();
        });

        public static ICommand ShowVirtualMemory { get; private set; } = new DelegateCommand(() => {
            Views.Ucs.VirtualMemory.ShowWindow();
        });
        public static ICommand ViewEvent { get; private set; } = new DelegateCommand(() => {
            EventPage.ShowWindow();
        });
        public static ICommand ShowSysDic { get; private set; } = new DelegateCommand(() => {
            SysDicPage.ShowWindow();
        });
        public static ICommand ShowGroups { get; private set; } = new DelegateCommand(() => {
            GroupPage.ShowWindow();
        });
        public static ICommand ShowCoins { get; private set; } = new DelegateCommand(() => {
            CoinPage.ShowWindow(null);
        });
        public static ICommand ManagePools { get; private set; } = new DelegateCommand<CoinViewModel>(coinVm => {
            CoinPageViewModel.Current.IsPoolTabSelected = true;
            CoinPage.ShowWindow(coinVm);
        });
        public static ICommand ManageWallet { get; private set; } = new DelegateCommand<CoinViewModel>(coinVm => {
            CoinPageViewModel.Current.IsWalletTabSelected = true;
            CoinPage.ShowWindow(coinVm);
        });
        public static ICommand ShowKernelInputs { get; private set; } = new DelegateCommand(() => {
            KernelInputPage.ShowWindow();
        });
        public static ICommand ShowKernelOutputs { get; private set; } = new DelegateCommand<KernelOutputViewModel>((selectedKernelOutputVm) => {
            KernelOutputPage.ShowWindow(selectedKernelOutputVm);
        });
        public static ICommand ShowKernels { get; private set; } = new DelegateCommand(() => {
            KernelPage.ShowWindow(Guid.Empty);
        });
        public static ICommand ShowAbout { get; private set; } = new DelegateCommand<string>((appType) => {
            AboutPage.ShowWindow(appType);
        });
        public static ICommand ShowSpeedChart { get; private set; } = new DelegateCommand(() => {
            SpeedCharts.ShowWindow();
        });
        public static ICommand ShowNTMinerUpdaterConfig { get; private set; } = new DelegateCommand(() => {
            NTMinerUpdaterConfig.ShowWindow();
        });
        public static ICommand ShowOnlineUpdate { get; private set; } = new DelegateCommand(() => {
            Upgrade(string.Empty, null);
        });
        public static void Upgrade(string ntminerFileName, Action callback) {
            try {
                string updaterDirFullName = Path.Combine(ClientId.GlobalDirFullName, "Updater");
                if (!Directory.Exists(updaterDirFullName)) {
                    Directory.CreateDirectory(updaterDirFullName);
                }
                Server.FileUrlService.GetNTMinerUpdaterUrlAsync((downloadFileUrl) => {
                    try {
                        if (string.IsNullOrEmpty(downloadFileUrl)) {
                            callback?.Invoke();
                            return;
                        }
                        string argument = string.Empty;
                        if (!string.IsNullOrEmpty(ntminerFileName)) {
                            argument = "ntminerFileName=" + ntminerFileName;
                        }
                        string ntMinerUpdaterFileFullName = Path.Combine(updaterDirFullName, "NTMinerUpdater.exe");
                        Uri uri = new Uri(downloadFileUrl);
                        string updaterVersion = NTMinerRegistry.GetUpdaterVersion();
                        if (string.IsNullOrEmpty(updaterVersion) || !File.Exists(ntMinerUpdaterFileFullName) || uri.AbsolutePath != updaterVersion) {
                            FileDownloader.ShowWindow(downloadFileUrl, "开源矿工更新器", (window, isSuccess, message, saveFileFullName) => {
                                try {
                                    if (isSuccess) {
                                        File.Copy(saveFileFullName, ntMinerUpdaterFileFullName, overwrite: true);
                                        File.Delete(saveFileFullName);
                                        NTMinerRegistry.SetUpdaterVersion(uri.AbsolutePath);
                                        window?.Close();
                                        Windows.Cmd.RunClose(ntMinerUpdaterFileFullName, argument);
                                        callback?.Invoke();
                                    }
                                    else {
                                        Execute.OnUIThread(() => {
                                            ControlCenterWindowViewModel.Current.Manager.CreateMessage()
                                                .Accent("#1751C3")
                                                .Background("Red")
                                                .HasBadge("Error")
                                                .HasMessage(message)
                                                .Dismiss()
                                                .WithDelay(TimeSpan.FromSeconds(2))
                                                .Queue();
                                        });
                                        callback?.Invoke();
                                    }
                                }
                                catch {
                                    callback?.Invoke();
                                }
                            });
                        }
                        else {
                            Windows.Cmd.RunClose(ntMinerUpdaterFileFullName, argument);
                            callback?.Invoke();
                        }
                    }
                    catch {
                        callback?.Invoke();
                    }
                });
            }
            catch {
                callback?.Invoke();
            }
        }
        public static ICommand ShowHelp { get; private set; } = new DelegateCommand(() => {
            Process.Start("https://github.com/ntminer/ntminer");
        });
        public static ICommand ManageMinerGroup { get; private set; } = new DelegateCommand(() => {
            MinerGroupPage.ShowWindow();
        });
        public static ICommand ManageMineWork { get; private set; } = new DelegateCommand(() => {
            MineWorkPage.ShowWindow();
        });
        public static ICommand ShowMinerClients { get; private set; } = new DelegateCommand(() => {
            MinerClients.ShowWindow();
        });
        public static ICommand ShowCalcConfig { get; private set; } = new DelegateCommand(() => {
            CalcConfig.ShowWindow();
        });
        public static ICommand ShowGlobalDir { get; private set; } = new DelegateCommand(() => {
            Process.Start(ClientId.GlobalDirFullName);
        });
        public static ICommand OpenLangLiteDb { get; private set; } = new DelegateCommand(() => {
            OpenLiteDb(ClientId.LangDbFileFullName);
        });
        public static ICommand OpenLocalLiteDb { get; private set; } = new DelegateCommand(() => {
            OpenLiteDb(SpecialPath.LocalDbFileFullName);
        });
        public static ICommand OpenServerLiteDb { get; private set; } = new DelegateCommand(() => {
            OpenLiteDb(SpecialPath.ServerDbFileFullName);
        });

        private static void OpenLiteDb(string dbFileFullName) {
            string liteDbExplorerDir = Path.Combine(ClientId.GlobalDirFullName, "LiteDBExplorerPortable");
            string liteDbExplorerFileFullName = Path.Combine(liteDbExplorerDir, "LiteDbExplorer.exe");
            if (!Directory.Exists(liteDbExplorerDir)) {
                Directory.CreateDirectory(liteDbExplorerDir);
            }
            if (!File.Exists(liteDbExplorerFileFullName)) {
                Server.FileUrlService.GetLiteDBExplorerUrlAsync(downloadFileUrl => {
                    if (string.IsNullOrEmpty(downloadFileUrl)) {
                        return;
                    }
                    FileDownloader.ShowWindow(downloadFileUrl, "LiteDB数据库管理工具", (window, isSuccess, message, saveFileFullName) => {
                        if (isSuccess) {
                            ZipUtil.DecompressZipFile(saveFileFullName, liteDbExplorerDir);
                            File.Delete(saveFileFullName);
                            window?.Close();
                            Windows.Cmd.RunClose(liteDbExplorerFileFullName, dbFileFullName);
                        }
                    });
                });
            }
            else {
                Windows.Cmd.RunClose(liteDbExplorerFileFullName, dbFileFullName);
            }
        }

        public static ICommand ShowCalc { get; private set; } = new DelegateCommand<CoinViewModel>(coin => {
            Calc.ShowWindow(coin);
        });

        public static ICommand OpenOfficialSite { get; private set; } = new DelegateCommand(() => {
            Process.Start("https://github.com/ntminer/ntminer");
        });

        public static ICommand OpenDiscussSite { get; private set; } = new DelegateCommand(() => {
            Process.Start("https://github.com/ntminer/ntminer/issues");
        });

        public static ICommand ShowQQGroupQrCode { get; private set; } = new DelegateCommand(() => {
            QQGroupQrCode.ShowWindow();
        });

        public static bool IsDebugMode {
            get {
                if (Design.IsInDesignMode) {
                    return true;
                }
                return DevMode.IsDebugMode;
            }
        }

        public static bool IsNotDebugMode {
            get {
                return !IsDebugMode;
            }
        }

        public static Visibility IsDebugModeVisible {
            get {
                if (IsDebugMode) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public static Visibility IsDevModeVisible {
            get {
                if (DevMode.IsDevMode) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        private static bool _isMinerClient;
        public static bool IsMinerClient {
            get {
                return _isMinerClient;
            }
            set {
                _isMinerClient = value;
                if (value) {
                    IsMinerClientVisible = Visibility.Visible;
                    IsMinerClientDevVisible = DevMode.IsDebugMode ? Visibility.Visible : Visibility.Collapsed;
                    IsMinerClientNotVisible = Visibility.Collapsed;
                }
                else {
                    IsMinerClientVisible = Visibility.Collapsed;
                    IsMinerClientDevVisible = Visibility.Collapsed;
                    IsMinerClientNotVisible = Visibility.Visible;
                }
            }
        }

        public static Visibility IsMinerClientVisible {
            get; private set;
        }

        public static Visibility IsMinerClientNotVisible {
            get; private set;
        }

        public static Visibility IsMinerClientDevVisible {
            get; private set;
        }
    }
}
