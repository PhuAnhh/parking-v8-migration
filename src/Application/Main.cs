using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public partial class Main : Form
{
    private readonly IServiceProvider _serviceProvider;
    private CancellationTokenSource? _cancellationTokenSource;

    public Main(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    private async void btnStart_Click(object sender, EventArgs e)
    {
        btnStart.Enabled = false;
        btnStop.Enabled = true;
        Log("Bắt đầu ...");

        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            DateTime fromDate = dtp.Value;
            var selected = cmbEvent.SelectedIndex;

            if (selected == 0)
            {
                var insertService = scope.ServiceProvider.GetRequiredService<InsertEntriesService>();
                await insertService.InsertEntries(fromDate, Log, token);
            }
            else if (selected == 1)
            {
                var insertService = scope.ServiceProvider.GetRequiredService<InsertExitsService>();
                await insertService.InsertExits(fromDate, Log);
            }

            Log("Hoàn tất!");
            MessageBox.Show("Success", "✔ Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (OperationCanceledException)
        {
            Log("Stop!");
            MessageBox.Show("Stop", "⚠ Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            Log($"Lỗi: {ex}");
            MessageBox.Show(ex.ToString(), "✖ Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
        _cancellationTokenSource?.Cancel();
    }

    private void Log(string message)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => Log(message)));
        }
        else
        {
            txtLog.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
        }
    }
}