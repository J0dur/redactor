using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.ComponentModel;

namespace redactor
{
    public partial class Form1 : Form
    {
        string currentFilePath = null;
        OpenFileDialog openFileDialog = new OpenFileDialog();
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        FontDialog fontDialog = new FontDialog();
        ColorDialog colorDialog = new ColorDialog();

        public Form1()
        {
            InitializeComponent();
            menuStrip.Renderer = new CustomMenuStripRenderer();
            this.FormClosing += Form1_FormClosing;
            ConfigureDialogs();
            ConfigureDialogs();
            this.KeyPreview = true;
            this.KeyDown += Form_KeyDown;



            // Загружаем сохраненную тему (по умолчанию false - светлая)
            bool isDarkMode = Properties.Settings.Default.DarkMode;

            // Применяем тему
            ApplyTheme(isDarkMode);

            // Обновляем галочки в меню
            UpdateThemeMenuItems(isDarkMode);
        }













        private void ApplyTheme(bool darkMode)
        {
            // Цвета для выбранной темы
            Color backColor, foreColor, menuBackColor, menuForeColor;

            if (darkMode)
            {
                // Темная тема
                backColor = Color.FromArgb(30, 30, 30);
                foreColor = Color.WhiteSmoke;
                menuBackColor = Color.FromArgb(45, 45, 45);
                menuForeColor = Color.White;
            }
            else
            {
                // Светлая тема
                backColor = SystemColors.Window;
                foreColor = SystemColors.WindowText;
                menuBackColor = SystemColors.MenuBar;
                menuForeColor = SystemColors.MenuText;
            }

            // Применяем цвета к элементам формы
            this.BackColor = backColor;

            // RichTextBox
            richTextBox_main.BackColor = backColor;
            richTextBox_main.ForeColor = foreColor;

            // MenuStrip
            menuStrip.BackColor = menuBackColor;
            menuStrip.ForeColor = menuForeColor;

            // Настройка рендерера для MenuStrip
            //menuStrip.Renderer = new CustomMenuStripRenderer(darkMode);

            // Сохраняем выбор темы
            Properties.Settings.Default.DarkMode = darkMode;
            Properties.Settings.Default.Save();
        }

        private void UpdateThemeMenuItems(bool darkMode)
        {
            // Устанавливаем галочку только для активной темы
            тёмнаяToolStripMenuItem.Checked = darkMode;
            светлаяПоУмолчаниюToolStripMenuItem.Checked = !darkMode;
        }



































        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                SaveFile();
                e.Handled = true;
            }
        }


        private void ConfigureDialogs()
        {
            
            saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt";
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.AddExtension = true;
            openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
        }

        public class CustomMenuStripRenderer : ToolStripProfessionalRenderer
        {
            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                base.OnRenderToolStripBorder(e);

                using (var pen = new Pen(Color.WhiteSmoke, 2))
                {
                    e.Graphics.DrawLine(pen,
                        new Point(0, e.ToolStrip.Height - 1),
                        new Point(e.ToolStrip.Width, e.ToolStrip.Height - 1));
                }
            }
        }

        private void SaveFile()
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                сохранитьКакToolStripMenuItem_Click(null, EventArgs.Empty);
            }
            else
            {
                try
                {
                    File.WriteAllText(currentFilePath, richTextBox_main.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }

        
        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(richTextBox_main.Text) &&
                MessageBox.Show("Сохранить изменения?", "Блокнот",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                SaveFile();
            }
            richTextBox_main.Clear();
            currentFilePath = null;
            this.Text = "Безымянный - Мой Блокнот";
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    richTextBox_main.Text = File.ReadAllText(openFileDialog.FileName);
                    currentFilePath = openFileDialog.FileName;
                    this.Text = Path.GetFileName(currentFilePath) + " - Мой Блокнот";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                currentFilePath = saveFileDialog.FileName;

                
                if (!currentFilePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    currentFilePath += ".txt";
                }

                SaveFile();
            }
        }

        private void шрифтToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (richTextBox_main.SelectionLength > 0)
            {
                fontDialog.ShowEffects = true;
                fontDialog.Font = richTextBox_main.SelectionFont ?? richTextBox_main.Font;

                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    richTextBox_main.SelectionFont = fontDialog.Font;
                }
            }
            else
            {
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    richTextBox_main.Font = fontDialog.Font;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (HasUnsavedChanges())
            {
                var result = MessageBox.Show("Сохранить изменения перед выходом?", "Redactor",
                                          MessageBoxButtons.YesNoCancel,
                                          MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    SaveFile();
                    if (string.IsNullOrEmpty(currentFilePath) && !string.IsNullOrEmpty(richTextBox_main.Text))
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private bool HasUnsavedChanges()
        {
            if (string.IsNullOrEmpty(currentFilePath) && !string.IsNullOrEmpty(richTextBox_main.Text))
                return true;
            if (!string.IsNullOrEmpty(currentFilePath))
            {
                try
                {
                    string savedText = File.ReadAllText(currentFilePath);
                    return savedText != richTextBox_main.Text;
                }
                catch
                {
                    return true;
                }
            }

            return false;
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(richTextBox_main.Text))
            {
                var result = MessageBox.Show("Сохранить изменения перед выходом?", "Redacror",
                                           MessageBoxButtons.YesNoCancel,
                                           MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    SaveFile();
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            Application.Exit();
        }

        private void сохранитьToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void цветТекстаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (richTextBox_main.SelectionLength > 0)
            {
                colorDialog.Color = richTextBox_main.SelectionColor;
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    richTextBox_main.SelectionColor = colorDialog.Color;
                }
            }
            else 
            {
                colorDialog.Color = richTextBox_main.ForeColor;
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    richTextBox_main.ForeColor = colorDialog.Color;
                }
            }
        }

        private void фонТекстаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (richTextBox_main.SelectionLength > 0)
            {
                colorDialog.Color = richTextBox_main.SelectionBackColor;
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    richTextBox_main.SelectionBackColor = colorDialog.Color;
                }
            }
            else
            {
                richTextBox_main.SelectAll();
                colorDialog.Color = richTextBox_main.SelectionBackColor;
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    richTextBox_main.SelectionBackColor = colorDialog.Color;
                }
            }
        }

        private void тёмнаяToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Устанавливаем темную тему
            ApplyTheme(darkMode: true);

            // Обновляем галочки в меню
            UpdateThemeMenuItems(darkMode: true);
        }

        private void светлаяПоУмолчаниюToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Устанавливаем светлую тему
            ApplyTheme(darkMode: false);

            // Обновляем галочки в меню
            UpdateThemeMenuItems(darkMode: false);
        }
    }
}