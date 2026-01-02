namespace EquipmentAccounting.Forms;

public class AboutForm : Form
{
    public AboutForm()
    {
        this.Text = "О программе";
        this.Width = 450;
        this.Height = 350;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterParent;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        var logoBox = new PictureBox
        {
            Left = 125,
            Top = 20,
            Width = 200,
            Height = 80,
            SizeMode = PictureBoxSizeMode.Zoom
        };

        string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
        if (File.Exists(logoPath))
        {
            logoBox.Image = Image.FromFile(logoPath);
        }
        else
        {
            logoBox.BackColor = Color.LightGray;
        }

        var lblTitle = new Label
        {
            Text = "Учёт контента телеканала",
            Left = 0,
            Top = 110,
            Width = 450,
            Height = 30,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblVersion = new Label
        {
            Text = "Версия 2.0",
            Left = 0,
            Top = 145,
            Width = 450,
            Height = 25,
            Font = new Font("Segoe UI", 10),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblDescription = new Label
        {
            Text = "Система управления контентом и правами на показ.\n" +
                   "Учёт правообладателей, фильмов, контактов и телепрограммы.",
            Left = 20,
            Top = 180,
            Width = 410,
            Height = 50,
            Font = new Font("Segoe UI", 9),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblTech = new Label
        {
            Text = "Технологии: .NET 9.0, Windows Forms, Entity Framework Core, PostgreSQL",
            Left = 0,
            Top = 240,
            Width = 450,
            Height = 25,
            Font = new Font("Segoe UI", 8),
            ForeColor = Color.Gray,
            TextAlign = ContentAlignment.MiddleCenter
        };

        var btnOk = new Button
        {
            Text = "ОК",
            Left = 175,
            Top = 275,
            Width = 100,
            Height = 30,
            DialogResult = DialogResult.OK
        };

        this.Controls.AddRange(new Control[] { logoBox, lblTitle, lblVersion, lblDescription, lblTech, btnOk });
        this.AcceptButton = btnOk;
    }
}
