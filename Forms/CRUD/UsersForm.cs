using EquipmentAccounting.Models;
using EquipmentAccounting.Utils;
using Microsoft.EntityFrameworkCore;

namespace EquipmentAccounting.Forms.CRUD;

public class UsersForm : CrudForm<User>
{
    public UsersForm() : base("Управление пользователями",
        showAddButton: SessionManager.CanManageUsers,
        showEditButton: SessionManager.CanManageUsers,
        showDeleteButton: SessionManager.CanManageUsers)
    {
    }

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

        if (dataGridView.Columns.Contains("Id"))
            dataGridView.Columns["Id"].Visible = false;
    }

    protected override void BtnAdd_Click(object? sender, EventArgs e)
    {
        string login = InputDialog.Show("Логин:", "Добавление пользователя");
        if (string.IsNullOrWhiteSpace(login)) return;

        // Check if login already exists
        if (context.Users.Any(u => u.Login == login))
        {
            MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        string password = InputDialog.Show("Пароль:", "Добавление пользователя");
        if (string.IsNullOrWhiteSpace(password)) return;

        // Select role
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

    protected override void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        var user = context.Users.Include(u => u.Role).First(u => u.Id == id);

        // Prevent editing own account's role if it's the only admin
        if (user.Id == SessionManager.CurrentUser?.Id)
        {
            var adminCount = context.Users.Count(u => u.Role.CanManageUsers);
            if (adminCount <= 1)
            {
                MessageBox.Show("Вы не можете изменить свою роль, так как вы единственный администратор",
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        string login = InputDialog.Show("Логин:", "Редактирование", user.Login);
        if (string.IsNullOrWhiteSpace(login)) return;

        // Check if new login already exists (excluding current user)
        if (login != user.Login && context.Users.Any(u => u.Login == login))
        {
            MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        string password = InputDialog.Show("Пароль (оставьте пустым чтобы не менять):", "Редактирование");

        user.Login = login;
        if (!string.IsNullOrWhiteSpace(password))
        {
            user.Password = password;
        }

        // Select role
        var roles = context.Roles.ToList();
        using var selectForm = new SelectRoleForm(roles, user.Role);
        if (selectForm.ShowDialog() == DialogResult.OK && selectForm.SelectedRole != null)
        {
            user.RoleId = selectForm.SelectedRole.Id;
        }

        context.SaveChanges();
        LoadData();
    }

    protected override void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        var user = context.Users.First(u => u.Id == id);

        // Prevent deleting self
        if (user.Id == SessionManager.CurrentUser?.Id)
        {
            MessageBox.Show("Вы не можете удалить свой собственный аккаунт",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (MessageBox.Show($"Удалить пользователя '{user.Login}'?", "Подтверждение",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            context.Users.Remove(user);
            context.SaveChanges();
            LoadData();
        }
    }

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

// Helper form for selecting role
public class SelectRoleForm : Form
{
    private ListBox listBox;
    private Button btnOk;
    private Button btnCancel;

    public Role? SelectedRole { get; private set; }

    public SelectRoleForm(List<Role> roles, Role? currentRole = null)
    {
        this.Text = "Выберите роль";
        this.Width = 400;
        this.Height = 300;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterParent;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

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

        if (currentRole != null)
        {
            listBox.SelectedItem = roles.FirstOrDefault(r => r.Id == currentRole.Id);
        }

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
