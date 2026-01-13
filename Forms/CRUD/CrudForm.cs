using EquipmentAccounting.Data;

namespace EquipmentAccounting.Forms.CRUD;

/// <summary>
/// Абстрактный базовый класс для форм управления данными (CRUD).
/// Предоставляет стандартный интерфейс с таблицей данных и кнопками действий.
/// Наследники реализуют конкретную логику работы с сущностями.
/// </summary>
/// <typeparam name="T">Тип сущности для управления</typeparam>
public abstract class CrudForm<T> : Form where T : class, new()
{
    /// <summary>Заголовок формы с названием таблицы</summary>
    protected Label titleLabel = null!;

    /// <summary>Таблица для отображения данных</summary>
    protected DataGridView dataGridView = null!;

    /// <summary>Кнопка добавления новой записи</summary>
    protected Button? btnAdd;

    /// <summary>Кнопка редактирования выбранной записи</summary>
    protected Button? btnEdit;

    /// <summary>Кнопка удаления выбранной записи</summary>
    protected Button? btnDelete;

    /// <summary>Кнопка поиска записей</summary>
    protected Button? btnSearch;

    /// <summary>Кнопка обновления данных в таблице</summary>
    protected Button btnRefresh = null!;

    /// <summary>Панель с кнопками управления</summary>
    protected Panel buttonPanel = null!;

    /// <summary>Контекст базы данных для работы с сущностями</summary>
    protected AppDbContext context = null!;

    /// <summary>
    /// Конструктор базовой CRUD-формы.
    /// Создаёт стандартный интерфейс с заголовком, таблицей и панелью кнопок.
    /// </summary>
    /// <param name="tableName">Название таблицы/сущности для отображения в заголовке</param>
    /// <param name="showAddButton">Показывать ли кнопку добавления</param>
    /// <param name="showEditButton">Показывать ли кнопку редактирования</param>
    /// <param name="showDeleteButton">Показывать ли кнопку удаления</param>
    /// <param name="showSearchButton">Показывать ли кнопку поиска</param>
    public CrudForm(string tableName, bool showAddButton = true, bool showEditButton = true,
        bool showDeleteButton = true, bool showSearchButton = true)
    {
        this.Width = 900;
        this.Height = 600;
        this.Text = tableName;

        // Заголовок формы с синим фоном
        titleLabel = new Label
        {
            Text = tableName,
            Dock = DockStyle.Top,
            Height = 40,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White
        };

        // Таблица данных с настройками отображения
        dataGridView = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None
        };

        // Панель с кнопками внизу формы
        buttonPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            Padding = new Padding(10)
        };

        // Позиционирование кнопок
        int btnLeft = 10;
        int btnWidth = 120;
        int btnSpacing = 10;

        // Кнопка "Добавить"
        if (showAddButton)
        {
            btnAdd = new Button { Text = "Добавить", Left = btnLeft, Top = 10, Width = btnWidth, Height = 30 };
            btnAdd.Click += BtnAdd_Click;
            buttonPanel.Controls.Add(btnAdd);
            btnLeft += btnWidth + btnSpacing;
        }

        // Кнопка "Редактировать"
        if (showEditButton)
        {
            btnEdit = new Button { Text = "Редактировать", Left = btnLeft, Top = 10, Width = btnWidth, Height = 30 };
            btnEdit.Click += BtnEdit_Click;
            buttonPanel.Controls.Add(btnEdit);
            btnLeft += btnWidth + btnSpacing;
        }

        // Кнопка "Удалить"
        if (showDeleteButton)
        {
            btnDelete = new Button { Text = "Удалить", Left = btnLeft, Top = 10, Width = btnWidth, Height = 30 };
            btnDelete.Click += BtnDelete_Click;
            buttonPanel.Controls.Add(btnDelete);
            btnLeft += btnWidth + btnSpacing;
        }

        // Кнопка "Найти"
        if (showSearchButton)
        {
            btnSearch = new Button { Text = "Найти", Left = btnLeft, Top = 10, Width = btnWidth, Height = 30 };
            btnSearch.Click += BtnSearch_Click;
            buttonPanel.Controls.Add(btnSearch);
            btnLeft += btnWidth + btnSpacing;
        }

        // Кнопка "Обновить" - всегда отображается
        btnRefresh = new Button { Text = "Обновить", Left = btnLeft, Top = 10, Width = btnWidth, Height = 30 };
        btnRefresh.Click += BtnRefresh_Click;
        buttonPanel.Controls.Add(btnRefresh);

        // Добавление элементов на форму
        this.Controls.Add(buttonPanel);
        this.Controls.Add(dataGridView);
        this.Controls.Add(titleLabel);

        // Инициализация контекста БД и загрузка данных
        context = new AppDbContext();
        LoadData();
    }

    /// <summary>
    /// Загрузка данных в таблицу. Реализуется в наследниках.
    /// </summary>
    protected abstract void LoadData();

    /// <summary>
    /// Обработчик кнопки "Добавить". Переопределяется в наследниках.
    /// </summary>
    protected virtual void BtnAdd_Click(object? sender, EventArgs e) { }

    /// <summary>
    /// Обработчик кнопки "Редактировать". Переопределяется в наследниках.
    /// </summary>
    protected virtual void BtnEdit_Click(object? sender, EventArgs e) { }

    /// <summary>
    /// Обработчик кнопки "Удалить". Переопределяется в наследниках.
    /// </summary>
    protected virtual void BtnDelete_Click(object? sender, EventArgs e) { }

    /// <summary>
    /// Обработчик кнопки "Найти". Переопределяется в наследниках.
    /// </summary>
    protected virtual void BtnSearch_Click(object? sender, EventArgs e) { }

    /// <summary>
    /// Обработчик кнопки "Обновить". Перезагружает данные в таблицу.
    /// </summary>
    protected virtual void BtnRefresh_Click(object? sender, EventArgs e)
    {
        LoadData();
    }

    /// <summary>
    /// Освобождение ресурсов при закрытии формы.
    /// Закрывает контекст базы данных.
    /// </summary>
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        context?.Dispose();
        base.OnFormClosed(e);
    }
}
