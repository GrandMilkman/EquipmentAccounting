using EquipmentAccounting.Data;

namespace EquipmentAccounting.Forms.CRUD;

public abstract class CrudForm<T> : Form where T : class, new()
{
    protected Label titleLabel = null!;
    protected DataGridView dataGridView = null!;
    protected Button? btnAdd;
    protected Button? btnEdit;
    protected Button? btnDelete;
    protected Button? btnSearch;
    protected Button btnRefresh = null!;
    protected Panel buttonPanel = null!;

    protected AppDbContext context = null!;

    public CrudForm(string tableName, bool showAddButton = true, bool showEditButton = true,
        bool showDeleteButton = true, bool showSearchButton = true)
    {
        this.Width = 900;
        this.Height = 600;
        this.Text = tableName;

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

        buttonPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            Padding = new Padding(10)
        };

        int btnLeft = 10;
        int btnWidth = 120;
        int btnSpacing = 10;

        if (showAddButton)
        {
            btnAdd = new Button { Text = "Добавить", Left = btnLeft, Top = 10, Width = btnWidth, Height = 30 };
            btnAdd.Click += BtnAdd_Click;
            buttonPanel.Controls.Add(btnAdd);
            btnLeft += btnWidth + btnSpacing;
        }

        if (showEditButton)
        {
            btnEdit = new Button { Text = "Редактировать", Left = btnLeft, Top = 10, Width = btnWidth, Height = 30 };
            btnEdit.Click += BtnEdit_Click;
            buttonPanel.Controls.Add(btnEdit);
            btnLeft += btnWidth + btnSpacing;
        }

        if (showDeleteButton)
        {
            btnDelete = new Button { Text = "Удалить", Left = btnLeft, Top = 10, Width = btnWidth, Height = 30 };
            btnDelete.Click += BtnDelete_Click;
            buttonPanel.Controls.Add(btnDelete);
            btnLeft += btnWidth + btnSpacing;
        }

        if (showSearchButton)
        {
            btnSearch = new Button { Text = "Найти", Left = btnLeft, Top = 10, Width = btnWidth, Height = 30 };
            btnSearch.Click += BtnSearch_Click;
            buttonPanel.Controls.Add(btnSearch);
            btnLeft += btnWidth + btnSpacing;
        }

        btnRefresh = new Button { Text = "Обновить", Left = btnLeft, Top = 10, Width = btnWidth, Height = 30 };
        btnRefresh.Click += BtnRefresh_Click;
        buttonPanel.Controls.Add(btnRefresh);

        this.Controls.Add(buttonPanel);
        this.Controls.Add(dataGridView);
        this.Controls.Add(titleLabel);

        context = new AppDbContext();
        LoadData();
    }

    protected abstract void LoadData();

    protected virtual void BtnAdd_Click(object? sender, EventArgs e) { }
    protected virtual void BtnEdit_Click(object? sender, EventArgs e) { }
    protected virtual void BtnDelete_Click(object? sender, EventArgs e) { }
    protected virtual void BtnSearch_Click(object? sender, EventArgs e) { }

    protected virtual void BtnRefresh_Click(object? sender, EventArgs e)
    {
        LoadData();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        context?.Dispose();
        base.OnFormClosed(e);
    }
}
