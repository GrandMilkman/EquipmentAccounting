using EquipmentAccounting.Models;
using EquipmentAccounting.Utils;

namespace EquipmentAccounting.Forms.CRUD;

public class ResponsibleForm : CrudForm<Responsible>
    {
        public ResponsibleForm() : base("Ответственные")
        {
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnSearch.Click += BtnSearch_Click;
            btnRefresh.Click += BtnRefresh_Click;
        }
        protected override void LoadData()
        {
            dataGridView.DataSource = context.Responsibles.ToList();
        }
        protected override void BtnAdd_Click(object sender, EventArgs e)
        {
            string fullName = InputDialog.Show("Введите ФИО:", "Добавление ответственного");
            string position = InputDialog.Show("Введите должность:", "Добавление ответственного");
            string phone = InputDialog.Show("Введите телефон:", "Добавление ответственного");
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                Responsible resp = new Responsible { FullName = fullName, Position = position, Phone = phone };
                context.Responsibles.Add(resp);
                context.SaveChanges();
                LoadData();
            }
        }
        protected override void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow != null)
            {
                Responsible resp = dataGridView.CurrentRow.DataBoundItem as Responsible;
                string fullName = InputDialog.Show("Введите новое ФИО:", "Редактирование ответственного", resp.FullName);
                string position = InputDialog.Show("Введите новую должность:", "Редактирование ответственного", resp.Position);
                string phone = InputDialog.Show("Введите новый телефон:", "Редактирование ответственного", resp.Phone);
                resp.FullName = fullName;
                resp.Position = position;
                resp.Phone = phone;
                context.SaveChanges();
                LoadData();
            }
        }
        protected override void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow != null)
            {
                Responsible resp = dataGridView.CurrentRow.DataBoundItem as Responsible;
                if (MessageBox.Show("Удалить запись?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    context.Responsibles.Remove(resp);
                    context.SaveChanges();
                    LoadData();
                }
            }
        }
        protected override void BtnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = InputDialog.Show("Введите ФИО для поиска:", "Поиск");
            var results = context.Responsibles.Where(r => r.FullName.Contains(searchTerm)).ToList();
            dataGridView.DataSource = results;
        }
    }