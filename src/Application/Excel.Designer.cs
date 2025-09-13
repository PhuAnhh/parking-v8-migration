using System.ComponentModel;

namespace Application;

partial class Excel
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Excel));
        lblFile = new System.Windows.Forms.Label();
        txtFile = new System.Windows.Forms.TextBox();
        btnFile = new System.Windows.Forms.Button();
        btnStart = new System.Windows.Forms.Button();
        txtLog = new System.Windows.Forms.TextBox();
        prgFileProcessing = new System.Windows.Forms.ProgressBar();
        SuspendLayout();
        // 
        // lblFile
        // 
        lblFile.Font = new System.Drawing.Font("Cascadia Code SemiLight", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)0));
        lblFile.ForeColor = System.Drawing.Color.Green;
        lblFile.Location = new System.Drawing.Point(12, 13);
        lblFile.Name = "lblFile";
        lblFile.Size = new System.Drawing.Size(79, 31);
        lblFile.TabIndex = 0;
        lblFile.Text = "Excel:";
        // 
        // txtFile
        // 
        txtFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        txtFile.BackColor = System.Drawing.Color.White;
        txtFile.Font = new System.Drawing.Font("Cascadia Code SemiLight", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)0));
        txtFile.Location = new System.Drawing.Point(97, 12);
        txtFile.Name = "txtFile";
        txtFile.ReadOnly = true;
        txtFile.Size = new System.Drawing.Size(543, 27);
        txtFile.TabIndex = 1;
        // 
        // btnFile
        // 
        btnFile.Cursor = System.Windows.Forms.Cursors.Hand;
        btnFile.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        btnFile.ForeColor = System.Drawing.Color.Green;
        btnFile.Location = new System.Drawing.Point(598, 12);
        btnFile.Name = "btnFile";
        btnFile.Size = new System.Drawing.Size(42, 27);
        btnFile.TabIndex = 2;
        btnFile.Text = "📂";
        btnFile.UseVisualStyleBackColor = true;
        btnFile.Click += btnFile_Click;
        // 
        // btnStart
        // 
        btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        btnStart.BackColor = System.Drawing.Color.White;
        btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
        btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnStart.ForeColor = System.Drawing.Color.Green;
        btnStart.Location = new System.Drawing.Point(12, 56);
        btnStart.Name = "btnStart";
        btnStart.Size = new System.Drawing.Size(628, 39);
        btnStart.TabIndex = 3;
        btnStart.Text = "▶ Start";
        btnStart.UseVisualStyleBackColor = false;
        btnStart.Click += btnStart_Click;
        // 
        // txtLog
        // 
        txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        txtLog.BackColor = System.Drawing.Color.White;
        txtLog.Font = new System.Drawing.Font("Cascadia Code SemiLight", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)0));
        txtLog.Location = new System.Drawing.Point(12, 112);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        txtLog.Size = new System.Drawing.Size(628, 366);
        txtLog.TabIndex = 4;
        // 
        // prgFileProcessing
        // 
        prgFileProcessing.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        prgFileProcessing.BackColor = System.Drawing.Color.White;
        prgFileProcessing.ForeColor = System.Drawing.Color.Green;
        prgFileProcessing.Location = new System.Drawing.Point(12, 474);
        prgFileProcessing.Name = "prgFileProcessing";
        prgFileProcessing.Size = new System.Drawing.Size(628, 17);
        prgFileProcessing.Step = 1;
        prgFileProcessing.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
        prgFileProcessing.TabIndex = 5;
        // 
        // Excel
        // 
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
        BackColor = System.Drawing.SystemColors.Control;
        ClientSize = new System.Drawing.Size(652, 503);
        Controls.Add(prgFileProcessing);
        Controls.Add(lblFile);
        Controls.Add(txtLog);
        Controls.Add(btnStart);
        Controls.Add(btnFile);
        Controls.Add(txtFile);
        Font = new System.Drawing.Font("Cascadia Code SemiLight", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)0));
        Icon = ((System.Drawing.Icon)resources.GetObject("$this.Icon"));
        Location = new System.Drawing.Point(19, 19);
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "Excel";
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.ProgressBar prgFileProcessing;
    private System.Windows.Forms.TextBox txtLog;
    private System.Windows.Forms.Button btnStart;
    private System.Windows.Forms.Button btnFile;
    private System.Windows.Forms.TextBox txtFile;
    private System.Windows.Forms.Label lblFile;

    #endregion
}