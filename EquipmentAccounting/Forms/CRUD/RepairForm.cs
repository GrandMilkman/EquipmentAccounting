using EquipmentAccounting.Models;
using EquipmentAccounting.Utils;

namespace EquipmentAccounting.Forms.CRUD;

    public class RepairForm : CrudForm<Repair>
    {
        public RepairForm() : base("Ремонт")
        {
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnSearch.Click += BtnSearch_Click;
            btnRefresh.Click += BtnRefresh_Click;
        }
        protected override void LoadData()
        {
            dataGridView.DataSource = context.Repairs.ToList();
        }
        protected override void BtnAdd_Click(object sender, EventArgs e)
        {
            string inventoryNumber = InputDialog.Show("Введите инвентарный номер:", "Добавление ремонта");
            string dateStr = InputDialog.Show("Введите дату (yyyy-MM-dd):", "Добавление ремонта");
            string description = InputDialog.Show("Введите описание:", "Добавление ремонта");
            if (DateTime.TryParse(dateStr, out DateTime date))
            {
                Repair repair = new Repair { InventoryNumber = inventoryNumber, Date = date, Description = description };
                context.Repairs.Add(repair);
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
                Repair repair = dataGridView.CurrentRow.DataBoundItem as Repair;
                string inventoryNumber = InputDialog.Show("Введите новый инвентарный номер:", "Редактирование ремонта", repair.InventoryNumber);
                string dateStr = InputDialog.Show("Введите новую дату (yyyy-MM-dd):", "Редактирование ремонта", repair.Date.ToString("yyyy-MM-dd"));
                string description = InputDialog.Show("Введите новое описание:", "Редактирование ремонта", repair.Description);
                if (DateTime.TryParse(dateStr, out DateTime date))
                {
                    repair.InventoryNumber = inventoryNumber;
                    repair.Date = date;
                    repair.Description = description;
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
                Repair repair = dataGridView.CurrentRow.DataBoundItem as Repair;
                if (MessageBox.Show("Удалить запись?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    context.Repairs.Remove(repair);
                    context.SaveChanges();
                    LoadData();
                }
            }
        }
        protected override void BtnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = InputDialog.Show("Введите описание для поиска:", "Поиск");
            var results = context.Repairs.Where(r => r.Description.Contains(searchTerm)).ToList();
            dataGridView.DataSource = results;
        }
    }