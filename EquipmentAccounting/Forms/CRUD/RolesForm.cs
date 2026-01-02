using EquipmentAccounting.Models;
using EquipmentAccounting.Utils;

namespace EquipmentAccounting.Forms.CRUD;

public class RolesForm : CrudForm<Role>
{
    public RolesForm() : base("Управление ролями",
        showAddButton: SessionManager.CanManageRoles,
        showEditButton: SessionManager.CanManageRoles,
        showDeleteButton: SessionManager.CanManageRoles)
    {
        this.Width = 1200;
    }

    protected override void LoadData()
    {
        var data = context.Roles
            .Select(r => new
            {
                r.Id,
                Название = r.Name,
                Описание = r.Description,
                Пользователи = r.CanManageUsers ? "Да" : "Нет",
                Роли = r.CanManageRoles ? "Да" : "Нет",
                СозданиеПО = r.CanCreateRightsOwners ? "Да" : "Нет",
                РедПО = r.CanEditRightsOwners ? "Да" : "Нет",
                СозданиеФильмов = r.CanCreateFilms ? "Да" : "Нет",
                РедОсновнИнфо = r.CanEditFilmBasicInfo ? "Да" : "Нет",
                РедПрава = r.CanEditFilmRightsInfo ? "Да" : "Нет",
                Контакты = r.CanManageContacts ? "Да" : "Нет",
                Программа = r.CanManageSchedule ? "Да" : "Нет",
                КолвоПользователей = r.Users.Count
            })
            .ToList();

        dataGridView.DataSource = data;

        if (dataGridView.Columns.Contains("Id"))
            dataGridView.Columns["Id"].Visible = false;
    }

    protected override void BtnAdd_Click(object? sender, EventArgs e)
    {
        using var editForm = new RoleEditForm();
        if (editForm.ShowDialog() == DialogResult.OK && editForm.Role != null)
        {
            context.Roles.Add(editForm.Role);
            context.SaveChanges();
            LoadData();
        }
    }

    protected override void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        var role = context.Roles.First(r => r.Id == id);

        using var editForm = new RoleEditForm(role);
        if (editForm.ShowDialog() == DialogResult.OK)
        {
            context.SaveChanges();
            LoadData();
        }
    }

    protected override void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        var role = context.Roles.First(r => r.Id == id);

        // Check if role is in use
        var usersWithRole = context.Users.Count(u => u.RoleId == id);
        if (usersWithRole > 0)
        {
            MessageBox.Show($"Невозможно удалить роль. Она используется {usersWithRole} пользователем(ями).",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show($"Удалить роль '{role.Name}'?", "Подтверждение",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            context.Roles.Remove(role);
            context.SaveChanges();
            LoadData();
        }
    }

    protected override void BtnSearch_Click(object? sender, EventArgs e)
    {
        string searchTerm = InputDialog.Show("Введите часть названия для поиска:", "Поиск");
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            LoadData();
            return;
        }

        var data = context.Roles
            .Where(r => r.Name.ToLower().Contains(searchTerm.ToLower()))
            .Select(r => new
            {
                r.Id,
                Название = r.Name,
                Описание = r.Description,
                Пользователи = r.CanManageUsers ? "Да" : "Нет",
                Роли = r.CanManageRoles ? "Да" : "Нет",
                СозданиеПО = r.CanCreateRightsOwners ? "Да" : "Нет",
                РедПО = r.CanEditRightsOwners ? "Да" : "Нет",
                СозданиеФильмов = r.CanCreateFilms ? "Да" : "Нет",
                РедОсновнИнфо = r.CanEditFilmBasicInfo ? "Да" : "Нет",
                РедПрава = r.CanEditFilmRightsInfo ? "Да" : "Нет",
                Контакты = r.CanManageContacts ? "Да" : "Нет",
                Программа = r.CanManageSchedule ? "Да" : "Нет",
                КолвоПользователей = r.Users.Count
            })
            .ToList();

        dataGridView.DataSource = data;

        if (dataGridView.Columns.Contains("Id"))
            dataGridView.Columns["Id"].Visible = false;
    }
}

// Role editing form
public class RoleEditForm : Form
{
    private TextBox txtName;
    private TextBox txtDescription;
    private CheckBox chkManageUsers;
    private CheckBox chkManageRoles;
    private CheckBox chkCreateRightsOwners;
    private CheckBox chkEditRightsOwners;
    private CheckBox chkDeleteRightsOwners;
    private CheckBox chkCreateFilms;
    private CheckBox chkEditFilmBasicInfo;
    private CheckBox chkEditFilmRightsInfo;
    private CheckBox chkDeleteFilms;
    private CheckBox chkManageContacts;
    private CheckBox chkManageSchedule;
    private CheckBox chkViewContent;
    private CheckBox chkViewSchedule;
    private CheckBox chkViewContacts;
    private Button btnOk;
    private Button btnCancel;

    public Role? Role { get; private set; }

    public RoleEditForm(Role? existingRole = null)
    {
        this.Text = existingRole == null ? "Добавление роли" : "Редактирование роли";
        this.Width = 450;
        this.Height = 550;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterParent;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        Role = existingRole;

        int top = 15;
        int labelWidth = 150;
        int controlLeft = 160;

        var lblName = new Label { Text = "Название:", Left = 15, Top = top, Width = labelWidth };
        txtName = new TextBox { Left = controlLeft, Top = top, Width = 250, Text = existingRole?.Name ?? "" };
        top += 30;

        var lblDescription = new Label { Text = "Описание:", Left = 15, Top = top, Width = labelWidth };
        txtDescription = new TextBox { Left = controlLeft, Top = top, Width = 250, Text = existingRole?.Description ?? "" };
        top += 40;

        var lblPermissions = new Label { Text = "Разрешения:", Left = 15, Top = top, Width = 200, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        top += 25;

        chkManageUsers = new CheckBox { Text = "Управление пользователями", Left = 15, Top = top, Width = 250, Checked = existingRole?.CanManageUsers ?? false };
        top += 25;
        chkManageRoles = new CheckBox { Text = "Управление ролями", Left = 15, Top = top, Width = 250, Checked = existingRole?.CanManageRoles ?? false };
        top += 25;
        chkCreateRightsOwners = new CheckBox { Text = "Создание правообладателей", Left = 15, Top = top, Width = 250, Checked = existingRole?.CanCreateRightsOwners ?? false };
        top += 25;
        chkEditRightsOwners = new CheckBox { Text = "Редактирование правообладателей", Left = 15, Top = top, Width = 250, Checked = existingRole?.CanEditRightsOwners ?? false };
        top += 25;
        chkDeleteRightsOwners = new CheckBox { Text = "Удаление правообладателей", Left = 15, Top = top, Width = 250, Checked = existingRole?.CanDeleteRightsOwners ?? false };
        top += 25;
        chkCreateFilms = new CheckBox { Text = "Создание фильмов", Left = 15, Top = top, Width = 250, Checked = existingRole?.CanCreateFilms ?? false };
        top += 25;
        chkEditFilmBasicInfo = new CheckBox { Text = "Ред. основной информации", Left = 15, Top = top, Width = 250, Checked = existingRole?.CanEditFilmBasicInfo ?? false };
        top += 25;
        chkEditFilmRightsInfo = new CheckBox { Text = "Ред. прав на показ", Left = 15, Top = top, Width = 250, Checked = existingRole?.CanEditFilmRightsInfo ?? false };
        top += 25;
        chkDeleteFilms = new CheckBox { Text = "Удаление фильмов", Left = 15, Top = top, Width = 250, Checked = existingRole?.CanDeleteFilms ?? false };
        top += 25;
        chkManageContacts = new CheckBox { Text = "Управление контактами", Left = 15, Top = top, Width = 250, Checked = existingRole?.CanManageContacts ?? false };
        top += 25;
        chkManageSchedule = new CheckBox { Text = "Управление программой", Left = 15, Top = top, Width = 250, Checked = existingRole?.CanManageSchedule ?? false };
        top += 25;
        chkViewContent = new CheckBox { Text = "Просмотр контента", Left = 15, Top = top, Width = 250, Checked = existingRole?.CanViewContent ?? true };
        top += 25;
        chkViewSchedule = new CheckBox { Text = "Просмотр программы", Left = 15, Top = top, Width = 250, Checked = existingRole?.CanViewSchedule ?? true };
        top += 25;
        chkViewContacts = new CheckBox { Text = "Просмотр контактов", Left = 15, Top = top, Width = 250, Checked = existingRole?.CanViewContacts ?? true };
        top += 35;

        btnOk = new Button { Text = "Сохранить", Left = 150, Top = top, Width = 100, DialogResult = DialogResult.OK };
        btnCancel = new Button { Text = "Отмена", Left = 260, Top = top, Width = 100, DialogResult = DialogResult.Cancel };

        btnOk.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название роли", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Role == null)
            {
                Role = new Role();
            }

            Role.Name = txtName.Text;
            Role.Description = txtDescription.Text;
            Role.CanManageUsers = chkManageUsers.Checked;
            Role.CanManageRoles = chkManageRoles.Checked;
            Role.CanCreateRightsOwners = chkCreateRightsOwners.Checked;
            Role.CanEditRightsOwners = chkEditRightsOwners.Checked;
            Role.CanDeleteRightsOwners = chkDeleteRightsOwners.Checked;
            Role.CanCreateFilms = chkCreateFilms.Checked;
            Role.CanEditFilmBasicInfo = chkEditFilmBasicInfo.Checked;
            Role.CanEditFilmRightsInfo = chkEditFilmRightsInfo.Checked;
            Role.CanDeleteFilms = chkDeleteFilms.Checked;
            Role.CanManageContacts = chkManageContacts.Checked;
            Role.CanManageSchedule = chkManageSchedule.Checked;
            Role.CanViewContent = chkViewContent.Checked;
            Role.CanViewSchedule = chkViewSchedule.Checked;
            Role.CanViewContacts = chkViewContacts.Checked;

            this.DialogResult = DialogResult.OK;
            this.Close();
        };

        this.Controls.AddRange(new Control[]
        {
            lblName, txtName, lblDescription, txtDescription, lblPermissions,
            chkManageUsers, chkManageRoles, chkCreateRightsOwners, chkEditRightsOwners,
            chkDeleteRightsOwners, chkCreateFilms, chkEditFilmBasicInfo, chkEditFilmRightsInfo,
            chkDeleteFilms, chkManageContacts, chkManageSchedule, chkViewContent, chkViewSchedule,
            chkViewContacts, btnOk, btnCancel
        });

        this.AcceptButton = btnOk;
        this.CancelButton = btnCancel;
    }
}
