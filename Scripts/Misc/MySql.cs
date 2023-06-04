using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskaServer
{
	public class MySql
	{
		private MySqlConnection mysqlConnection;
		private MySqlDataReader mysqlDataReader;

		public static string ConnectionString { get; set; }

		public MySql()
		{
			mysqlConnection = new MySqlConnection(ConnectionString);
			mysqlConnection.Open();
		}

		public MySqlDataReader Select(string cmd)
		{
			if (mysqlDataReader != null && !mysqlDataReader.IsClosed) mysqlDataReader.Close();
			mysqlDataReader = new MySqlCommand(cmd, mysqlConnection).ExecuteReader();
			return mysqlDataReader;
		}

		public void Update(string cmd)
		{
			if (mysqlDataReader != null && !mysqlDataReader.IsClosed) mysqlDataReader.Close();
			new MySqlCommand(cmd, mysqlConnection).ExecuteNonQuery();
		}

		public void Close()
		{
			mysqlConnection.Close();
		}
	}
}
