using EquipmentAccounting.Models;
using EquipmentAccounting.Utils;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EquipmentAccounting.Forms.CRUD;

/// <summary>
/// Форма управления телепрограммой.
/// Позволяет планировать показы фильмов, отслеживать их статус и управлять эфиром.
/// Автоматически отмечает показанные записи и уменьшает счётчик показов.
/// </summary>
public class TvScheduleForm : CrudForm<TvScheduleEntry>
{
    /// <summary>Фильтр по дате для отображения записей</summary>
    private DateTimePicker dateFilter = null!;

    /// <summary>Кнопка ручной отметки записи как показанной</summary>
    private Button? btnMarkAired;

    /// <summary>Таймер автоматического обновления (каждую минуту)</summary>
    private System.Windows.Forms.Timer? autoRefreshTimer;

    /// <summary>
    /// Конструктор формы телепрограммы.
    /// Инициализирует фильтры, кнопки и таймер автообновления.
    /// </summary>
    public TvScheduleForm() : base("Телепрограмма",
        showAddButton: SessionManager.CanManageSchedule,
        showEditButton: SessionManager.CanManageSchedule,
        showDeleteButton: SessionManager.CanManageSchedule)
    {
        this.Width = 1100;

        // Панель фильтрации по дате
        var filterPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40
        };

        var lblDate = new Label { Text = "Дата:", Left = 10, Top = 10, AutoSize = true };
        dateFilter = new DateTimePicker
        {
            Left = 50,
            Top = 7,
            Width = 150,
            Format = DateTimePickerFormat.Short
        };
        dateFilter.ValueChanged += (s, e) => LoadData();

        // Кнопка быстрого перехода на сегодня
        var btnToday = new Button { Text = "Сегодня", Left = 210, Top = 6, Width = 80, Height = 25 };
        btnToday.Click += (s, e) => { dateFilter.Value = DateTime.Today; };

        // Кнопка отображения расписания на неделю
        var btnWeek = new Button { Text = "Неделя", Left = 300, Top = 6, Width = 80, Height = 25 };
        btnWeek.Click += (s, e) => LoadWeekSchedule();

        filterPanel.Controls.AddRange(new Control[] { lblDate, dateFilter, btnToday, btnWeek });
        this.Controls.Add(filterPanel);

        // Размещение панели фильтра под заголовком
        this.Controls.SetChildIndex(filterPanel, this.Controls.GetChildIndex(titleLabel));

        // Кнопка "Отметить показанным" для ручной отметки
        if (SessionManager.CanManageSchedule)
        {
            btnMarkAired = new Button
            {
                Text = "Отметить показанным",
                Left = btnRefresh.Right + 10,
                Top = 10,
                Width = 150,
                Height = 30
            };
            btnMarkAired.Click += BtnMarkAired_Click;
            buttonPanel.Controls.Add(btnMarkAired);
        }

        // Обработка прошедших записей (автоматическая отметка и уменьшение счётчика)
        ProcessPastScheduleEntries();

        // Таймер автообновления каждую минуту
        autoRefreshTimer = new System.Windows.Forms.Timer();
        autoRefreshTimer.Interval = 60000; // 60 секунд
        autoRefreshTimer.Tick += (s, e) => ProcessPastScheduleEntries();
        autoRefreshTimer.Start();
    }

    /// <summary>
    /// Загрузка данных для выбранной даты.
    /// </summary>
    protected override void LoadData()
    {
        var selectedDate = dateFilter?.Value.Date ?? DateTime.Today;
        LoadScheduleForDate(selectedDate);
    }

    /// <summary>
    /// Загрузка расписания на конкретную дату с цветовой индикацией статуса.
    /// </summary>
    /// <param name="date">Дата для отображения</param>
    private void LoadScheduleForDate(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var data = context.TvScheduleEntries
            .Include(t => t.Film)
            .ThenInclude(f => f.RightsOwner)
            .Where(t => t.ScheduledDateTime >= startOfDay && t.ScheduledDateTime < endOfDay)
            .OrderBy(t => t.ScheduledDateTime)
            .Select(t => new
            {
                t.Id,
                Время = t.ScheduledDateTime.ToString("HH:mm"),
                Название = t.Film.Title,
                Правообладатель = t.Film.RightsOwner.Name,
                Хронометраж = $"{t.Film.Duration} мин",
                Возраст = t.Film.AgeRestriction,
                ПоказовОсталось = t.Film.ShowCount,
                Статус = t.IsAired ? "Показан" : (t.ScheduledDateTime <= DateTime.Now ? "В эфире" : "Запланирован"),
                Примечания = t.Notes
            })
            .ToList();

        dataGridView.DataSource = data;

        // Скрытие технической колонки
        if (dataGridView.Columns.Contains("Id"))
            dataGridView.Columns["Id"].Visible = false;

        // Цветовая индикация статуса строк
        dataGridView.CellFormatting += FormatCells;
    }

    /// <summary>
    /// Загрузка расписания на неделю вперёд.
    /// </summary>
    private void LoadWeekSchedule()
    {
        var startDate = DateTime.Today;
        var endDate = startDate.AddDays(7);

        var data = context.TvScheduleEntries
            .Include(t => t.Film)
            .ThenInclude(f => f.RightsOwner)
            .Where(t => t.ScheduledDateTime >= startDate && t.ScheduledDateTime < endDate)
            .OrderBy(t => t.ScheduledDateTime)
            .Select(t => new
            {
                t.Id,
                Дата = t.ScheduledDateTime.ToString("dd.MM.yyyy"),
                Время = t.ScheduledDateTime.ToString("HH:mm"),
                Название = t.Film.Title,
                Правообладатель = t.Film.RightsOwner.Name,
                Хронометраж = $"{t.Film.Duration} мин",
                Возраст = t.Film.AgeRestriction,
                ПоказовОсталось = t.Film.ShowCount,
                Статус = t.IsAired ? "Показан" : (t.ScheduledDateTime <= DateTime.Now ? "В эфире" : "Запланирован"),
                Примечания = t.Notes
            })
            .ToList();

        dataGridView.DataSource = data;

        if (dataGridView.Columns.Contains("Id"))
            dataGridView.Columns["Id"].Visible = false;
    }

    /// <summary>
    /// Форматирование ячеек таблицы с цветовой индикацией статуса.
    /// Серый - показан, зелёный - в эфире, красный - нет показов.
    /// </summary>
    private void FormatCells(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0) return;

        var row = dataGridView.Rows[e.RowIndex];
        var statusCell = row.Cells["Статус"];
        var showsCell = row.Cells["ПоказовОсталось"];

        if (statusCell.Value?.ToString() == "Показан")
        {
            // Серый - уже показанные записи
            row.DefaultCellStyle.BackColor = Color.LightGray;
        }
        else if (statusCell.Value?.ToString() == "В эфире")
        {
            // Зелёный - текущий показ
            row.DefaultCellStyle.BackColor = Color.LightGreen;
        }
        else if (int.TryParse(showsCell.Value?.ToString(), out int shows) && shows <= 0)
        {
            // Красный - нет доступных показов
            row.DefaultCellStyle.BackColor = Color.LightCoral;
        }
    }

    /// <summary>
    /// Добавление записи в программу с выбором фильма, даты и времени.
    /// Проверяет действительность прав на показ.
    /// </summary>
    protected override void BtnAdd_Click(object? sender, EventArgs e)
    {
        // Выбор фильма из списка
        using var selectFilmForm = new SelectFilmForm(context);
        if (selectFilmForm.ShowDialog() != DialogResult.OK || selectFilmForm.SelectedFilm == null)
        {
            return;
        }

        var film = selectFilmForm.SelectedFilm;

        // Проверка действительности прав на показ
        if (!film.HasValidRights)
        {
            MessageBox.Show("Невозможно добавить фильм в программу: недействительные права или недостаточно показов.",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Ввод даты показа
        string dateStr = InputDialog.Show("Дата показа (dd.MM.yyyy):", "Добавление в программу",
            dateFilter.Value.ToString("dd.MM.yyyy"));
        if (!DateTime.TryParseExact(dateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
        {
            MessageBox.Show("Неверный формат даты", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Ввод времени показа
        string timeStr = InputDialog.Show("Время показа (HH:mm):", "Добавление в программу", "20:00");
        if (!TimeSpan.TryParse(timeStr, out TimeSpan time))
        {
            MessageBox.Show("Неверный формат времени", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        string notes = InputDialog.Show("Примечания (необязательно):", "Добавление в программу");

        var scheduledDateTime = date.Date.Add(time);

        var entry = new TvScheduleEntry
        {
            FilmId = film.Id,
            ScheduledDateTime = scheduledDateTime,
            IsAired = false,
            Notes = notes,
            CreatedAt = DateTime.Now
        };

        context.TvScheduleEntries.Add(entry);
        context.SaveChanges();
        LoadData();
    }

    /// <summary>
    /// Редактирование записи программы (дата, время, примечания).
    /// Нельзя редактировать уже показанные записи.
    /// </summary>
    protected override void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        var entry = context.TvScheduleEntries.Include(t => t.Film).First(t => t.Id == id);

        // Запрет редактирования показанных записей
        if (entry.IsAired)
        {
            MessageBox.Show("Невозможно редактировать уже показанную запись.",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Редактирование даты
        string dateStr = InputDialog.Show("Дата показа (dd.MM.yyyy):", "Редактирование",
            entry.ScheduledDateTime.ToString("dd.MM.yyyy"));
        if (!DateTime.TryParseExact(dateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
        {
            MessageBox.Show("Неверный формат даты", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Редактирование времени
        string timeStr = InputDialog.Show("Время показа (HH:mm):", "Редактирование",
            entry.ScheduledDateTime.ToString("HH:mm"));
        if (!TimeSpan.TryParse(timeStr, out TimeSpan time))
        {
            MessageBox.Show("Неверный формат времени", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        string notes = InputDialog.Show("Примечания:", "Редактирование", entry.Notes);

        entry.ScheduledDateTime = date.Date.Add(time);
        entry.Notes = notes;

        context.SaveChanges();
        LoadData();
    }

    /// <summary>
    /// Удаление записи из программы с подтверждением.
    /// </summary>
    protected override void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        var entry = context.TvScheduleEntries.Include(t => t.Film).First(t => t.Id == id);

        if (MessageBox.Show($"Удалить запись '{entry.Film.Title}' из программы?", "Подтверждение",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            context.TvScheduleEntries.Remove(entry);
            context.SaveChanges();
            LoadData();
        }
    }

    /// <summary>
    /// Поиск записей программы по названию фильма.
    /// </summary>
    protected override void BtnSearch_Click(object? sender, EventArgs e)
    {
        string searchTerm = InputDialog.Show("Введите часть названия фильма для поиска:", "Поиск");
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            LoadData();
            return;
        }

        var data = context.TvScheduleEntries
            .Include(t => t.Film)
            .ThenInclude(f => f.RightsOwner)
            .Where(t => t.Film.Title.ToLower().Contains(searchTerm.ToLower()))
            .OrderBy(t => t.ScheduledDateTime)
            .Select(t => new
            {
                t.Id,
                Дата = t.ScheduledDateTime.ToString("dd.MM.yyyy"),
                Время = t.ScheduledDateTime.ToString("HH:mm"),
                Название = t.Film.Title,
                Правообладатель = t.Film.RightsOwner.Name,
                Хронометраж = $"{t.Film.Duration} мин",
                Возраст = t.Film.AgeRestriction,
                ПоказовОсталось = t.Film.ShowCount,
                Статус = t.IsAired ? "Показан" : (t.ScheduledDateTime <= DateTime.Now ? "В эфире" : "Запланирован"),
                Примечания = t.Notes
            })
            .ToList();

        dataGridView.DataSource = data;

        if (dataGridView.Columns.Contains("Id"))
            dataGridView.Columns["Id"].Visible = false;
    }

    /// <summary>
    /// Ручная отметка записи как показанной.
    /// </summary>
    private void BtnMarkAired_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        MarkEntryAsAired(id);
        LoadData();
    }

    /// <summary>
    /// Автоматическая обработка прошедших записей.
    /// Отмечает их как показанные и уменьшает счётчик показов.
    /// </summary>
    private void ProcessPastScheduleEntries()
    {
        var now = DateTime.Now;
        var pastEntries = context.TvScheduleEntries
            .Include(t => t.Film)
            .Where(t => !t.IsAired && t.ScheduledDateTime <= now)
            .ToList();

        foreach (var entry in pastEntries)
        {
            MarkEntryAsAired(entry.Id);
        }

        if (pastEntries.Any())
        {
            LoadData();
        }
    }

    /// <summary>
    /// Отметка записи как показанной с уменьшением счётчика показов фильма.
    /// </summary>
    /// <param name="entryId">Идентификатор записи программы</param>
    private void MarkEntryAsAired(int entryId)
    {
        var entry = context.TvScheduleEntries.Include(t => t.Film).FirstOrDefault(t => t.Id == entryId);
        if (entry == null || entry.IsAired) return;

        entry.IsAired = true;

        // Уменьшение счётчика показов фильма
        if (entry.Film.ShowCount > 0)
        {
            entry.Film.ShowCount--;
        }

        context.SaveChanges();
    }

    /// <summary>
    /// Очистка ресурсов при закрытии формы.
    /// Останавливает таймер автообновления.
    /// </summary>
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        autoRefreshTimer?.Stop();
        autoRefreshTimer?.Dispose();
        base.OnFormClosed(e);
    }
}

/// <summary>
/// Вспомогательная форма для выбора фильма при добавлении в программу.
/// Поддерживает поиск и отображает доступность фильма для показа.
/// </summary>
public class SelectFilmForm : Form
{
    /// <summary>Таблица с фильмами</summary>
    private DataGridView gridView;

    /// <summary>Поле поиска</summary>
    private TextBox searchBox;

    /// <summary>Кнопка подтверждения</summary>
    private Button btnOk;

    /// <summary>Кнопка отмены</summary>
    private Button btnCancel;

    /// <summary>Список всех фильмов для фильтрации</summary>
    private List<Film> allFilms;

    /// <summary>Выбранный фильм</summary>
    public Film? SelectedFilm { get; private set; }

    /// <summary>
    /// Конструктор формы выбора фильма.
    /// </summary>
    /// <param name="context">Контекст базы данных</param>
    public SelectFilmForm(Data.AppDbContext context)
    {
        this.Text = "Выберите фильм";
        this.Width = 800;
        this.Height = 500;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterParent;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        // Панель поиска
        var searchPanel = new Panel { Dock = DockStyle.Top, Height = 40 };
        var lblSearch = new Label { Text = "Поиск:", Left = 10, Top = 10, AutoSize = true };
        searchBox = new TextBox { Left = 60, Top = 7, Width = 300 };
        searchBox.TextChanged += (s, e) => FilterFilms();
        searchPanel.Controls.AddRange(new Control[] { lblSearch, searchBox });

        // Таблица фильмов
        gridView = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        // Панель кнопок
        var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 50 };
        btnOk = new Button { Text = "Выбрать", Left = 300, Top = 10, Width = 100, DialogResult = DialogResult.OK };
        btnCancel = new Button { Text = "Отмена", Left = 410, Top = 10, Width = 100, DialogResult = DialogResult.Cancel };

        btnOk.Click += (s, e) =>
        {
            if (gridView.CurrentRow == null)
            {
                MessageBox.Show("Выберите фильм", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int filmId = (int)gridView.CurrentRow.Cells["Id"].Value;
            SelectedFilm = allFilms.First(f => f.Id == filmId);

            this.DialogResult = DialogResult.OK;
            this.Close();
        };

        buttonPanel.Controls.AddRange(new Control[] { btnOk, btnCancel });

        this.Controls.Add(gridView);
        this.Controls.Add(searchPanel);
        this.Controls.Add(buttonPanel);

        // Загрузка списка фильмов
        allFilms = context.Films
            .Include(f => f.RightsOwner)
            .OrderBy(f => f.RightsOwner.Name)
            .ThenBy(f => f.Title)
            .ToList();

        DisplayFilms(allFilms);

        this.AcceptButton = btnOk;
        this.CancelButton = btnCancel;
    }

    /// <summary>
    /// Фильтрация фильмов по поисковому запросу.
    /// </summary>
    private void FilterFilms()
    {
        var searchTerm = searchBox.Text.ToLower();
        var filtered = allFilms
            .Where(f => f.Title.ToLower().Contains(searchTerm) ||
                        f.RightsOwner.Name.ToLower().Contains(searchTerm))
            .ToList();
        DisplayFilms(filtered);
    }

    /// <summary>
    /// Отображение списка фильмов в таблице с цветовой индикацией доступности.
    /// </summary>
    /// <param name="films">Список фильмов для отображения</param>
    private void DisplayFilms(List<Film> films)
    {
        var data = films.Select(f => new
        {
            f.Id,
            Название = f.Title,
            Правообладатель = f.RightsOwner.Name,
            Возраст = f.AgeRestriction,
            Хронометраж = $"{f.Duration} мин",
            ПоказовОсталось = f.ShowCount,
            ПраваДо = f.RightsExpirationDate?.ToString("dd.MM.yyyy") ?? "Не указано",
            Доступен = f.HasValidRights ? "Да" : "Нет"
        }).ToList();

        gridView.DataSource = data;

        if (gridView.Columns.Contains("Id"))
            gridView.Columns["Id"].Visible = false;

        // Цветовая индикация недоступных фильмов
        gridView.CellFormatting += (s, e) =>
        {
            if (e.RowIndex >= 0)
            {
                var row = gridView.Rows[e.RowIndex];
                if (row.Cells["Доступен"].Value?.ToString() == "Нет")
                {
                    row.DefaultCellStyle.BackColor = Color.LightCoral;
                }
            }
        };
    }
}
