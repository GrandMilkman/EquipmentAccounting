namespace EquipmentAccounting.Forms;

public class AboutForm : Form
{
    public AboutForm()
    {
        this.Text = "О программе";
        this.Width = 400;
        this.Height = 300;
        Label label = new Label
        {
            Text = "Приложение \"Учет оборудования\"\nВерсия 1.0\nРазработано на C# и .NET",
            Dock = DockStyle.Fill,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        };
        this.Controls.Add(label);
    }
}