using EquipmentAccounting.Models;
using EquipmentAccounting.Utils;

namespace EquipmentAccounting.Forms.CRUD;

public class UsersForm : CrudForm<User>
    {
        public UsersForm() : base("Пользователи")
        {
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnSearch.Click += BtnSearch_Click;
            btnRefresh.Click += BtnRefresh_Click;
        }
        protected override void LoadData()
        {
            dataGridView.DataSource = context.Users.ToList();
        }
        protected override void BtnAdd_Click(object sender, EventArgs e)
        {
            string login = InputDialog.Show("Введите логин:", "Добавление пользователя");
            string password = InputDialog.Show("Введите пароль:", "Добавление пользователя");
            string role = InputDialog.Show("Введите роль (Администратор/Пользователь):", "Добавление пользователя");
            if (!string.IsNullOrWhiteSpace(login) && !string.IsNullOrWhiteSpace(password))
            {
                User user = new User { Login = login, Password = password, Role = role };
                context.Users.Add(user);
                context.SaveChanges();
                LoadData();
            }
        }
        protected override void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow != null)
            {
                User user = dataGridView.CurrentRow.DataBoundItem as User;
                string role = InputDialog.Show("Введите новую роль:", "Редактирование пользователя", user.Role);
                user.Role = role;
                context.SaveChanges();
                LoadData();
            }
        }
        protected override void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow != null)
            {
                User user = dataGridView.CurrentRow.DataBoundItem as User;
                if (MessageBox.Show("Удалить запись?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    context.Users.Remove(user);
                    context.SaveChanges();
                    LoadData();
                }
            }
        }
        protected override void BtnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = InputDialog.Show("Введите логин или роль для поиска:", "Поиск");
            var results = context.Users.Where(u => u.Login.Contains(searchTerm) || u.Role.Contains(searchTerm)).ToList();
            dataGridView.DataSource = results;
        }
    }