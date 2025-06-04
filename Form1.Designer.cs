namespace VoiceHelper
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label labelDevice;
        private System.Windows.Forms.ComboBox comboBoxDevice;
        private System.Windows.Forms.Label labelRate;
        private System.Windows.Forms.ComboBox comboBoxRate;
        private System.Windows.Forms.Label labelSocketStatus;
        private System.Windows.Forms.PictureBox picSocketStatus;
        private System.Windows.Forms.Label labelRecordingStatus;
        private System.Windows.Forms.PictureBox picRecordingStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.labelDevice = new System.Windows.Forms.Label();
            this.comboBoxDevice = new System.Windows.Forms.ComboBox();
            this.labelRate = new System.Windows.Forms.Label();
            this.comboBoxRate = new System.Windows.Forms.ComboBox();
            this.labelSocketStatus = new System.Windows.Forms.Label();
            this.picSocketStatus = new System.Windows.Forms.PictureBox();
            this.labelRecordingStatus = new System.Windows.Forms.Label();
            this.picRecordingStatus = new System.Windows.Forms.PictureBox();
            this.labelClientCount = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picSocketStatus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picRecordingStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // labelDevice
            // 
            this.labelDevice.AutoSize = true;
            this.labelDevice.Location = new System.Drawing.Point(20, 25);
            this.labelDevice.Name = "labelDevice";
            this.labelDevice.Size = new System.Drawing.Size(80, 17);
            this.labelDevice.TabIndex = 0;
            this.labelDevice.Text = "音频输入设备";
            // 
            // comboBoxDevice
            // 
            this.comboBoxDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDevice.FormattingEnabled = true;
            this.comboBoxDevice.Location = new System.Drawing.Point(120, 22);
            this.comboBoxDevice.Name = "comboBoxDevice";
            this.comboBoxDevice.Size = new System.Drawing.Size(300, 25);
            this.comboBoxDevice.TabIndex = 1;
            // 
            // labelRate
            // 
            this.labelRate.AutoSize = true;
            this.labelRate.Location = new System.Drawing.Point(20, 65);
            this.labelRate.Name = "labelRate";
            this.labelRate.Size = new System.Drawing.Size(44, 17);
            this.labelRate.TabIndex = 2;
            this.labelRate.Text = "采样率";
            // 
            // comboBoxRate
            // 
            this.comboBoxRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRate.FormattingEnabled = true;
            this.comboBoxRate.Items.AddRange(new object[] {
            "8000",
            "16000",
            "44100"});
            this.comboBoxRate.Location = new System.Drawing.Point(120, 62);
            this.comboBoxRate.Name = "comboBoxRate";
            this.comboBoxRate.Size = new System.Drawing.Size(120, 25);
            this.comboBoxRate.TabIndex = 3;
            // 
            // labelSocketStatus
            // 
            this.labelSocketStatus.AutoSize = true;
            this.labelSocketStatus.Location = new System.Drawing.Point(20, 110);
            this.labelSocketStatus.Name = "labelSocketStatus";
            this.labelSocketStatus.Size = new System.Drawing.Size(102, 17);
            this.labelSocketStatus.TabIndex = 4;
            this.labelSocketStatus.Text = "WebSocket 服务";
            // 
            // picSocketStatus
            // 
            this.picSocketStatus.Image = global::VoiceHelper.Properties.Resources.green;
            this.picSocketStatus.Location = new System.Drawing.Point(130, 107);
            this.picSocketStatus.Name = "picSocketStatus";
            this.picSocketStatus.Size = new System.Drawing.Size(20, 20);
            this.picSocketStatus.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picSocketStatus.TabIndex = 5;
            this.picSocketStatus.TabStop = false;
            // 
            // labelRecordingStatus
            // 
            this.labelRecordingStatus.AutoSize = true;
            this.labelRecordingStatus.Location = new System.Drawing.Point(20, 145);
            this.labelRecordingStatus.Name = "labelRecordingStatus";
            this.labelRecordingStatus.Size = new System.Drawing.Size(56, 17);
            this.labelRecordingStatus.TabIndex = 6;
            this.labelRecordingStatus.Text = "录音状态";
            // 
            // picRecordingStatus
            // 
            this.picRecordingStatus.Image = global::VoiceHelper.Properties.Resources.gray;
            this.picRecordingStatus.Location = new System.Drawing.Point(130, 142);
            this.picRecordingStatus.Name = "picRecordingStatus";
            this.picRecordingStatus.Size = new System.Drawing.Size(20, 20);
            this.picRecordingStatus.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picRecordingStatus.TabIndex = 7;
            this.picRecordingStatus.TabStop = false;
            // 
            // labelClientCount
            // 
            this.labelClientCount.AutoSize = true;
            this.labelClientCount.Location = new System.Drawing.Point(318, 110);
            this.labelClientCount.Name = "labelClientCount";
            this.labelClientCount.Size = new System.Drawing.Size(99, 17);
            this.labelClientCount.TabIndex = 8;
            this.labelClientCount.Text = "已连接客户端 [0]";
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(450, 200);
            this.Controls.Add(this.labelClientCount);
            this.Controls.Add(this.picRecordingStatus);
            this.Controls.Add(this.labelRecordingStatus);
            this.Controls.Add(this.picSocketStatus);
            this.Controls.Add(this.labelSocketStatus);
            this.Controls.Add(this.comboBoxRate);
            this.Controls.Add(this.labelRate);
            this.Controls.Add(this.comboBoxDevice);
            this.Controls.Add(this.labelDevice);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Voice Helper";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picSocketStatus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picRecordingStatus)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label labelClientCount;
    }
}