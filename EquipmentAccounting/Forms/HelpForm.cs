namespace EquipmentAccounting.Forms;

public class HelpForm : Form
{
    public HelpForm()
    {
        this.Text = "Справка";
        this.Width = 400;
        this.Height = 300;
        Label label = new Label
        {
            Text = "Справочная информация по работе с приложением...",
            Dock = DockStyle.Fill,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        };
        this.Controls.Add(label);
    }
}