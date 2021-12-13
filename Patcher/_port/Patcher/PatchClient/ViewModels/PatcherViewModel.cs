﻿using Avalonia;
using PatchClient.Models;
using PatcherUtils;
using Splat;
using System;
using ReactiveUI;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace PatchClient.ViewModels
{
    public class PatcherViewModel : ViewModelBase
    {
        private bool initLineItemProgress = true;

        public ObservableCollection<LineItemProgress> LineItems { get; set; } = new ObservableCollection<LineItemProgress>();

        private string _ProgressMessage;
        public string ProgressMessage
        {
            get => _ProgressMessage;
            set => this.RaiseAndSetIfChanged(ref _ProgressMessage, value);
        }

        private int _PatchPercent;
        public int PatchPercent
        {
            get => _PatchPercent;
            set => this.RaiseAndSetIfChanged(ref _PatchPercent, value);
        }

        private string _PatchMessage;
        public string PatchMessage
        {
            get => _PatchMessage;
            set => this.RaiseAndSetIfChanged(ref _PatchMessage, value);
        }

        private ViewNavigator navigator => Locator.Current.GetService<ViewNavigator>();

        public PatcherViewModel()
        {
            RunPatcher();
        }

        [Obsolete]
        private void Test()
        {
            Task.Run(() =>
            {
                LineItem x = new LineItem("test 1", 100);
                LineItem xx = new LineItem("test 2", 100);
                LineItem xxx = new LineItem("test 3", 100);

                LineItems.Add(new LineItemProgress(x));
                LineItems.Add(new LineItemProgress(xx));
                LineItems.Add(new LineItemProgress(xxx));

                for (int i = 0; i <= 100; i++)
                {
                    System.Threading.Thread.Sleep(20);
                    PatchPercent = i;
                    ProgressMessage = $"Patching @ {i}%";

                    foreach(var item in LineItems)
                    {
                        item.UpdateProgress(i);
                    }
                }

                //navigator.SelectedViewModel = new MessageViewModel("Patch completed without issues");
            });
        }

        private void RunPatcher()
        {
            Task.Run(() =>
            {
                FilePatcher bp = new FilePatcher()
                {
                    TargetBase = Environment.CurrentDirectory,
                    PatchBase = LazyOperations.PatchFolder.FromCwd()
                };

                bp.ProgressChanged += Bp_ProgressChanged;

                try
                {
                    if (bp.Run())
                    {
                        //navigator.SelectedViewModel = new MessageViewModel("Patch completed without issues");
                    }
                    else
                    {
                        navigator.SelectedViewModel = new MessageViewModel("Failed to patch client");
                    }
                }
                catch (Exception ex)
                {
                    navigator.SelectedViewModel = new MessageViewModel(ex.Message);
                }
            });
        }

        private void Bp_ProgressChanged(object Sender, int Progress, int Total, int Percent, string Message = "", params LineItem[] AdditionalLineItems)
        {
            foreach (LineItem item in AdditionalLineItems)
            {
                if (item.ItemValue <= 0) continue;

                if(initLineItemProgress)
                {

                    LineItems.Add(new LineItemProgress(item));
                }

                LineItems.FirstOrDefault(x => x.Info == item.ItemText).UpdateProgress(item.ItemValue);
            }

            initLineItemProgress = false;

            PatchPercent = Percent;

            if (!string.IsNullOrWhiteSpace(Message))
            {
                PatchMessage = Message;
            }

            ProgressMessage = $"Patching: {Progress} / {Total} - {Percent}%";
        }
    }
}
