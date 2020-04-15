using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MovieIndexer
{
    public partial class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Path { get; set; }
        public string Location { get; set; }
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
    public partial class MainWindow : Window
    {
        List<ScanPath> ScanPaths = new List<ScanPath>();
        List<Movie> Movies = new List<Movie>();

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

            grdMovies.AutoGenerateColumns = true;
            LoadGrid();
        }

        private void btnUpdateDB_Click(object sender, RoutedEventArgs e)
        {
            Movies.Clear();
            var _movies = new List<Movie>();
            foreach (ScanPath item in ScanPaths)
            {
                _movies.AddRange(Directory.EnumerateDirectories(item.Path, "*.*", SearchOption.TopDirectoryOnly).Select(m => new Movie()
                {
                    Id = Movies.Count + 1,
                    Location = item.Location,
                    Path = m,
                    Title = Path.GetFileNameWithoutExtension(m)
                }));
            }

            if (_movies.Count > 0) Movies.Clear();
            Movies = _movies.Where(m => !m.Title.ToLower().Contains("recycle")).ToList();
            File.WriteAllText(this.movieFile, JsonConvert.SerializeObject(Movies));

            LoadGrid();

        }

        private void grdMovies_Loaded(object sender, RoutedEventArgs e)
        {
            lblStat.Content = "Total Movies - " + grdMovies.Items.Count;
        }

        public void LoadGrid()
        {
            if (File.Exists(this.movieFile))
                Movies = JsonConvert.DeserializeObject<List<Movie>>(File.ReadAllText(this.movieFile));
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
