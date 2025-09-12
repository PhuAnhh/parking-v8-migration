namespace Application;

public partial class Main : Form
{
    private readonly IServiceProvider _serviceProvider;
    
    public Main(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    private void btnEvent_Click(object sender, EventArgs e)
    {
        this.Hide();
        
        var eventForm = new Event(_serviceProvider);
        eventForm.FormClosed += (s, args) => this.Show();
        eventForm.ShowDialog();
    }

    private void btnExcel_Click(object sender, EventArgs e)
    {
        this.Hide();
        
        var excelForm = new Excel();
        excelForm.FormClosed += (s, args) => this.Show();
        excelForm.ShowDialog();
    }
}