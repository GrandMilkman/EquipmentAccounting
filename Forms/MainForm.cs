using EquipmentAccounting.Data;
using EquipmentAccounting.Forms.CRUD;
using EquipmentAccounting.Models;
using EquipmentAccounting.Utils;
using Microsoft.EntityFrameworkCore;

namespace EquipmentAccounting.Forms;

/// <summary>
/// Главная форма приложения (MDI-контейнер).
/// Содержит меню навигации, панель заголовка с логотипом и информацией о пользователе,
/// а также дашборд с кнопками быстрого доступа и строкой поиска фильмов.
/// </summary>
public class MainForm : Form
{
    // Элементы меню и заголовка
    private MenuStrip menuStrip = null!;
    private Panel headerPanel = null!;
    private PictureBox logoBox = null!;
    private Label userInfoLabel = null!;

    // Элементы дашборда
    private Panel dashboardPanel = null!;
    private TextBox searchBox = null!;
    private Button searchButton = null!;
    private FlowLayoutPanel buttonsPanel = null!;
    private ListBox searchResultsListBox = null!;

    /// <summary>
    /// Конструктор главной формы. Инициализирует MDI-контейнер,
    /// заголовок, меню и дашборд с кнопками навигации.
    /// </summary>
    public MainForm()
    {
        this.IsMdiContainer = true;
        this.WindowState = FormWindowState.Maximized;
        this.Text = "Учёт контента телеканала";

        InitializeHeader();
        InitializeMenu();
        InitializeDashboard();

        // Обработка изменения размера окна для адаптивного интерфейса
        this.Resize += MainForm_Resize;
    }

    /// <summary>
    /// Инициализация панели заголовка с логотипом и информацией о текущем пользователе.
    /// </summary>
    private void InitializeHeader()
    {
        headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.FromArgb(45, 45, 48)
        };

        // Логотип телеканала
        logoBox = new PictureBox
        {
            Left = 10,
            Top = 5,
            Width = 100,
            Height = 50,
            SizeMode = PictureBoxSizeMode.Zoom
        };

        // Загрузка логотипа из файла, если он существует
        string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
        if (File.Exists(logoPath))
        {
            logoBox.Image = Image.FromFile(logoPath);
        }
        else
        {
            logoBox.BackColor = Color.Gray;
        }

        // Информация о текущем пользователе и его роли (с защитой от null)
        string userName = SessionManager.CurrentUser?.Login ?? "Не авторизован";
        string roleName = SessionManager.CurrentRole?.Name ?? "Не назначена";
        userInfoLabel = new Label
        {
            Text = $"Пользователь: {userName} | Роль: {roleName}",
            ForeColor = Color.White,
            AutoSize = true,
            Font = new Font("Segoe UI", 10),
            Top = 20
        };
        userInfoLabel.Left = this.Width - userInfoLabel.Width - 250;
        this.Resize += (s, e) => userInfoLabel.Left = this.Width - 350;

        headerPanel.Controls.Add(logoBox);
        headerPanel.Controls.Add(userInfoLabel);
        this.Controls.Add(headerPanel);
    }

    /// <summary>
    /// Инициализация главного меню с учётом прав доступа текущего пользователя.
    /// Меню "Программа" скрыто, но функционал сохранён.
    /// </summary>
    private void InitializeMenu()
    {
        menuStrip = new MenuStrip();
        menuStrip.Dock = DockStyle.Top;
        this.MainMenuStrip = menuStrip;

        // Меню "Файл" - доступно всем пользователям
        var menuFile = new ToolStripMenuItem("Файл");
        menuFile.DropDownItems.Add(new ToolStripMenuItem("Выход", null, (s, e) => this.Close()));

        // Меню "Контент" - доступно пользователям с правом просмотра контента
        ToolStripMenuItem? menuContent = null;
        if (SessionManager.CanViewContent)
        {
            menuContent = new ToolStripMenuItem("Контент");
            menuContent.DropDownItems.Add(new ToolStripMenuItem("Правообладатели", null, (s, e) => OpenChildForm(new RightsOwnersForm())));
        }

        // Меню "Контакты" - доступно пользователям с правом просмотра контактов
        ToolStripMenuItem? menuContacts = null;
        if (SessionManager.CanViewContacts)
        {
            menuContacts = new ToolStripMenuItem("Контакты");
            menuContacts.DropDownItems.Add(new ToolStripMenuItem("Список контактов", null, (s, e) => OpenChildForm(new ContactsForm())));
        }

        // Меню "Программа" (Телепрограмма) - СКРЫТО по требованию, но функционал сохранён
        // Для восстановления раскомментируйте следующий блок:
        /*
        ToolStripMenuItem? menuSchedule = null;
        if (SessionManager.CanViewSchedule)
        {
            menuSchedule = new ToolStripMenuItem("Программа");
            menuSchedule.DropDownItems.Add(new ToolStripMenuItem("Телепрограмма", null, (s, e) => OpenChildForm(new TvScheduleForm())));
        }
        */

        // Меню "Администрирование" - доступно администраторам
        ToolStripMenuItem? menuAdmin = null;
        if (SessionManager.HasAdminAccess)
        {
            menuAdmin = new ToolStripMenuItem("Администрирование");

            // Управление пользователями
            if (SessionManager.CanManageUsers)
            {
                menuAdmin.DropDownItems.Add(new ToolStripMenuItem("Пользователи", null, (s, e) => OpenChildForm(new UsersForm())));
            }

            // Управление ролями
            if (SessionManager.CanManageRoles)
            {
                menuAdmin.DropDownItems.Add(new ToolStripMenuItem("Роли", null, (s, e) => OpenChildForm(new RolesForm())));
            }
        }

        // Меню "Справка" - доступно всем пользователям
        var menuHelp = new ToolStripMenuItem("Справка");
        menuHelp.DropDownItems.Add(new ToolStripMenuItem("О программе", null, (s, e) => OpenChildForm(new AboutForm())));

        // Добавление пунктов меню в строку меню
        menuStrip.Items.Add(menuFile);

        if (menuContent != null)
            menuStrip.Items.Add(menuContent);

        if (menuContacts != null)
            menuStrip.Items.Add(menuContacts);

        // Меню "Программа" скрыто, не добавляем в строку меню
        // if (menuSchedule != null)
        //     menuStrip.Items.Add(menuSchedule);

        if (menuAdmin != null)
            menuStrip.Items.Add(menuAdmin);

        menuStrip.Items.Add(menuHelp);

        this.Controls.Add(menuStrip);
    }

    /// <summary>
    /// Инициализация дашборда с кнопками быстрого доступа и строкой поиска фильмов.
    /// Использует адаптивную вёрстку для разных размеров окна и ролей пользователей.
    /// </summary>
    private void InitializeDashboard()
    {
        // Главная панель дашборда, размещённая в MDI-клиентской области
        dashboardPanel = new Panel
        {
            BackColor = Color.FromArgb(240, 240, 240),
            AutoScroll = true
        };

        // Панель поиска фильмов
        var searchPanel = new Panel
        {
            Height = 80,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(240, 240, 240)
        };

        // Заголовок поиска
        var searchLabel = new Label
        {
            Text = "🔍 Поиск фильма по названию:",
            Font = new Font("Segoe UI", 11),
            AutoSize = true,
            Top = 15
        };

        // Поле ввода для поиска
        searchBox = new TextBox
        {
            Width = 350,
            Height = 30,
            Font = new Font("Segoe UI", 11),
            Top = 40
        };
        searchBox.KeyDown += SearchBox_KeyDown;

        // Кнопка поиска
        searchButton = new Button
        {
            Text = "Найти",
            Width = 80,
            Height = 30,
            Top = 40,
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        searchButton.FlatAppearance.BorderSize = 0;
        searchButton.Click += SearchButton_Click;

        // Выпадающий список результатов поиска (скрыт по умолчанию)
        searchResultsListBox = new ListBox
        {
            Width = 440,
            Height = 150,
            Font = new Font("Segoe UI", 10),
            Visible = false,
            Top = 70
        };
        searchResultsListBox.DoubleClick += SearchResultsListBox_DoubleClick;

        searchPanel.Controls.Add(searchLabel);
        searchPanel.Controls.Add(searchBox);
        searchPanel.Controls.Add(searchButton);
        searchPanel.Controls.Add(searchResultsListBox);

        // Панель с кнопками навигации (адаптивная вёрстка)
        buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoScroll = true,
            Padding = new Padding(20),
            BackColor = Color.FromArgb(240, 240, 240)
        };

        // Создание кнопок навигации с учётом прав доступа
        CreateNavigationButtons();

        dashboardPanel.Controls.Add(buttonsPanel);
        dashboardPanel.Controls.Add(searchPanel);
        this.Controls.Add(dashboardPanel);

        // Скрытие результатов поиска при клике вне списка
        dashboardPanel.Click += (s, e) => searchResultsListBox.Visible = false;
        buttonsPanel.Click += (s, e) => searchResultsListBox.Visible = false;
        searchPanel.Click += (s, e) => searchResultsListBox.Visible = false;

        // Центрирование элементов поиска
        CenterSearchElements(searchPanel, searchLabel);
    }

    /// <summary>
    /// Создание кнопок навигации на дашборде с учётом прав доступа текущего пользователя.
    /// </summary>
    private void CreateNavigationButtons()
    {
        buttonsPanel.Controls.Clear();

        // Кнопка "Правообладатели" - доступна пользователям с правом просмотра контента
        if (SessionManager.CanViewContent)
        {
            AddNavigationButton("📁", "Правообладатели", "Управление правообладателями\nи их фильмами",
                () => OpenChildForm(new RightsOwnersForm()));
        }

        // Кнопка "Контакты" - доступна пользователям с правом просмотра контактов
        if (SessionManager.CanViewContacts)
        {
            AddNavigationButton("📞", "Контакты", "Контактная информация\nпродавцов прав",
                () => OpenChildForm(new ContactsForm()));
        }

        // Кнопка "Пользователи" - доступна администраторам
        if (SessionManager.CanManageUsers)
        {
            AddNavigationButton("👥", "Пользователи", "Управление учётными\nзаписями пользователей",
                () => OpenChildForm(new UsersForm()));
        }

        // Кнопка "Роли" - доступна администраторам
        if (SessionManager.CanManageRoles)
        {
            AddNavigationButton("🔐", "Роли", "Управление ролями\nи правами доступа",
                () => OpenChildForm(new RolesForm()));
        }
    }

    /// <summary>
    /// Добавление кнопки навигации в виде карточки на дашборд.
    /// </summary>
    /// <param name="icon">Иконка кнопки (emoji)</param>
    /// <param name="title">Заголовок кнопки</param>
    /// <param name="description">Описание функции</param>
    /// <param name="action">Действие при нажатии</param>
    private void AddNavigationButton(string icon, string title, string description, Action action)
    {
        // Карточка-контейнер для кнопки
        var cardPanel = new Panel
        {
            Width = 180,
            Height = 160,
            Margin = new Padding(15),
            BackColor = Color.White,
            Cursor = Cursors.Hand
        };

        // Добавление тени/границы для эффекта карточки
        cardPanel.Paint += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(200, 200, 200), 1);
            e.Graphics.DrawRectangle(pen, 0, 0, cardPanel.Width - 1, cardPanel.Height - 1);
        };

        // Иконка (emoji)
        var iconLabel = new Label
        {
            Text = icon,
            Font = new Font("Segoe UI Emoji", 32),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.Transparent
        };

        // Заголовок кнопки
        var titleLabel = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 30,
            BackColor = Color.Transparent
        };

        // Описание функции
        var descLabel = new Label
        {
            Text = description,
            Font = new Font("Segoe UI", 8),
            ForeColor = Color.Gray,
            TextAlign = ContentAlignment.TopCenter,
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent
        };

        cardPanel.Controls.Add(descLabel);
        cardPanel.Controls.Add(titleLabel);
        cardPanel.Controls.Add(iconLabel);

        // Обработка клика по карточке
        void OnClick(object? sender, EventArgs e) => action();
        cardPanel.Click += OnClick;
        iconLabel.Click += OnClick;
        titleLabel.Click += OnClick;
        descLabel.Click += OnClick;

        // Эффект наведения мыши
        void OnMouseEnter(object? sender, EventArgs e) => cardPanel.BackColor = Color.FromArgb(245, 250, 255);
        void OnMouseLeave(object? sender, EventArgs e) => cardPanel.BackColor = Color.White;
        cardPanel.MouseEnter += OnMouseEnter;
        cardPanel.MouseLeave += OnMouseLeave;
        iconLabel.MouseEnter += OnMouseEnter;
        iconLabel.MouseLeave += OnMouseLeave;
        titleLabel.MouseEnter += OnMouseEnter;
        titleLabel.MouseLeave += OnMouseLeave;
        descLabel.MouseEnter += OnMouseEnter;
        descLabel.MouseLeave += OnMouseLeave;

        buttonsPanel.Controls.Add(cardPanel);
    }

    /// <summary>
    /// Центрирование элементов поиска на панели.
    /// </summary>
    private void CenterSearchElements(Panel searchPanel, Label searchLabel)
    {
        // Вычисление центра панели поиска
        int centerX = (searchPanel.Width > 0 ? searchPanel.Width : this.ClientSize.Width) / 2;
        int totalWidth = searchBox.Width + searchButton.Width + 10;
        int startX = centerX - totalWidth / 2;

        searchLabel.Left = startX;
        searchBox.Left = startX;
        searchButton.Left = searchBox.Right + 10;
        searchResultsListBox.Left = startX;
    }

    /// <summary>
    /// Обработка изменения размера окна для адаптивного позиционирования элементов.
    /// </summary>
    private void MainForm_Resize(object? sender, EventArgs e)
    {
        if (dashboardPanel == null) return;

        // Размещение дашборда в клиентской области MDI
        dashboardPanel.SetBounds(0, menuStrip.Bottom, this.ClientSize.Width,
            this.ClientSize.Height - headerPanel.Height - menuStrip.Height);

        // Перецентрирование элементов поиска
        if (dashboardPanel.Controls.Count > 1 && dashboardPanel.Controls[1] is Panel searchPanel)
        {
            var searchLabel = searchPanel.Controls.OfType<Label>().FirstOrDefault();
            if (searchLabel != null)
            {
                CenterSearchElements(searchPanel, searchLabel);
            }
        }

        // Центрирование панели кнопок
        CenterButtonsPanel();
    }

    /// <summary>
    /// Центрирование панели с кнопками навигации.
    /// </summary>
    private void CenterButtonsPanel()
    {
        if (buttonsPanel == null || buttonsPanel.Controls.Count == 0) return;

        // Вычисление общей ширины карточек для центрирования
        int cardCount = buttonsPanel.Controls.Count;
        int cardWidth = 180 + 30; // ширина карточки + отступы
        int totalCardsWidth = cardWidth * Math.Min(cardCount, 4);
        int panelPadding = Math.Max(20, (buttonsPanel.Width - totalCardsWidth) / 2);

        buttonsPanel.Padding = new Padding(panelPadding, 20, panelPadding, 20);
    }

    /// <summary>
    /// Обработка нажатия Enter в поле поиска.
    /// </summary>
    private void SearchBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            SearchButton_Click(sender, e);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
        else if (e.KeyCode == Keys.Escape)
        {
            searchResultsListBox.Visible = false;
        }
    }

    /// <summary>
    /// Выполнение поиска фильма по названию.
    /// </summary>
    private void SearchButton_Click(object? sender, EventArgs e)
    {
        string searchTerm = searchBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            searchResultsListBox.Visible = false;
            return;
        }

        using var context = new AppDbContext();

        // Поиск фильмов по названию с включением информации о правообладателе
        var results = context.Films
            .Include(f => f.RightsOwner)
            .Where(f => f.Title.ToLower().Contains(searchTerm.ToLower()))
            .Select(f => new FilmSearchResult
            {
                FilmId = f.Id,
                FilmTitle = f.Title,
                RightsOwnerId = f.RightsOwnerId,
                RightsOwnerName = f.RightsOwner != null ? f.RightsOwner.Name : "Неизвестен"
            })
            .ToList();

        if (results.Count == 0)
        {
            MessageBox.Show("Фильмы не найдены.", "Поиск", MessageBoxButtons.OK, MessageBoxIcon.Information);
            searchResultsListBox.Visible = false;
            return;
        }

        // Отображение результатов поиска в выпадающем списке
        searchResultsListBox.Items.Clear();
        foreach (var result in results)
        {
            searchResultsListBox.Items.Add(result);
        }
        searchResultsListBox.DisplayMember = "DisplayText";
        searchResultsListBox.Visible = true;
    }

    /// <summary>
    /// Обработка двойного клика по результату поиска - открытие формы с фильмом.
    /// </summary>
    private void SearchResultsListBox_DoubleClick(object? sender, EventArgs e)
    {
        if (searchResultsListBox.SelectedItem is FilmSearchResult result)
        {
            OpenFilmFromSearch(result);
        }
    }

    /// <summary>
    /// Открытие формы правообладателя и фильмов для выбранного результата поиска.
    /// </summary>
    private void OpenFilmFromSearch(FilmSearchResult result)
    {
        searchResultsListBox.Visible = false;

        // Открытие формы правообладателей
        var rightsOwnersForm = new RightsOwnersForm();
        rightsOwnersForm.MdiParent = this;
        rightsOwnersForm.Show();

        // Открытие формы фильмов для найденного правообладателя
        var filmsForm = new FilmsForm(result.RightsOwnerId, result.RightsOwnerName, result.FilmId);
        filmsForm.MdiParent = this;
        filmsForm.Show();
    }

    /// <summary>
    /// Открытие дочерней формы в MDI-контейнере.
    /// </summary>
    /// <param name="child">Форма для открытия</param>
    private void OpenChildForm(Form child)
    {
        // Скрытие результатов поиска при открытии любой формы
        searchResultsListBox.Visible = false;
        child.MdiParent = this;
        child.Show();
    }
}

/// <summary>
/// Вспомогательный класс для хранения результата поиска фильма.
/// </summary>
internal class FilmSearchResult
{
    /// <summary>Идентификатор фильма</summary>
    public int FilmId { get; set; }

    /// <summary>Название фильма</summary>
    public string FilmTitle { get; set; } = "";

    /// <summary>Идентификатор правообладателя</summary>
    public int RightsOwnerId { get; set; }

    /// <summary>Название правообладателя</summary>
    public string RightsOwnerName { get; set; } = "";

    /// <summary>
    /// Текст для отображения в списке результатов поиска.
    /// </summary>
    public string DisplayText => $"{FilmTitle} — {RightsOwnerName}";

    public override string ToString() => DisplayText;
}
