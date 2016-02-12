using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using AmazonSearcher;
using BookDatabase;


namespace AddBookForm
{
    public partial class MainWindow
    {
        #region Fields
        private const string CLOSE_COMMAND = "close";
        private const string ISBN_CAPTURE = @"(?i)isbn[\D]+([\d-]+)";
        private const string OPEN_COMMAND = "open";
        private const string SQLSERVER = "sqlServer";

        private const string SQLSERVER_PROCESS =
            @"F:\C#\ServiceCommander\ServiceCommander\bin\Release\ServiceCommander.exe";

        private readonly List<Button> _addCmdButtons;
        private BookModel _bookDbContext;
        private readonly Queue<string> _fileQueue = new Queue<string>();
        private string _lastClipboardText;
        #endregion


        #region  Constructors & Destructor
        public MainWindow()
        {
            InitializeComponent();
            _addCmdButtons = new List<Button>
            {
                cmdAddBookItem,
                cmdAddBookEdition,
                cmdAddBookTitle,
                cmdAddBookTopic,
                cmdAddEbookType,
                cmdAddAuthor
            };
            BookTopic.RootFolderPath = @"D:\Book";
            AddEventHandlers();
        }
        #endregion


        #region Override
        protected override async void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (!Clipboard.ContainsText()) return;

            var clipboardText = Clipboard.GetText();
            if (clipboardText.Equals(_lastClipboardText)) return;
            _lastClipboardText = clipboardText;

            if (!DoWantToSearchBook(clipboardText)) return;
            await SearchBookAsync(clipboardText);
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            await HandleSqlServerServiceAsync(CLOSE_COMMAND);
        }
        #endregion


        #region Event Handlers
        private void AmazonSearch_OnBookInfoSent(object sender, BookInfoEventArgs e)
        {
            txtTitle.Text = e.Info.Title;
        }

        private void CmdAddAuthor_OnClick(object sender, RoutedEventArgs e)
        {
            var author = ctnNewAuthor.DataContext as Author;
            AddOrRemoveSelectedAuthor(author);
        }

        private async void CmdGoAmazonIsbn_OnClick(object sender, RoutedEventArgs e)
        {
            var isbn = txtIsbn.Text;
            await SearchBookAsync(isbn);
        }

        private async void CmdGoAmazonTitle_OnClick(object sender, RoutedEventArgs e)
        {
            var title = txtTitle.Text;
            await SearchBookAsync(title);
        }

        private void LstAuthorsButtons_OnClick(object sender, RoutedEventArgs e)
        {
            AddOrRemoveSelectedAuthor(e, true);
        }

        private void LstBookEditions_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFilePath();
        }

        private void LstEbookTypes_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFilePath();
        }

        private void LstSelectedAuthorsButton_OnClick(object sender, RoutedEventArgs e)
        {
            AddOrRemoveSelectedAuthor(e, false);
        }

        private void LstSuperTopics_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFolderPath();
        }

        private async void MainWindow_OnDrop(object sender, DragEventArgs e)
        {
            /*var dropItems = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            if (dropItems != null)
            {
                QueueFiles(dropItems);
            }*/

            var files = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            if (files == null || files.Length == 0) return;

            var file = files[0];
            OpenFile(file);
            await SearchFileInfoAsync(file);
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            await HandleSqlServerServiceAsync(OPEN_COMMAND);
            _bookDbContext = new BookModel();
            await BindDataAsync();
        }

        private void TxtIsbn_OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            var pastingText = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
            e.CancelCommand();
            if (pastingText == null) return;

            var normalizedText = Regex.Replace(pastingText, @"[^\d]", "");
            txtIsbn.Text = normalizedText;
        }

        private void TxtTopic_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFolderPath();
        }
        #endregion


        #region Implementation
        private async Task<TItem> AddDbItem<TItem>(FrameworkElement dataContextContainer, IDbSet<TItem> dbSet)
            where TItem: class, new()
        {
            var item = dataContextContainer.DataContext as TItem;
            if (!await AddDbItem(item, dbSet)) return null;

            dataContextContainer.DataContext = new TItem();
            return item;
        }

        private async Task<bool> AddDbItem<TItem>(TItem item, IDbSet<TItem> dbSet) where TItem: class
        {
            try
            {
                dbSet.Add(item);
                await _bookDbContext.SaveChangesAsync();
                await dbSet.LoadAsync(); // Update DbSet.Local
                return true;
            }
            catch (Exception exception)
            {
                while (exception.InnerException != null)
                {
                    exception = exception.InnerException;
                }
                MessageBox.Show(exception.Message, "Adding Item Error");
                return false;
            }
        }

        private void AddEventHandlers()
        {
            DataObject.AddPastingHandler(txtIsbn, TxtIsbn_OnPaste);
        }

        private void AddOrRemoveSelectedAuthor(Author author, bool add = true)
        {
            var selectedAuthors = lstSelectedAuthors.ItemsSource as ICollection<Author>;
            if (author == null || selectedAuthors == null) return;
            if (!add)
            {
                selectedAuthors.Remove(author);
            }
            else if (!selectedAuthors.Contains(author))
            {
                selectedAuthors.Add(author);
            }
            lstSelectedAuthors.Items.Refresh();
        }

        private void AddOrRemoveSelectedAuthor(RoutedEventArgs e, bool add)
        {
            var button = e.OriginalSource as Button;
            if (button != null)
            {
                var author = button.DataContext as Author;
                AddOrRemoveSelectedAuthor(author, add);
            }
        }

        private async Task BindDataAsync()
        {
            txtQueue.DataContext = _fileQueue;
            _addCmdButtons.ForEach(btn => btn.IsEnabled = false);
            await BindDataAsync(_bookDbContext.BookItems, ctnNewBookItem, cmdAddBookItem, null);
            await
                BindDataAsync(_bookDbContext.EbookTypes, ctnNewEbookType, cmdAddEbookType, tglEbookType, lstEbookTypes);
            await
                BindDataAsync(_bookDbContext.BookEditions, ctnNewBookEdition, cmdAddBookEdition, tglBookEdition,
                    lstBookEditions);
            await
                BindDataAsync(_bookDbContext.BookTitles, ctnNewBookTitle, cmdAddBookTitle, tglBookTitle, lstBookTitles);
            await
                BindDataAsync(_bookDbContext.BookTopics, ctnNewBookTopic, cmdAddBookTopic, tglBookTopic, lstBookTopics,
                    lstSuperTopics);
            await BindDataAsync(_bookDbContext.Authors, ctnNewAuthor, cmdAddAuthor, tglAuthor, lstAuthors);
            _addCmdButtons.ForEach(btn => btn.IsEnabled = true);
        }

        private async Task BindDataAsync<TItem>(IDbSet<TItem> dbSet, FrameworkElement newItemContainer,
            ButtonBase addItemButton, ToggleButton expandToggleButton, params Selector[] itemsContainers)
            where TItem: class, new()
        {
            await dbSet.LoadAsync();
            foreach (var itemsContainer in itemsContainers)
            {
                itemsContainer.ItemsSource = dbSet.Local;
            }

            newItemContainer.DataContext = new TItem();

            addItemButton.Click += async (o, e) =>
            {
                addItemButton.IsEnabled = false;
                var selectedItem = await AddDbItem(newItemContainer, dbSet);
                if (selectedItem != null)
                {
                    foreach (var itemsContainer in itemsContainers)
                    {
                        itemsContainer.SelectedItem = selectedItem;
                    }
                    if (expandToggleButton != null)
                    {
                        expandToggleButton.IsChecked = false;
                    }
                }
                addItemButton.IsEnabled = true;
            };
        }

        private async Task CloseSqlServerAsync()
        {
            /*if (_sqlServerService.Controller.Status != ServiceControllerStatus.Stopped
                && DoWantToCloseServer() && !await _sqlServerService.StopAsync(_sqlServerServiceTimeout))
                MessageBox.Show("Cannot close SQL Server service. Please close it manually!", Title);*/
        }

        private bool DoWantTo(string content)
            => MessageBox.Show(content, Title, MessageBoxButton.YesNo) == MessageBoxResult.Yes;

        private bool DoWantToCloseServer()
            => DoWantTo("SQL Server service is still running. Do you want to stop the service?");

        private bool DoWantToOpenServer()
            => DoWantTo("SQL Server service is not running. Do you want to start the service?");

        private bool DoWantToSearchBook(string search)
            => DoWantTo($"Do you want to search for {search}?");

        private static string GetSqlServerServiceName()
        {
            return ConfigurationManager.AppSettings[SQLSERVER];
        }

        private void HandleNextFile()
        {
            var nextFile = _fileQueue.Dequeue();
        }

        private static void HandleSqlServerService(string command)
        {
            var sqlServerProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = SQLSERVER_PROCESS,
                    Arguments = $"{command} {GetSqlServerServiceName()}",
                    Verb = "runas"
                }
            };
            sqlServerProcess.Start();
            sqlServerProcess.WaitForExit();
        }

        private static async Task HandleSqlServerServiceAsync(string command)
        {
            /*if (_sqlServerService.Controller.Status != ServiceControllerStatus.Running
                && DoWantToOpenServer() && !await _sqlServerService.StartAsync(_sqlServerServiceTimeout))
                MessageBox.Show("Cannot open SQL Server service. Please open it manually!", Title);*/
            await Task.Run(() => HandleSqlServerService(command));
        }

        private static void OpenFile(string filePath)
        {
            Process.Start(filePath);
        }

        private void QueueFiles(IEnumerable<string> fileSystemEntries)
        {
            foreach (var entry in fileSystemEntries)
            {
                if (File.Exists(entry))
                {
                    _fileQueue.Enqueue(entry);
                }
                else if (Directory.Exists(entry))
                {
                    QueueFiles(Directory.EnumerateFileSystemEntries(entry));
                }
            }
        }

        private async Task SearchBookAsync(string search)
        {
            var amazonSearch = new AmazonSearcher.MainWindow
            {
                WindowState = WindowState.Maximized
            };
            amazonSearch.BookInfoSent += AmazonSearch_OnBookInfoSent;
            amazonSearch.Show();

            //amazonSearch.SearchBookAsync(search);
            await amazonSearch.SearchBook(search);
            amazonSearch.Activate();
            WindowState = WindowState.Minimized;
        }

        private async Task SearchFileInfoAsync(string filePath)
        {
            using (var textFinder = new TextFinder(filePath))
            {
                var isbnMatch = await textFinder.FindAsync(ISBN_CAPTURE);
                var search = isbnMatch?.Groups[1].Value ?? Path.GetFileNameWithoutExtension(filePath);
                await SearchBookAsync(search);
            }
        }

        private void UpdateFilePath()
        {
            var bookItem = ctnNewBookItem.DataContext as BookItem;
            txtFilePath.Text = bookItem?.SuggestFilePath();
        }

        private void UpdateFolderPath()
        {
            var bookTopic = ctnNewBookTopic.DataContext as BookTopic;
            txtFolderPath.Text = bookTopic?.SuggestFolderPath();
        }
        #endregion


        /*private readonly Service _sqlServerService = new Service(GetSqlServerServiceName());
        private readonly TimeSpan _sqlServerServiceTimeout = TimeSpan.FromSeconds(8);*/
    }
}


//TODO: Stored Procedures: Map
//TODO: Stored Procedures Delete, Update: Foreign Key Check: Edit
//TODO: ComboBoxItem DataTemplate: Button Add: Test
//TODO: ListBoxItem DataTemplate: Button Remove: Test
//TODO: Asynchronous Data Load: Test
//TODO: Go Amazon: ISBN & Title: Implement
//TODO: BookEdition Model: Add Price Property
//TODO: Read PDF to find ISBN
//TODO: GeneralInfo not replace
//TODO: right time to check Clipboard