﻿using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Diagnostics;

namespace Puch.FirebirdHelper
{
    public class Connector
    {
        internal static FbConnection Connection = new FbConnection();
        public static FbTransaction BeginTransaction()
        {
            return Connection.BeginTransaction();
        }

        public static bool Connect(string connectionString)
        {
            try
            {
                Connection.ConnectionString = connectionString;
                Connection.Open();
                Connection.InfoMessage += Connection_InfoMessage;
            }
            catch (Exception e)
            { 
                Debug.WriteLine(e);
                return false;
            }
            return true;
        }

        static void Connection_InfoMessage(object sender, FbInfoMessageEventArgs e)
        {
            Debug.WriteLine(e.Message);
        }

        public static void Execute(string statement)
        {
            Execute(statement, null);
        }

        public static void Execute(string statement, FbTransaction transaction)
        {
            FbCommand c;
            if (transaction == null)
                c = new FbCommand(statement, Connection, transaction);
            else
                c = new FbCommand(statement, Connection);
                c.ExecuteNonQuery();
        }

        public static object ExecuteScalar(string statement, params object[] parameters)
        {
            return ExecuteScalar(statement, null, parameters);
        }

        public static object ExecuteScalar(string statement, FbTransaction transaction, params object[] parameters)
        {
            FbCommand c = GetCommand(statement, transaction, parameters);
            return c.ExecuteScalar();
        }

        internal static long GenNextGenValue(string generatorName, FbTransaction transaction)
        {
            FbCommand c = new FbCommand(string.Format("select gen_id({0}, 1) from rdb$database;", generatorName), Connection, transaction);
            return (long)c.ExecuteScalar();
        }

        internal static FbCommand GetCommand(string statement, FbTransaction transaction, params object[] parameters)
        {
            FbCommand command = transaction == null ? new FbCommand(statement, Connection) : new FbCommand(statement, Connection, transaction);
            int parameterNumber = 0;
            foreach (string parameter in statement.Split(new char[] { ' ', '=', ',', '<', '>', '(', ')', '%', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Where(s => s.StartsWith("@")))
            {
                object parameterValue = parameters[parameterNumber++];
                if (parameterValue.GetType().IsEnum)
                    parameterValue = Convert.ChangeType(parameterValue, parameterValue.GetType().GetEnumUnderlyingType());
                command.Parameters.Add(parameter, parameterValue);
            }
            return command;
        }

    }
}
