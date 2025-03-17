using EquipmentAccounting.Data;

public abstract class CrudForm<T> : Form where T : class, new()
{
    protected Label titleLabel;
    protected DataGridView dataGridView;
    protected Button btnAdd;
    protected Button btnEdit;
    protected Button btnDelete;
    protected Button btnSearch;
    protected Button btnRefresh;

    protected AppDbContext context;

    // При создании передаём название, чтобы показывать в заголовке
    public CrudForm(string tableName)
    {
        this.Width = 800;
        this.Height = 600;

        titleLabel = new Label
        {
            Text = tableName,
            Dock = DockStyle.Top,
            Height = 30,
            Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold),
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        };
        this.Controls.Add(titleLabel);

        dataGridView = new DataGridView
        {
            Left = 10,
            Top = titleLabel.Bottom + 10,
            Width = 760,
            Height = 400,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        btnAdd = new Button { Text = "Добавить", Left = 10, Top = dataGridView.Bottom + 10, Width = 100 };
        btnEdit = new Button { Text = "Редактировать", Left = 120, Top = dataGridView.Bottom + 10, Width = 100 };
        btnDelete = new Button { Text = "Удалить", Left = 230, Top = dataGridView.Bottom + 10, Width = 100 };
        btnSearch = new Button { Text = "Найти", Left = 340, Top = dataGridView.Bottom + 10, Width = 100 };
        btnRefresh = new Button { Text = "Обновить", Left = 450, Top = dataGridView.Bottom + 10, Width = 100 };

        // Добавляем в форму
        this.Controls.Add(dataGridView);
        this.Controls.Add(btnAdd);
        this.Controls.Add(btnEdit);
        this.Controls.Add(btnDelete);
        this.Controls.Add(btnSearch);
        this.Controls.Add(btnRefresh);

        // Инициализируем контекст
        context = new AppDbContext();
        // Загружаем данные
        LoadData();

        // Подписываемся на события
        btnAdd.Click += BtnAdd_Click;
        btnEdit.Click += BtnEdit_Click;
        btnDelete.Click += BtnDelete_Click;
        btnSearch.Click += BtnSearch_Click;
        btnRefresh.Click += BtnRefresh_Click;
    }

    protected abstract void LoadData();
    protected abstract void BtnAdd_Click(object sender, EventArgs e);
    protected abstract void BtnEdit_Click(object sender, EventArgs e);
    protected abstract void BtnDelete_Click(object sender, EventArgs e);
    protected abstract void BtnSearch_Click(object sender, EventArgs e);

    protected virtual void BtnRefresh_Click(object sender, EventArgs e)
    {
        LoadData();
    }
}