using System;
using System.Diagnostics;
using System.Windows.Forms;
using mRemoteNG.UI.Window;
using mRemoteNG.App;
using mRemoteNG.UI.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace mRemoteNG.Messages
{
    public class MessageCollector
    {
        private Timer _ECTimer;
        private ErrorAndInfoWindow _MCForm;

        public ErrorAndInfoWindow MCForm
		{
			get { return _MCForm; }
			set { _MCForm = value; }
		}

        public MessageCollector(ErrorAndInfoWindow MessageCollectorForm)
        {
            _MCForm = MessageCollectorForm;
            CreateTimer();
        }

        #region Public Methods
        public void AddMessage(MessageClass MsgClass, string MsgText, bool OnlyLog = false)
        {
            Message nMsg = new Message(MsgClass, MsgText, DateTime.Now);

            if (nMsg.MsgClass == MessageClass.ReportMsg)
            {
                AddReportMessage(nMsg);
                return;
            }

            if (Settings.Default.SwitchToMCOnInformation && nMsg.MsgClass == MessageClass.InformationMsg)
                AddInfoMessage(nMsg);
            
            if (Settings.Default.SwitchToMCOnWarning && nMsg.MsgClass == MessageClass.WarningMsg)
                AddWarningMessage(nMsg);

            if (Settings.Default.SwitchToMCOnError && nMsg.MsgClass == MessageClass.ErrorMsg)
                AddErrorMessage(nMsg);

            if (!OnlyLog)
            {
                if (Settings.Default.ShowNoMessageBoxes)
                    _ECTimer.Enabled = true;
                else
                    ShowMessageBox(nMsg);

                ListViewItem lvItem = BuildListViewItem(nMsg);
                AddToList(lvItem);
            }
        }

        private void AddInfoMessage(Message nMsg)
        {
            Debug.Print("Info: " + nMsg.MsgText);
            if (Settings.Default.WriteLogFile)
                Logger.Instance.Info(nMsg.MsgText);
        }

        private void AddWarningMessage(Message nMsg)
        {
            Debug.Print("Warning: " + nMsg.MsgText);
            if (Settings.Default.WriteLogFile)
                Logger.Instance.Warn(nMsg.MsgText);
        }

        private void AddErrorMessage(Message nMsg)
        {
            Debug.Print("Error: " + nMsg.MsgText);
            Logger.Instance.Error(nMsg.MsgText);
        }

        private static void AddReportMessage(Message nMsg)
        {
            Debug.Print("Report: " + nMsg.MsgText);
            if (Settings.Default.WriteLogFile)
                Logger.Instance.Info(nMsg.MsgText);
        }

        private static ListViewItem BuildListViewItem(Message nMsg)
        {
            ListViewItem lvItem = new ListViewItem
            {
                ImageIndex = Convert.ToInt32(nMsg.MsgClass),
                Text = nMsg.MsgText.Replace(Environment.NewLine, "  "),
                Tag = nMsg
            };
            return lvItem;
        }

        public void AddExceptionMessage(string message, Exception ex, MessageClass msgClass = MessageClass.ErrorMsg, bool logOnly = false)
        {
            AddMessage(msgClass, message + Environment.NewLine + Tools.MiscTools.GetExceptionMessageRecursive(ex), logOnly);
        }

        public void AddExceptionStackTrace(string message, Exception ex, MessageClass msgClass = MessageClass.ErrorMsg, bool logOnly = false)
        {
            AddMessage(msgClass, message + Environment.NewLine + ex.StackTrace, logOnly);
        }
        #endregion

        #region Private Methods
        private void CreateTimer()
        {
            _ECTimer = new Timer
            {
                Enabled = false,
                Interval = 300
            };
            _ECTimer.Tick += SwitchTimerTick;
        }

        private void SwitchTimerTick(object sender, EventArgs e)
        {
            SwitchToMessage();
            _ECTimer.Enabled = false;
        }

        private void SwitchToMessage()
        {
            _MCForm.PreviousActiveForm = (DockContent)frmMain.Default.pnlDock.ActiveContent;
            ShowMCForm();
            _MCForm.lvErrorCollector.Focus();
            _MCForm.lvErrorCollector.SelectedItems.Clear();
            _MCForm.lvErrorCollector.Items[0].Selected = true;
            _MCForm.lvErrorCollector.FocusedItem = _MCForm.lvErrorCollector.Items[0];
        }

        private static void ShowMessageBox(Message Msg)
        {
            switch (Msg.MsgClass)
            {
                case MessageClass.InformationMsg:
                    MessageBox.Show(Msg.MsgText, string.Format(Language.strTitleInformation, Msg.MsgDate), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case MessageClass.WarningMsg:
                    MessageBox.Show(Msg.MsgText, string.Format(Language.strTitleWarning, Msg.MsgDate), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case MessageClass.ErrorMsg:
                    MessageBox.Show(Msg.MsgText, string.Format(Language.strTitleError, Msg.MsgDate), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }
        #endregion
		
        #region Delegates
		private delegate void ShowMCFormCB();
		private void ShowMCForm()
		{
			if (frmMain.Default.pnlDock.InvokeRequired)
			{
				ShowMCFormCB d = new ShowMCFormCB(ShowMCForm);
				frmMain.Default.pnlDock.Invoke(d);
			}
			else
			{
                _MCForm.Show(frmMain.Default.pnlDock);
			}
		}

        delegate void AddToListCB(ListViewItem lvItem);
        private void AddToList(ListViewItem lvItem)
        {
            if (_MCForm.lvErrorCollector.InvokeRequired)
            {
                AddToListCB d = new AddToListCB(AddToList);
                _MCForm.lvErrorCollector.Invoke(d, new object[] { lvItem });
            }
            else
            {
                _MCForm.lvErrorCollector.Items.Insert(0, lvItem);
            }
        }
        #endregion
	}
}