using EquipmentAccounting.Models;
using EquipmentAccounting.Utils;

namespace EquipmentAccounting.Forms.CRUD;

    public class TransferForm : CrudForm<Transfer>
    {
        public TransferForm() : base("Перемещение")
        {
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnSearch.Click += BtnSearch_Click;
            btnRefresh.Click += BtnRefresh_Click;
        }
        protected override void LoadData()
        {
            dataGridView.DataSource = context.Transfers.ToList();
        }
        protected override void BtnAdd_Click(object sender, EventArgs e)
        {
            string inventoryNumber = InputDialog.Show("Введите инвентарный номер:", "Добавление перемещения");
            string dateStr = InputDialog.Show("Введите дату (yyyy-MM-dd):", "Добавление перемещения");
            string newLocation = InputDialog.Show("Введите новое местоположение:", "Добавление перемещения");
            if (DateTime.TryParse(dateStr, out DateTime date))
            {
                Transfer transfer = new Transfer { InventoryNumber = inventoryNumber, Date = date, NewLocation = newLocation };
                context.Transfers.Add(transfer);
                context.SaveChanges();
                LoadData();
            }
            else
            {
                MessageBox.Show("Неверный формат даты");
            }
        }
        protected override void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow != null)
            {
                Transfer transfer = dataGridView.CurrentRow.DataBoundItem as Transfer;
                string inventoryNumber = InputDialog.Show("Введите новый инвентарный номер:", "Редактирование перемещения", transfer.InventoryNumber);
                string dateStr = InputDialog.Show("Введите новую дату (yyyy-MM-dd):", "Редактирование перемещения", transfer.Date.ToString("yyyy-MM-dd"));
                string newLocation = InputDialog.Show("Введите новое местоположение:", "Редактирование перемещения", transfer.NewLocation);
                if (DateTime.TryParse(dateStr, out DateTime date))
                {
                    transfer.InventoryNumber = inventoryNumber;
                    transfer.Date = date;
                    transfer.NewLocation = newLocation;
                    context.SaveChanges();
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Неверный формат даты");
                }
            }
        }
        protected override void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow != null)
            {
                Transfer transfer = dataGridView.CurrentRow.DataBoundItem as Transfer;
                if (MessageBox.Show("Удалить запись?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    context.Transfers.Remove(transfer);
                    context.SaveChanges();
                    LoadData();
                }
            }
        }
        protected override void BtnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = InputDialog.Show("Введите инвентарный номер или новое местоположение для поиска:", "Поиск");
            var results = context.Transfers.Where(t => t.InventoryNumber.Contains(searchTerm) || t.NewLocation.Contains(searchTerm)).ToList();
            dataGridView.DataSource = results;
        }
    }