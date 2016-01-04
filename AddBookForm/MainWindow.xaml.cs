using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using BookDatabase;


namespace AddBookForm
{
    public partial class MainWindow
    {
        #region Fields
        private readonly List<Button> _addCmdButtons;
        private readonly Queue<string> _fileQueue = new Queue<string>();
        private readonly BookModel _bookDbContext = new BookModel();
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


        #region Event Handlers
        private void CmdAddAuthor_OnClick(object sender, RoutedEventArgs e)
        {
            var author = ctnNewAuthor.DataContext as Author;
            AddOrRemoveSelectedAuthor(author);
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

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            await BindData();
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
            where TItem : class, new()
        {
            var item = dataContextContainer.DataContext as TItem;
            if (!await AddDbItem(item, dbSet)) return null;

            dataContextContainer.DataContext = new TItem();
            return item;
        }

        private async Task<bool> AddDbItem<TItem>(TItem item, IDbSet<TItem> dbSet) where TItem : class
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

        private async Task BindData()
        {
            txtQueue.DataContext = _fileQueue;
            _addCmdButtons.ForEach(btn => btn.IsEnabled = false);
            await BindData(_bookDbContext.BookItems, ctnNewBookItem, cmdAddBookItem, null);
            await BindData(_bookDbContext.EbookTypes, ctnNewEbookType, cmdAddEbookType, tglEbookType, lstEbookTypes);
            await BindData(_bookDbContext.BookEditions, ctnNewBookEdition, cmdAddBookEdition, tglBookEdition, lstBookEditions);
            await BindData(_bookDbContext.BookTitles, ctnNewBookTitle, cmdAddBookTitle, tglBookTitle, lstBookTitles);
            await BindData(_bookDbContext.BookTopics, ctnNewBookTopic, cmdAddBookTopic, tglBookTopic, lstBookTopics, lstSuperTopics);
            await BindData(_bookDbContext.Authors, ctnNewAuthor, cmdAddAuthor, tglAuthor, lstAuthors);
            _addCmdButtons.ForEach(btn => btn.IsEnabled = true);
        }

        private async Task BindData<TItem>(IDbSet<TItem> dbSet, FrameworkElement newItemContainer,
            ButtonBase addItemButton, ToggleButton expandToggleButton, params Selector[] itemsContainers)
            where TItem : class, new()
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


        private void MainWindow_OnPreviewDragEnter(object sender, DragEventArgs e)
        {

        }

        private void MainWindow_OnDrop(object sender, DragEventArgs e)
        {
            var dropItems = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            if (dropItems != null)
            {
                QueueFiles(dropItems);
            }
        }

        private void QueueFiles(IEnumerable<string> fileSystemEntries)
        {
            foreach (var entry in fileSystemEntries)
            {
                if (File.Exists(entry))
                {
                    _fileQueue.Enqueue(entry);
                }
                else if(Directory.Exists(entry))
                {
                    QueueFiles(Directory.EnumerateFileSystemEntries(entry));
                }
            }
        }

        private void HandleNextFile()
        {
            var nextFile = _fileQueue.Dequeue();
        }

        private void CmdGoAmazonIsbn_OnClick(object sender, RoutedEventArgs e)
        {
            var amazonSearch = new AmazonSearcher.MainWindow();
            amazonSearch.BookInfoSent += AmazonSearch_OnBookInfoSent;
            amazonSearch.Show();
            amazonSearch.SearchBookIsbn(txtIsbn.Text);
        }

        private void AmazonSearch_OnBookInfoSent(object sender, AmazonSearcher.BookInfoEventArgs e)
        {
            txtTitle.Text = e.Info.Title;
        }
    }
}


//TODO: Stored Procedures Delete, Update: Foreign Key Check: Edit
//TODO: ComboBoxItem DataTemplate: Button Add: Test
//TODO: ListBoxItem DataTemplate: Button Remove: Test
//TODO: Asynchronous Data Load: Test
//TODO: Go Amazon: ISBN & Title: Implement
//TODO: BookEdition Model: Add Price Property
