﻿using NTMiner.Core;
using NTMiner.Core.Gpus;
using NTMiner.Vms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NTMiner {
    public partial class AppContext {
        public class GpuSpeedViewModels : ViewModelBase {
            public static readonly GpuSpeedViewModels Instance = new GpuSpeedViewModels();

            private readonly List<GpuSpeedViewModel> _list = new List<GpuSpeedViewModel>();
            private readonly GpuSpeedViewModel _totalSpeedVm;

            private Guid _mainCoinId;
            private double _incomeMainCoinPerDay;
            private double _incomeMainCoinUsdPerDay;
            private double _incomeMainCoinCnyPerDay;
            private double _incomeDualCoinPerDay;
            private double _incomeDualCoinUsdPerDay;
            private double _incomeDualCoinCnyPerDay;

            private void ResetIfMainCoinSwitched() {
                Guid mainCoinId = NTMinerRoot.Instance.MinerProfile.CoinId;
                if (_mainCoinId != mainCoinId) {
                    _mainCoinId = mainCoinId;
                    foreach (var item in _list) {
                        item.MainCoinSpeed.Reset();
                        item.DualCoinSpeed.Reset();
                    }
                    IncomeMainCoinPerDay = 0;
                    IncomeMainCoinUsdPerDay = 0;
                    IncomeMainCoinCnyPerDay = 0;
                    IncomeDualCoinPerDay = 0;
                    IncomeDualCoinUsdPerDay = 0;
                    IncomeDualCoinCnyPerDay = 0;
                }
            }

            private GpuSpeedViewModels() {
#if DEBUG
                Write.Stopwatch.Start();
#endif
                if (WpfUtil.IsInDesignMode) {
                    return;
                }
                this.GpuAllVm = AppContext.Instance.GpuVms.FirstOrDefault(a => a.Index == NTMinerRoot.GpuAllId);
                IGpusSpeed gpuSpeeds = NTMinerRoot.Instance.GpusSpeed;
                foreach (var item in gpuSpeeds.AsEnumerable()) {
                    this._list.Add(new GpuSpeedViewModel(item));
                }
                _totalSpeedVm = this._list.FirstOrDefault(a => a.GpuVm.Index == NTMinerRoot.GpuAllId);
                BuildEventPath<GpuShareChangedEvent>("显卡份额变更后刷新VM内存", LogEnum.DevConsole,
                    action: message => {
                        ResetIfMainCoinSwitched();
                        int index = message.Source.Gpu.Index;
                        GpuSpeedViewModel gpuSpeedVm = _list.FirstOrDefault(a => a.GpuVm.Index == index);
                        if (gpuSpeedVm != null) {
                            gpuSpeedVm.MainCoinSpeed.FoundShare = message.Source.MainCoinSpeed.FoundShare;
                            gpuSpeedVm.MainCoinSpeed.AcceptShare = message.Source.MainCoinSpeed.AcceptShare;
                            gpuSpeedVm.MainCoinSpeed.RejectShare = message.Source.MainCoinSpeed.RejectShare;
                        }
                    });
                BuildEventPath<FoundShareIncreasedEvent>("找到一个份额后刷新VM内存", LogEnum.DevConsole,
                    action: message => {
                        ResetIfMainCoinSwitched();
                        int index = message.Source.Gpu.Index;
                        GpuSpeedViewModel gpuSpeedVm = _list.FirstOrDefault(a => a.GpuVm.Index == index);
                        if (gpuSpeedVm != null) {
                            gpuSpeedVm.MainCoinSpeed.FoundShare = message.Source.MainCoinSpeed.FoundShare;
                        }
                    });
                BuildEventPath<AcceptShareIncreasedEvent>("接受一个份额后刷新VM内存", LogEnum.DevConsole,
                    action: message => {
                        ResetIfMainCoinSwitched();
                        int index = message.Source.Gpu.Index;
                        GpuSpeedViewModel gpuSpeedVm = _list.FirstOrDefault(a => a.GpuVm.Index == index);
                        if (gpuSpeedVm != null) {
                            gpuSpeedVm.MainCoinSpeed.AcceptShare = message.Source.MainCoinSpeed.AcceptShare;
                        }
                    });
                BuildEventPath<RejectShareIncreasedEvent>("拒绝一个份额后刷新VM内存", LogEnum.DevConsole,
                    action: message => {
                        ResetIfMainCoinSwitched();
                        int index = message.Source.Gpu.Index;
                        GpuSpeedViewModel gpuSpeedVm = _list.FirstOrDefault(a => a.GpuVm.Index == index);
                        if (gpuSpeedVm != null) {
                            gpuSpeedVm.MainCoinSpeed.RejectShare = message.Source.MainCoinSpeed.RejectShare;
                        }
                    });
                BuildEventPath<IncorrectShareIncreasedEvent>("产生一个错误份额后刷新VM内存", LogEnum.DevConsole,
                    action: message => {
                        ResetIfMainCoinSwitched();
                        int index = message.Source.Gpu.Index;
                        GpuSpeedViewModel gpuSpeedVm = _list.FirstOrDefault(a => a.GpuVm.Index == index);
                        if (gpuSpeedVm != null) {
                            gpuSpeedVm.MainCoinSpeed.IncorrectShare = message.Source.MainCoinSpeed.IncorrectShare;
                        }
                    });
                BuildEventPath<GpuSpeedChangedEvent>("显卡算力变更后刷新VM内存", LogEnum.DevConsole,
                    action: (message) => {
                        ResetIfMainCoinSwitched();
                        int index = message.Source.Gpu.Index;
                        GpuSpeedViewModel gpuSpeedVm = _list.FirstOrDefault(a => a.GpuVm.Index == index);
                        if (gpuSpeedVm != null) {
                            if (message.IsDual) {
                                gpuSpeedVm.DualCoinSpeed.UpdateSpeed(message.Source.DualCoinSpeed.Value, message.Source.DualCoinSpeed.SpeedOn);
                                gpuSpeedVm.OnPropertyChanged(nameof(gpuSpeedVm.AverageDualCoinSpeedText));
                            }
                            else {
                                gpuSpeedVm.MainCoinSpeed.UpdateSpeed(message.Source.MainCoinSpeed.Value, message.Source.MainCoinSpeed.SpeedOn);
                                gpuSpeedVm.OnPropertyChanged(nameof(gpuSpeedVm.AverageMainCoinSpeedText));
                            }
                        }
                        if (index == _totalSpeedVm.GpuVm.Index) {
                            IMineContext mineContext = NTMinerRoot.Instance.LockedMineContext;
                            if (mineContext == null) {
                                IncomeMainCoinPerDay = 0;
                                IncomeMainCoinUsdPerDay = 0;
                                IncomeMainCoinCnyPerDay = 0;
                                IncomeDualCoinPerDay = 0;
                                IncomeDualCoinUsdPerDay = 0;
                                IncomeDualCoinCnyPerDay = 0;
                            }
                            else {
                                if (message.IsDual) {
                                    if (mineContext is IDualMineContext dualMineContext) {
                                        IncomePerDay incomePerDay = NTMinerRoot.Instance.CalcConfigSet.GetIncomePerHashPerDay(dualMineContext.DualCoin.Code);
                                        IncomeDualCoinPerDay = _totalSpeedVm.DualCoinSpeed.Value * incomePerDay.IncomeCoin;
                                        IncomeDualCoinUsdPerDay = _totalSpeedVm.DualCoinSpeed.Value * incomePerDay.IncomeUsd;
                                        IncomeDualCoinCnyPerDay = _totalSpeedVm.DualCoinSpeed.Value * incomePerDay.IncomeCny;
                                    }
                                }
                                else {
                                    IncomePerDay incomePerDay = NTMinerRoot.Instance.CalcConfigSet.GetIncomePerHashPerDay(mineContext.MainCoin.Code);
                                    IncomeMainCoinPerDay = _totalSpeedVm.MainCoinSpeed.Value * incomePerDay.IncomeCoin;
                                    IncomeMainCoinUsdPerDay = _totalSpeedVm.MainCoinSpeed.Value * incomePerDay.IncomeUsd;
                                    IncomeMainCoinCnyPerDay = _totalSpeedVm.MainCoinSpeed.Value * incomePerDay.IncomeCny;
                                }
                            }
                        }
                    });
                BuildEventPath<Per1SecondEvent>("每秒钟更新算力活动时间", LogEnum.None,
                    action: message => {
                        TotalSpeedVm.MainCoinSpeed.OnPropertyChanged(nameof(SpeedViewModel.LastSpeedOnText));
                        TotalSpeedVm.DualCoinSpeed.OnPropertyChanged(nameof(SpeedViewModel.LastSpeedOnText));
                    });
#if DEBUG
                var elapsedMilliseconds = Write.Stopwatch.Stop();
                Write.DevTimeSpan($"耗时{elapsedMilliseconds} {this.GetType().Name}.ctor");
#endif
            }

            public void Refresh() {
                foreach (var item in this._list) {
                    var data = NTMinerRoot.Instance.GpusSpeed.AsEnumerable().FirstOrDefault(a => a.Gpu.Index == item.GpuVm.Index);
                    if (data != null) {
                        if (item.GpuVm.Index == NTMinerRoot.GpuAllId) {
                            TotalSpeedVm.MainCoinSpeed.UpdateSpeed(data.MainCoinSpeed.Value, data.MainCoinSpeed.SpeedOn);
                            TotalSpeedVm.DualCoinSpeed.UpdateSpeed(data.DualCoinSpeed.Value, data.DualCoinSpeed.SpeedOn);
                        }
                    }
                }
            }

            public GpuViewModel GpuAllVm {
                get; set;
            }

            public MinerProfileViewModel MinerProfile {
                get {
                    return AppContext.Instance.MinerProfileVm;
                }
            }

            public int Count {
                get {
                    if (_totalSpeedVm != null) {
                        return _list.Count - 1;
                    }
                    return _list.Count;
                }
            }

            public string ProfitCnyPerDayText {
                get {
                    return (this.IncomeMainCoinCnyPerDay + this.IncomeDualCoinCnyPerDay - AppContext.Instance.GpuVms.GpuAllVm.ECharge).ToString("f2");
                }
            }

            public double IncomeMainCoinPerDay {
                get => _incomeMainCoinPerDay;
                set {
                    _incomeMainCoinPerDay = value;
                    OnPropertyChanged(nameof(IncomeMainCoinPerDay));
                    OnPropertyChanged(nameof(IncomeMainCoinPerDayText));
                }
            }

            public string IncomeMainCoinPerDayText {
                get {
                    return IncomeMainCoinPerDay.ToString("f7");
                }
            }

            public double IncomeMainCoinUsdPerDay {
                get { return _incomeMainCoinUsdPerDay; }
                set {
                    _incomeMainCoinUsdPerDay = value;
                    OnPropertyChanged(nameof(IncomeMainCoinUsdPerDay));
                    OnPropertyChanged(nameof(IncomeMainCoinUsdPerDayText));
                }
            }

            public string IncomeMainCoinUsdPerDayText {
                get {
                    return IncomeMainCoinUsdPerDay.ToString("f2");
                }
            }

            public double IncomeMainCoinCnyPerDay {
                get { return _incomeMainCoinCnyPerDay; }
                set {
                    _incomeMainCoinCnyPerDay = value;
                    OnPropertyChanged(nameof(IncomeMainCoinCnyPerDay));
                    OnPropertyChanged(nameof(IncomeMainCoinCnyPerDayText));
                    OnPropertyChanged(nameof(ProfitCnyPerDayText));
                }
            }

            public string IncomeMainCoinCnyPerDayText {
                get {
                    return IncomeMainCoinCnyPerDay.ToString("f2");
                }
            }

            public double IncomeDualCoinPerDay {
                get => _incomeDualCoinPerDay;
                set {
                    _incomeDualCoinPerDay = value;
                    OnPropertyChanged(nameof(IncomeDualCoinPerDay));
                    OnPropertyChanged(nameof(IncomeDualCoinPerDayText));
                }
            }

            public string IncomeDualCoinPerDayText {
                get {
                    return IncomeDualCoinPerDay.ToString("f7");
                }
            }

            public double IncomeDualCoinUsdPerDay {
                get { return _incomeDualCoinUsdPerDay; }
                set {
                    _incomeDualCoinUsdPerDay = value;
                    OnPropertyChanged(nameof(IncomeDualCoinUsdPerDay));
                    OnPropertyChanged(nameof(IncomeDualCoinUsdPerDayText));
                }
            }

            public string IncomeDualCoinUsdPerDayText {
                get {
                    return IncomeDualCoinUsdPerDay.ToString("f2");
                }
            }

            public double IncomeDualCoinCnyPerDay {
                get { return _incomeDualCoinCnyPerDay; }
                set {
                    _incomeDualCoinCnyPerDay = value;
                    OnPropertyChanged(nameof(IncomeDualCoinCnyPerDay));
                    OnPropertyChanged(nameof(IncomeDualCoinCnyPerDayText));
                    OnPropertyChanged(nameof(ProfitCnyPerDayText));
                }
            }

            public string IncomeDualCoinCnyPerDayText {
                get {
                    return IncomeDualCoinCnyPerDay.ToString("f2");
                }
            }

            public GpuSpeedViewModel TotalSpeedVm {
                get { return _totalSpeedVm; }
            }

            /// <summary>
            /// 除去GpuAllId
            /// </summary>
            public List<GpuSpeedViewModel> List {
                get {
                    return _list.Where(a => a.GpuVm.Index != NTMinerRoot.GpuAllId).ToList();
                }
            }

            public List<GpuSpeedViewModel> All {
                get {
                    return _list;
                }
            }
        }
    }
}
