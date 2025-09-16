using OfficeOpenXml;

namespace Application;

public partial class Excel : Form
{
    private string sourceFile = "";

    public Excel()
    {
        InitializeComponent();
        ExcelPackage.License.SetNonCommercialPersonal("Excel");
    }

    private void btnFile_Click(object sender, EventArgs e)
    {
        using (OpenFileDialog ofd = new OpenFileDialog())
        {
            ofd.Filter = "Excel Files|*.xlsx;*.xls";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                sourceFile = ofd.FileName;
                txtFile.Text = sourceFile;
                Log("Selected file: " + sourceFile);
            }
        }
    }

    private void btnStart_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(sourceFile))
        {
            MessageBox.Show("Please select an Excel file!", "⚠ Warning",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        try
        {
            string destFile = Path.Combine(
                Path.GetDirectoryName(sourceFile),
                "Result.xlsx"
            );

            prgFileProcessing.Visible = true;
            prgFileProcessing.Value = 0;

            ProcessExcel(sourceFile, destFile);

            Log("Completed! File created: " + destFile);
            MessageBox.Show("File has been saved successfully:\r\n" + destFile, "✔ Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            Log("Error: " + ex.Message);
            MessageBox.Show("Error: " + ex.Message, "✖ Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ProcessExcel(string sourceFile, string destFile)
    {
        using (var src = new ExcelPackage(new FileInfo(sourceFile)))
        using (var dest = new ExcelPackage())
        {
            var wsSrc = src.Workbook.Worksheets[0];
            var wsNew = dest.Workbook.Worksheets.Add("Sheet1");

            wsNew.Cells[1, 1].Value = "STT";
            wsNew.Cells[1, 2].Value = "Mã định danh";
            wsNew.Cells[1, 3].Value = "Tên định danh";
            wsNew.Cells[1, 4].Value = "Loại";
            wsNew.Cells[1, 5].Value = "Trạng thái định danh";
            wsNew.Cells[1, 6].Value = "Mã nhóm định danh";
            wsNew.Cells[1, 7].Value = "Ghi chú";
            wsNew.Cells[1, 8].Value = "Tên phương tiện";
            wsNew.Cells[1, 9].Value = "Biển số hiện tại";
            wsNew.Cells[1, 10].Value = "Biển số mới";
            wsNew.Cells[1, 11].Value = "Trạng thái phương tiện";
            wsNew.Cells[1, 12].Value = "Ghi chú";
            wsNew.Cells[1, 13].Value = "Ngày hết hạn";
            wsNew.Cells[1, 14].Value = "Tên khách hàng";
            wsNew.Cells[1, 15].Value = "Mã khách hàng";
            wsNew.Cells[1, 16].Value = "Nhóm khách hàng";
            wsNew.Cells[1, 17].Value = "Số điện thoại";
            wsNew.Cells[1, 18].Value = "Địa chỉ";
            wsNew.Cells[1, 19].Value = "Ngày hoạt động";

            using (var headerRange = wsNew.Cells[1, 1, 1, 19])
            {
                headerRange.Style.Font.Name = "Times New Roman";
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.Size = 12;
            }

            int destRow = 2;
            int processed = 0;

            int lastRow = wsSrc.Cells
                .Where(c => c.Value != null && !string.IsNullOrEmpty(c.Text))
                .Select(c => c.Start.Row)
                .DefaultIfEmpty(7)
                .Max();

            int total = lastRow - 7;

            prgFileProcessing.Minimum = 0;
            prgFileProcessing.Maximum = total;
            prgFileProcessing.Value = 0;

            for (int row = 8; row <= wsSrc.Dimension.Rows; row++)
            {
                if (wsSrc.Cells[row, 1].Value == null && wsSrc.Cells[row, 2].Value == null)
                    break;

                wsNew.Cells[destRow, 1].Value = wsSrc.Cells[row, 1].Value; // STT
                wsNew.Cells[destRow, 2].Value = wsSrc.Cells[row, 3].Value; // Mã thẻ
                wsNew.Cells[destRow, 3].Value = wsSrc.Cells[row, 2].Value; // CardNo
                wsNew.Cells[destRow, 4].Value = "Thẻ";
                wsNew.Cells[destRow, 5].Value = wsSrc.Cells[row, 15].Value; //Trạng thái
                wsNew.Cells[destRow, 6].Value = wsSrc.Cells[row, 4].Value; // Nhóm thẻ
                wsNew.Cells[destRow, 7].Value = "";
                wsNew.Cells[destRow, 8].Value = wsSrc.Cells[row, 7].Value; // Tên xe
                wsNew.Cells[destRow, 9].Value = "";
                wsNew.Cells[destRow, 10].Value = wsSrc.Cells[row, 6].Value; // Biển số
                wsNew.Cells[destRow, 11].Value = "Sử dụng";
                wsNew.Cells[destRow, 12].Value = "";
                wsNew.Cells[destRow, 13].Value = wsSrc.Cells[row, 5].Value; // Ngày hết hạn
                wsNew.Cells[destRow, 14].Value = wsSrc.Cells[row, 9].Value; // Khách hàng
                wsNew.Cells[destRow, 15].Value = wsSrc.Cells[row, 8].Value; // Mã khách hàng
                wsNew.Cells[destRow, 16].Value = wsSrc.Cells[row, 10].Value; // Nhóm khách hàng
                wsNew.Cells[destRow, 17].Value = wsSrc.Cells[row, 13].Value; // Số điện thoại
                wsNew.Cells[destRow, 18].Value = wsSrc.Cells[row, 14].Value; // Địa chỉ
                wsNew.Cells[destRow, 19].Value = wsSrc.Cells[row, 16].Value; // Ngày đăng ký

                processed++;
                Log($"Processed {processed}/{total}");

                prgFileProcessing.Value = processed;
                prgFileProcessing.Refresh();
                destRow++;
            }

            using (var dataRange = wsNew.Cells[2, 1, destRow - 1, 19])
            {
                dataRange.Style.Font.Name = "Times New Roman";
            }

            wsNew.Cells[wsNew.Dimension.Address].AutoFitColumns();

            dest.SaveAs(new FileInfo(destFile));
        }
    }

    private void Log(string message)
    {
        txtLog.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\r\n");
    }
}