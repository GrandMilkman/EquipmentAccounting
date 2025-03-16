using EquipmentAccounting.Models;
using EquipmentAccounting.Utils;

namespace EquipmentAccounting.Forms.CRUD;

public class LocationForm : CrudForm<Location>
    {
        public LocationForm() : base("Местоположение")
        {
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnSearch.Click += BtnSearch_Click;
            btnRefresh.Click += BtnRefresh_Click;
        }
        
        protected override void LoadData()
        {
            dataGridView.DataSource = context.Locations.ToList();
        }
        protected override void BtnAdd_Click(object sender, EventArgs e)
        {
            string name = InputDialog.Show("Введите наименование местоположения:", "Добавление местоположения");
            if (!string.IsNullOrWhiteSpace(name))
            {
                Location loc = new Location { Name = name };
                context.Locations.Add(loc);
                context.SaveChanges();
                LoadData();
            }
        }
        protected override void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow != null)
            {
                Location loc = dataGridView.CurrentRow.DataBoundItem as Location;
                string name = InputDialog.Show("Введите новое наименование местоположения:", "Редактирование местоположения", loc.Name);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    loc.Name = name;
                    context.SaveChanges();
                    LoadData();
                }
            }
        }
        protected override void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow != null)
            {
                Location loc = dataGridView.CurrentRow.DataBoundItem as Location;
                if (MessageBox.Show("Удалить запись?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    context.Locations.Remove(loc);
                    context.SaveChanges();
                    LoadData();
                }
            }
        }
        protected override void BtnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = InputDialog.Show("Введите значение для поиска по наименованию:", "Поиск");
            var results = context.Locations.Where(l => l.Name.Contains(searchTerm)).ToList();
            dataGridView.DataSource = results;
        }
    }