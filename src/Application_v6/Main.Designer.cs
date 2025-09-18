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
        cmb = new System.Windows.Forms.ComboBox();
        dtp = new System.Windows.Forms.DateTimePicker();
        btnStart = new System.Windows.Forms.Button();
        btnStop = new System.Windows.Forms.Button();
        txtLog = new System.Windows.Forms.TextBox();
        SuspendLayout();
        // 
        // cmb
        // 
        cmb.FormattingEnabled = true;
        cmb.Items.AddRange(new object[] { "Nhóm định danh", "Định danh", "Nhóm khách hàng", "Khách hàng ", "Làn", "Xe vào bãi", "Xe đã ra" });
        cmb.Location = new System.Drawing.Point(35, 28);
        cmb.Name = "cmb";
        cmb.Size = new System.Drawing.Size(273, 28);
        cmb.TabIndex = 0;
        // 
        // dtp
        // 
        dtp.Cursor = System.Windows.Forms.Cursors.Hand;
        dtp.CustomFormat = "dd-MM-yyyy HH:mm:ss";
        dtp.DropDownAlign = System.Windows.Forms.LeftRightAlignment.Right;
        dtp.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        dtp.Location = new System.Drawing.Point(375, 29);
        dtp.Name = "dtp";
        dtp.Size = new System.Drawing.Size(371, 27);
        dtp.TabIndex = 1;
        // 
        // btnStart
        // 
        btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
        btnStart.Location = new System.Drawing.Point(157, 85);
        btnStart.Name = "btnStart";
        btnStart.Size = new System.Drawing.Size(151, 47);
        btnStart.TabIndex = 2;
        btnStart.Text = "▶ Start";
        btnStart.UseVisualStyleBackColor = true;
        btnStart.Click += btnStart_Click;
        // 
        // btnStop
        // 
        btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
        btnStop.Location = new System.Drawing.Point(400, 85);
        btnStop.Name = "btnStop";
        btnStop.Size = new System.Drawing.Size(151, 47);
        btnStop.TabIndex = 3;
        btnStop.Text = "⬛ Stop";
        btnStop.UseVisualStyleBackColor = true;
        btnStop.Click += btnStop_Click;
        // 
        // txtLog
        // 
        txtLog.Location = new System.Drawing.Point(64, 177);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        txtLog.Size = new System.Drawing.Size(682, 353);
        txtLog.TabIndex = 5;
        // 
        // Main
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(800, 580);
        Controls.Add(txtLog);
        Controls.Add(btnStop);
        Controls.Add(btnStart);
        Controls.Add(dtp);
        Controls.Add(cmb);
        Text = "Form1";
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.TextBox txtLog;

    private System.Windows.Forms.Button btnStart;
    private System.Windows.Forms.Button btnStop;

    private System.Windows.Forms.DateTimePicker dtp;

    private System.Windows.Forms.ComboBox cmb;

    #endregion
}