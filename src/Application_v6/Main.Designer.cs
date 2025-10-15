namespace Application_v6;

partial class Main
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
        cmb = new System.Windows.Forms.ComboBox();
        btnStart = new System.Windows.Forms.Button();
        btnStop = new System.Windows.Forms.Button();
        txtLog = new System.Windows.Forms.TextBox();
        SuspendLayout();
        // 
        // cmb
        // 
        cmb.Font = new System.Drawing.Font("Cascadia Mono Light", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)0));
        cmb.FormattingEnabled = true;
        cmb.Items.AddRange(new object[] { "👥 Nhóm khách hàng", "👤 Khách hàng", "🆔 Nhóm định danh", "🔑 Định danh", "🚪 Cổng", "💻 Máy tính", "📷 Camera", "🎛️ Bộ điều khiển", "🚦 Làn", "💡 Led", "🚗 Xe trong bãi", "🏎️ Xe đã ra" });
        cmb.Location = new System.Drawing.Point(23, 27);
        cmb.Name = "cmb";
        cmb.Size = new System.Drawing.Size(749, 32);
        cmb.TabIndex = 0;
        cmb.SelectedIndex = 0;
        // 
        // btnStart
        // 
        btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
        btnStart.Font = new System.Drawing.Font("Cascadia Mono Light", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)0));
        btnStart.Location = new System.Drawing.Point(23, 84);
        btnStart.Name = "btnStart";
        btnStart.Size = new System.Drawing.Size(354, 53);
        btnStart.TabIndex = 2;
        btnStart.Text = "⚡ Bắt đầu";
        btnStart.UseVisualStyleBackColor = true;
        btnStart.Click += btnStart_Click;
        // 
        // btnStop
        // 
        btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
        btnStop.Font = new System.Drawing.Font("Cascadia Mono Light", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)0));
        btnStop.Location = new System.Drawing.Point(398, 84);
        btnStop.Name = "btnStop";
        btnStop.Size = new System.Drawing.Size(374, 53);
        btnStop.TabIndex = 3;
        btnStop.Text = "⏹️ Dừng";
        btnStop.UseVisualStyleBackColor = true;
        btnStop.Click += btnStop_Click;
        // 
        // txtLog
        // 
        txtLog.BackColor = System.Drawing.Color.White;
        txtLog.Font = new System.Drawing.Font("Cascadia Mono Light", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)0));
        txtLog.Location = new System.Drawing.Point(23, 163);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        txtLog.Size = new System.Drawing.Size(749, 317);
        txtLog.TabIndex = 5;
        // 
        // Main
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(800, 506);
        Controls.Add(txtLog);
        Controls.Add(btnStop);
        Controls.Add(btnStart);
        Controls.Add(cmb);
        Icon = ((System.Drawing.Icon)resources.GetObject("$this.Icon"));
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "Parking v8 Migration";
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.TextBox txtLog;
    private System.Windows.Forms.Button btnStart;
    private System.Windows.Forms.Button btnStop;
    private System.Windows.Forms.ComboBox cmb;

    #endregion
}