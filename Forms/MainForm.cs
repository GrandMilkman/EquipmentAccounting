using EquipmentAccounting.Forms.CRUD;
using EquipmentAccounting.Utils;

namespace EquipmentAccounting.Forms;

public class MainForm : Form
{
    private MenuStrip menuStrip = null!;
    private Panel headerPanel = null!;
    private PictureBox logoBox = null!;
    private Label userInfoLabel = null!;

    public MainForm()
    {
        this.IsMdiContainer = true;
        this.WindowState = FormWindowState.Maximized;
        this.Text = "Учёт контента телеканала";

        InitializeHeader();
        InitializeMenu();
    }

    private void InitializeHeader()
    {
        headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.FromArgb(45, 45, 48)
        };

        logoBox = new PictureBox
        {
            Left = 10,
            Top = 5,
            Width = 100,
            Height = 50,
            SizeMode = PictureBoxSizeMode.Zoom
        };

        string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
        if (File.Exists(logoPath))
        {
            logoBox.Image = Image.FromFile(logoPath);
        }
        else
        {
            logoBox.BackColor = Color.Gray;
        }

        userInfoLabel = new Label
        {
            Text = $"Пользователь: {SessionManager.CurrentUser?.Login} | Роль: {SessionManager.CurrentRole?.Name}",
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

    private void InitializeMenu()
    {
        menuStrip = new MenuStrip();
        menuStrip.Dock = DockStyle.Top;
        this.MainMenuStrip = menuStrip;

        // Файл menu
        var menuFile = new ToolStripMenuItem("Файл");
        menuFile.DropDownItems.Add(new ToolStripMenuItem("Выход", null, (s, e) => this.Close()));

        // Контент menu (visible to those who can view content)
        ToolStripMenuItem? menuContent = null;
        if (SessionManager.CanViewContent)
        {
            menuContent = new ToolStripMenuItem("Контент");
            menuContent.DropDownItems.Add(new ToolStripMenuItem("Правообладатели", null, (s, e) => OpenChildForm(new RightsOwnersForm())));
        }

        // Контакты menu (visible to those who can view contacts)
        ToolStripMenuItem? menuContacts = null;
        if (SessionManager.CanViewContacts)
        {
            menuContacts = new ToolStripMenuItem("Контакты");
            menuContacts.DropDownItems.Add(new ToolStripMenuItem("Список контактов", null, (s, e) => OpenChildForm(new ContactsForm())));
        }

        // Программа menu (TV Schedule)
        ToolStripMenuItem? menuSchedule = null;
        if (SessionManager.CanViewSchedule)
        {
            menuSchedule = new ToolStripMenuItem("Программа");
            menuSchedule.DropDownItems.Add(new ToolStripMenuItem("Телепрограмма", null, (s, e) => OpenChildForm(new TvScheduleForm())));
        }

        // Администрирование menu (visible to admins)
        ToolStripMenuItem? menuAdmin = null;
        if (SessionManager.HasAdminAccess)
        {
            menuAdmin = new ToolStripMenuItem("Администрирование");

            if (SessionManager.CanManageUsers)
            {
                menuAdmin.DropDownItems.Add(new ToolStripMenuItem("Пользователи", null, (s, e) => OpenChildForm(new UsersForm())));
            }

            if (SessionManager.CanManageRoles)
            {
                menuAdmin.DropDownItems.Add(new ToolStripMenuItem("Роли", null, (s, e) => OpenChildForm(new RolesForm())));
            }
        }

        // Справка menu
        var menuHelp = new ToolStripMenuItem("Справка");
        menuHelp.DropDownItems.Add(new ToolStripMenuItem("О программе", null, (s, e) => OpenChildForm(new AboutForm())));

        // Add menus to strip
        menuStrip.Items.Add(menuFile);

        if (menuContent != null)
            menuStrip.Items.Add(menuContent);

        if (menuContacts != null)
            menuStrip.Items.Add(menuContacts);

        if (menuSchedule != null)
            menuStrip.Items.Add(menuSchedule);

        if (menuAdmin != null)
            menuStrip.Items.Add(menuAdmin);

        menuStrip.Items.Add(menuHelp);

        this.Controls.Add(menuStrip);
    }

    private void OpenChildForm(Form child)
    {
        child.MdiParent = this;
        child.Show();
    }
}
