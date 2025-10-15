using Application_v6.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application_v6;

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

        txtLog.Clear();

        Log("Bắt đầu ...");

        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var selected = cmb.SelectedIndex;

            if (selected == 0)
            {
                var insertService = scope.ServiceProvider.GetRequiredService<CustomerCollectionService>();
                await insertService.InsertCustomerCollection(Log, token);
            }
            else if (selected == 1)
            {
                var insertService = scope.ServiceProvider.GetRequiredService<CustomerService>();
                await insertService.InsertCustomer(Log, token);
            }
            else if (selected == 2)
            {
                var insertService = scope.ServiceProvider.GetRequiredService<AccessKeyCollectionService>();
                await insertService.InsertAccessKeyCollection(Log, token);
            }
            else if (selected == 3)
            {
                var insertService = scope.ServiceProvider.GetRequiredService<AccessKeyService>();
                await insertService.InsertAccessKey(Log, token);
            }
            else if (selected == 4)
            {
                var insertService = scope.ServiceProvider.GetRequiredService<DeviceService>();
                await insertService.InsertGate(Log, token);
            }
            else if (selected == 5)
            {
                var insertService = scope.ServiceProvider.GetRequiredService<DeviceService>();
                await insertService.InsertComputer(Log, token);
            }
            else if (selected == 6)
            {
                var insertService = scope.ServiceProvider.GetRequiredService<DeviceService>();
                await insertService.InsertCamera(Log, token);
            }
            else if (selected == 7)
            {
                var insertService = scope.ServiceProvider.GetRequiredService<DeviceService>();
                await insertService.InsertControlUnit(Log, token);
            }
            else if (selected == 8)
            {
                var insertService = scope.ServiceProvider.GetRequiredService<DeviceService>();
                await insertService.InsertLane(Log, token);
            }
            else if (selected == 9)
            {
                var insertService = scope.ServiceProvider.GetRequiredService<DeviceService>();
                await insertService.InsertLed(Log, token);
            }
            else if (selected == 10)
            {
                var insertService = scope.ServiceProvider.GetRequiredService<EntryService>();
                await insertService.InsertEntry(Log, token);
            }
            else if (selected == 11)
            {
                var insertService = scope.ServiceProvider.GetRequiredService<ExitService>();
                await insertService.InsertExit(Log, token);
            }
            MessageBox.Show("Success", "✔ Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (OperationCanceledException)
        {
            Log("Stop!");
            MessageBox.Show("Stop", "⚠ Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            Log($"Error: {ex}");
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
        txtLog.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\r\n");
    }
}