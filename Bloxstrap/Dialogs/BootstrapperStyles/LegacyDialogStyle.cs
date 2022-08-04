using Bloxstrap.Helpers;
using Bloxstrap.Helpers.RSMM;

namespace Bloxstrap.Dialogs.BootstrapperStyles
{
    // TODO - universal implementation for winforms-based styles? (to reduce duplicate code)

    // example: https://youtu.be/3K9oCEMHj2s?t=35

    // so this specifically emulates the 2011 version of the legacy dialog,
    // but once winforms code is cleaned up we could also do the 2009 version too
    // example: https://youtu.be/VpduiruysuM?t=18

    public partial class LegacyDialogStyle : Form
    {
        private Bootstrapper? Bootstrapper;

        public LegacyDialogStyle(Bootstrapper? bootstrapper = null)
        {
            InitializeComponent();

            if (bootstrapper is not null)
            {
                Bootstrapper = bootstrapper;
                Bootstrapper.PromptShutdownEvent += new EventHandler(PromptShutdown);
                Bootstrapper.ShowSuccessEvent += new ChangeEventHandler<string>(ShowSuccess);
                Bootstrapper.MessageChanged += new ChangeEventHandler<string>(MessageChanged);
                Bootstrapper.ProgressBarValueChanged += new ChangeEventHandler<int>(ProgressBarValueChanged);
                Bootstrapper.ProgressBarStyleChanged += new ChangeEventHandler<ProgressBarStyle>(ProgressBarStyleChanged);
                Bootstrapper.CancelEnabledChanged += new ChangeEventHandler<bool>(CancelEnabledChanged);
            }

            Icon icon = IconManager.GetIconResource();

            this.Text = Program.ProjectName;
            this.Icon = icon;
            this.IconBox.Image = icon.ToBitmap();

            if (Bootstrapper is null)
            {
                this.Message.Text = "Click the Cancel button to return to preferences";
                this.CancelButton.Enabled = true;
                this.CancelButton.Visible = true;
            }
            else
            {
                Task.Run(() => RunBootstrapper());
            }
        }

        public async void RunBootstrapper()
        {
            try
            {
                await Bootstrapper.Run();
            }
            catch (Exception ex)
            {
                // string message = String.Format("{0}: {1}", ex.GetType(), ex.Message);
                string message = ex.ToString();
                ShowError(message);

                Program.Exit();
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(
                $"An error occurred while starting Roblox\n\nDetails: {message}", 
                Program.ProjectName, 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Error
            );
        }

        private void ShowSuccess(object sender, ChangeEventArgs<string> e)
        {
            MessageBox.Show(
                e.Value,
                Program.ProjectName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void PromptShutdown(object? sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Roblox is currently running, but needs to close. Would you like close Roblox now?",
                Program.ProjectName,
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information
            );

            if (result != DialogResult.OK)
                Environment.Exit(0);
        }

        private void MessageChanged(object sender, ChangeEventArgs<string> e)
        {
            if (this.InvokeRequired)
            {
                ChangeEventHandler<string> handler = new(MessageChanged);
                this.Message.Invoke(handler, sender, e);
            }
            else
            {
                this.Message.Text = e.Value;
            }
        }

        private void ProgressBarValueChanged(object sender, ChangeEventArgs<int> e)
        {
            if (this.ProgressBar.InvokeRequired)
            {
                ChangeEventHandler<int> handler = new(ProgressBarValueChanged);
                this.ProgressBar.Invoke(handler, sender, e);
            }
            else
            {
                this.ProgressBar.Value = e.Value;
            }
        }

        private void ProgressBarStyleChanged(object sender, ChangeEventArgs<ProgressBarStyle> e)
        {
            if (this.ProgressBar.InvokeRequired)
            {
                ChangeEventHandler<ProgressBarStyle> handler = new(this.ProgressBarStyleChanged);
                this.ProgressBar.Invoke(handler, sender, e);
            }
            else
            {
                this.ProgressBar.Style = e.Value;
            }
        }

        private void CancelEnabledChanged(object sender, ChangeEventArgs<bool> e)
        {
            if (this.CancelButton.InvokeRequired)
            {
                ChangeEventHandler<bool> handler = new(CancelEnabledChanged);
                this.CancelButton.Invoke(handler, sender, e);
            }
            else
            {
                this.CancelButton.Enabled = e.Value;
                this.CancelButton.Visible = e.Value;
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (Bootstrapper is null)
                this.Close();
            else
                Task.Run(() => Bootstrapper.CancelButtonClicked());
        }
    }
}