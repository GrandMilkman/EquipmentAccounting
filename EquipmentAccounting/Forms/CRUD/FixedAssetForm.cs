using EquipmentAccounting.Models;
using EquipmentAccounting.Utils;

namespace EquipmentAccounting.Forms.CRUD;

    public class FixedAssetForm : CrudForm<FixedAsset>
    {
        public FixedAssetForm() : base("Основные средства")
        {
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnSearch.Click += BtnSearch_Click;
            btnRefresh.Click += BtnRefresh_Click;
        }
        
        protected override void LoadData()
        {
            dataGridView.DataSource = context.FixedAssets.ToList();
        }
        protected override void BtnAdd_Click(object sender, EventArgs e)
        {
            string inventoryNumber = InputDialog.Show("Введите инвентарный номер:", "Добавление основного средства");
            string yearStr = InputDialog.Show("Введите год выпуска:", "Добавление основного средства");
            string actNumber = InputDialog.Show("Введите номер акта:", "Добавление основного средства");
            string location = InputDialog.Show("Введите местоположение:", "Добавление основного средства");
            string costStr = InputDialog.Show("Введите стоимость:", "Добавление основного средства");
            string depRateStr = InputDialog.Show("Введите норму амортизации:", "Добавление основного средства");
            string description = InputDialog.Show("Введите характеристику объекта:", "Добавление основного средства");
            string responsibleName = InputDialog.Show("Введите ФИО ответственного:", "Добавление основного средства");
            
            if (int.TryParse(yearStr, out int year) && decimal.TryParse(costStr, out decimal cost) && int.TryParse(depRateStr, out int depRate))
            {
                FixedAsset asset = new FixedAsset
                {
                    InventoryNumber = inventoryNumber,
                    Year = year,
                    ActNumber = actNumber,
                    Location = location,
                    Cost = cost,
                    DepreciationRate = depRate,
                    Description = description,
                    ResponsibleName = responsibleName
                };
                context.FixedAssets.Add(asset);
                context.SaveChanges();
                LoadData();
            }
            else
            {
                MessageBox.Show("Неверный формат числовых данных");
            }
        }
        protected override void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow != null)
            {
                FixedAsset asset = dataGridView.CurrentRow.DataBoundItem as FixedAsset;
                string inventoryNumber = InputDialog.Show("Введите новый инвентарный номер:", "Редактирование основного средства", asset.InventoryNumber);
                string yearStr = InputDialog.Show("Введите новый год выпуска:", "Редактирование основного средства", asset.Year.ToString());
                string actNumber = InputDialog.Show("Введите новый номер акта:", "Редактирование основного средства", asset.ActNumber);
                string location = InputDialog.Show("Введите новое местоположение:", "Редактирование основного средства", asset.Location);
                string costStr = InputDialog.Show("Введите новую стоимость:", "Редактирование основного средства", asset.Cost.ToString());
                string depRateStr = InputDialog.Show("Введите новую норму амортизации:", "Редактирование основного средства", asset.DepreciationRate.ToString());
                string description = InputDialog.Show("Введите новую характеристику объекта:", "Редактирование основного средства", asset.Description);
                string responsibleName = InputDialog.Show("Введите новое ФИО ответственного:", "Редактирование основного средства", asset.ResponsibleName);
                
                if (int.TryParse(yearStr, out int year) && decimal.TryParse(costStr, out decimal cost) && int.TryParse(depRateStr, out int depRate))
                {
                    asset.InventoryNumber = inventoryNumber;
                    asset.Year = year;
                    asset.ActNumber = actNumber;
                    asset.Location = location;
                    asset.Cost = cost;
                    asset.DepreciationRate = depRate;
                    asset.Description = description;
                    asset.ResponsibleName = responsibleName;
                    context.SaveChanges();
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Неверный формат числовых данных");
                }
            }
        }
        protected override void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow != null)
            {
                FixedAsset asset = dataGridView.CurrentRow.DataBoundItem as FixedAsset;
                if (MessageBox.Show("Удалить запись?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    context.FixedAssets.Remove(asset);
                    context.SaveChanges();
                    LoadData();
                }
            }
        }
        protected override void BtnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = InputDialog.Show("Введите инвентарный номер или описание для поиска:", "Поиск");
            var results = context.FixedAssets.Where(a => a.InventoryNumber.Contains(searchTerm) || a.Description.Contains(searchTerm)).ToList();
            dataGridView.DataSource = results;
        }
    }