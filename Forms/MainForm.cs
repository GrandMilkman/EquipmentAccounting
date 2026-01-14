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
    private Button logoutButton = null!;

    // Элементы дашборда
    private Panel dashboardPanel = null!;
    private Panel searchPanel = null!; // Сохраняем ссылку на панель поиска
    private TextBox searchBox = null!;
    private FlowLayoutPanel buttonsPanel = null!;
    private ListBox searchResultsListBox = null!;
    private System.Windows.Forms.Timer searchTimer = null!; // Таймер для задержки live-поиска

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

        // Кнопка выхода
        logoutButton = new Button
        {
            Text = "Выйти",
            Width = 80,
            Height = 30,
            Font = new Font("Segoe UI", 9),
            BackColor = Color.FromArgb(70, 70, 75),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Top = 15
        };
        logoutButton.FlatAppearance.BorderSize = 0;
        logoutButton.Click += LogoutButton_Click;

        // Позиционирование элементов в заголовке
        UpdateHeaderElementsPosition();

        // Обновление позиции при изменении размера окна
        this.Resize += (s, e) => UpdateHeaderElementsPosition();

        headerPanel.Controls.Add(logoBox);
        headerPanel.Controls.Add(userInfoLabel);
        headerPanel.Controls.Add(logoutButton);
        this.Controls.Add(headerPanel);
    }

    /// <summary>
    /// Обновление позиции элементов в заголовке при изменении размера окна.
    /// </summary>
    private void UpdateHeaderElementsPosition()
    {
        if (userInfoLabel == null || logoutButton == null) return;

        // Позиционируем кнопку выхода справа с большим отступом для лучшего внешнего вида
        logoutButton.Left = this.Width - logoutButton.Width - 25;

        // Позиционируем информацию о пользователе слева от кнопки выхода
        userInfoLabel.Left = logoutButton.Left - userInfoLabel.Width - 15;
    }

    /// <summary>
    /// Обработчик нажатия кнопки выхода.
    /// </summary>
    private void LogoutButton_Click(object? sender, EventArgs e)
    {
        // Подтверждение выхода
        var result = MessageBox.Show(
            "Вы уверены, что хотите выйти из системы?",
            "Выход",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            // Очищаем сессию
            SessionManager.ClearSession();

            // Закрываем все дочерние формы
            foreach (Form childForm in this.MdiChildren)
            {
                childForm.Close();
            }

            // Скрываем главную форму вместо закрытия, чтобы не завершать приложение
            this.Hide();

            // Открываем форму входа
            var loginForm = new LoginForm();
            loginForm.Show();

            // При закрытии формы входа закрываем главную форму
            loginForm.FormClosed += (s, args) =>
            {
                this.Close();
            };
        }
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
        // Используем специальный подход для MDI: добавляем панель в MDI клиентскую область
        dashboardPanel = new Panel
        {
            BackColor = Color.FromArgb(240, 240, 240),
            AutoScroll = true
        };

        // Панель поиска фильмов
        searchPanel = new Panel
        {
            Height = 100,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(240, 240, 240),
            Padding = new Padding(0, 10, 0, 10) // Отступ сверху для визуального разделения
        };

        // Заголовок поиска
        var searchLabel = new Label
        {
            Text = "🔍 Поиск фильма по названию:",
            Font = new Font("Segoe UI", 11),
            AutoSize = true,
            Top = 20 // Увеличен отступ сверху
        };

        // Поле ввода для поиска
        searchBox = new TextBox
        {
            Width = 350,
            Height = 30,
            Font = new Font("Segoe UI", 11),
            Top = 50 // Увеличен отступ сверху
        };
        searchBox.KeyDown += SearchBox_KeyDown;
        searchBox.TextChanged += SearchBox_TextChanged; // Live-поиск при вводе

        // Выпадающий список результатов поиска (скрыт по умолчанию)
        // Размещаем его в dashboardPanel, чтобы он мог отображаться поверх кнопок
        searchResultsListBox = new ListBox
        {
            Width = 440,
            Height = 200, // Начальная высота, будет динамически изменяться
            Font = new Font("Segoe UI", 10),
            Visible = false,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        searchResultsListBox.DoubleClick += SearchResultsListBox_DoubleClick;

        // Инициализация таймера для задержки live-поиска (чтобы не делать запрос на каждое нажатие)
        searchTimer = new System.Windows.Forms.Timer
        {
            Interval = 300 // Задержка 300 мс после последнего ввода
        };
        searchTimer.Tick += SearchTimer_Tick;

        searchPanel.Controls.Add(searchLabel);
        searchPanel.Controls.Add(searchBox);
        // Кнопку "Найти" убрали, так как есть live-поиск
        
        // Список результатов добавляем в dashboardPanel, чтобы он был поверх кнопок
        dashboardPanel.Controls.Add(searchResultsListBox);

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
        
        // Добавляем дашборд в MDI клиентскую область после загрузки формы
        this.Load += MainForm_Load;
        
        // Временно добавляем в Controls для инициализации, потом переместим в MDI клиент
        this.Controls.Add(dashboardPanel);

        // Скрытие результатов поиска при клике вне списка
        // Но не скрываем при клике на сам список или поле поиска
        dashboardPanel.Click += (s, e) =>
        {
            if (e is MouseEventArgs me && !searchResultsListBox.Bounds.Contains(me.Location) && !searchBox.Bounds.Contains(me.Location))
            {
                searchResultsListBox.Visible = false;
            }
        };
        buttonsPanel.Click += (s, e) =>
        {
            if (e is MouseEventArgs me && !searchResultsListBox.Bounds.Contains(me.Location))
            {
                searchResultsListBox.Visible = false;
            }
        };

        // Центрирование элементов поиска будет выполнено после загрузки формы,
        // когда панель получит правильный размер
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
        if (searchPanel == null || searchBox == null || searchLabel == null) return;
        
        // Вычисление центра панели поиска
        // Используем реальную ширину панели или ширину dashboardPanel
        int panelWidth = searchPanel.Width > 0 ? searchPanel.Width : 
                        (dashboardPanel != null && dashboardPanel.Width > 0 ? dashboardPanel.Width : this.ClientSize.Width);
        int centerX = panelWidth / 2;
        
        // Центрируем поле ввода
        int startX = centerX - searchBox.Width / 2;
        searchBox.Left = startX;
        
        // Центрируем заголовок относительно поля ввода (или по центру панели)
        int labelStartX = centerX - searchLabel.Width / 2;
        searchLabel.Left = labelStartX;
    }

    /// <summary>
    /// Обработка загрузки формы - правильное размещение дашборда.
    /// </summary>
    private void MainForm_Load(object? sender, EventArgs e)
    {
        // Обновляем позицию дашборда после загрузки
        UpdateDashboardPosition();
        
        // Центрируем элементы поиска после загрузки, когда панель получила правильный размер
        if (searchPanel != null)
        {
            var searchLabel = searchPanel.Controls.OfType<Label>().FirstOrDefault();
            if (searchLabel != null)
            {
                CenterSearchElements(searchPanel, searchLabel);
            }
            
            // Подписываемся на изменение размера searchPanel для обновления позиции элементов
            searchPanel.Resize += (s, e) =>
            {
                // Обновляем центрирование элементов поиска
                var label = searchPanel.Controls.OfType<Label>().FirstOrDefault();
                if (label != null)
                {
                    CenterSearchElements(searchPanel, label);
                }
                
                // Обновляем позицию списка результатов, если он виден
                if (searchResultsListBox != null && searchResultsListBox.Visible)
                {
                    UpdateSearchResultsPosition();
                }
            };
        }
        
        // Подписываемся на события открытия/закрытия дочерних форм
        this.MdiChildActivate += MainForm_MdiChildActivate;
    }

    /// <summary>
    /// Обработка активации дочерних форм - скрываем дашборд когда открыты формы.
    /// </summary>
    private void MainForm_MdiChildActivate(object? sender, EventArgs e)
    {
        UpdateDashboardVisibility();
    }

    /// <summary>
    /// Обновление видимости дашборда в зависимости от наличия дочерних форм.
    /// </summary>
    private void UpdateDashboardVisibility()
    {
        if (dashboardPanel == null) return;
        
        // Скрываем дашборд если есть активные дочерние формы
        // Фильтруем только не закрытые формы
        bool hasActiveChildren = this.MdiChildren.Any(child => !child.IsDisposed && child.Visible);
        dashboardPanel.Visible = !hasActiveChildren;
    }

    /// <summary>
    /// Обновление позиции дашборда с учётом headerPanel и menuStrip.
    /// </summary>
    private void UpdateDashboardPosition()
    {
        if (dashboardPanel == null || headerPanel == null || menuStrip == null) return;

        // Правильное позиционирование: учитываем высоту headerPanel и menuStrip
        int topPosition = headerPanel.Height + menuStrip.Height;
        int availableHeight = this.ClientSize.Height - topPosition;
        
        dashboardPanel.SetBounds(0, topPosition, this.ClientSize.Width, availableHeight);
    }

    /// <summary>
    /// Обработка изменения размера окна для адаптивного позиционирования элементов.
    /// </summary>
    private void MainForm_Resize(object? sender, EventArgs e)
    {
        if (dashboardPanel == null) return;

        // Обновляем позицию дашборда
        UpdateDashboardPosition();

        // Перецентрирование элементов поиска
        if (dashboardPanel.Controls.Count > 1 && dashboardPanel.Controls[1] is Panel searchPanel)
        {
            var searchLabel = searchPanel.Controls.OfType<Label>().FirstOrDefault();
            if (searchLabel != null)
            {
                CenterSearchElements(searchPanel, searchLabel);
            }
        }
        
        // Обновляем позицию списка результатов, если он виден
        if (searchResultsListBox != null && searchResultsListBox.Visible)
        {
            UpdateSearchResultsPosition();
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
            // Останавливаем таймер
            searchTimer.Stop();
            
            // Если список результатов виден и содержит только один элемент - открываем его сразу
            if (searchResultsListBox != null && searchResultsListBox.Visible && searchResultsListBox.Items.Count == 1)
            {
                // Автоматически выбираем единственный результат и открываем его
                searchResultsListBox.SelectedIndex = 0;
                if (searchResultsListBox.SelectedItem is FilmSearchResult result)
                {
                    OpenFilmFromSearch(result);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
            }
            
            // Если результатов больше одного или список не виден - выполняем поиск
            PerformSearch();
            
            // После выполнения поиска проверяем, если результат один - открываем его
            if (searchResultsListBox != null && searchResultsListBox.Visible && searchResultsListBox.Items.Count == 1)
            {
                searchResultsListBox.SelectedIndex = 0;
                if (searchResultsListBox.SelectedItem is FilmSearchResult singleResult)
                {
                    OpenFilmFromSearch(singleResult);
                }
            }
            
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
        else if (e.KeyCode == Keys.Escape)
        {
            searchResultsListBox.Visible = false;
            searchTimer.Stop();
        }
    }

    /// <summary>
    /// Обработка изменения текста в поле поиска - запуск live-поиска с задержкой.
    /// </summary>
    private void SearchBox_TextChanged(object? sender, EventArgs e)
    {
        // Останавливаем предыдущий таймер
        searchTimer.Stop();
        
        string searchTerm = searchBox.Text.Trim();
        
        // Если поле пустое, скрываем результаты
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            searchResultsListBox.Visible = false;
            searchResultsListBox.Items.Clear();
            return;
        }
        
        // Запускаем таймер для задержки поиска
        searchTimer.Start();
    }

    /// <summary>
    /// Обработчик таймера - выполнение поиска после задержки.
    /// </summary>
    private void SearchTimer_Tick(object? sender, EventArgs e)
    {
        searchTimer.Stop();
        PerformSearch();
    }

    /// <summary>
    /// Выполнение поиска фильмов по названию.
    /// </summary>
    private void PerformSearch()
    {
        string searchTerm = searchBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            searchResultsListBox.Visible = false;
            searchResultsListBox.Items.Clear();
            return;
        }

        using var context = new AppDbContext();

        // Поиск фильмов по названию с включением информации о правообладателе
        // Используем AsNoTracking() для получения актуальных данных из БД без кэширования
        var results = context.Films
            .AsNoTracking()
            .Include(f => f.RightsOwner)
            .Where(f => f.Title.ToLower().Contains(searchTerm.ToLower()))
            .OrderBy(f => f.Title) // Сортировка по названию для удобства
            .Select(f => new FilmSearchResult
            {
                FilmId = f.Id,
                FilmTitle = f.Title,
                RightsOwnerId = f.RightsOwnerId,
                RightsOwnerName = f.RightsOwner != null ? f.RightsOwner.Name : "Неизвестен"
            })
            .ToList();

        // Очищаем предыдущие результаты
        searchResultsListBox.Items.Clear();

        if (results.Count == 0)
        {
            // Не показываем сообщение для live-поиска, просто скрываем список
            searchResultsListBox.Visible = false;
            return;
        }

        // Устанавливаем DisplayMember перед добавлением элементов
        searchResultsListBox.DisplayMember = "DisplayText";
        
        // Отображение всех результатов поиска в выпадающем списке
        searchResultsListBox.BeginUpdate(); // Отключаем перерисовку для быстрой загрузки
        
        foreach (var result in results)
        {
            searchResultsListBox.Items.Add(result);
        }
        
        searchResultsListBox.EndUpdate(); // Включаем перерисовку обратно
        
        // Позиционируем список результатов под полем поиска
        UpdateSearchResultsPosition();
        
        // Автоматически подстраиваем высоту списка под количество результатов
        // Показываем до 10 элементов без прокрутки, максимум 300px для лучшей видимости
        int itemHeight = searchResultsListBox.ItemHeight;
        int maxVisibleItems = Math.Min(results.Count, 10);
        int calculatedHeight = maxVisibleItems * itemHeight + 4; // +4 для границ
        searchResultsListBox.Height = Math.Min(300, Math.Max(100, calculatedHeight)); // Минимум 100px, максимум 300px
        
        // Показываем список и выводим его на передний план
        searchResultsListBox.Visible = true;
        searchResultsListBox.BringToFront(); // Выводим поверх всех элементов
    }

    /// <summary>
    /// Обновление позиции списка результатов поиска.
    /// </summary>
    private void UpdateSearchResultsPosition()
    {
        if (searchResultsListBox == null || searchPanel == null || searchBox == null || dashboardPanel == null) return;
        
        // Получаем позицию поля поиска относительно dashboardPanel
        Point searchBoxLocation = searchBox.Location;
        Point searchPanelLocation = searchPanel.Location;
        
        // Вычисляем абсолютную позицию поля поиска относительно dashboardPanel
        int searchBoxAbsoluteLeft = searchPanelLocation.X + searchBoxLocation.X;
        int searchBoxAbsoluteTop = searchPanelLocation.Y + searchBoxLocation.Y;
        
        // Центрируем список результатов относительно поля поиска
        // Список должен быть выровнен по центру поля поиска
        int centerX = searchPanelLocation.X + (searchPanel.Width / 2);
        int startX = centerX - (searchResultsListBox.Width / 2);
        
        // Позиция по Y: сразу под полем поиска (нижняя граница поля + небольшой отступ)
        int topPosition = searchPanelLocation.Y + searchPanel.Height;
        
        // Устанавливаем позицию относительно dashboardPanel
        searchResultsListBox.Left = startX;
        searchResultsListBox.Top = topPosition;
        searchResultsListBox.BringToFront(); // Выводим поверх всех элементов в dashboardPanel
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
    /// Открытие формы фильмов для выбранного результата поиска.
    /// </summary>
    private void OpenFilmFromSearch(FilmSearchResult result)
    {
        searchResultsListBox.Visible = false;

        // Открытие только формы фильмов для найденного правообладателя
        var filmsForm = new FilmsForm(result.RightsOwnerId, result.RightsOwnerName, result.FilmId);
        filmsForm.MdiParent = this;
        filmsForm.FormClosed += (s, e) => UpdateDashboardVisibility();
        filmsForm.Show();
        
        // Обновляем видимость дашборда после открытия формы
        UpdateDashboardVisibility();
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
        
        // Обработка закрытия формы для обновления видимости дашборда
        child.FormClosed += (s, e) =>
        {
            // Обновляем видимость дашборда после закрытия формы
            UpdateDashboardVisibility();
        };
        
        child.Show();
        
        // Обновляем видимость дашборда после открытия формы
        UpdateDashboardVisibility();
    }

    /// <summary>
    /// Освобождение ресурсов при закрытии формы.
    /// </summary>
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        // Останавливаем и освобождаем таймер поиска
        if (searchTimer != null)
        {
            searchTimer.Stop();
            searchTimer.Dispose();
        }
        
        base.OnFormClosed(e);
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
