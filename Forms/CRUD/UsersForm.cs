using EquipmentAccounting.Models;
using EquipmentAccounting.Utils;
using Microsoft.EntityFrameworkCore;

namespace EquipmentAccounting.Forms.CRUD;

/// <summary>
/// Форма управления пользователями системы.
/// Доступна только администраторам. Позволяет создавать, редактировать и удалять учётные записи.
/// </summary>
public class UsersForm : CrudForm<User>
{
    /// <summary>
    /// Конструктор формы управления пользователями.
    /// Все операции доступны только при наличии права CanManageUsers.
    /// </summary>
    public UsersForm() : base("Управление пользователями",
        showAddButton: SessionManager.CanManageUsers,
        showEditButton: SessionManager.CanManageUsers,
        showDeleteButton: SessionManager.CanManageUsers)
    {
    }

    /// <summary>
    /// Загрузка списка всех пользователей с их ролями.
    /// </summary>
    protected override void LoadData()
    {
        var data = context.Users
            .Include(u => u.Role)
            .Select(u => new
            {
                u.Id,
                Логин = u.Login,
                Роль = u.Role.Name
            })
            .ToList();

        dataGridView.DataSource = data;

        // Скрытие технической колонки Id
        if (dataGridView.Columns.Contains("Id"))
            dataGridView.Columns["Id"].Visible = false;
    }

    /// <summary>
    /// Добавление нового пользователя с выбором роли.
    /// Проверяет уникальность логина.
    /// </summary>
    protected override void BtnAdd_Click(object? sender, EventArgs e)
    {
        // Ввод логина
        string login = InputDialog.Show("Логин:", "Добавление пользователя");
        if (string.IsNullOrWhiteSpace(login)) return;

        // Проверка уникальности логина
        if (context.Users.Any(u => u.Login == login))
        {
            MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Ввод пароля
        string password = InputDialog.Show("Пароль:", "Добавление пользователя");
        if (string.IsNullOrWhiteSpace(password)) return;

        // Выбор роли из списка
        var roles = context.Roles.ToList();
        using var selectForm = new SelectRoleForm(roles);
        if (selectForm.ShowDialog() != DialogResult.OK || selectForm.SelectedRole == null)
        {
            return;
        }

        var user = new User
        {
            Login = login,
            Password = password,
            RoleId = selectForm.SelectedRole.Id
        };

        context.Users.Add(user);
        context.SaveChanges();
        LoadData();
    }

    /// <summary>
    /// Редактирование выбранного пользователя.
    /// Защищает от изменения роли единственного администратора.
    /// </summary>
    protected override void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        var user = context.Users.Include(u => u.Role).First(u => u.Id == id);

        // Защита от изменения роли единственного администратора
        if (user.Id == SessionManager.CurrentUser?.Id)
        {
            var adminCount = context.Users.Count(u => u.Role.CanManageUsers);
            if (adminCount <= 1)
            {
                MessageBox.Show("Вы не можете изменить свою роль, так как вы единственный администратор",
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Редактирование логина
        string login = InputDialog.Show("Логин:", "Редактирование", user.Login);
        if (string.IsNullOrWhiteSpace(login)) return;

        // Проверка уникальности нового логина
        if (login != user.Login && context.Users.Any(u => u.Login == login))
        {
            MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Редактирование пароля (пустое значение = без изменений)
        string password = InputDialog.Show("Пароль (оставьте пустым чтобы не менять):", "Редактирование");

        user.Login = login;
        if (!string.IsNullOrWhiteSpace(password))
        {
            user.Password = password;
        }

        // Выбор новой роли
        var roles = context.Roles.ToList();
        using var selectForm = new SelectRoleForm(roles, user.Role);
        if (selectForm.ShowDialog() == DialogResult.OK && selectForm.SelectedRole != null)
        {
            user.RoleId = selectForm.SelectedRole.Id;
        }

        context.SaveChanges();
        LoadData();
    }

    /// <summary>
    /// Удаление пользователя. Нельзя удалить свой собственный аккаунт.
    /// </summary>
    protected override void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        var user = context.Users.First(u => u.Id == id);

        // Запрет удаления собственного аккаунта
        if (user.Id == SessionManager.CurrentUser?.Id)
        {
            MessageBox.Show("Вы не можете удалить свой собственный аккаунт",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Подтверждение удаления
        if (MessageBox.Show($"Удалить пользователя '{user.Login}'?", "Подтверждение",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            context.Users.Remove(user);
            context.SaveChanges();
            LoadData();
        }
    }

    /// <summary>
    /// Поиск пользователей по логину или названию роли.
    /// </summary>
    protected override void BtnSearch_Click(object? sender, EventArgs e)
    {
        string searchTerm = InputDialog.Show("Введите часть логина для поиска:", "Поиск");
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            LoadData();
            return;
        }

        var data = context.Users
            .Include(u => u.Role)
            .Where(u => u.Login.ToLower().Contains(searchTerm.ToLower()) ||
                        u.Role.Name.ToLower().Contains(searchTerm.ToLower()))
            .Select(u => new
            {
                u.Id,
                Логин = u.Login,
                Роль = u.Role.Name
            })
            .ToList();

        dataGridView.DataSource = data;

        if (dataGridView.Columns.Contains("Id"))
            dataGridView.Columns["Id"].Visible = false;
    }
}

/// <summary>
/// Вспомогательная форма для выбора роли пользователя.
/// </summary>
public class SelectRoleForm : Form
{
    /// <summary>Список ролей для выбора</summary>
    private ListBox listBox;

    /// <summary>Кнопка подтверждения</summary>
    private Button btnOk;

    /// <summary>Кнопка отмены</summary>
    private Button btnCancel;

    /// <summary>Выбранная роль</summary>
    public Role? SelectedRole { get; private set; }

    /// <summary>
    /// Конструктор формы выбора роли.
    /// </summary>
    /// <param name="roles">Список доступных ролей</param>
    /// <param name="currentRole">Текущая роль пользователя (для редактирования)</param>
    public SelectRoleForm(List<Role> roles, Role? currentRole = null)
    {
        this.Text = "Выберите роль";
        this.Width = 400;
        this.Height = 300;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterParent;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        // Список ролей
        listBox = new ListBox
        {
            Left = 10,
            Top = 10,
            Width = 360,
            Height = 200
        };

        foreach (var role in roles)
        {
            listBox.Items.Add(role);
        }
        listBox.DisplayMember = "Name";

        // Выбор текущей роли, если указана
        if (currentRole != null)
        {
            listBox.SelectedItem = roles.FirstOrDefault(r => r.Id == currentRole.Id);
        }

        // Кнопки управления
        btnOk = new Button { Text = "ОК", Left = 150, Top = 220, Width = 80, DialogResult = DialogResult.OK };
        btnCancel = new Button { Text = "Отмена", Left = 240, Top = 220, Width = 80, DialogResult = DialogResult.Cancel };

        btnOk.Click += (s, e) =>
        {
            SelectedRole = listBox.SelectedItem as Role;
            if (SelectedRole == null)
            {
                MessageBox.Show("Выберите роль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        };

        this.Controls.Add(listBox);
        this.Controls.Add(btnOk);
        this.Controls.Add(btnCancel);

        this.AcceptButton = btnOk;
        this.CancelButton = btnCancel;
    }
}
