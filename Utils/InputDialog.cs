namespace EquipmentAccounting.Utils;

/// <summary>
/// Вспомогательный класс для отображения диалога ввода текста.
/// Используется для запроса данных у пользователя в модальном окне.
/// </summary>
public static class InputDialog
{
    /// <summary>
    /// Отображение диалога ввода текста.
    /// </summary>
    /// <param name="text">Текст приглашения (подсказка для пользователя)</param>
    /// <param name="caption">Заголовок окна диалога</param>
    /// <param name="defaultValue">Значение по умолчанию в поле ввода</param>
    /// <returns>Введённый текст или пустая строка при отмене</returns>
    public static string Show(string text, string caption, string defaultValue = "")
    {
        // Создание формы диалога
        Form prompt = new Form()
        {
            Width = 400,
            Height = 150,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = caption,
            StartPosition = FormStartPosition.CenterScreen,
            MaximizeBox = false,
            MinimizeBox = false
        };

        // Текст приглашения
        Label textLabel = new Label()
        {
            Left = 20,
            Top = 20,
            Text = text,
            Width = 340
        };

        // Поле ввода текста
        TextBox textBox = new TextBox()
        {
            Left = 20,
            Top = 50,
            Width = 340,
            Text = defaultValue
        };

        // Кнопка подтверждения
        Button confirmation = new Button()
        {
            Text = "ОК",
            Left = 280,
            Width = 80,
            Top = 80,
            DialogResult = DialogResult.OK
        };
        confirmation.Click += (sender, e) => { prompt.Close(); };

        // Добавление элементов на форму
        prompt.Controls.Add(textLabel);
        prompt.Controls.Add(textBox);
        prompt.Controls.Add(confirmation);
        prompt.AcceptButton = confirmation;

        // Отображение диалога и возврат результата
        return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
    }
}
