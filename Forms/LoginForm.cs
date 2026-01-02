using EquipmentAccounting.Data;
using EquipmentAccounting.Utils;
using Microsoft.EntityFrameworkCore;

namespace EquipmentAccounting.Forms;

public class LoginForm : Form
{
    private PictureBox logoBox;
    private TextBox txtLogin;
    private TextBox txtPassword;
    private Button btnLogin;

    public LoginForm()
    {
        this.Text = "Авторизация";
        this.Width = 350;
        this.Height = 320;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        // Logo
        logoBox = new PictureBox
        {
            Left = 75,
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
            // Placeholder if no logo
            logoBox.BackColor = Color.LightGray;
            var placeholderLabel = new Label
            {
                Text = "ЛОГОТИП",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            logoBox.Controls.Add(placeholderLabel);
        }

        Label lblLogin = new Label { Text = "Логин:", Left = 30, Top = 120, Width = 80 };
        txtLogin = new TextBox { Left = 120, Top = 117, Width = 180 };

        Label lblPassword = new Label { Text = "Пароль:", Left = 30, Top = 160, Width = 80 };
        txtPassword = new TextBox { Left = 120, Top = 157, Width = 180, PasswordChar = '*' };

        btnLogin = new Button
        {
            Text = "Вход",
            Left = 120,
            Top = 200,
            Width = 100,
            Height = 30
        };
        btnLogin.Click += BtnLogin_Click;

        this.AcceptButton = btnLogin;

        this.Controls.Add(logoBox);
        this.Controls.Add(lblLogin);
        this.Controls.Add(txtLogin);
        this.Controls.Add(lblPassword);
        this.Controls.Add(txtPassword);
        this.Controls.Add(btnLogin);
    }

    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        using (var context = new AppDbContext())
        {
            var user = context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Login == txtLogin.Text && u.Password == txtPassword.Text);

            if (user != null)
            {
                SessionManager.SetCurrentUser(user);
                this.Hide();
                MainForm mainForm = new MainForm();
                mainForm.FormClosed += (s, args) =>
                {
                    SessionManager.ClearSession();
                    this.Close();
                };
                mainForm.Show();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
