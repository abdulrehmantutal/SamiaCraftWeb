using System.Data;
using System.Data.SqlClient;

namespace samiacraft.Helpers
{
    public class DBHelper
    {
//        private static readonly string connectionString =
//"Data Source=SQL8005.site4now.net;" +
//"Initial Catalog=db_ac2bb3_dba7326alhilal;" +
//"User Id=db_ac2bb3_dba7326alhilal_admin;" +
//"Password=Tech@123;" +  // replace YOUR_DB_PASSWORD with actual password
//"Encrypt=True;" +
//"TrustServerCertificate=True;";
        private static readonly string connectionString = "data source=sql5053.site4now.net;initial catalog=db_a74425_premiumpos;user id=db_a74425_premiumpos_admin;password=PPpp1212#; MultipleActiveResultSets=true;";
        public DataTable GetTableFromSP(string sp, Dictionary<string, object> parametersCollection)
        {

            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                SqlCommand command = new SqlCommand(sp, connection) { CommandType = CommandType.StoredProcedure, CommandTimeout = connection.ConnectionTimeout };

                foreach (KeyValuePair<string, object> parameter in parametersCollection)
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);

                DataSet dataSet = new DataSet();
                (new SqlDataAdapter(command)).Fill(dataSet);
                command.Parameters.Clear();

                if (dataSet.Tables.Count > 0)
                {
                    return dataSet.Tables[0];
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //return null;
            }
            finally
            {
                connection.Close();

            }
        }

        public DataTable GetTableFromSP(string sp, SqlParameter[] prms)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                SqlCommand command = new SqlCommand(sp, connection) { CommandType = CommandType.StoredProcedure, CommandTimeout = connection.ConnectionTimeout };
                connection.Open();

                command.Parameters.AddRange(prms);

                DataSet dataSet = new DataSet();
                (new SqlDataAdapter(command)).Fill(dataSet);
                command.Parameters.Clear();

                if (dataSet.Tables.Count > 0)
                {
                    return dataSet.Tables[0];
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //return null;
            }
            finally
            {
                connection.Close();
            }
        }

        public DataTable GetTableFromSP(string sp)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand();
            try
            {
                command = new SqlCommand(sp, connection) { CommandType = CommandType.StoredProcedure, CommandTimeout = connection.ConnectionTimeout };
                connection.Open();

                DataSet dataSet = new DataSet();
                (new SqlDataAdapter(command)).Fill(dataSet);
                command.Parameters.Clear();

                if (dataSet.Tables.Count > 0)
                {
                    return dataSet.Tables[0];
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //return null;
            }
            finally
            {
                connection.Close();
                command.Dispose();
            }
        }

        public void ExecuteNonQuery(string sp, SqlParameter[] prms)
        {

            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand();
            try
            {
                command = new SqlCommand(sp, connection) { CommandType = CommandType.StoredProcedure, CommandTimeout = connection.ConnectionTimeout };
                connection.Open();

                command.Parameters.AddRange(prms);

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;

            }
            finally
            {
                connection.Close();
                command.Dispose();
            }
        }

        public void ExecuteNonQuery(string sp, SqlParameter prms)
        {

            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand();
            try
            {
                command = new SqlCommand(sp, connection) { CommandType = CommandType.StoredProcedure, CommandTimeout = connection.ConnectionTimeout };
                connection.Open();
                prms.SqlDbType = SqlDbType.Structured;
                command.Parameters.Add(prms);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
                command.Dispose();
            }
        }

        public void ExecuteNonQuery(string sp, SqlParameter prm, SqlParameter[] prms)
        {

            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand();
            try
            {
                command = new SqlCommand(sp, connection) { CommandType = CommandType.StoredProcedure, CommandTimeout = connection.ConnectionTimeout };
                connection.Open();
                prm.SqlDbType = SqlDbType.Structured;
                command.Parameters.Add(prm);
                command.Parameters.AddRange(prms);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
                command.Dispose();
            }
        }

        public DataTable GetTableRow(string sp, SqlParameter[] prms)
        {


            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                SqlCommand command = new SqlCommand(sp, connection) { CommandType = CommandType.StoredProcedure, CommandTimeout = connection.ConnectionTimeout };
                command.Parameters.AddRange(prms);
                connection.Open();

                DataSet dataSet = new DataSet();
                (new SqlDataAdapter(command)).Fill(dataSet);
                command.Parameters.Clear();

                if (dataSet.Tables.Count > 0)
                {
                    return dataSet.Tables[0];
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //return null;
            }
            finally
            {
                connection.Close();
            }
        }

        public DataSet GetDatasetFromSP(string sp, SqlParameter[] prms)
        {

            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                SqlCommand command = new SqlCommand(sp, connection) { CommandType = CommandType.StoredProcedure, CommandTimeout = connection.ConnectionTimeout };
                connection.Open();

                command.Parameters.AddRange(prms);

                DataSet dataSet = new DataSet();
                (new SqlDataAdapter(command)).Fill(dataSet);
                command.Parameters.Clear();

                return dataSet;
            }
            catch (Exception ex)
            {
                throw ex;
                //return null;
            }
            finally
            {
                connection.Close();
            }
        }

        public int ExecuteNonQueryReturn(string sp, SqlParameter[] prms)
        {


            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand();
            try
            {
                command = new SqlCommand(sp, connection) { CommandType = CommandType.StoredProcedure, CommandTimeout = connection.ConnectionTimeout };
                connection.Open();
                command.Parameters.AddRange(prms);
                int result = command.ExecuteNonQuery();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
                command.Dispose();
            }
        }

        public string ExecuteScalarFunction(string CommandText)
        {
            string Result = "";

            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand();
            try
            {
                connection.Open();
                command = new SqlCommand(CommandText, connection);
                SqlDataAdapter da = new SqlDataAdapter(command);
                DataTable dt = new DataTable();
                da.Fill(dt);

                Result = dt.Rows[0][0].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
                command.Dispose();
            }

            return Result;

        }

        public void ExecuteMultipleDatatable(string sp, SqlParameter[] prms, DataSet ds)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand();
            try
            {
                command = new SqlCommand(sp, connection) { CommandType = CommandType.StoredProcedure, CommandTimeout = connection.ConnectionTimeout };
                connection.Open();
                command.Parameters.AddRange(prms);
                if (null != ds)
                {
                    foreach (DataTable dt in ds.Tables)
                    {
                        SqlParameter parameter = new SqlParameter();
                        parameter.SqlDbType = SqlDbType.Structured;

                        //DataTable.TableName is the parameter Name
                        //e.g: @AppList
                        parameter.ParameterName = dt.TableName;
                        //DataTable.DisplayExpression is the equivalent SQLType Name. i.e. Name of the UserDefined Table type
                        //e.g: AppCollectionType
                        //parameter.TypeName = dt.DisplayExpression;
                        parameter.TypeName = dt.Namespace;
                        parameter.Value = dt;

                        command.Parameters.Add(parameter);
                    }
                }
                int result = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
                command.Dispose();
            }


        }
    }
}
