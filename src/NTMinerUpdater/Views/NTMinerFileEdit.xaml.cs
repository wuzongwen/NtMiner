﻿using NTMiner.Vms;
using NTMiner.Wpf;
using System.Windows;
using System.Windows.Media;

namespace NTMiner.Views {
    public partial class NTMinerFileEdit : BlankWindow {
        public NTMinerFileViewModel Vm {
            get {
                return (NTMinerFileViewModel)this.DataContext;
            }
        }

        public NTMinerFileEdit(string title, string iconName, NTMinerFileViewModel vm) {
            this.DataContext = vm;
            InitializeComponent();
            this.TbTitle.Text = title;
            this.PathIcon.Data = (Geometry)Application.Current.Resources[iconName];
            this.Owner = TopWindow.GetTopWindow();
        }
    }
}
