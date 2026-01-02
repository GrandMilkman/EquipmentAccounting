namespace EquipmentAccounting.Utils;

public static class InputDialog
{
    public static string Show(string text, string caption, string defaultValue = "")
    {
        Form prompt = new Form()
        {
            Width = 400,
            Height = 150,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = caption,
            StartPosition = FormStartPosition.CenterScreen
        };
        Label textLabel = new Label() { Left = 20, Top = 20, Text = text, Width = 340 };
        TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 340, Text = defaultValue };
        Button confirmation = new Button() { Text = "ОК", Left = 280, Width = 80, Top = 80, DialogResult = DialogResult.OK };
        confirmation.Click += (sender, e) => { prompt.Close(); };
        prompt.Controls.Add(textLabel);
        prompt.Controls.Add(textBox);
        prompt.Controls.Add(confirmation);
        prompt.AcceptButton = confirmation;
        return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
    }
}
