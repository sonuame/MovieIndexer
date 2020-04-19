using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MovieIndexer
{
    public partial class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Path { get; set; }
        public string Location { get; set; }
        public string DateAdded { get; set; }
        public long Epoch => ((long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
    }
    public partial class ScanPath
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string Location { get; set; }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public class ResultSort : IComparer
    {
        ListSortDirection direction;
        public ResultSort(ListSortDirection direction)
        {
            this.direction = direction;
        }

        public int Compare(object x, object y)
        {
            System.DateTime.TryParseExact(((Movie)x).DateAdded,
                "dd-MM-yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var _x);

            System.DateTime.TryParseExact(((Movie)y).DateAdded,
                "dd-MM-yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var _y);

            return direction == ListSortDirection.Ascending ? (_x > _y ? 1 : -1) : (_x > _y ? -1 : 1);
        }
    }
    public partial class MainWindow : Window
    {
        List<ScanPath> ScanPaths = new List<ScanPath>();
        List<Movie> Movies = new List<Movie>();
        bool updateButtonVisible = true;

        public string scanFile => Path.Combine(Directory.GetCurrentDirectory(), "scanPaths.txt");
        public string movieFile => Path.Combine(Directory.GetCurrentDirectory(), "movies.json");

        public MainWindow()
        {
            InitializeComponent();
            if (File.Exists(this.scanFile))
            {
                var scanFile = File.ReadAllLines(this.scanFile);
                ScanPaths = scanFile.Select(m => new ScanPath
                {
                    Location = m.Split(',')[1],
                    Path = m.Split(',')[0]
                }).ToList();
            }

            btnUpdateDB.IsEnabled = updateButtonVisible;

            grdMovies.AutoGenerateColumns = true;
            grdMovies.Loaded += GrdMovies_Loaded;
            grdMovies.Sorting += GrdMovies_Sorting;
            LoadGrid();
        }

        private void GrdMovies_Sorting(object sender, System.Windows.Controls.DataGridSortingEventArgs e)
        {
            DataGridColumn column = e.Column;
            if (column.Header.ToString() == "DateAdded")
            {
                IComparer comparer = null;
                e.Handled = true;

                ListSortDirection direction = (column.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;
                column.SortDirection = direction;
                ListCollectionView lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(grdMovies.ItemsSource);
                comparer = new ResultSort(direction);
                lcv.CustomSort = comparer;
            }

        }

        private void GrdMovies_Loaded(object sender, RoutedEventArgs e)
        {
            var datecol = grdMovies.Columns.FirstOrDefault(m => m.Header.ToString().Trim().Equals("DateAdded"));
            if (datecol != null)
            {

            }
        }

        private void btnUpdateDB_Click(object sender, RoutedEventArgs e)
        {
            var _movies = new List<Movie>();
            foreach (ScanPath item in ScanPaths)
            {
                try
                {
                    if (Directory.Exists(item.Path))
                        _movies.AddRange(Directory.EnumerateDirectories(item.Path, "*.*", SearchOption.TopDirectoryOnly).Select(m => new Movie()
                        {
                            Location = item.Location,
                            Path = m,
                            Title = Path.GetFileNameWithoutExtension(m),
                            DateAdded = new FileInfo(m).CreationTime.ToString("dd-MM-yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture)
                        }));
                }
                catch (Exception)
                {

                }
            }

            if (_movies.Count > 0)
            {
                Movies.Clear();
                Movies = _movies.Where(m => !m.Title.ToLower().Contains("recycle")).ToList();
                var count = 0;
                Movies.ForEach(m =>
                {
                    m.Id = ++count;
                    m.DateAdded = DateTime.Parse(m.DateAdded).ToString("dd-MM-yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                });
                if (File.Exists(this.movieFile))
                    File.Move(this.movieFile, this.movieFile + ".bak");
                File.WriteAllText(this.movieFile, JsonConvert.SerializeObject(Movies));
                LoadGrid();
            }
        }

        private void grdMovies_Loaded(object sender, RoutedEventArgs e)
        {
            lblStat.Content = "Total Movies - " + grdMovies.Items.Count;
            var epochColumn = grdMovies.Columns.FirstOrDefault(m => m.Header.ToString().ToLower() == "epoch");
            epochColumn.Visibility = Visibility.Hidden;
            //grdMovies.Columns.FirstOrDefault(m => m.Header.ToString().ToLower() == "dateadded").SortDirection = ListSortDirection.Descending;
        }

        public void LoadGrid()
        {
            if (File.Exists(this.movieFile))
                Movies = JsonConvert.DeserializeObject<List<Movie>>(File.ReadAllText(this.movieFile));
            Movies = Movies.OrderBy(m => m.Epoch).ToList();
            grdMovies.ItemsSource = Movies;
            lblStat.Content = "Total Movies - " + grdMovies.Items.Count;

        }

        private void txtTitle_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                grdMovies.ItemsSource = Movies.Where(m => m.Title.ToLower().Contains(txtTitle.Text.ToLower()));
            }
        }

        private void grdMovies_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Movie m = grdMovies.SelectedItem as Movie;
            System.Diagnostics.Process.Start(m.Path);
        }
    }
}
