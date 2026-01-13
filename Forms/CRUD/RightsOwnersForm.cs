using EquipmentAccounting.Models;
using EquipmentAccounting.Utils;
using Microsoft.EntityFrameworkCore;

namespace EquipmentAccounting.Forms.CRUD;

/// <summary>
/// Форма управления правообладателями контента.
/// Позволяет просматривать, добавлять, редактировать и удалять правообладателей.
/// Из этой формы можно перейти к списку фильмов выбранного правообладателя.
/// </summary>
public class RightsOwnersForm : CrudForm<RightsOwner>
{
    /// <summary>Кнопка для открытия списка фильмов правообладателя</summary>
    private Button btnOpenFilms;

    /// <summary>
    /// Конструктор формы правообладателей.
    /// Добавляет кнопку для перехода к фильмам и обработчик двойного клика.
    /// </summary>
    public RightsOwnersForm() : base("Правообладатели",
        showAddButton: SessionManager.CanCreateRightsOwners,
        showEditButton: SessionManager.CanEditRightsOwners,
        showDeleteButton: SessionManager.CanDeleteRightsOwners)
    {
        // Кнопка "Открыть фильмы" для перехода к списку фильмов правообладателя
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

        // Двойной клик по строке также открывает фильмы
        dataGridView.DoubleClick += (s, e) => BtnOpenFilms_Click(s, e);
    }

    /// <summary>
    /// Загрузка списка всех правообладателей с информацией о контактах и количестве фильмов.
    /// </summary>
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

        // Скрытие технической колонки Id
        if (dataGridView.Columns.Contains("Id"))
            dataGridView.Columns["Id"].Visible = false;
    }

    /// <summary>
    /// Добавление нового правообладателя с возможностью выбора контакта.
    /// </summary>
    protected override void BtnAdd_Click(object? sender, EventArgs e)
    {
        // Название правообладателя - обязательное поле
        string name = InputDialog.Show("Введите название правообладателя:", "Добавление");
        if (string.IsNullOrWhiteSpace(name)) return;

        string description = InputDialog.Show("Введите описание:", "Добавление");

        var rightsOwner = new RightsOwner
        {
            Name = name,
            Description = description,
            DateAdded = DateTime.Now
        };

        // Выбор контакта из списка существующих
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

    /// <summary>
    /// Редактирование выбранного правообладателя.
    /// </summary>
    protected override void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        var rightsOwner = context.RightsOwners.Include(r => r.Contact).First(r => r.Id == id);

        // Редактирование названия
        string name = InputDialog.Show("Название:", "Редактирование", rightsOwner.Name);
        if (string.IsNullOrWhiteSpace(name)) return;

        string description = InputDialog.Show("Описание:", "Редактирование", rightsOwner.Description);

        rightsOwner.Name = name;
        rightsOwner.Description = description;

        // Выбор или изменение контакта
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

    /// <summary>
    /// Удаление правообладателя с проверкой наличия фильмов.
    /// Нельзя удалить правообладателя, у которого есть фильмы.
    /// </summary>
    protected override void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        var rightsOwner = context.RightsOwners.Include(r => r.Films).First(r => r.Id == id);

        // Проверка наличия фильмов
        if (rightsOwner.Films.Any())
        {
            MessageBox.Show("Невозможно удалить правообладателя с фильмами. Сначала удалите все фильмы.",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Подтверждение удаления
        if (MessageBox.Show($"Удалить правообладателя '{rightsOwner.Name}'?", "Подтверждение",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            context.RightsOwners.Remove(rightsOwner);
            context.SaveChanges();
            LoadData();
        }
    }

    /// <summary>
    /// Поиск правообладателей по названию.
    /// </summary>
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

    /// <summary>
    /// Открытие формы со списком фильмов выбранного правообладателя.
    /// </summary>
    private void BtnOpenFilms_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        string name = dataGridView.CurrentRow.Cells["Название"].Value?.ToString() ?? "";

        // Создание и отображение формы фильмов
        var filmsForm = new FilmsForm(id, name);
        filmsForm.MdiParent = this.MdiParent;
        filmsForm.Show();
    }
}

/// <summary>
/// Вспомогательная форма для выбора контакта из списка.
/// Используется при добавлении/редактировании правообладателя.
/// </summary>
public class SelectContactForm : Form
{
    /// <summary>Список контактов для выбора</summary>
    private ListBox listBox;

    /// <summary>Кнопка подтверждения выбора</summary>
    private Button btnOk;

    /// <summary>Кнопка отмены</summary>
    private Button btnCancel;

    /// <summary>Кнопка очистки выбора</summary>
    private Button btnClear;

    /// <summary>Выбранный контакт (null если очищен)</summary>
    public Contact? SelectedContact { get; private set; }

    /// <summary>
    /// Конструктор формы выбора контакта.
    /// </summary>
    /// <param name="contacts">Список доступных контактов</param>
    /// <param name="currentContact">Текущий выбранный контакт (для редактирования)</param>
    public SelectContactForm(List<Contact> contacts, Contact? currentContact = null)
    {
        this.Text = "Выберите контакт";
        this.Width = 400;
        this.Height = 300;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterParent;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        // Список контактов
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

        // Выбор текущего контакта, если указан
        if (currentContact != null)
        {
            listBox.SelectedItem = contacts.FirstOrDefault(c => c.Id == currentContact.Id);
        }

        // Кнопки управления
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
