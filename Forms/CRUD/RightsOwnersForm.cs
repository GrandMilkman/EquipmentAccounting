using EquipmentAccounting.Models;
using EquipmentAccounting.Utils;
using Microsoft.EntityFrameworkCore;

namespace EquipmentAccounting.Forms.CRUD;

public class RightsOwnersForm : CrudForm<RightsOwner>
{
    private Button btnOpenFilms;

    public RightsOwnersForm() : base("Правообладатели",
        showAddButton: SessionManager.CanCreateRightsOwners,
        showEditButton: SessionManager.CanEditRightsOwners,
        showDeleteButton: SessionManager.CanDeleteRightsOwners)
    {
        // Add button to open films for selected rights owner
        btnOpenFilms = new Button
        {
            Text = "Открыть фильмы",
            Left = btnRefresh.Right + 10,
            Top = 10,
            Width = 120,
            Height = 30
        };
        btnOpenFilms.Click += BtnOpenFilms_Click;
        buttonPanel.Controls.Add(btnOpenFilms);

        dataGridView.DoubleClick += (s, e) => BtnOpenFilms_Click(s, e);
    }

    protected override void LoadData()
    {
        var data = context.RightsOwners
            .Include(r => r.Contact)
            .Select(r => new
            {
                r.Id,
                Название = r.Name,
                Описание = r.Description,
                Контакт = r.Contact != null ? r.Contact.CompanyName : "Не указан",
                ДатаДобавления = r.DateAdded.ToString("dd.MM.yyyy"),
                КоличествоФильмов = r.Films.Count
            })
            .ToList();

        dataGridView.DataSource = data;

        if (dataGridView.Columns.Contains("Id"))
            dataGridView.Columns["Id"].Visible = false;
    }

    protected override void BtnAdd_Click(object? sender, EventArgs e)
    {
        string name = InputDialog.Show("Введите название правообладателя:", "Добавление");
        if (string.IsNullOrWhiteSpace(name)) return;

        string description = InputDialog.Show("Введите описание:", "Добавление");

        var rightsOwner = new RightsOwner
        {
            Name = name,
            Description = description,
            DateAdded = DateTime.Now
        };

        // Select contact
        var contacts = context.Contacts.ToList();
        if (contacts.Any())
        {
            using var selectForm = new SelectContactForm(contacts);
            if (selectForm.ShowDialog() == DialogResult.OK && selectForm.SelectedContact != null)
            {
                rightsOwner.ContactId = selectForm.SelectedContact.Id;
            }
        }

        context.RightsOwners.Add(rightsOwner);
        context.SaveChanges();
        LoadData();
    }

    protected override void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        var rightsOwner = context.RightsOwners.Include(r => r.Contact).First(r => r.Id == id);

        string name = InputDialog.Show("Название:", "Редактирование", rightsOwner.Name);
        if (string.IsNullOrWhiteSpace(name)) return;

        string description = InputDialog.Show("Описание:", "Редактирование", rightsOwner.Description);

        rightsOwner.Name = name;
        rightsOwner.Description = description;

        // Select contact
        var contacts = context.Contacts.ToList();
        if (contacts.Any())
        {
            using var selectForm = new SelectContactForm(contacts, rightsOwner.Contact);
            if (selectForm.ShowDialog() == DialogResult.OK)
            {
                rightsOwner.ContactId = selectForm.SelectedContact?.Id;
            }
        }

        context.SaveChanges();
        LoadData();
    }

    protected override void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        var rightsOwner = context.RightsOwners.Include(r => r.Films).First(r => r.Id == id);

        if (rightsOwner.Films.Any())
        {
            MessageBox.Show("Невозможно удалить правообладателя с фильмами. Сначала удалите все фильмы.",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show($"Удалить правообладателя '{rightsOwner.Name}'?", "Подтверждение",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            context.RightsOwners.Remove(rightsOwner);
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

        var data = context.RightsOwners
            .Include(r => r.Contact)
            .Where(r => r.Name.ToLower().Contains(searchTerm.ToLower()))
            .Select(r => new
            {
                r.Id,
                Название = r.Name,
                Описание = r.Description,
                Контакт = r.Contact != null ? r.Contact.CompanyName : "Не указан",
                ДатаДобавления = r.DateAdded.ToString("dd.MM.yyyy"),
                КоличествоФильмов = r.Films.Count
            })
            .ToList();

        dataGridView.DataSource = data;

        if (dataGridView.Columns.Contains("Id"))
            dataGridView.Columns["Id"].Visible = false;
    }

    private void BtnOpenFilms_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        string name = dataGridView.CurrentRow.Cells["Название"].Value?.ToString() ?? "";

        var filmsForm = new FilmsForm(id, name);
        filmsForm.MdiParent = this.MdiParent;
        filmsForm.Show();
    }
}

// Helper form for selecting contact
public class SelectContactForm : Form
{
    private ListBox listBox;
    private Button btnOk;
    private Button btnCancel;
    private Button btnClear;

    public Contact? SelectedContact { get; private set; }

    public SelectContactForm(List<Contact> contacts, Contact? currentContact = null)
    {
        this.Text = "Выберите контакт";
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

        foreach (var contact in contacts)
        {
            listBox.Items.Add(contact);
        }
        listBox.DisplayMember = "CompanyName";

        if (currentContact != null)
        {
            listBox.SelectedItem = contacts.FirstOrDefault(c => c.Id == currentContact.Id);
        }

        btnOk = new Button { Text = "ОК", Left = 100, Top = 220, Width = 80, DialogResult = DialogResult.OK };
        btnCancel = new Button { Text = "Отмена", Left = 190, Top = 220, Width = 80, DialogResult = DialogResult.Cancel };
        btnClear = new Button { Text = "Очистить", Left = 280, Top = 220, Width = 80 };
        btnClear.Click += (s, e) => listBox.SelectedItem = null;

        btnOk.Click += (s, e) =>
        {
            SelectedContact = listBox.SelectedItem as Contact;
            this.DialogResult = DialogResult.OK;
            this.Close();
        };

        this.Controls.Add(listBox);
        this.Controls.Add(btnOk);
        this.Controls.Add(btnCancel);
        this.Controls.Add(btnClear);

        this.AcceptButton = btnOk;
        this.CancelButton = btnCancel;
    }
}
