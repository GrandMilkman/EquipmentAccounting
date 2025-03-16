using EquipmentAccounting.Models;
using EquipmentAccounting.Utils;

namespace EquipmentAccounting.Forms.CRUD
{
    public class ActAcceptanceForm : CrudForm<ActAcceptance>
    {
        public ActAcceptanceForm() : base("Акты приемки")
        {
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnSearch.Click += BtnSearch_Click;
            btnRefresh.Click += BtnRefresh_Click;
        }

        protected override void LoadData()
        {
            dataGridView.DataSource = context.ActAcceptances.ToList();
        }

        protected override void BtnAdd_Click(object sender, EventArgs e)
        {
            string actNumber = InputDialog.Show("Введите номер акта:", "Добавление акта приемки");
            string dateStr = InputDialog.Show("Введите дату (yyyy-MM-dd):", "Добавление акта приемки");
            if (DateTime.TryParse(dateStr, out DateTime date))
            {
                ActAcceptance act = new ActAcceptance { ActNumber = actNumber, Date = date };
                context.ActAcceptances.Add(act);
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
                ActAcceptance act = dataGridView.CurrentRow.DataBoundItem as ActAcceptance;
                string actNumber = InputDialog.Show("Введите новый номер акта:", "Редактирование акта приемки", act.ActNumber);
                string dateStr = InputDialog.Show("Введите новую дату (yyyy-MM-dd):", "Редактирование акта приемки", act.Date.ToString("yyyy-MM-dd"));
                if (DateTime.TryParse(dateStr, out DateTime date))
                {
                    act.ActNumber = actNumber;
                    act.Date = date;
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
                ActAcceptance act = dataGridView.CurrentRow.DataBoundItem as ActAcceptance;
                if (MessageBox.Show("Удалить запись?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    context.ActAcceptances.Remove(act);
                    context.SaveChanges();
                    LoadData();
                }
            }
        }

        protected override void BtnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = InputDialog.Show("Введите номер акта для поиска:", "Поиск");
            var results = context.ActAcceptances.Where(a => a.ActNumber.Contains(searchTerm)).ToList();
            dataGridView.DataSource = results;
        }
    }
}
