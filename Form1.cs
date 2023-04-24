using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace lab2_SGBD
{
    public partial class parent : Form
    {
        private static readonly string connectionString = @"Server=DESKTOP-P0J5H9P;Database=Port;Integrated Security=true;TrustServerCertificate=true;";
        private static readonly string selectQueryChild = ConfigurationManager.AppSettings["selectQueryChild"];
        private static readonly string parentTableName = ConfigurationManager.AppSettings["parentTable"];
        private static readonly string childTableName = ConfigurationManager.AppSettings["childTable"];
        private static readonly string childID = ConfigurationManager.AppSettings["childID"];
        private static readonly string childNoOfColumns = ConfigurationManager.AppSettings["childNoOfColumns"];
        private List<string> childInsertParams = new List<string>(ConfigurationManager.AppSettings["childInsertParams"].Split(','));
        private List<string> childColumnNames = new List<string>(ConfigurationManager.AppSettings["childColumnNames"].Split(','));
        private IDictionary<string, TextBox> textBoxDictionary = new Dictionary<string, TextBox>();
        private static readonly DataSet ds = new DataSet();
        private static readonly SqlDataAdapter parentDataAdapter = new SqlDataAdapter();
        private static readonly SqlDataAdapter childDataAdapter = new SqlDataAdapter();
        private static readonly BindingSource parentBindingSource = new BindingSource();
        private static readonly BindingSource childBindingSource = new BindingSource();


        public parent()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PopulatePanels();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string parentQuery = ConfigurationManager.AppSettings["selectQueryParent"];
                    string childQuery = ConfigurationManager.AppSettings["selectQueryChild"];
                    parentDataAdapter.SelectCommand = new SqlCommand(parentQuery, conn);
                    childDataAdapter.SelectCommand = new SqlCommand(childQuery, conn);
                    parentDataAdapter.Fill(ds, parentTableName);
                    childDataAdapter.Fill(ds, childTableName);

                    string parentID = ConfigurationManager.AppSettings["parentID"];
                    string foreignKey = ConfigurationManager.AppSettings["foreignKey"];
                    DataColumn parentColumn = ds.Tables[parentTableName].Columns[parentID];
                    DataColumn childColumn = ds.Tables[childTableName].Columns[parentID];
                    DataRelation relation = new DataRelation(foreignKey, parentColumn, childColumn);
                    ds.Relations.Add(relation);

                    parentBindingSource.DataSource = ds.Tables[parentTableName];
                    dataGridViewParent.DataSource = parentBindingSource;
                    childBindingSource.DataSource = parentBindingSource;
                    childBindingSource.DataMember = foreignKey;
                    dataGridViewChild.DataSource = childBindingSource;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PopulatePanels()
        {
            Console.WriteLine(childColumnNames.Count);
            foreach(string column in childColumnNames)
            {
                TextBox txtBox = new TextBox();
                textBoxPanel.Controls.Add(txtBox);
                textBoxDictionary.Add(column, txtBox);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Port"].ConnectionString.ToString()))
                {
                    string deleteQuery = ConfigurationManager.AppSettings["deleteQuery"];
                    string childID = ConfigurationManager.AppSettings["childID"];
                    childDataAdapter.SelectCommand = new SqlCommand(selectQueryChild, conn);
                    childDataAdapter.DeleteCommand = new SqlCommand(deleteQuery, conn);
                    childDataAdapter.DeleteCommand.Parameters.Add("@id", SqlDbType.Int).Value = dataGridViewChild.Rows[dataGridViewChild.SelectedRows[0].Index].Cells[childID].Value.ToString();
                    conn.Open();
                    ds.Tables[childTableName].Rows[0].Delete();
                    int rowsAffected = childDataAdapter.Update(ds, childTableName);

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Deleted successfully!");
                        ds.Tables[childTableName].Clear();
                        childDataAdapter.Fill(ds, childTableName);
                    }
                    else
                    {
                        MessageBox.Show("Error");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Port"].ConnectionString))
                {
                    string updateQuery = ConfigurationManager.AppSettings["updateQuery"];
                    childDataAdapter.UpdateCommand = new SqlCommand(updateQuery, conn);
                    foreach(var column in textBoxDictionary.Keys)
                    {
                        childDataAdapter.UpdateCommand.Parameters.AddWithValue("@" + column, textBoxDictionary[column].Text);
                    }
                    childDataAdapter.UpdateCommand.Parameters.Add("@id", SqlDbType.Int).Value = dataGridViewChild.Rows[dataGridViewChild.SelectedRows[0].Index].Cells[childID].Value.ToString();
                    conn.Open();
                    int rows = childDataAdapter.UpdateCommand.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        MessageBox.Show("Updated!");
                    }
                    conn.Close();

                    conn.Open();
                    childDataAdapter.SelectCommand = new SqlCommand(selectQueryChild, conn);
                    ds.Tables[childTableName].Clear();
                    childDataAdapter.Fill(ds, childTableName);
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Port"].ConnectionString))
                {
                    string insertQuery = ConfigurationManager.AppSettings["insertQuery"];
                    childDataAdapter.InsertCommand = new SqlCommand(insertQuery, conn);
                    foreach (var column in textBoxDictionary.Keys)
                    {
                        childDataAdapter.InsertCommand.Parameters.AddWithValue("@" + column, textBoxDictionary[column].Text);
                    }
                    conn.Open();
                        childDataAdapter.InsertCommand.ExecuteNonQuery();
                        conn.Close();

                        conn.Open();
                        childDataAdapter.SelectCommand = new SqlCommand(selectQueryChild, conn);
                        ds.Tables[childTableName].Clear();
                        childDataAdapter.Fill(ds, childTableName);
                        conn.Close();
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
