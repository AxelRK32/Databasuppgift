using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Databasuppgift
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MySqlConnection conn;
        bool singelOrSeries;
        public MainWindow()
        {
            InitializeComponent();
            userInfo info = new userInfo();

            string server = "localhost";
            string database = "bookdatabase";
            string user = info.getUsername();
            string pass = info.getPassword();

            string connString = $"SERVER={server};DATABASE={database};UID={user};PASSWORD={pass};";
            conn = new MySqlConnection(connString);

        }
        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Single_book_Click(object sender, RoutedEventArgs e)
        {          
            singelOrSeries = true;
            SelectFromDatabase(Searchbartxt.Text);
        }
        private void Series_Click(object sender, RoutedEventArgs e)
        {
            singelOrSeries = false;
            SelectFromDatabase(Searchbartxt.Text);
        }
       
        private void newSingleBook_Click(object sender, RoutedEventArgs e)
        {
            singelOrSeries = true;
            AddNew addNew = new AddNew(singelOrSeries);
            addNew.ShowDialog();
        }

        private void newSeriesBook_Click(object sender, RoutedEventArgs e)
        {
            singelOrSeries = false;
            AddNew addNew = new AddNew(singelOrSeries);
            addNew.ShowDialog();
        }

        private void SelectFromDatabase(string keyword = "")
        {
            string SQLquerry = "";
            if (singelOrSeries == true && keyword == "")
            {
                SQLquerry = "SELECT * FROM all_single_books;";
            }
            else if (singelOrSeries == true && keyword != "")
            {
                SQLquerry = $"CALL SearchSingle('{keyword}');";
            }
            else if (singelOrSeries == false && keyword == "")
            {
                SQLquerry = "SELECT * FROM all_series;";
            }
            else if (singelOrSeries == false && keyword != "")
            {
                SQLquerry = $"CALL SearchSeries('{keyword}');";
            }

            MySqlCommand cmd = new MySqlCommand(SQLquerry, conn);
            
                try
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    da.Fill(dt);
                    datagrid.ItemsSource = dt.DefaultView;                
                    conn.Close();

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
        }
        private void datagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            getSelectedGenre();
            
        }
        private void getSelectedGenre()
        {
            DataRowView dataRowView = (DataRowView)datagrid.SelectedItem;
            int id = Convert.ToInt32(dataRowView.Row[0]);
            string SQLquerry = "";
            selecteditem.Content = dataRowView.Row[1];

            //if statment to see what stored procedure SQLquerry will call
            if (singelOrSeries == true)
            {
                SQLquerry = $"CALL ViewGenreForBook('{id}');";
            }
            else if (singelOrSeries == false)
            {
                SQLquerry = $"CALL ViewGenreForSeries('{id}')";
                getBooksInSeries();
            }

            MySqlCommand cmd = new MySqlCommand(SQLquerry, conn);
            try
            {
                conn.Open();

                MySqlDataReader reader = cmd.ExecuteReader();
                DataTable dataTable = new DataTable();
                dataTable.Load(reader);
                genreGrid.ItemsSource = dataTable.DefaultView;

                conn.Close();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }
        private void getBooksInSeries()
        {
            DataRowView dataRowView = (DataRowView)datagrid.SelectedItem;
            int id = Convert.ToInt32(dataRowView.Row[0]);
            string SQLquerry = $"CALL ViewBooksInSeries('{id}');";

            MySqlCommand cmd = new MySqlCommand(SQLquerry, conn);
            try
            {
                conn.Open();

                MySqlDataReader reader = cmd.ExecuteReader();
                DataTable dataTable = new DataTable();
                dataTable.Load(reader);
                SeriesbooksGrid.ItemsSource = dataTable.DefaultView;

                conn.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void deletebtn_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItems.Count != 1) return;
            string querry = "";
            if (singelOrSeries)
            {

                DataRowView row = (DataRowView)datagrid.SelectedItem;
                int id = Convert.ToInt32(row[0]);
                querry = $"CALL DeleteSingle({id});";
            }
            else if (!singelOrSeries)
            {
                //delete series
                return; //temporary until delete series is implemented
            }
                MySqlCommand cmd = new MySqlCommand(querry, conn);
                try
                {
                    conn.Open();

                    cmd.ExecuteReader();

                    conn.Close();
                    MessageBox.Show("Deletion successfull");

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        private void updatebtn_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItems.Count != 1) return;
            singelOrSeries = true;
            DataRowView row = (DataRowView)datagrid.SelectedItem;
            AddNew addNew = new AddNew(singelOrSeries,Convert.ToInt32(row.Row[0]));
            addNew.ShowDialog();
        }
    }
}
