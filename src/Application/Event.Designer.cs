using System.ComponentModel;

namespace Application;

partial class Event
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Event));
        btnStart = new System.Windows.Forms.Button();
        dtp = new System.Windows.Forms.DateTimePicker();
        txtLog = new System.Windows.Forms.TextBox();
        cmbEvent = new System.Windows.Forms.ComboBox();
        btnStop = new System.Windows.Forms.Button();
        tipDateTime = new System.Windows.Forms.ToolTip(components);
        SuspendLayout();
        // 
        // btnStart
        // 
        btnStart.BackColor = System.Drawing.Color.White;
        btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
        btnStart.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonShadow;
        btnStart.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        btnStart.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnStart.Font = new System.Drawing.Font("Cascadia Code SemiLight", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)0));
        btnStart.ForeColor = System.Drawing.Color.RoyalBlue;
        btnStart.Location = new System.Drawing.Point(12, 54);
        btnStart.Name = "btnStart";
        btnStart.Size = new System.Drawing.Size(307, 39);
        btnStart.TabIndex = 0;
        btnStart.Text = "▶ Start";
        btnStart.UseVisualStyleBackColor = false;
        btnStart.Click += btnStart_Click;
        // 
        // dtp
        // 
        dtp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        dtp.Cursor = System.Windows.Forms.Cursors.Hand;
        dtp.CustomFormat = "dd-MM-yyyy HH:mm:ss";
        dtp.DropDownAlign = System.Windows.Forms.LeftRightAlignment.Right;
        dtp.Font = new System.Drawing.Font("Cascadia Code SemiLight", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)0));
        dtp.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        dtp.Location = new System.Drawing.Point(213, 13);
        dtp.Name = "dtp";
        dtp.Size = new System.Drawing.Size(427, 27);
        dtp.TabIndex = 1;
        tipDateTime.SetToolTip(dtp, "UTC+7 (giờ VN)");
        // 
        // txtLog
        // 
        txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        txtLog.Cursor = System.Windows.Forms.Cursors.IBeam;
        txtLog.Font = new System.Drawing.Font("Cascadia Code SemiLight", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)0));
        txtLog.Location = new System.Drawing.Point(12, 108);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        txtLog.Size = new System.Drawing.Size(628, 383);
        txtLog.TabIndex = 2;
        // 
        // cmbEvent
        // 
        cmbEvent.Cursor = System.Windows.Forms.Cursors.Hand;
        cmbEvent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        cmbEvent.Font = new System.Drawing.Font("Cascadia Code SemiLight", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)0));
        cmbEvent.FormattingEnabled = true;
        cmbEvent.Items.AddRange(new object[] { "1 | Xe trong bãi", "2 | Xe đã ra" });
        cmbEvent.Location = new System.Drawing.Point(12, 12);
        cmbEvent.MinimumSize = new System.Drawing.Size(2, 0);
        cmbEvent.Name = "cmbEvent";
        cmbEvent.Size = new System.Drawing.Size(187, 30);
        cmbEvent.TabIndex = 3;
        cmbEvent.SelectedIndex = 0;
        // 
        // btnStop
        // 
        btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
        btnStop.BackColor = System.Drawing.Color.White;
        btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
        btnStop.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonShadow;
        btnStop.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
        btnStop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
        btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnStop.Font = new System.Drawing.Font("Cascadia Code SemiLight", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)0));
        btnStop.ForeColor = System.Drawing.Color.Red;
        btnStop.Location = new System.Drawing.Point(334, 54);
        btnStop.Name = "btnStop";
        btnStop.Size = new System.Drawing.Size(307, 39);
        btnStop.TabIndex = 4;
        btnStop.Text = "⬛ Stop";
        btnStop.UseVisualStyleBackColor = false;
        btnStop.Click += btnStop_Click;
        // 
        // tipDateTime
        // 
        tipDateTime.AutomaticDelay = 200;
        // 
        // Main
        // 
        BackColor = System.Drawing.SystemColors.Control;
        ClientSize = new System.Drawing.Size(652, 503);
        Controls.Add(btnStop);
        Controls.Add(cmbEvent);
        Controls.Add(txtLog);
        Controls.Add(dtp);
        Controls.Add(btnStart);
        Icon = ((System.Drawing.Icon)resources.GetObject("$this.Icon"));
        Location = new System.Drawing.Point(19, 19);
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "Parking v8 Migration";
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.Button btnStart;
    private System.Windows.Forms.Button btnStop;
    private System.Windows.Forms.DateTimePicker dtp;
    private System.Windows.Forms.TextBox txtLog;
    private System.Windows.Forms.ComboBox cmbEvent;
    private System.Windows.Forms.ToolTip tipDateTime;
    
    #endregion
}