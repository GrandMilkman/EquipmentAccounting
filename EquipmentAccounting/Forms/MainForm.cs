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
            this.Text = "Учет оборудования";

            menuStrip = new MenuStrip();
            this.MainMenuStrip = menuStrip;

            ToolStripMenuItem menuFile = new ToolStripMenuItem("Файл");
            ToolStripMenuItem menuExit = new ToolStripMenuItem("Выход", null, (s, e) => this.Close());
            menuFile.DropDownItems.Add(menuExit);

            ToolStripMenuItem menuTables = new ToolStripMenuItem("Таблицы");
            ToolStripMenuItem menuActAcceptance = new ToolStripMenuItem("Акты приемки", null, (s, e) => OpenChildForm(new ActAcceptanceForm()));
            ToolStripMenuItem menuLocation = new ToolStripMenuItem("Местоположение", null, (s, e) => OpenChildForm(new LocationForm()));
            ToolStripMenuItem menuUsers = new ToolStripMenuItem("Пользователи", null, (s, e) => OpenChildForm(new UsersForm()));
            ToolStripMenuItem menuResponsible = new ToolStripMenuItem("Ответственные", null, (s, e) => OpenChildForm(new ResponsibleForm()));
            ToolStripMenuItem menuFixedAsset = new ToolStripMenuItem("Основные средства", null, (s, e) => OpenChildForm(new FixedAssetForm()));
            ToolStripMenuItem menuRepair = new ToolStripMenuItem("Ремонт", null, (s, e) => OpenChildForm(new RepairForm()));
            ToolStripMenuItem menuTransfer = new ToolStripMenuItem("Перемещение", null, (s, e) => OpenChildForm(new TransferForm()));
            menuTables.DropDownItems.AddRange(new ToolStripItem[] { menuActAcceptance, menuLocation, menuUsers, menuResponsible, menuFixedAsset, menuRepair, menuTransfer });

            ToolStripMenuItem menuHelp = new ToolStripMenuItem("Справка");
            ToolStripMenuItem menuAbout = new ToolStripMenuItem("О программе", null, (s, e) => OpenChildForm(new AboutForm()));
            ToolStripMenuItem menuHelpContent = new ToolStripMenuItem("Справка", null, (s, e) => OpenChildForm(new HelpForm()));
            menuHelp.DropDownItems.AddRange(new ToolStripItem[] { menuAbout, menuHelpContent });

            menuStrip.Items.AddRange(new ToolStripItem[] { menuFile, menuTables, menuHelp });
            this.Controls.Add(menuStrip);
        }

        private void OpenChildForm(Form child)
        {
            child.MdiParent = this;
            child.Show();
        }
    }