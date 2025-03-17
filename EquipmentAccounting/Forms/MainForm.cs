using EquipmentAccounting.Forms.CRUD;
using EquipmentAccounting.Models;

namespace EquipmentAccounting.Forms;

public class MainForm : Form
{
    private User currentUser;
    private MenuStrip menuStrip;

    public MainForm(User user)
    {
        currentUser = user;
        this.IsMdiContainer = true;
        this.WindowState = FormWindowState.Maximized;
        this.Text = "Учёт фильмов / Поставщики";

        menuStrip = new MenuStrip();
        this.MainMenuStrip = menuStrip;

        ToolStripMenuItem menuFile = new ToolStripMenuItem("Файл");
        ToolStripMenuItem menuExit = new ToolStripMenuItem("Выход", null, (s, e) => this.Close());
        menuFile.DropDownItems.Add(menuExit);

        ToolStripMenuItem menuTables = new ToolStripMenuItem("Таблицы");
        ToolStripMenuItem menuUsers =
            new ToolStripMenuItem("Пользователи", null, (s, e) => OpenChildForm(new UsersForm()));
        ToolStripMenuItem menuBelarusfilm =
            new ToolStripMenuItem("Беларусьфильм", null, (s, e) => OpenChildForm(new BelarusfilmForm()));
        ToolStripMenuItem menuVolga = new ToolStripMenuItem("Вольга", null, (s, e) => OpenChildForm(new VolgaForm()));
        ToolStripMenuItem menuFPL = new ToolStripMenuItem("FPL", null, (s, e) => OpenChildForm(new FPLForm()));
        ToolStripMenuItem menuParamount =
            new ToolStripMenuItem("Paramount", null, (s, e) => OpenChildForm(new ParamountForm()));
        ToolStripMenuItem menuWarner =
            new ToolStripMenuItem("WarnerBros", null, (s, e) => OpenChildForm(new WarnerBrosForm()));

        menuTables.DropDownItems.AddRange(new ToolStripItem[]
        {
            menuUsers, menuBelarusfilm, menuVolga, menuFPL, menuParamount, menuWarner
        });

        ToolStripMenuItem menuNotes = new ToolStripMenuItem("Справка");
        ToolStripMenuItem menuAbout =
            new ToolStripMenuItem("О программе", null, (s, e) => OpenChildForm(new AboutForm()));
        ToolStripMenuItem menuHelp = new ToolStripMenuItem("Помощь", null, (s, e) => OpenChildForm(new HelpForm()));

        menuNotes.DropDownItems.AddRange(new ToolStripItem[]
        {
            menuAbout, menuHelp
        });

        menuStrip.Items.AddRange(new ToolStripItem[] { menuFile, menuTables, menuNotes });
        this.Controls.Add(menuStrip);
    }

    private void OpenChildForm(Form child)
    {
        child.MdiParent = this;
        child.Show();
    }
}
