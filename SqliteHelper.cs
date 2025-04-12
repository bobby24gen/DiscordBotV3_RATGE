namespace DiscordBotV3;

using Discord;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

public class SqliteHelper
{
    string connectionString = "Data Source=Db.db";
    
    private Random random = new Random();

    public List<string> GetPidorasi()
    {
        List<string> list = new List<string>();       
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.Connection = connection;

            command.CommandText = "Select Name From Pidorasi";
            try
            {
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string name = reader.GetString(0);
                            list.Add(name);
                        }
                        connection.Close();
                        return list;

                    }
                    else
                    {
                        connection.Close();
                        return list;
                    }
                }
            }
            catch (Exception e)
            {
                list.Add(e.Message);
                return list;
            }
            
        }
    }


    public bool AddPidorasi(string name)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.Connection = connection;

            command.CommandText = "INSERT INTO Pidorasi(Name) VALUES (@name)";
            command.Parameters.AddWithValue("name", name);

            try
            {
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;

        }
    }

    public string GetMemes(string tag)
    {
        string link = string.Empty;
        List<string> links = new List<string>();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.Connection = connection;

            CountMemes(tag);

            command.CommandText = $"Select Link From Memes where Tag = \"{tag}\" and IsShown = 0";

            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        links.Add(name);
                    }
                    
                    int r = random.Next(links.Count);
                    link = links[r];

                    SqliteCommand updateCommand = connection.CreateCommand();
                    updateCommand.Connection = connection;
                    updateCommand.CommandText = $"Update Memes Set IsShown = 1 Where Link = \"@link\"";
                    command.Parameters.AddWithValue("link", link);

                    updateCommand.ExecuteNonQuery();

                    connection.Close();
                    return link;
                }
                else
                {
                    connection.Close();
                    return link;
                }
            }



        }
    }
    /// <summary>
    /// Count memes and reset if counter is 0
    /// </summary>
    /// <param name="tag"></param>
    public void CountMemes(string tag)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.Connection = connection;

            command.CommandText = $"Select Count(*) From Memes where Tag = \"{tag}\" and IsShown = 0";
            
            int count = Convert.ToInt32(command.ExecuteScalar());

            if (count == 0)
            {
                ResetShownMemes(tag);
            }
        }
    }

    private void ResetShownMemes(string tag)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.Connection = connection;

            command.CommandText = $"Update Memes Set IsShown = 0 where Tag = \"{tag}\"";

            command.ExecuteNonQuery();

            connection.Close();
        }
    }


    public int GetLastMeme()
    {
        int id = -1;
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.Connection = connection;

            command.CommandText = $"Select Value From RuntimeValues where Name = \"LastMeme\"";
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        id = Int32.Parse(reader.GetString(0));
                    }
                    connection.Close();
                    return id;
                }
                connection.Close();
                return -1;
            }

        }
    }

    public bool SetLastMeme(int id)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.Connection = connection;

            command.CommandText = $"Update RuntimeValues Set Value = {id} where Name = \"LastMeme\"";
            try
            {
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;

        }
    }

    public bool AddMeme(string tag, string link)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.Connection = connection;

            command.CommandText = "INSERT into Memes(link,tag) VALUES (@link,@tag)";
            command.Parameters.AddWithValue("tag", tag);
            command.Parameters.AddWithValue("link", link);

            try
            {
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }

    public List<string> GetTags()
    {
        List<string> tags = new List<string>();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.Connection = connection;

            command.CommandText = "Select Tag From Memes";

            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        tags.Add(name);
                    }
                    connection.Close();
                    return tags;
                }
                else
                {
                    connection.Close();
                    return tags;
                }
            }
        }
    }
    
    public bool DelTag(string tag)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            if (DoTagExist(tag))
            {
                connection.Open();

                SqliteCommand command = connection.CreateCommand();
                command.Connection = connection;

                command.CommandText = "Delete From Memes Where Tag = @tag";
                command.Parameters.AddWithValue("tag", tag);
                try
                {
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool DelMeme(string link)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            if (DoMemeExist(link)) // check if link is valid for deletion
            {
                connection.Open();

                SqliteCommand command = connection.CreateCommand();
                command.Connection = connection;

                command.CommandText = "Delete From Memes Where Link = @link";
                command.Parameters.AddWithValue("link", link);
                try
                {
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    
    private bool DoMemeExist(string link)// should not dublicate this, but fuck
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.Connection = connection;

            command.CommandText = "Select * From Memes Where Link = @link";
            command.Parameters.AddWithValue("link", link);
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    connection.Close();
                    return true;
                }
                else
                {
                    connection.Close();
                    return false;
                }
            }

        }
    }
    private bool DoTagExist(string tag)// should not dublicate this, but fuck
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.Connection = connection;

            command.CommandText = "Select * From Memes Where Tag = @tag";
            command.Parameters.AddWithValue("tag", tag);
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    connection.Close();
                    return true;
                }
                else
                {
                    connection.Close();
                    return false;
                }
            }

        }
    }
}