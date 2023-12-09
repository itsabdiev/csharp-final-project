using System;
using Npgsql;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

namespace Graveyard
{
    public partial class MainForm : Form
    {
        private string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=1234;Database=csharp";

        DataTable repo = new DataTable();
        public MainForm()

        {


            InitializeComponent();
        }

        private void btnPickImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files (*.bmp;*.jpg;*.png)|*.bmp;*.jpg;*.png|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {

                    string selectedImagePath = openFileDialog.FileName;


                    pictureBox1.Image = System.Drawing.Image.FromFile(selectedImagePath);


                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                }
            }
        }

        private void LoadDataFromDatabase()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT title, body, date FROM notes";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string title = reader["title"].ToString();
                            string body = reader["body"].ToString();
                            string date = reader["date"].ToString();

                            repo.Rows.Add(title, body, date);
                        }
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            String title = richTextBox1.Text;
            String body = richTextBox2.Text;
            DateTime currentDateTime = DateTime.Now;
            String formattedDateTime = currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");

            if (title.Length >= 1 && body.Length >= 1)
            {
                repo.Rows.Add(title, body, formattedDateTime);
                SaveDataToDatabase(title, body, formattedDateTime);

                richTextBox1.Text = "";
                richTextBox2.Text = "";
            }
        }

        private void SaveDataToDatabase(string title, string body, string formattedDateTime)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "INSERT INTO notes (title, body, date) VALUES (@title, @body, @date)";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@title", title);
                    command.Parameters.AddWithValue("@body", body);

                    DateTime dateTimeValue = DateTime.ParseExact(formattedDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                    command.Parameters.AddWithValue("@date", dateTimeValue);

                    command.ExecuteNonQuery();
                }
            }
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            richTextBox2.Text = "";

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            repo.Columns.Add("Title");
            repo.Columns.Add("Body");
            repo.Columns.Add("Date");
            LoadDataFromDatabase();
            dataGridView1.DataSource = repo;

        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                int selectedIndex = dataGridView1.CurrentCell.RowIndex;

                if (selectedIndex >= 0)
                {
                   
                    string titleToDelete = repo.Rows[selectedIndex]["Title"].ToString();

                    
                    DeleteRecordFromDatabase(titleToDelete);

                    
                    repo.Rows[selectedIndex].Delete();
                    richTextBox1.Text = "";
                    richTextBox2.Text = "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception " + ex);
            }
        }

        private void DeleteRecordFromDatabase(string titleToDelete)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "DELETE FROM notes WHERE Title = @title";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@title", titleToDelete);

                    command.ExecuteNonQuery();
                }
            }
        }


        private void doubleCellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                string stringValue1 = repo.Rows[dataGridView1.CurrentCell.RowIndex].ItemArray[0].ToString();
                string stringValue2 = repo.Rows[dataGridView1.CurrentCell.RowIndex].ItemArray[1].ToString();
                richTextBox1.Text = stringValue1;
                richTextBox2.Text = stringValue2;
            }
            catch (Exception ec) {
                Console.WriteLine("Exception while double clicking cell" + ec);
            }
        }
    }
}
