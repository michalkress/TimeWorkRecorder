using Prism.Mvvm;
using Prism.Commands;
using Prism.Dialogs;
using System;

namespace TimeWorkRecorder.Modules.TimeTracker.ViewModels
{
    public class BreakDialogViewModel : BindableBase, IDialogAware
    {
        public event Action<IDialogResult>? RequestClose;

        private string _selectedReason = "Œniadanie";
        public string SelectedReason
        {
            get => _selectedReason;
            set => SetProperty(ref _selectedReason, value);
        }

        public DelegateCommand OkCommand { get; }
        public DelegateCommand CancelCommand { get; }

        DialogCloseListener IDialogAware.RequestClose => throw new NotImplementedException();

        public BreakDialogViewModel()
        {
            OkCommand = new DelegateCommand(OnOk);
            CancelCommand = new DelegateCommand(OnCancel);
        }

        private void OnOk()
        {
            var p = new DialogParameters { { "Reason", SelectedReason } };
            RaiseRequestClose(new DialogResult(ButtonResult.OK) { Parameters = p });
        }

        private void OnCancel()
        {
            RaiseRequestClose(new DialogResult(ButtonResult.Cancel));
        }

        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }
        public void OnDialogOpened(IDialogParameters parameters) { }

        protected void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }
    }
}
