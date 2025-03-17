using EquipmentAccounting.Data;

namespace EquipmentAccounting.Forms;

public class LoginForm : Form
{
    private TextBox txtLogin;
    private TextBox txtPassword;
    private Button btnLogin;

    public LoginForm()
    {
        this.Text = "Авторизация";
        this.Width = 300;
        this.Height = 200;

        Label lblLogin = new Label { Text = "Логин:", Left = 10, Top = 20, Width = 80 };
        txtLogin = new TextBox { Left = 100, Top = 20, Width = 150 };

        Label lblPassword = new Label { Text = "Пароль:", Left = 10, Top = 60, Width = 80 };
        txtPassword = new TextBox { Left = 100, Top = 60, Width = 150, PasswordChar = '*' };

        btnLogin = new Button { Text = "Вход", Left = 100, Top = 100, Width = 80 };
        btnLogin.Click += BtnLogin_Click;

        this.Controls.Add(lblLogin);
        this.Controls.Add(txtLogin);
        this.Controls.Add(lblPassword);
        this.Controls.Add(txtPassword);
        this.Controls.Add(btnLogin);
    }

    private void BtnLogin_Click(object sender, EventArgs e)
    {
        using (var context = new AppDbContext())
        {
            var user = context.Users
                .FirstOrDefault(u => u.Login == txtLogin.Text && u.Password == txtPassword.Text);
            if (user != null)
            {
                MessageBox.Show("Авторизация прошла успешно");
                this.Hide();
                MainForm mainForm = new MainForm(user);
                mainForm.FormClosed += (s, args) => this.Close();
                mainForm.Show();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль");
            }
        }
    }
}