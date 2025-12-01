using System.ComponentModel;

namespace Application_TV;

partial class Main
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
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
        btnStart = new System.Windows.Forms.Button();
        btnStop = new System.Windows.Forms.Button();
        txtLog = new System.Windows.Forms.TextBox();
        cmb = new System.Windows.Forms.ComboBox();
        SuspendLayout();
        // 
        // btnStart
        // 
        btnStart.Location = new System.Drawing.Point(75, 97);
        btnStart.Name = "btnStart";
        btnStart.Size = new System.Drawing.Size(161, 64);
        btnStart.TabIndex = 0;
        btnStart.Text = "Chạy đê";
        btnStart.UseVisualStyleBackColor = true;
        // 
        // btnStop
        // 
        btnStop.Location = new System.Drawing.Point(403, 97);
        btnStop.Name = "btnStop";
        btnStop.Size = new System.Drawing.Size(208, 64);
        btnStop.TabIndex = 1;
        btnStop.Text = "Dừng";
        btnStop.UseVisualStyleBackColor = true;
        // 
        // txtLog
        // 
        txtLog.Location = new System.Drawing.Point(75, 199);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        txtLog.Size = new System.Drawing.Size(639, 226);
        txtLog.TabIndex = 2;
        // 
        // cmb
        // 
        cmb.FormattingEnabled = true;
        cmb.Items.AddRange(new object[] { "Nhóm khách hàng", "Khách hàng", "Nhóm định danh", "Định danh", "Cổng", "Máy tính", "Camera", "Bộ điều khiển", "Làn", "Led" });
        cmb.Location = new System.Drawing.Point(75, 33);
        cmb.Name = "cmb";
        cmb.Size = new System.Drawing.Size(607, 28);
        cmb.TabIndex = 3;
        // 
        // Main
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(800, 450);
        Controls.Add(cmb);
        Controls.Add(txtLog);
        Controls.Add(btnStop);
        Controls.Add(btnStart);
        Text = "Main1";
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.Button btnStart;
    private System.Windows.Forms.Button btnStop;
    private System.Windows.Forms.TextBox txtLog;
    private System.Windows.Forms.ComboBox cmb;

    #endregion
}