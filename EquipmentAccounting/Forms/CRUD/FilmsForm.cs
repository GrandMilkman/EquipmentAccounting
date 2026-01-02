using EquipmentAccounting.Models;
using EquipmentAccounting.Utils;
using System.Globalization;

namespace EquipmentAccounting.Forms.CRUD;

public class FilmsForm : CrudForm<Film>
{
    private readonly int _rightsOwnerId;

    public FilmsForm(int rightsOwnerId, string rightsOwnerName) : base($"Фильмы: {rightsOwnerName}",
        showAddButton: SessionManager.CanCreateFilms,
        showEditButton: SessionManager.CanEditAnyFilmInfo,
        showDeleteButton: SessionManager.CanDeleteFilms)
    {
        _rightsOwnerId = rightsOwnerId;
        this.Width = 1100;
    }

    protected override void LoadData()
    {
        var films = context.Films
            .Where(f => f.RightsOwnerId == _rightsOwnerId)
            .OrderBy(f => f.Id)
            .ToList();

        var data = films.Select((f, index) => new
        {
            f.Id,
            Номер = index + 1,
            Название = f.Title,
            Возраст = f.AgeRestriction,
            Хронометраж = $"{f.Duration} мин",
            ДатаЗакупки = f.PurchaseDate?.ToString("dd.MM.yyyy") ?? "Не указана",
            ОкончаниеПрав = f.RightsExpirationDate?.ToString("dd.MM.yyyy") ?? "Не указано",
            Показов = f.ShowCount,
            Путь = f.FilePath,
            ПраваДействительны = f.HasValidRights ? "Да" : "Нет"
        }).ToList();

        dataGridView.DataSource = data;

        if (dataGridView.Columns.Contains("Id"))
            dataGridView.Columns["Id"].Visible = false;

        // Color code rows based on rights validity
        dataGridView.CellFormatting += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var row = dataGridView.Rows[e.RowIndex];
                var validCell = row.Cells["ПраваДействительны"];
                if (validCell.Value?.ToString() == "Нет")
                {
                    row.DefaultCellStyle.BackColor = Color.LightCoral;
                }
                else
                {
                    var showsCell = row.Cells["Показов"];
                    if (int.TryParse(showsCell.Value?.ToString(), out int shows) && shows <= 5)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightYellow;
                    }
                }
            }
        };
    }

    protected override void BtnAdd_Click(object? sender, EventArgs e)
    {
        string title = InputDialog.Show("Название произведения:", "Добавление фильма");
        if (string.IsNullOrWhiteSpace(title)) return;

        string age = InputDialog.Show("Возрастное ограничение (напр. 12+):", "Добавление фильма", "0+");
        string durStr = InputDialog.Show("Хронометраж (мин):", "Добавление фильма");
        string filePath = InputDialog.Show("Путь к файлу:", "Добавление фильма");

        if (!int.TryParse(durStr, out int duration))
        {
            MessageBox.Show("Неверный формат хронометража", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var film = new Film
        {
            Title = title,
            AgeRestriction = age,
            Duration = duration,
            FilePath = filePath,
            DateAdded = DateTime.Now,
            RightsOwnerId = _rightsOwnerId,
            ShowCount = 0
        };

        // If user can edit rights info, ask for it
        if (SessionManager.CanEditFilmRightsInfo)
        {
            string purchaseDateStr = InputDialog.Show("Дата закупки (dd.MM.yyyy):", "Добавление фильма");
            if (!string.IsNullOrWhiteSpace(purchaseDateStr) && DateTime.TryParseExact(purchaseDateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime purchaseDate))
            {
                film.PurchaseDate = purchaseDate;
            }

            string expirationDateStr = InputDialog.Show("Дата окончания прав (dd.MM.yyyy):", "Добавление фильма");
            if (!string.IsNullOrWhiteSpace(expirationDateStr) && DateTime.TryParseExact(expirationDateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime expirationDate))
            {
                film.RightsExpirationDate = expirationDate;
            }

            string showCountStr = InputDialog.Show("Количество показов:", "Добавление фильма", "0");
            if (int.TryParse(showCountStr, out int showCount))
            {
                film.ShowCount = showCount;
            }
        }

        context.Films.Add(film);
        context.SaveChanges();
        LoadData();
    }

    protected override void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        var film = context.Films.First(f => f.Id == id);

        // Basic info (for video editor)
        if (SessionManager.CanEditFilmBasicInfo)
        {
            string title = InputDialog.Show("Название:", "Редактирование", film.Title);
            if (!string.IsNullOrWhiteSpace(title)) film.Title = title;

            string age = InputDialog.Show("Возрастное ограничение:", "Редактирование", film.AgeRestriction);
            if (!string.IsNullOrWhiteSpace(age)) film.AgeRestriction = age;

            string durStr = InputDialog.Show("Хронометраж (мин):", "Редактирование", film.Duration.ToString());
            if (int.TryParse(durStr, out int duration)) film.Duration = duration;

            string filePath = InputDialog.Show("Путь к файлу:", "Редактирование", film.FilePath);
            if (!string.IsNullOrWhiteSpace(filePath)) film.FilePath = filePath;
        }

        // Rights info (for specialist)
        if (SessionManager.CanEditFilmRightsInfo)
        {
            // Specialist can also edit age restriction
            if (!SessionManager.CanEditFilmBasicInfo)
            {
                string age = InputDialog.Show("Возрастное ограничение:", "Редактирование", film.AgeRestriction);
                if (!string.IsNullOrWhiteSpace(age)) film.AgeRestriction = age;
            }

            string purchaseDateStr = InputDialog.Show("Дата закупки (dd.MM.yyyy):", "Редактирование",
                film.PurchaseDate?.ToString("dd.MM.yyyy") ?? "");
            if (!string.IsNullOrWhiteSpace(purchaseDateStr) && DateTime.TryParseExact(purchaseDateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime purchaseDate))
            {
                film.PurchaseDate = purchaseDate;
            }
            else if (string.IsNullOrWhiteSpace(purchaseDateStr))
            {
                film.PurchaseDate = null;
            }

            string expirationDateStr = InputDialog.Show("Дата окончания прав (dd.MM.yyyy):", "Редактирование",
                film.RightsExpirationDate?.ToString("dd.MM.yyyy") ?? "");
            if (!string.IsNullOrWhiteSpace(expirationDateStr) && DateTime.TryParseExact(expirationDateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime expirationDate))
            {
                film.RightsExpirationDate = expirationDate;
            }
            else if (string.IsNullOrWhiteSpace(expirationDateStr))
            {
                film.RightsExpirationDate = null;
            }

            string showCountStr = InputDialog.Show("Количество показов:", "Редактирование", film.ShowCount.ToString());
            if (int.TryParse(showCountStr, out int showCount))
            {
                film.ShowCount = showCount;
            }
        }

        context.SaveChanges();
        LoadData();
    }

    protected override void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        var film = context.Films.First(f => f.Id == id);

        // Check if film has schedule entries
        var hasSchedule = context.TvScheduleEntries.Any(s => s.FilmId == id);
        if (hasSchedule)
        {
            MessageBox.Show("Невозможно удалить фильм с записями в программе. Сначала удалите записи из программы.",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show($"Удалить фильм '{film.Title}'?", "Подтверждение",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            context.Films.Remove(film);
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

        var films = context.Films
            .Where(f => f.RightsOwnerId == _rightsOwnerId && f.Title.ToLower().Contains(searchTerm.ToLower()))
            .OrderBy(f => f.Id)
            .ToList();

        var data = films.Select((f, index) => new
        {
            f.Id,
            Номер = index + 1,
            Название = f.Title,
            Возраст = f.AgeRestriction,
            Хронометраж = $"{f.Duration} мин",
            ДатаЗакупки = f.PurchaseDate?.ToString("dd.MM.yyyy") ?? "Не указана",
            ОкончаниеПрав = f.RightsExpirationDate?.ToString("dd.MM.yyyy") ?? "Не указано",
            Показов = f.ShowCount,
            Путь = f.FilePath,
            ПраваДействительны = f.HasValidRights ? "Да" : "Нет"
        }).ToList();

        dataGridView.DataSource = data;

        if (dataGridView.Columns.Contains("Id"))
            dataGridView.Columns["Id"].Visible = false;
    }
}
