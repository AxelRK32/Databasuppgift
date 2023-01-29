using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Databasuppgift
{
    /// <summary>
    /// Interaction logic for AddNew.xaml
    /// </summary>
    public partial class AddNew : Window
    {
        MySqlConnection conn;
        int x = 0;
        DataTable genres = new DataTable();
        bool Sos;
        int authorid = -1;
        int seriesid = -1;
        int updateID = -1;

        public AddNew(bool sos)
        {
            InitializeComponent();
            genres.Columns.Add("ID", typeof(string));
            genres.Columns.Add("Genre", typeof(string));
            userInfo info = new userInfo();

            string server = "localhost";
            string database = "bookdatabase";
            string user = info.getUsername();
            string pass = info.getPassword();

            string connString = $"SERVER={server};DATABASE={database};UID={user};PASSWORD={pass};";
            conn = new MySqlConnection(connString);

            Sos = SingleOrSeriesSetup(sos);

        }
        public AddNew(bool sos, int ID)
        {
            InitializeComponent();
            genres.Columns.Add("ID", typeof(string));
            genres.Columns.Add("Genre", typeof(string));
            userInfo info = new userInfo();

            string server = "localhost";
            string database = "bookdatabase";
            string user = info.getUsername();
            string pass = info.getPassword();

            string connString = $"SERVER={server};DATABASE={database};UID={user};PASSWORD={pass};";
            conn = new MySqlConnection(connString);

            Sos = SingleOrSeriesSetup(sos);
            updateID = ID;
            UpdateSetup(ID);
        }
        private void UpdateSetup(int id)
        {
            authorbtn.IsEnabled = false;
            genrebtn.IsEnabled = false;
            seriesbtn.IsEnabled = false;
            Title = "Update single book";
            int bookid = -1;
            string genreQuerry = $"CALL ViewGenreForBook({id});";
            MySqlCommand cmd = new MySqlCommand(genreQuerry, conn);

            try
            {
                conn.Open();

                DataTable dt = new DataTable();
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(dt);
                chosenGenreGrid.ItemsSource = dt.DefaultView;

                conn.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            string SQLquerry = $"SELECT * FROM `single book` WHERE book_id = {id};";
            MySqlCommand mySql = new MySqlCommand(SQLquerry, conn);
            try
            {
                conn.Open();
                using (MySqlDataReader view = mySql.ExecuteReader())
                {
                    if (view.Read())
                    {
                        bookid = (int)view[0];
                        nametxt.Text = (string)view[1];
                        yeartxt.Text = view[2].ToString();
                        authorid = (int)view[4];
                    }
                }
                conn.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            string authorquerry = $"SELECT * FROM author WHERE author_id = {authorid};";
            MySqlCommand mySql2 = new MySqlCommand(authorquerry, conn);
            try
            {
                conn.Open();
                using (MySqlDataReader view2 = mySql2.ExecuteReader())
                {
                    if (view2.Read())
                    {
                        authorlbl.Content = view2[1];
                    }
                }
                conn.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string status = statusbox.Text;
        }

        private void cancelbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool SingleOrSeriesSetup(bool sos)
        {
            if (sos /*&& updateID == -1*/)
            {
                seriesbtn.IsEnabled = false;
                ordertxt.IsEnabled = false;
                seriesGrid.IsEnabled = false;
                genreGrid.IsEnabled = true;
                Title = "Add single book";
                fillDataGrids("");
                return true;
            }
            else //if (!sos && updateID == -1)
            {
                seriesbtn.IsEnabled = true;
                ordertxt.IsEnabled = true;
                seriesGrid.IsEnabled = true;
                genreGrid.IsEnabled = false;
                string SQLquerry = $"SELECT * FROM all_series;";
                Title = "Add book in series";
                fillDataGrids(SQLquerry);
                return false;
            }

        }
        private void fillDataGrids(string SQLquerry)
        {
            if (SQLquerry != "")
            {
                MySqlCommand cmd = new MySqlCommand(SQLquerry, conn);

                try
                {
                    conn.Open();

                    DataTable dt = new DataTable();
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    da.Fill(dt);
                    seriesGrid.ItemsSource = dt.DefaultView;


                    conn.Close();

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            string SQLquerry1 = $"SELECT * FROM all_authors;";
            string SQLquerry2 = $"SELECT * FROM all_genres;";

            MySqlCommand cmd1 = new MySqlCommand(SQLquerry1, conn);
            MySqlCommand cmd2 = new MySqlCommand(SQLquerry2, conn);

            try
            {
                conn.Open();

                DataTable dt1 = new DataTable();
                MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1);
                da1.Fill(dt1);
                authorGrid.ItemsSource = dt1.DefaultView;

                DataTable dt2 = new DataTable();
                MySqlDataAdapter da2 = new MySqlDataAdapter(cmd2);
                da2.Fill(dt2);
                genreGrid.ItemsSource = dt2.DefaultView;

                conn.Close();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }


        private void seriesbtn_Click(object sender, RoutedEventArgs e)
        {
            yeartxt.IsEnabled = false;
            ordertxt.IsEnabled = false;
            authorbtn.IsEnabled = false;
            genrebtn.IsEnabled = false;
            authorGrid.IsEnabled = false;
            seriesGrid.IsEnabled = false;
            statusbox.IsEnabled = false;
            finishbtn.IsEnabled = false;
            ordertxt.IsEnabled = false;
            genreGrid.IsEnabled = true;
            x += 1;
            int seriesID = -1;
            if (x != 1)
            {
                bool ok = nametxtValid();
                if (!ok)
                {
                    MessageBox.Show("Please enter the series name in the name field!");
                    nametxt.Background = Brushes.Red;
                    x = 1;
                }
                else
                {
                    string SQLquerry = $"CALL InsertSeries('{nametxt.Text}');";
                    MySqlCommand cmd = new MySqlCommand(SQLquerry, conn);
                    nametxt.Background = default;

                    try
                    {
                        conn.Open();
                        cmd.ExecuteReader();
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    try 
                    { 
                        conn.Open();
                        string maxid = $"SELECT series_id FROM `book series` WHERE series_id=(SELECT MAX(series_id) FROM `book series`);";
                        MySqlCommand mySql = new MySqlCommand(maxid, conn);
                        using (MySqlDataReader view = mySql.ExecuteReader())
                        {
                            if (view.Read())
                            {
                                seriesID = (int)view[0];
                            }
                        }
                        conn.Close();

                        foreach (DataRowView row in chosenGenreGrid.Items)
                        {
                            int genreID = Convert.ToInt32(row.Row.ItemArray[0]);
                            conn.Open();
                            string SQLquerry2 = $"CALL AssignSeriesGenre({seriesID},{genreID});";
                            MySqlCommand assign = new MySqlCommand(SQLquerry2, conn);
                            assign.ExecuteReader();
                            conn.Close();
                        }
                        MessageBox.Show("Creation successfull");
                        seriesGrid.Items.Refresh();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    x = 0;

                    yeartxt.IsEnabled = true;
                    ordertxt.IsEnabled = true;
                    authorbtn.IsEnabled = true;
                    genrebtn.IsEnabled = true;
                    authorGrid.IsEnabled = true;
                    seriesGrid.IsEnabled = true;
                    statusbox.IsEnabled = true;
                    finishbtn.IsEnabled = true;
                    ordertxt.IsEnabled = true;
                }
            }
        }
        private bool nametxtValid()
        {
            bool valid = true;
            if (nametxt.Text.Equals(""))
            {
                valid = false;
            }

            return valid;
        }

        private void genreGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)genreGrid.SelectedItem;
            var row = genres.NewRow();
            row["ID"] = dataRowView.Row[0];
            row["Genre"] = dataRowView.Row[1];
            genres.Rows.Add(row);
            chosenGenreGrid.ItemsSource = genres.DefaultView;
        }

        private void genrebtn_Click(object sender, RoutedEventArgs e)
        {
            yeartxt.IsEnabled = false;
            ordertxt.IsEnabled = false;
            authorbtn.IsEnabled = false;
            authorGrid.IsEnabled = false;
            seriesGrid.IsEnabled = false;
            statusbox.IsEnabled = false;
            finishbtn.IsEnabled = false;
            ordertxt.IsEnabled = false;
            genreGrid.IsEnabled = false;
            chosenGenreGrid.IsEnabled = false;
            seriesbtn.IsEnabled = false;
            x += 1;
            if (x != 1)
            {
                bool ok = nametxtValid();
                if (!ok)
                {
                    MessageBox.Show("Please enter the genre name in the name field!");
                    nametxt.Background = Brushes.Red;
                    x = 1;
                }
                else
                {
                    string SQLquerry = $"CALL InsertNewGenre ('{nametxt.Text}');";
                    MySqlCommand cmd = new MySqlCommand(SQLquerry, conn);
                    try
                    {
                        conn.Open();
                        cmd.ExecuteReader();
                        conn.Close();
                        MessageBox.Show("Creation successfull");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    x = 0;
                    yeartxt.IsEnabled = true;
                    ordertxt.IsEnabled = true;
                    authorbtn.IsEnabled = true;
                    authorGrid.IsEnabled = true;
                    seriesGrid.IsEnabled = true;
                    statusbox.IsEnabled = true;
                    finishbtn.IsEnabled = true;
                    ordertxt.IsEnabled = true;
                    genreGrid.IsEnabled = true;
                    chosenGenreGrid.IsEnabled = true;
                    seriesbtn.IsEnabled = true;
                }
            }
            CollectionViewSource.GetDefaultView(genreGrid.ItemsSource).Refresh();
        }

        private void chosenGenreGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)chosenGenreGrid.SelectedItem;
            dataRowView.Delete();
        }

        private void finishbtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Convert.ToInt32(yeartxt.Text);
            }
            catch
            {
                MessageBox.Show("Please enter a number in the year field");
                yeartxt.Background = Brushes.Red;
                return;
            }
            if (!Sos)
            {
                try
                {
                    Convert.ToInt32(ordertxt.Text);
                }
                catch
                {
                    MessageBox.Show("Please enter a number for the reading order");
                    ordertxt.Background = Brushes.Red;
                    return;
                }
            }
            if (!nametxtValid() || authorlbl.Content.Equals(""))
            {
                if (!nametxtValid())
                {
                    MessageBox.Show("Please enter the book name in the name field");
                    nametxt.Background = Brushes.Red;
                }
                if (authorlbl.Content.Equals(""))
                {
                    MessageBox.Show("Plese select the author");
                    authorGrid.Background = Brushes.Red;
                }
                return;
            }
            if (serieslbl.Content.Equals("") && !Sos)
            {
                MessageBox.Show("Please select the series");
                seriesGrid.Background = Brushes.Red;
                return;
            }
            else if (Sos)
            {
                if (updateID == -1)
                {
                    addSingleBook();
                    this.Close();
                }
                else
                {
                    updateSingleBook();
                    this.Close();
                }
            }
            else
            {
                addSeriesBook();
            }
        }
        private void addSingleBook()
        {
            int bookID = -1;
            int year = Convert.ToInt32(yeartxt.Text);

            string SQLquerry = $"CALL InsertSingleBook ('{nametxt.Text}',{year},'{statusbox.Text}',{authorid});";
            MySqlCommand cmd = new MySqlCommand(SQLquerry, conn);

            try
            {
                conn.Open();
                cmd.ExecuteReader();
                conn.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            try
            {
                conn.Open();
                string maxid = $"SELECT book_id FROM `single book` WHERE book_id=(SELECT MAX(book_id) FROM `single book`);";
                MySqlCommand mySql = new MySqlCommand(maxid, conn);
                using (MySqlDataReader view = mySql.ExecuteReader())
                {
                    if (view.Read())
                    {
                        bookID = (int)view[0];
                    }
                }
                conn.Close();

                foreach (DataRowView row in chosenGenreGrid.Items)
                {
                    int genreID = Convert.ToInt32(row.Row.ItemArray[0]);
                    conn.Open();
                    string SQLquerry2 = $"CALL AssignBookGenre({bookID},{genreID});";
                    MySqlCommand assign = new MySqlCommand(SQLquerry2, conn);
                    assign.ExecuteReader();
                    conn.Close();
                }
                MessageBox.Show("Creation successfull");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void addSeriesBook()
        {
            int year = Convert.ToInt32(yeartxt.Text);

            string SQLquerry = $"CALL InsertSeriesBook ('{nametxt.Text}',{year},'{statusbox.Text}', {Convert.ToInt32(ordertxt.Text)}, {seriesid},{authorid});";
            MySqlCommand cmd = new MySqlCommand(SQLquerry, conn);

            try
            {
                conn.Open();
                cmd.ExecuteReader();
                conn.Close();
                MessageBox.Show("Creation successfull");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void updateSingleBook() 
        {
            string updatequerry = $"UPDATE `single book` SET name = '{nametxt.Text}', year_published = {Convert.ToInt32(yeartxt.Text)}, read_status = '{statusbox.Text}', author_id = {authorid} WHERE {updateID} = book_id;";
            MySqlCommand cmd = new MySqlCommand(updatequerry, conn);
            try
            {
                conn.Open();
                cmd.ExecuteReader();
                conn.Close();
                MessageBox.Show("Update Successfull");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void authorbtn_Click(object sender, RoutedEventArgs e)
        {
            yeartxt.IsEnabled = false;
            ordertxt.IsEnabled = false;
            authorGrid.IsEnabled = false;
            seriesGrid.IsEnabled = false;
            statusbox.IsEnabled = false;
            finishbtn.IsEnabled = false;
            ordertxt.IsEnabled = false;
            genrebtn.IsEnabled = false;
            genreGrid.IsEnabled = false;
            chosenGenreGrid.IsEnabled = false;
            seriesbtn.IsEnabled = false;
            alivecheck.IsEnabled = true;
            deadcheck.IsEnabled = true;
            x += 1;

            if(x != 1)
            {
                if (!nametxtValid())
                {
                    MessageBox.Show("Please enter the Authors name in the name field");
                    nametxt.Background = Brushes.Red;
                    x = 1;
                    return;
                }
                else if ((bool)!alivecheck.IsChecked && (bool)!deadcheck.IsChecked)
                {
                    MessageBox.Show("Please select 'Alive' or 'Dead'");
                    alivecheck.Background = Brushes.Red;
                    deadcheck.Background = Brushes.Red;
                    x = 1;
                    return;
                }
                else
                {
                    addAuthor();
                }
                yeartxt.IsEnabled = true;
                ordertxt.IsEnabled = true;
                authorGrid.IsEnabled = true;
                seriesGrid.IsEnabled = true;
                statusbox.IsEnabled = true;
                finishbtn.IsEnabled = true;
                ordertxt.IsEnabled = true;
                genrebtn.IsEnabled = true;
                genreGrid.IsEnabled = true;
                chosenGenreGrid.IsEnabled = true;
                seriesbtn.IsEnabled = true;
                alivecheck.IsEnabled = false;
                deadcheck.IsEnabled = false;
                x = 0;
            }
        }
        private void addAuthor()
        {
            string status = "";
            if ((bool)alivecheck.IsChecked)
            {
                status = "Alive";
            }
            else if ((bool)deadcheck.IsChecked)
            {
                status = "Dead";
            }
            string sqlquerry = $"CALL InsertNewAuthor('{nametxt.Text}','{status}');";
            MySqlCommand cmd = new MySqlCommand(sqlquerry,conn);

            try
            {
                conn.Open();
                cmd.ExecuteReader();
                conn.Close();
                MessageBox.Show("New Author Added");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void authorGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)authorGrid.SelectedItem;
            authorlbl.Content = dataRowView.Row[1];
            authorid = (int)dataRowView.Row[0];
        }

        private void seriesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)seriesGrid.SelectedItem;
            serieslbl.Content = dataRowView.Row[1];
            seriesid = (int)dataRowView.Row[0];
        }
    }
}
