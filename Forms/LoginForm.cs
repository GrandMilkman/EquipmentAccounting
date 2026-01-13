using EquipmentAccounting.Data;
using EquipmentAccounting.Utils;
using Microsoft.EntityFrameworkCore;

namespace EquipmentAccounting.Forms;

/// <summary>
/// Форма авторизации пользователя.
/// Точка входа в приложение. Проверяет логин и пароль, устанавливает текущую сессию.
/// </summary>
public class LoginForm : Form
{
    /// <summary>Логотип приложения/телеканала</summary>
    private PictureBox logoBox;

    /// <summary>Поле ввода логина</summary>
    private TextBox txtLogin;

    /// <summary>Поле ввода пароля</summary>
    private TextBox txtPassword;

    /// <summary>Кнопка входа в систему</summary>
    private Button btnLogin;

    /// <summary>
    /// Конструктор формы авторизации.
    /// Создаёт интерфейс входа с логотипом и полями ввода.
    /// </summary>
    public LoginForm()
    {
        this.Text = "Авторизация";
        this.Width = 350;
        this.Height = 320;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        // Логотип телеканала
        logoBox = new PictureBox
        {
            Left = 75,
            Top = 20,
            Width = 200,
            Height = 80,
            SizeMode = PictureBoxSizeMode.Zoom
        };

        // Загрузка логотипа из файла
        string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
        if (File.Exists(logoPath))
        {
            logoBox.Image = Image.FromFile(logoPath);
        }
        else
        {
            // Заглушка, если логотип отсутствует
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

        // Поле ввода логина
        Label lblLogin = new Label { Text = "Логин:", Left = 30, Top = 120, Width = 80 };
        txtLogin = new TextBox { Left = 120, Top = 117, Width = 180 };

        // Поле ввода пароля (с маскировкой символов)
        Label lblPassword = new Label { Text = "Пароль:", Left = 30, Top = 160, Width = 80 };
        txtPassword = new TextBox { Left = 120, Top = 157, Width = 180, PasswordChar = '*' };

        // Кнопка входа
        btnLogin = new Button
        {
            Text = "Вход",
            Left = 120,
            Top = 200,
            Width = 100,
            Height = 30
        };
        btnLogin.Click += BtnLogin_Click;

        // Enter нажимает кнопку входа
        this.AcceptButton = btnLogin;

        // Добавление элементов на форму
        this.Controls.Add(logoBox);
        this.Controls.Add(lblLogin);
        this.Controls.Add(txtLogin);
        this.Controls.Add(lblPassword);
        this.Controls.Add(txtPassword);
        this.Controls.Add(btnLogin);
    }

    /// <summary>
    /// Обработка попытки входа в систему.
    /// Проверяет учётные данные и при успехе открывает главную форму.
    /// </summary>
    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        using (var context = new AppDbContext())
        {
            // Поиск пользователя по логину и паролю с загрузкой роли
            var user = context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Login == txtLogin.Text && u.Password == txtPassword.Text);

            if (user != null)
            {
                // Успешная авторизация - установка сессии и открытие главной формы
                SessionManager.SetCurrentUser(user);
                this.Hide();
                MainForm mainForm = new MainForm();
                mainForm.FormClosed += (s, args) =>
                {
                    // Очистка сессии при закрытии главной формы
                    SessionManager.ClearSession();
                    this.Close();
                };
                mainForm.Show();
            }
            else
            {
                // Неверные учётные данные
                MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
