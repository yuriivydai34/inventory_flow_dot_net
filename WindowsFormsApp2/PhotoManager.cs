using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace InventoryFlow
{
    public class PhotoManager : Form
    {
        private readonly PhotoApiClient _api;
        private readonly string _inventoryNumber;
        private ListView _list;
        private Button _btnAdd;
        private Button _btnOpen;
        private Button _btnSave;
        private Button _btnDelete;
        private Button _btnClose;

        public PhotoManager(string apiBaseUrl, string apiKey, string inventoryNumber)
        {
            _inventoryNumber = inventoryNumber;
            _api = new PhotoApiClient(apiBaseUrl, apiKey);
            BuildUi();
            Load += async (s, e) => await ReloadAsync();
        }

        private void BuildUi()
        {
            Text = $"Фото — {_inventoryNumber}";
            Width = 640;
            Height = 420;
            StartPosition = FormStartPosition.CenterParent;

            _list = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                MultiSelect = true,
            };
            _list.Columns.Add("Назва файлу", 300);
            _list.Columns.Add("Розмір", 100);
            _list.Columns.Add("Додано", 160);

            var panel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40, FlowDirection = FlowDirection.LeftToRight };
            _btnAdd = new Button { Text = "Додати фото..." };
            _btnOpen = new Button { Text = "Відкрити" };
            _btnSave = new Button { Text = "Зберегти як..." };
            _btnDelete = new Button { Text = "Видалити" };
            _btnClose = new Button { Text = "Закрити" };

            _btnAdd.Click += async (s, e) => await AddPhotosAsync();
            _btnOpen.Click += async (s, e) => await OpenSelectedAsync();
            _btnSave.Click += async (s, e) => await SaveSelectedAsync();
            _btnDelete.Click += async (s, e) => await DeleteSelectedAsync();
            _btnClose.Click += (s, e) => Close();

            panel.Controls.Add(_btnAdd);
            panel.Controls.Add(_btnOpen);
            panel.Controls.Add(_btnSave);
            panel.Controls.Add(_btnDelete);
            panel.Controls.Add(_btnClose);

            Controls.Add(_list);
            Controls.Add(panel);
        }

        private async System.Threading.Tasks.Task ReloadAsync()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                var photos = await _api.ListAsync(_inventoryNumber);
                _list.Items.Clear();
                foreach (var p in photos)
                {
                    var item = new ListViewItem(p.original_name) { Tag = p };
                    item.SubItems.Add(FormatSize(p.size_bytes));
                    item.SubItems.Add(p.created_at);
                    _list.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося завантажити список фото: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private async System.Threading.Tasks.Task AddPhotosAsync()
        {
            using (var dlg = new OpenFileDialog
            {
                Filter = "Зображення (*.jpg;*.jpeg;*.png;*.webp;*.heic;*.heif)|*.jpg;*.jpeg;*.png;*.webp;*.heic;*.heif",
                Multiselect = true,
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                Cursor = Cursors.WaitCursor;
                try
                {
                    foreach (var file in dlg.FileNames)
                        await _api.UploadAsync(_inventoryNumber, file);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося завантажити файл: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
                await ReloadAsync();
            }
        }

        private async System.Threading.Tasks.Task OpenSelectedAsync()
        {
            var photo = SelectedPhoto();
            if (photo == null) return;

            var tempPath = Path.Combine(Path.GetTempPath(), photo.original_name);
            try
            {
                Cursor = Cursors.WaitCursor;
                await _api.DownloadAsync(photo.id, tempPath);
                Process.Start(tempPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося відкрити файл: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private async System.Threading.Tasks.Task SaveSelectedAsync()
        {
            var photo = SelectedPhoto();
            if (photo == null) return;

            using (var dlg = new SaveFileDialog { FileName = photo.original_name })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    Cursor = Cursors.WaitCursor;
                    await _api.DownloadAsync(photo.id, dlg.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося зберегти файл: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private async System.Threading.Tasks.Task DeleteSelectedAsync()
        {
            if (_list.SelectedItems.Count == 0) return;
            if (MessageBox.Show($"Видалити обрані фото ({_list.SelectedItems.Count})?", "Підтвердження",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            Cursor = Cursors.WaitCursor;
            try
            {
                foreach (ListViewItem item in _list.SelectedItems.Cast<ListViewItem>().ToList())
                {
                    var photo = (PhotoInfo)item.Tag;
                    await _api.DeleteAsync(photo.id);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося видалити фото: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
            await ReloadAsync();
        }

        private PhotoInfo SelectedPhoto()
        {
            if (_list.SelectedItems.Count == 0) return null;
            return (PhotoInfo)_list.SelectedItems[0].Tag;
        }

        private static string FormatSize(long bytes)
        {
            if (bytes >= 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):0.0} МБ";
            if (bytes >= 1024) return $"{bytes / 1024.0:0.0} КБ";
            return $"{bytes} Б";
        }
    }
}
