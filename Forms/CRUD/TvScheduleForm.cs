using EquipmentAccounting.Models;
using EquipmentAccounting.Utils;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EquipmentAccounting.Forms.CRUD;

public class TvScheduleForm : CrudForm<TvScheduleEntry>
{
    private DateTimePicker dateFilter = null!;
    private Button? btnMarkAired;
    private System.Windows.Forms.Timer? autoRefreshTimer;

    public TvScheduleForm() : base("Телепрограмма",
        showAddButton: SessionManager.CanManageSchedule,
        showEditButton: SessionManager.CanManageSchedule,
        showDeleteButton: SessionManager.CanManageSchedule)
    {
        this.Width = 1100;

        // Add date filter
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

        var btnToday = new Button { Text = "Сегодня", Left = 210, Top = 6, Width = 80, Height = 25 };
        btnToday.Click += (s, e) => { dateFilter.Value = DateTime.Today; };

        var btnWeek = new Button { Text = "Неделя", Left = 300, Top = 6, Width = 80, Height = 25 };
        btnWeek.Click += (s, e) => LoadWeekSchedule();

        filterPanel.Controls.AddRange(new Control[] { lblDate, dateFilter, btnToday, btnWeek });
        this.Controls.Add(filterPanel);

        // Reorder: filterPanel should be below titleLabel (lower index = docked later = lower position)
        this.Controls.SetChildIndex(filterPanel, this.Controls.GetChildIndex(titleLabel));

        // Add Mark as Aired button
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

        // Process past schedule entries (auto-mark as aired and decrement show count)
        ProcessPastScheduleEntries();

        // Set up auto-refresh timer (check every minute)
        autoRefreshTimer = new System.Windows.Forms.Timer();
        autoRefreshTimer.Interval = 60000; // 1 minute
        autoRefreshTimer.Tick += (s, e) => ProcessPastScheduleEntries();
        autoRefreshTimer.Start();
    }

    protected override void LoadData()
    {
        var selectedDate = dateFilter?.Value.Date ?? DateTime.Today;
        LoadScheduleForDate(selectedDate);
    }

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

        if (dataGridView.Columns.Contains("Id"))
            dataGridView.Columns["Id"].Visible = false;

        // Color code rows
        dataGridView.CellFormatting += FormatCells;
    }

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

    private void FormatCells(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0) return;

        var row = dataGridView.Rows[e.RowIndex];
        var statusCell = row.Cells["Статус"];
        var showsCell = row.Cells["ПоказовОсталось"];

        if (statusCell.Value?.ToString() == "Показан")
        {
            row.DefaultCellStyle.BackColor = Color.LightGray;
        }
        else if (statusCell.Value?.ToString() == "В эфире")
        {
            row.DefaultCellStyle.BackColor = Color.LightGreen;
        }
        else if (int.TryParse(showsCell.Value?.ToString(), out int shows) && shows <= 0)
        {
            row.DefaultCellStyle.BackColor = Color.LightCoral;
        }
    }

    protected override void BtnAdd_Click(object? sender, EventArgs e)
    {
        // Select film
        using var selectFilmForm = new SelectFilmForm(context);
        if (selectFilmForm.ShowDialog() != DialogResult.OK || selectFilmForm.SelectedFilm == null)
        {
            return;
        }

        var film = selectFilmForm.SelectedFilm;

        // Check if film has valid rights
        if (!film.HasValidRights)
        {
            MessageBox.Show("Невозможно добавить фильм в программу: недействительные права или недостаточно показов.",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Select date and time
        string dateStr = InputDialog.Show("Дата показа (dd.MM.yyyy):", "Добавление в программу",
            dateFilter.Value.ToString("dd.MM.yyyy"));
        if (!DateTime.TryParseExact(dateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
        {
            MessageBox.Show("Неверный формат даты", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

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

    protected override void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        var entry = context.TvScheduleEntries.Include(t => t.Film).First(t => t.Id == id);

        if (entry.IsAired)
        {
            MessageBox.Show("Невозможно редактировать уже показанную запись.",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string dateStr = InputDialog.Show("Дата показа (dd.MM.yyyy):", "Редактирование",
            entry.ScheduledDateTime.ToString("dd.MM.yyyy"));
        if (!DateTime.TryParseExact(dateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
        {
            MessageBox.Show("Неверный формат даты", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

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

    private void BtnMarkAired_Click(object? sender, EventArgs e)
    {
        if (dataGridView.CurrentRow == null) return;

        int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;
        MarkEntryAsAired(id);
        LoadData();
    }

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

    private void MarkEntryAsAired(int entryId)
    {
        var entry = context.TvScheduleEntries.Include(t => t.Film).FirstOrDefault(t => t.Id == entryId);
        if (entry == null || entry.IsAired) return;

        entry.IsAired = true;

        // Decrement show count
        if (entry.Film.ShowCount > 0)
        {
            entry.Film.ShowCount--;
        }

        context.SaveChanges();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        autoRefreshTimer?.Stop();
        autoRefreshTimer?.Dispose();
        base.OnFormClosed(e);
    }
}

// Helper form for selecting a film
public class SelectFilmForm : Form
{
    private DataGridView gridView;
    private TextBox searchBox;
    private Button btnOk;
    private Button btnCancel;
    private List<Film> allFilms;

    public Film? SelectedFilm { get; private set; }

    public SelectFilmForm(Data.AppDbContext context)
    {
        this.Text = "Выберите фильм";
        this.Width = 800;
        this.Height = 500;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterParent;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        var searchPanel = new Panel { Dock = DockStyle.Top, Height = 40 };
        var lblSearch = new Label { Text = "Поиск:", Left = 10, Top = 10, AutoSize = true };
        searchBox = new TextBox { Left = 60, Top = 7, Width = 300 };
        searchBox.TextChanged += (s, e) => FilterFilms();
        searchPanel.Controls.AddRange(new Control[] { lblSearch, searchBox });

        gridView = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

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

        // Load films
        allFilms = context.Films
            .Include(f => f.RightsOwner)
            .OrderBy(f => f.RightsOwner.Name)
            .ThenBy(f => f.Title)
            .ToList();

        DisplayFilms(allFilms);

        this.AcceptButton = btnOk;
        this.CancelButton = btnCancel;
    }

    private void FilterFilms()
    {
        var searchTerm = searchBox.Text.ToLower();
        var filtered = allFilms
            .Where(f => f.Title.ToLower().Contains(searchTerm) ||
                        f.RightsOwner.Name.ToLower().Contains(searchTerm))
            .ToList();
        DisplayFilms(filtered);
    }

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

        // Color code unavailable films
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
