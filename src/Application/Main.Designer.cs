using System.ComponentModel;

namespace Application;

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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
        pbLogo = new System.Windows.Forms.PictureBox();
        btnEvent = new System.Windows.Forms.Button();
        btnExcel = new System.Windows.Forms.Button();
        ((System.ComponentModel.ISupportInitialize)pbLogo).BeginInit();
        SuspendLayout();
        // 
        // pbLogo
        // 
        pbLogo.Image = ((System.Drawing.Image)resources.GetObject("pbLogo.Image"));
        pbLogo.Location = new System.Drawing.Point(182, 36);
        pbLogo.Name = "pbLogo";
        pbLogo.Size = new System.Drawing.Size(200, 56);
        pbLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        pbLogo.TabIndex = 0;
        pbLogo.TabStop = false;
        // 
        // btnEvent
        // 
        btnEvent.Cursor = System.Windows.Forms.Cursors.Hand;
        btnEvent.Font = new System.Drawing.Font("Cascadia Code SemiLight", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)0));
        btnEvent.ForeColor = System.Drawing.Color.MidnightBlue;
        btnEvent.Location = new System.Drawing.Point(89, 124);
        btnEvent.Name = "btnEvent";
        btnEvent.Size = new System.Drawing.Size(179, 61);
        btnEvent.TabIndex = 1;
        btnEvent.Text = "Sự kiện vào/ra";
        btnEvent.UseVisualStyleBackColor = true;
        btnEvent.Click += btnEvent_Click;
        // 
        // btnExcel
        // 
        btnExcel.Cursor = System.Windows.Forms.Cursors.Hand;
        btnExcel.Font = new System.Drawing.Font("Cascadia Code SemiLight", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)0));
        btnExcel.ForeColor = System.Drawing.Color.MidnightBlue;
        btnExcel.Location = new System.Drawing.Point(313, 124);
        btnExcel.Name = "btnExcel";
        btnExcel.Size = new System.Drawing.Size(170, 61);
        btnExcel.TabIndex = 2;
        btnExcel.Text = "Excel";
        btnExcel.UseVisualStyleBackColor = true;
        btnExcel.Click += btnExcel_Click;
        // 
        // Main
        // 
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
        BackColor = System.Drawing.Color.White;
        BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
        ClientSize = new System.Drawing.Size(573, 226);
        Controls.Add(btnExcel);
        Controls.Add(btnEvent);
        Controls.Add(pbLogo);
        Icon = ((System.Drawing.Icon)resources.GetObject("$this.Icon"));
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "Parking v8 Migration";
        ((System.ComponentModel.ISupportInitialize)pbLogo).EndInit();
        ResumeLayout(false);
    }

    private System.Windows.Forms.Button btnEvent;
    private System.Windows.Forms.Button btnExcel;

    private System.Windows.Forms.PictureBox pbLogo;

    #endregion
}