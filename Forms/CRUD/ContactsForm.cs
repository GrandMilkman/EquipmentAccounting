using EquipmentAccounting.Models;
using EquipmentAccounting.Utils;

namespace EquipmentAccounting.Forms.CRUD;

/// <summary>
/// Форма управления контактами продавцов прав на контент.
/// Позволяет просматривать, добавлять, редактировать и удалять контактную информацию.
/// </summary>
public class ContactsForm : CrudForm<Contact>
{
    /// <summary>
    /// Конструктор формы контактов.
    /// Кнопки управления доступны только пользователям с правом CanManageContacts.
    /// </summary>
    public ContactsForm() : base("Контакты",
        showAddButton: SessionManager.CanManageContacts,
        showEditButton: SessionManager.CanManageContacts,
        showDeleteButton: SessionManager.CanManageContacts)
    {
        this.Width = 1000;
    }

    /// <summary>
    /// Загрузка списка всех контактов с количеством связанных правообладателей.
    /// </summary>
    protected override void LoadData()
    {
        var data = context.Contacts
            .Select(c => new
            {
                c.Id,
                Компания = c.CompanyName,
                Телефон = c.Phone,
                Email = c.Email,
                Адрес = c.Address,
                КонтактноеЛицо = c.ContactPerson,
                Примечания = c.Notes,
                Правообладателей = c.RightsOwners.Count
            })
            .ToList();

        dataGridView.DataSource = data;

        // Скрытие технической колонки Id
        if (dataGridView.Columns.Contains("Id"))
            dataGridView.Columns["Id"].Visible = false;
    }

    /// <summary>
    /// Добавление нового контакта с запросом всех полей через диалоги.
    /// </summary>
    protected override void BtnAdd_Click(object? sender, EventArgs e)
    {
        // Название компании - обязательное поле
        string companyName = InputDialog.Show("Название компании:", "Добавление контакта");
        if (string.IsNullOrWhiteSpace(companyName)) return;

        // Дополнительная контактная информация
        string phone = InputDialog.Show("Телефон:", "Добавление контакта");
        string email = InputDialog.Show("Email:", "Добавление контакта");
        string address = InputDialog.Show("Адрес:", "Добавление контакта");
        string contactPerson = InputDialog.Show("Контактное лицо:", "Добавление контакта");
        string notes = InputDialog.Show("Примечания:", "Добавление контакта");

        var contact = new Contact
        {
            CompanyName = companyName,
            Phone = phone,
            Email = email,
            Address = address,
            ContactPerson = contactPerson,
            Notes = notes
        };

        context.Contacts.Add(contact);
        context.SaveChanges();
        LoadData();
    }

    /// <summary>
    /// Редактирование выбранного контакта.
    /// </summary>
    protected override void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        var contact = context.Contacts.First(c => c.Id == id);

        // Редактирование названия компании
        string companyName = InputDialog.Show("Название компании:", "Редактирование", contact.CompanyName);
        if (string.IsNullOrWhiteSpace(companyName)) return;

        // Обновление всех полей контакта
        contact.CompanyName = companyName;
        contact.Phone = InputDialog.Show("Телефон:", "Редактирование", contact.Phone);
        contact.Email = InputDialog.Show("Email:", "Редактирование", contact.Email);
        contact.Address = InputDialog.Show("Адрес:", "Редактирование", contact.Address);
        contact.ContactPerson = InputDialog.Show("Контактное лицо:", "Редактирование", contact.ContactPerson);
        contact.Notes = InputDialog.Show("Примечания:", "Редактирование", contact.Notes);

        context.SaveChanges();
        LoadData();
    }

    /// <summary>
    /// Удаление контакта с проверкой на использование правообладателями.
    /// </summary>
    protected override void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        var contact = context.Contacts.First(c => c.Id == id);

        // Проверка: нельзя удалить контакт, если он используется правообладателями
        var usedBy = context.RightsOwners.Where(r => r.ContactId == id).Select(r => r.Name).ToList();
        if (usedBy.Any())
        {
            MessageBox.Show($"Невозможно удалить контакт. Он используется правообладателями:\n{string.Join(", ", usedBy)}",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Подтверждение удаления
        if (MessageBox.Show($"Удалить контакт '{contact.CompanyName}'?", "Подтверждение",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            context.Contacts.Remove(contact);
            context.SaveChanges();
            LoadData();
        }
    }

    /// <summary>
    /// Поиск контактов по названию компании или контактному лицу.
    /// </summary>
    protected override void BtnSearch_Click(object? sender, EventArgs e)
    {
        string searchTerm = InputDialog.Show("Введите часть названия для поиска:", "Поиск");
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            LoadData();
            return;
        }

        // Поиск по названию компании или контактному лицу
        var data = context.Contacts
            .Where(c => c.CompanyName.ToLower().Contains(searchTerm.ToLower()) ||
                        c.ContactPerson.ToLower().Contains(searchTerm.ToLower()))
            .Select(c => new
            {
                c.Id,
                Компания = c.CompanyName,
                Телефон = c.Phone,
                Email = c.Email,
                Адрес = c.Address,
                КонтактноеЛицо = c.ContactPerson,
                Примечания = c.Notes,
                Правообладателей = c.RightsOwners.Count
            })
            .ToList();

        dataGridView.DataSource = data;

        if (dataGridView.Columns.Contains("Id"))
            dataGridView.Columns["Id"].Visible = false;
    }
}
