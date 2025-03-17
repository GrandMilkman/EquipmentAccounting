using EquipmentAccounting.Models;
using EquipmentAccounting.Utils;

namespace EquipmentAccounting.Forms.CRUD;

public class WarnerBrosForm : CrudForm<WarnerBros>
{
    public WarnerBrosForm() : base("WarnerBros")
    {
    }

    protected override void LoadData()
    {
        dataGridView.DataSource = context.WarnerBrosSet.ToList();
    }

    protected override void BtnAdd_Click(object sender, EventArgs e)
    {
        string title = InputDialog.Show("Введите название фильма:", "Добавление");
        string dateStr = InputDialog.Show("Введите дату внесения (yyyy-MM-dd):", "Добавление");
        string age = InputDialog.Show("Введите возрастное ограничение (напр. 12+):", "Добавление");
        string durStr = InputDialog.Show("Введите хронометраж (мин):", "Добавление");
        string filePath = InputDialog.Show("Введите путь до файла:", "Добавление");

        if (DateTime.TryParse(dateStr, out DateTime date) && int.TryParse(durStr, out int dur))
        {
            var film = new WarnerBros
            {
                Title = title,
                DateAdded = date,
                AgeRestriction = age,
                Duration = dur,
                FilePath = filePath
            };
            context.WarnerBrosSet.Add(film);
            context.SaveChanges();
            LoadData();
        }
        else
        {
            MessageBox.Show("Неверные данные (дата или хронометраж)");
        }
    }

    protected override void BtnEdit_Click(object sender, EventArgs e)
    {
        if (dataGridView.CurrentRow != null)
        {
            var film = dataGridView.CurrentRow.DataBoundItem as WarnerBros;
            string title = InputDialog.Show("Название:", "Редактирование", film.Title);
            string dateStr = InputDialog.Show("Дата внесения (yyyy-MM-dd):", "Редактирование",
                film.DateAdded.ToString("yyyy-MM-dd"));
            string age = InputDialog.Show("Возрастное ограничение:", "Редактирование", film.AgeRestriction);
            string durStr = InputDialog.Show("Хронометраж (мин):", "Редактирование", film.Duration.ToString());
            string filePath = InputDialog.Show("Путь до файла:", "Редактирование", film.FilePath);

            if (DateTime.TryParse(dateStr, out DateTime date) && int.TryParse(durStr, out int dur))
            {
                film.Title = title;
                film.DateAdded = date;
                film.AgeRestriction = age;
                film.Duration = dur;
                film.FilePath = filePath;
                context.SaveChanges();
                LoadData();
            }
            else
            {
                MessageBox.Show("Неверные данные (дата или хронометраж)");
            }
        }
    }

    protected override void BtnDelete_Click(object sender, EventArgs e)
    {
        if (dataGridView.CurrentRow != null)
        {
            var film = dataGridView.CurrentRow.DataBoundItem as WarnerBros;
            if (MessageBox.Show("Удалить запись?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                context.WarnerBrosSet.Remove(film);
                context.SaveChanges();
                LoadData();
            }
        }
    }

    protected override void BtnSearch_Click(object sender, EventArgs e)
    {
        string searchTerm = InputDialog.Show("Введите часть названия для поиска:", "Поиск");
        var results = context.WarnerBrosSet
            .Where(f => f.Title.Contains(searchTerm))
            .ToList();
        dataGridView.DataSource = results;
    }
}