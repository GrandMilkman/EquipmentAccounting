using EquipmentAccounting.Models;
using EquipmentAccounting.Utils;
using System.Globalization;

namespace EquipmentAccounting.Forms.CRUD;

/// <summary>
/// Форма управления фильмами для конкретного правообладателя.
/// Позволяет просматривать, добавлять, редактировать и удалять фильмы.
/// Поддерживает разделение прав на базовую информацию и права показа.
/// </summary>
public class FilmsForm : CrudForm<Film>
{
    /// <summary>Идентификатор правообладателя, фильмы которого отображаются</summary>
    private readonly int _rightsOwnerId;

    /// <summary>Идентификатор фильма для выделения при открытии (из поиска)</summary>
    private readonly int? _highlightFilmId;

    /// <summary>Флаг инициализации обработчика CellFormatting (для предотвращения утечки памяти)</summary>
    private bool _cellFormattingInitialized;

    /// <summary>
    /// Конструктор формы фильмов.
    /// </summary>
    /// <param name="rightsOwnerId">Идентификатор правообладателя</param>
    /// <param name="rightsOwnerName">Название правообладателя для заголовка</param>
    /// <param name="highlightFilmId">Опциональный ID фильма для выделения (при поиске)</param>
    public FilmsForm(int rightsOwnerId, string rightsOwnerName, int? highlightFilmId = null) : base($"Фильмы: {rightsOwnerName}",
        showAddButton: SessionManager.CanCreateFilms,
        showEditButton: SessionManager.CanEditAnyFilmInfo,
        showDeleteButton: SessionManager.CanDeleteFilms)
    {
        _rightsOwnerId = rightsOwnerId;
        _highlightFilmId = highlightFilmId;
        this.Width = 1100;
        
        // Обеспечиваем загрузку данных после полной инициализации формы
        this.Load += FilmsForm_Load;
    }

    /// <summary>
    /// Обработчик загрузки формы - перезагружает данные для гарантии их отображения.
    /// </summary>
    private void FilmsForm_Load(object? sender, EventArgs e)
    {
        // Перезагружаем данные после полной загрузки формы
        LoadData();
    }

    /// <summary>
    /// Загрузка списка фильмов правообладателя с цветовой индикацией статуса прав.
    /// Красный цвет - недействительные права, жёлтый - мало показов осталось.
    /// </summary>
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

        // Скрытие колонки Id от пользователя
        if (dataGridView.Columns.Contains("Id"))
            dataGridView.Columns["Id"].Visible = false;

        // Подписка на CellFormatting только один раз (предотвращение утечки памяти)
        if (!_cellFormattingInitialized)
        {
            dataGridView.CellFormatting += DataGridView_CellFormatting;
            _cellFormattingInitialized = true;
        }

        // Выделение фильма, если был указан ID для подсветки (при поиске)
        if (_highlightFilmId.HasValue)
        {
            HighlightFilm(_highlightFilmId.Value);
        }
    }

    /// <summary>
    /// Обработчик форматирования ячеек для цветовой индикации статуса прав.
    /// Красный цвет - права недействительны, жёлтый - мало показов осталось.
    /// </summary>
    private void DataGridView_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
        {
            var row = dataGridView.Rows[e.RowIndex];
            var validCell = row.Cells["ПраваДействительны"];
            if (validCell.Value?.ToString() == "Нет")
            {
                // Красный цвет - права недействительны
                row.DefaultCellStyle.BackColor = Color.LightCoral;
            }
            else
            {
                var showsCell = row.Cells["Показов"];
                if (int.TryParse(showsCell.Value?.ToString(), out int shows) && shows <= 5)
                {
                    // Жёлтый цвет - мало показов осталось (5 и меньше)
                    row.DefaultCellStyle.BackColor = Color.LightYellow;
                }
            }
        }
    }

    /// <summary>
    /// Выделение и прокрутка к указанному фильму в таблице.
    /// Сохраняет цветовую индикацию статуса прав (красный/жёлтый).
    /// </summary>
    /// <param name="filmId">Идентификатор фильма для выделения</param>
    private void HighlightFilm(int filmId)
    {
        foreach (DataGridViewRow row in dataGridView.Rows)
        {
            if (row.Cells["Id"].Value != null && (int)row.Cells["Id"].Value == filmId)
            {
                // Выделяем строку
                row.Selected = true;
                dataGridView.CurrentCell = row.Cells[1]; // Первая видимая ячейка

                // Прокрутка к строке с защитой от исключения (если грид ещё не отрисован)
                try
                {
                    if (row.Index >= 0 && row.Index < dataGridView.Rows.Count)
                    {
                        dataGridView.FirstDisplayedScrollingRowIndex = row.Index;
                    }
                }
                catch (InvalidOperationException)
                {
                    // Грид ещё не готов к прокрутке - игнорируем
                }

                // Подсвечиваем голубым только если нет предупреждений (красный/жёлтый)
                var validCell = row.Cells["ПраваДействительны"];
                var showsCell = row.Cells["Показов"];
                bool hasInvalidRights = validCell.Value?.ToString() == "Нет";
                bool hasLowShows = int.TryParse(showsCell.Value?.ToString(), out int shows) && shows <= 5;

                // Не перезаписываем цвета предупреждений - они важнее для бизнес-логики
                if (!hasInvalidRights && !hasLowShows)
                {
                    row.DefaultCellStyle.BackColor = Color.LightSkyBlue;
                }
                break;
            }
        }
    }

    /// <summary>
    /// Добавление нового фильма. Поля зависят от прав пользователя:
    /// базовая информация (название, возраст, хронометраж, путь) и
    /// информация о правах (даты, количество показов).
    /// </summary>
    protected override void BtnAdd_Click(object? sender, EventArgs e)
    {
        // Базовая информация о фильме
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

        // Если пользователь может редактировать информацию о правах, запрашиваем её
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

    /// <summary>
    /// Редактирование выбранного фильма. Доступные поля зависят от прав пользователя.
    /// </summary>
    protected override void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        var film = context.Films.First(f => f.Id == id);

        // Редактирование базовой информации (для инженера видеомонтажа)
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

        // Редактирование информации о правах (для специалиста)
        if (SessionManager.CanEditFilmRightsInfo)
        {
            // Специалист также может редактировать возрастное ограничение
            if (!SessionManager.CanEditFilmBasicInfo)
            {
                string age = InputDialog.Show("Возрастное ограничение:", "Редактирование", film.AgeRestriction);
                if (!string.IsNullOrWhiteSpace(age)) film.AgeRestriction = age;
            }

            // Дата закупки прав
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

            // Дата окончания прав
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

            // Количество разрешённых показов
            string showCountStr = InputDialog.Show("Количество показов:", "Редактирование", film.ShowCount.ToString());
            if (int.TryParse(showCountStr, out int showCount))
            {
                film.ShowCount = showCount;
            }
        }

        context.SaveChanges();
        LoadData();
    }

    /// <summary>
    /// Удаление выбранного фильма с проверкой наличия записей в телепрограмме.
    /// </summary>
    protected override void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        var film = context.Films.First(f => f.Id == id);

        // Проверка: нельзя удалить фильм, если есть записи в телепрограмме
        var hasSchedule = context.TvScheduleEntries.Any(s => s.FilmId == id);
        if (hasSchedule)
        {
            MessageBox.Show("Невозможно удалить фильм с записями в программе. Сначала удалите записи из программы.",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Подтверждение удаления
        if (MessageBox.Show($"Удалить фильм '{film.Title}'?", "Подтверждение",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            context.Films.Remove(film);
            context.SaveChanges();
            LoadData();
        }
    }

    /// <summary>
    /// Поиск фильмов по названию среди фильмов текущего правообладателя.
    /// </summary>
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
