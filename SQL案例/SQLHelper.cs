//===============================================================================
// This file is based on the Microsoft Data Access Application Block for .NET
// For more information please go to 
// http://msdn.microsoft.com/library/en-us/dnbda/html/daab-rm.asp
//===============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Web;
using Flow.Model;
using Microsoft.VisualBasic;

namespace Flow.SQLHelper
{

    /// <summary>
    /// The SqlHelper class is intended to encapsulate high performance, 
    /// scalable best practices for common uses of SqlClient.
    /// </summary>
    public abstract class SqlHelper : System.Web.UI.Page
    {

        //Database connection strings
        public static readonly string ConnectionWorkFlow = ConfigurationSettings.AppSettings["ConnectionWorkFlow"];
        public static readonly string ConnectionWF = ConfigurationSettings.AppSettings["ConnectionWF"];
        public static readonly string ConnectionERP = ConfigurationSettings.AppSettings["ConnectionERP"];
        public static readonly string ConnectionERP_Scy = ConfigurationSettings.AppSettings["ConnectionERP_Scy"];
        public static readonly string ConnectionERP_CIM = ConfigurationSettings.AppSettings["ConnectionCIM_BU4"];
        public static readonly string ConnectionCIM_BU1 = ConfigurationSettings.AppSettings["ConnectionCIM_BU1"];
        public static readonly string ConnectionFlow = ConfigurationSettings.AppSettings["ConnectionFlow"];
        public static readonly string ConnectionCIM_BU1_3 = ConfigurationSettings.AppSettings["ConnectionCIM_BU1_3"];
        public static readonly string ConnectionCIM_BU2 = ConfigurationSettings.AppSettings["ConnectionCIM_BU2"];
        public static readonly string ConnectionCIM_BU2A = ConfigurationSettings.AppSettings["ConnectionCIM_BU2A"];
        public static readonly string ConnectionSMS = ConfigurationSettings.AppSettings["ConnectionSMS"];//上网时数数据库
        public static readonly string ConnectionWFIT = ConfigurationSettings.AppSettings["ConnectionWorkFlowIT"];
        public static readonly string ConnectionLibrary = ConfigurationSettings.AppSettings["ConnectionLibrary"]; //圖書管理系統
        public static readonly string ConnectionSmartIT = ConfigurationSettings.AppSettings["ConnectionSmartIT"];
        //ConnectionCIM_BU1_3
        // Hashtable to store cached parameters
        private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="ConnectionWorkFlow">a valid connection string for a SqlConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string ConnectionWorkFlow, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {

            SqlCommand cmd = new SqlCommand();

            cmd.CommandTimeout = 100;

            using (SqlConnection conn = new SqlConnection(ConnectionWorkFlow))
            {
                int val = 0;
                try
                {
                    if (commandParameters != null)
                    {
                        for (int i = 0; i < commandParameters.Length; i++)
                        {
                            string dbtype = commandParameters[i].SqlDbType.ToString().Trim();

                            if (dbtype == "Char" || dbtype == "VarChar")
                            {
                                commandParameters[i].SqlValue = ToTChinese(commandParameters[i].SqlValue.ToString());
                            }
                        }
                    }

                    PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                    val = cmd.ExecuteNonQuery();

                    cmd.Parameters.Clear();
                }
                catch (Exception e)
                {
                    conn.Close();
                    SendMail(e, cmdText);
                }
                return val;
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against an existing database connection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="conn">an existing database connection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            int val = 0;
            SqlCommand cmd = new SqlCommand();

            cmd.CommandTimeout = 100;

            try
            {
                if (commandParameters != null)
                {
                    for (int i = 0; i < commandParameters.Length; i++)
                    {
                        string dbtype = commandParameters[i].SqlDbType.ToString().Trim();

                        if (dbtype == "Char" || dbtype == "VarChar")
                        {
                            commandParameters[i].SqlValue = ToTChinese(commandParameters[i].SqlValue.ToString());
                        }
                    }
                }

                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }
            catch (Exception e)
            {
                connection.Close();
                SendMail(e, cmdText);
            }
            return val;
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) using an existing SQL Transaction 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="trans">an existing sql transaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlTransaction trans, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            int val = 0;
            try
            {
                if (commandParameters != null)
                {
                    for (int i = 0; i < commandParameters.Length; i++)
                    {
                        string dbtype = commandParameters[i].SqlDbType.ToString().Trim();

                        if (dbtype == "Char" || dbtype == "VarChar")
                        {
                            commandParameters[i].SqlValue = ToTChinese(commandParameters[i].SqlValue.ToString());
                        }
                    }
                }

                SqlCommand cmd = new SqlCommand();

                cmd.CommandTimeout = 100;

                PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
                val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }
            catch (Exception e)
            {
                SendMail(e, cmdText);
            }
            return val;
        }

        /// <summary>

        /// 枚举,标识数据库连接是由SqlHelper提供还是由调用者提供

        /// </summary>

        private enum SqlConnectionOwnership
        {

            /// <summary>由SqlHelper提供连接</summary>

            Internal,

            /// <summary>由调用者提供连接</summary>

            External

        }



        /// <summary>

        /// 执行指定数据库连接对象的数据阅读器.

        /// </summary>

        /// <remarks>

        /// 如果是SqlHelper打开连接,当连接关闭DataReader也将关闭.

        /// 如果是调用都打开连接,DataReader由调用都管理.

        /// </remarks>

        /// <param>一个有效的数据库连接对象</param>

        /// <param>一个有效的事务,或者为 'null'</param>

        /// <param>命令类型 (存储过程,命令文本或其它)</param>

        /// <param>存储过程名或T-SQL语句</param>

        /// <param>SqlParameters参数数组,如果没有参数则为'null'</param>

        /// <param>标识数据库连接对象是由调用者提供还是由SqlHelper提供</param>

        /// <returns>返回包含结果集的SqlDataReader</returns>

        private static SqlDataReader ExecuteReader(SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, SqlConnectionOwnership connectionOwnership)
        {

            if (connection == null) throw new ArgumentNullException("connection");

            bool mustCloseConnection = false;

            // 创建命令

            SqlCommand cmd = new SqlCommand();

            cmd.CommandTimeout = 100;


            try
            {

                PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);



                // 创建数据阅读器

                SqlDataReader dataReader;

                if (connectionOwnership == SqlConnectionOwnership.External)
                {

                    dataReader = cmd.ExecuteReader();

                }

                else
                {

                    dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                }



                // 清除参数,以便再次使用..

                // HACK: There is a problem here, the output parameter values are fletched 

                // when the reader is closed, so if the parameters are detached from the command

                // then the SqlReader can磘 set its values. 

                // When this happen, the parameters can磘 be used again in other command.

                bool canClear = true;

                foreach (SqlParameter commandParameter in cmd.Parameters)
                {

                    if (commandParameter.Direction != ParameterDirection.Input)

                        canClear = false;

                }



                if (canClear)
                {

                    cmd.Parameters.Clear();

                }

                return dataReader;

            }

            catch
            {

                if (mustCloseConnection)

                    connection.Close();

                throw;

            }

        }




        /// <summary>
        /// Execute a SqlCommand that returns a resultset against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="ConnectionWorkFlow">a valid connection string for a SqlConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>A SqlDataReader containing the results</returns>
        public static SqlDataReader ExecuteReader(string ConnectionWorkFlow, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            if (ConnectionWorkFlow == null || ConnectionWorkFlow.Length == 0) throw new ArgumentNullException("ConnectionWorkFlow");

            SqlConnection connection = null;
            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work
            try
            {
                connection = new SqlConnection(ConnectionWorkFlow);

                connection.Open();
                return ExecuteReader(connection, null, cmdType, cmdText, commandParameters, SqlConnectionOwnership.Internal);

            }
            catch (Exception e)
            {
                //cmd.Clone();
                SendMail(e, cmdText);
                if (connection != null) connection.Close();
                return null;
            }
        }

        public static SqlDataReader ExecuteReaderAuto(string ConnectionWorkFlow, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();


            SqlConnection conn = new SqlConnection(ConnectionWorkFlow);
            SqlDataReader rdr = null;
            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work

            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            cmd.Parameters.Clear();
            return rdr;
        }

        /// <summary>
        /// Execute a SqlCommand that returns the first column of the first record against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="ConnectionWorkFlow">a valid connection string for a SqlConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
        public static object ExecuteScalar(string ConnectionWorkFlow, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            using (SqlConnection connection = new SqlConnection(ConnectionWorkFlow))
            {
                object val = null;
                try
                {
                    PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                    val = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();
                }
                catch (Exception e)
                {
                    connection.Close();
                    SendMail(e, cmdText);
                }
                return val;
            }
        }

        /// <summary>
        /// Execute a SqlCommand that returns the first column of the first record against an existing database connection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="conn">an existing database connection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
        public static object ExecuteScalar(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            object val = null;
            try
            {
                SqlCommand cmd = new SqlCommand();

                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
            }
            catch (Exception e)
            {
                connection.Close();
                SendMail(e, cmdText);
            }
            return val;
        }

        /// <summary>
        /// add parameter array to the cache
        /// </summary>
        /// <param name="cacheKey">Key to the parameter cache</param>
        /// <param name="cmdParms">an array of SqlParamters to be cached</param>
        public static void CacheParameters(string cacheKey, params SqlParameter[] commandParameters)
        {
            try
            {
                parmCache[cacheKey] = commandParameters;
            }
            catch (Exception e)
            {
                SendMail(e, cacheKey);
            }
        }

        /// <summary>
        /// Retrieve cached parameters
        /// </summary>
        /// <param name="cacheKey">key used to lookup parameters</param>
        /// <returns>Cached SqlParamters array</returns>
        public static SqlParameter[] GetCachedParameters(string cacheKey)
        {
            SqlParameter[] cachedParms = (SqlParameter[])parmCache[cacheKey];
            SqlParameter[] clonedParms = null;
            try
            {
                if (cachedParms == null)
                    return null;

                clonedParms = new SqlParameter[cachedParms.Length];

                for (int i = 0, j = cachedParms.Length; i < j; i++)
                    clonedParms[i] = (SqlParameter)((ICloneable)cachedParms[i]).Clone();
            }
            catch (Exception e)
            {
                SendMail(e, cacheKey);
            }
            return clonedParms;
        }

        /// <summary>

        /// 预处理用户提供的命令,数据库连接/事务/命令类型/参数

        /// </summary>

        /// <param>要处理的SqlCommand</param>

        /// <param>数据库连接</param>

        /// <param>一个有效的事务或者是null值</param>

        /// <param>命令类型 (存储过程,命令文本, 其它.)</param>

        /// <param>存储过程名或都T-SQL命令文本</param>

        /// <param>和命令相关联的SqlParameter参数数组,如果没有参数为'null'</param>

        /// <param><c>true</c> 如果连接是打开的,则为true,其它情况下为false.</param>

        private static void PrepareCommand(SqlCommand command, SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, out bool mustCloseConnection)
        {

            if (command == null) throw new ArgumentNullException("command");

            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            // If the provided connection is not open, we will open it

            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }

            else
            {
                mustCloseConnection = false;
            }

            // 给命令分配一个数据库连接.

            command.Connection = connection;

            // 设置命令文本(存储过程名或SQL语句)

            command.CommandText = commandText;

            // 分配事务

            if (transaction != null)
            {

                if (transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

                command.Transaction = transaction;

            }

            // 设置命令类型.

            command.CommandType = commandType;

            // 分配命令参数

            if (commandParameters != null)
            {

                AttachParameters(command, commandParameters);

            }

            return;

        }
        /// <summary>

        /// 将SqlParameter参数数组(参数值)分配给SqlCommand命令.

        /// 这个方法将给任何一个参数分配DBNull.Value;

        /// 该操作将阻止默认值的使用.

        /// </summary>

        /// <param>命令名</param>

        /// <param>SqlParameters数组</param>

        private static void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
        {

            if (command == null) throw new ArgumentNullException("command");

            if (commandParameters != null)
            {

                foreach (SqlParameter p in commandParameters)
                {

                    if (p != null)
                    {

                        // 检查未分配值的输出参数,将其分配以DBNull.Value.

                        if ((p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Input) &&

                            (p.Value == null))
                        {

                            p.Value = DBNull.Value;

                        }

                        command.Parameters.Add(p);

                    }

                }

            }

        }



        /// <summary>
        /// Prepare a command for execution
        /// </summary>
        /// <param name="cmd">SqlCommand object</param>
        /// <param name="conn">SqlConnection object</param>
        /// <param name="trans">SqlTransaction object</param>
        /// <param name="cmdType">Cmd type e.g. stored procedure or text</param>
        /// <param name="cmdText">Command text, e.g. Select * from Products</param>
        /// <param name="cmdParms">SqlParameters to use in the command</param>
        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
        {

            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.CommandText = cmdText;

            if (trans != null)
                cmd.Transaction = trans;

            cmd.CommandType = cmdType;

            if (cmdParms != null)
            {
                foreach (SqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }
        /// <summary>
        /// 11.03.10
        /// </summary>
        /// <param name="mail"></param>
        /// <returns></returns>
        public static int SendEMail(Flow.Model.EMailInfo mail)
        {
            string Sql_ExecMail = "EXEC @return = msdb.dbo.sp_send_dbmail ";
            Sql_ExecMail += " @profile_name = 'workflow', ";
            Sql_ExecMail += " @recipients = @Address, ";
            Sql_ExecMail += " @copy_recipients=@CopyAddress, ";
            Sql_ExecMail += " @subject=@HeadNote, ";
            Sql_ExecMail += " @body=@Note";
            SqlParameter[] parats = { 
                new SqlParameter("@Address",SqlDbType.VarChar,200),
                new SqlParameter("@CopyAddress",SqlDbType.VarChar,500),
                new SqlParameter("@HeadNote",SqlDbType.NVarChar,200),
                new SqlParameter("@Note",SqlDbType.NVarChar,1000),
                new SqlParameter("@return",SqlDbType.Int)
            };
            #region 判斷是否為聯興人員
            string TempAddress = mail.Address;
            string TempCopyAddress = string.IsNullOrEmpty(mail.CopyAddress) ? "" : mail.CopyAddress; ;
            while (true)
            {
                int index = TempAddress.IndexOf("@");
                if (index < 0)
                    break;
                else
                {
                    string EmpID = TempAddress.Substring(index - 7, 7);
                    if (EmpID.Substring(0, 1) == "1")
                    {
                        if (Convert.ToInt32(EmpID.Substring(2, 2)) > 12)
                        {
                            mail.Address = mail.Address.Replace(EmpID + "@sz.unimicron.com", EmpID + "@unimicrontouch.com");
                        }
                    }

                }
                TempAddress = TempAddress.Substring(index + 1);
            }
            while (true)
            {
                int index = TempCopyAddress.IndexOf("@");
                if (index < 0)
                    break;
                else
                {
                    string EmpID = TempCopyAddress.Substring(index - 7, 7);
                    if (EmpID.Substring(0, 1) == "1")
                    {
                        if (Convert.ToInt32(EmpID.Substring(2, 2)) > 12)
                        {
                            mail.CopyAddress = mail.CopyAddress.Replace(EmpID + "@sz.unimicron.com", EmpID + "@unimicrontouch.com");
                        }
                    }

                }
                TempCopyAddress = TempCopyAddress.Substring(index + 1);
            }
            #endregion
            parats[0].Value = string.IsNullOrEmpty(mail.Address) ? System.Configuration.ConfigurationSettings.AppSettings["admin_mail"] : mail.Address;
            parats[1].Value = string.IsNullOrEmpty(mail.CopyAddress) ? "" : "ZhangLv@unimicrontouch.com;" + mail.CopyAddress;
            parats[2].Value = mail.HeadNote;
            parats[3].Value = mail.Note;
            parats[4].Direction = ParameterDirection.Output;
            SqlHelper.ExecuteNonQuery(SqlHelper.ConnectionWorkFlow, CommandType.Text, Sql_ExecMail, parats);
            return Convert.ToInt32(parats[4].Value);

        }
        public static void SendMail(Exception e, string Sql)
        {
            Flow.Model.EMailInfo info = new Flow.Model.EMailInfo();
            info.Address = "";
            string admin_mail = System.Configuration.ConfigurationSettings.AppSettings["admin_mail"];
            string Urls = System.Configuration.ConfigurationSettings.AppSettings["URL"];
            string SqlStr = "select empid from workuser(Nolock) where formid=@FormId";
            SqlParameter[] parats = { 
                new SqlParameter("@FormId",SqlDbType.Int)};
            parats[0].Value = 0;
            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnectionWorkFlow, CommandType.Text, SqlStr, parats))
            {
                DataTable tab = new DataTable();
                tab.Load(reader);

                for (int i = 0; i < tab.Rows.Count; i++)
                {
                    info.Address += tab.Rows[i]["empid"].ToString().Trim() + "@sz.unimicron.com;";
                }
                info.CopyAddress = "";
                string pcid = "";
                string pc = "";
                HttpCookie MyCookie = HttpContext.Current.Request.Cookies["UserInfo"];
                if (MyCookie != null)
                {
                    pcid = MyCookie["login"].ToString();
                    pc = MyCookie["comp"].ToString();
                }



                info.HeadNote = "【系統異常】:請管理員及時處理！！！" + " 用户名:" + HttpContext.Current.User.Identity.Name + " 電腦登陸ID:" + pcid + " 電腦號:" + pc;
                info.Note = "【異常原因】:" + e.ToString().Substring(0, 300) + "\n";
                string url = GetUrl();
                url = url.Replace("https", "http");
                info.Note += "【引發頁面】:" + url + "\n";
                info.Note += "【引發語句】:" + Sql;
                SendEMail(info);
                if (url.IndexOf("FormFlowBottom.aspx") == -1)
                {
                    int index = url.ToLower().IndexOf("flowweb");

                    if (e.ToString().IndexOf("已超過連接逾時的設定。在作業完成之前超過逾時等待的時間") > -1 || e.ToString().IndexOf("處理序鎖死") > -1)
                    {
                        url = url.Substring(0, index + 8) + "Error.aspx?ExId=1";
                    }
                    else if (e.ToString().IndexOf("記錄檔檔案已滿") > -1)
                    {
                        url = url.Substring(0, index + 8) + "Error.aspx?ExId=2";
                    }
                    else
                        url = url.Substring(0, index + 8) + "Error.aspx?ExId=0";
                    System.Web.HttpContext.Current.Response.Redirect(url);
                    System.Web.HttpContext.Current.Response.End();
                }
                else
                {
                    string StrNum = url;
                    int begin = StrNum.IndexOf("formid=");
                    int begin2 = StrNum.IndexOf("&paperno=");
                    int begin3 = StrNum.IndexOf("&key");
                    string FormID = "";
                    string PaperNO = "";
                    if (begin > -1 && begin2 > begin)
                    {
                        FormID = StrNum.Substring(begin + 7, begin2 - begin - 7);
                    }
                    if (begin3 > -1 && begin3 > begin2)
                    {
                        PaperNO = StrNum.Substring(begin2 + 9, begin3 - begin2 - 9);
                    }
                    if (begin3 == -1)
                    {
                        PaperNO = StrNum.Substring(begin2 + 9);
                    }
                    //插入異常表
                    try
                    {
                        string StrSql2 = "if not exists(select paperno from ExceptionTable(nolock) where formid=@FormId and paperno=@PaperNo)  insert into ExceptionTable(FormId,PaperNo,ExceptionType,ExTime) values(@FormId,@PaperNo,@ExceptionType,getdate())";
                        SqlParameter[] parameterss = 
                                                {
					
                                      new SqlParameter("@FormId", SqlDbType.VarChar,20),
					                  new SqlParameter("@PaperNo", SqlDbType.VarChar,20),
                                      new SqlParameter("@ExceptionType", SqlDbType.VarChar,225)
                                               };

                        parameterss[0].Value = FormID == null ? "" : FormID;
                        parameterss[1].Value = PaperNO == null ? "" : PaperNO;
                        parameterss[2].Value = e.ToString().Length < 200 ? e.ToString() : e.ToString().Substring(0, 200);
                        SqlHelper.ExecuteNonQuery(SqlHelper.ConnectionWorkFlow, CommandType.Text, StrSql2, parameterss);
                    }
                    catch (Exception exc)
                    {
                        EMailInfo emailinfoA = new EMailInfo();
                        emailinfoA.Address = admin_mail;
                        emailinfoA.HeadNote = "workflow異常通知:添加異常表失敗!";
                        emailinfoA.Note = "【單據編號】:" + PaperNO + ((char)10).ToString();
                        emailinfoA.Note += "【FormId】:" + FormID + ((char)10).ToString();
                        emailinfoA.Note += "【異常原因】:" + exc.ToString() + ((char)10).ToString() + "\n";
                        emailinfoA.Note += "【查閱單據】: " + Urls + "/workflow/FormFlow.aspx?formid=" + FormID + "&paperno=" + PaperNO;
                        SendEMail(emailinfoA);
                    }
                    //取出表單名稱
                    string StrSql = "select FormName from SEC_ClassForm(Nolock) where formid=@FormID";
                    SqlParameter[] paratsss = { 
                              new SqlParameter("@FormID",SqlDbType.Int)
                                   };
                    paratsss[0].Value = FormID;
                    object obj = SqlHelper.ExecuteScalar(SqlHelper.ConnectionWorkFlow, CommandType.Text, StrSql, paratsss);
                    string TableName = obj.ToString();
                    //取出填單人
                    string StrSql1 = "select PaperEmpId from SEC_FormFlowBas(NOLOCK) where FormID=@FormID and PaperNO=@PaperNO";
                    string Kyer = "";
                    SqlParameter[] paratss = { 
                          new SqlParameter("@FormID",SqlDbType.Int),
                          new SqlParameter("@PaperNO",SqlDbType.VarChar,20)
                                   };
                    paratss[0].Value = FormID;
                    paratss[1].Value = PaperNO;
                    SqlDataReader readers = SqlHelper.ExecuteReader(SqlHelper.ConnectionWorkFlow, CommandType.Text, StrSql1, paratss);
                    DataTable table = new DataTable();
                    table.Load(readers);
                    if (table.Rows.Count > 0)
                        //Kyer = table.Rows[0]["AppEmpId"].ToString().Replace(" ", "") + "@sz.unimicron.com;";
                        Kyer = HttpContext.Current.User.Identity.Name;
                    else Kyer = "1007544@sz.unimicron.com;";
                    //取出管理員

                    EMailInfo emailinfo = new EMailInfo();

                    if (FormID == "5" || FormID == "9" || FormID == "56" || FormID == "57")
                    {
                        emailinfo.Address = "1007544@sz.unimicron.com;1108343@sz.unimicron.com;1209036@sz.unimicron.com;1124003@Unimicrontouch.com";
                    }
                    else
                    {
                        emailinfo.Address = admin_mail + ";" + Kyer;
                        emailinfo.CopyAddress = "ITManagerusz@sz.unimicron.com;1124003@unimicrontouch.com;";//加入資訊主管窗口
                    }

                    emailinfo.HeadNote = "workflow異常通知:" + TableName + ":" + PaperNO + "拋入ERP失敗!";
                    emailinfo.Note = "【單據編號】:" + PaperNO + ((char)10).ToString();
                    emailinfo.Note += "【表單名稱】:" + TableName + ((char)10).ToString();
                    string ExString = "";
                    if (e.ToString().Length > 200)
                        ExString = e.ToString().Substring(0, 200);
                    else
                        ExString = e.ToString();
                    emailinfo.Note += "【異常原因】:" + ExString + "\n";
                    emailinfo.Note += "【查閱單據】: " + Urls + "/workflow/FormFlow.aspx?formid=" + FormID + "&paperno=" + PaperNO;
                    SendEMail(emailinfo);

                    //判斷異常原因
                    if (e.ToString().IndexOf("已超過連接逾時的設定。在作業完成之前超過逾時等待的時間") > -1)
                    {
                        int index = url.ToLower().IndexOf("flowweb");
                        url = url.Substring(0, index + 8) + "Error2.aspx";
                        System.Web.HttpContext.Current.Response.Redirect(url);
                    }
                }
            }

        }
        public static string GetUrl()
        {

            string strTemp = "http://";

            if (System.Web.HttpContext.Current.Request.ServerVariables["HTTPS"] == "off")
            {

                strTemp = "http://";

            }

            else
            {

                strTemp = "https://";

            }

            strTemp = strTemp + System.Web.HttpContext.Current.Request.ServerVariables["SERVER_NAME"];

            if (System.Web.HttpContext.Current.Request.ServerVariables["SERVER_PORT"] != "80")
            {

                strTemp = strTemp + ":" + System.Web.HttpContext.Current.Request.ServerVariables["SERVER_PORT"];

            }

            strTemp = strTemp + System.Web.HttpContext.Current.Request.ServerVariables["URL"];

            if (System.Web.HttpContext.Current.Request.QueryString.AllKeys.Length > 0)
            {

                strTemp = strTemp + "?" + System.Web.HttpContext.Current.Request.QueryString;

            }

            return strTemp;

        }
        /////////////////////////////////////////////////////////////////
        public static string GetPagerSQL(string tblName, int pageSize, int pageIndex, string fldSort, bool fldDir, string condition)
        {
            string strDir = fldDir ? " ASC" : " DESC";

            if (pageIndex == 1)
            {
                return "select top " + pageSize.ToString() + " * from " + tblName.ToString()
                    + ((string.IsNullOrEmpty(condition)) ? string.Empty : (" where " + condition))
                    + " order by " + fldSort.ToString() + strDir;
            }
            else
            {
                System.Text.StringBuilder strSql = new System.Text.StringBuilder();
                strSql.AppendFormat("select top {0} * from {1} ", pageSize, tblName);
                strSql.AppendFormat(" where {1} not in (select top {0} {1} from {2} ", pageSize * (pageIndex - 1),
                    (fldSort.Substring(fldSort.LastIndexOf(',') + 1, fldSort.Length - fldSort.LastIndexOf(',') - 1)), tblName);
                if (!string.IsNullOrEmpty(condition))
                {
                    strSql.AppendFormat(" where {0} order by {1}{2}) and {0}", condition, fldSort, strDir);
                }
                else
                {
                    strSql.AppendFormat(" order by {0}{1}) ", fldSort, strDir);
                }
                strSql.AppendFormat(" order by {0}{1}", fldSort, strDir);
                return strSql.ToString();
            }
        }
        public static SqlDataReader GetPageList(string ConnectionWorkFlow, string tblName, int pageSize,
            int pageIndex, string fldSort, bool fldDir, string condition)
        {
            string sql = GetPagerSQL(tblName, pageSize, pageIndex, fldSort, fldDir, condition);
            return ExecuteReader(ConnectionWorkFlow, CommandType.Text, sql, null);
        }




        public static SqlParameter CreateInSqlParameter(string paraName, DbType dbType, int size, object value)
        {
            return CreateSqlParameter(paraName, dbType, size, value, ParameterDirection.Input);
        }

        /// <summary>
        /// 创造输入SqlParameter的实例
        /// </summary>
        public static SqlParameter CreateInSqlParameter(string paraName, DbType dbType, object value)
        {
            return CreateSqlParameter(paraName, dbType, 0, value, ParameterDirection.Input);
        }

        /// <summary>
        /// 创造输出SqlParameter的实例
        /// </summary>        
        public static SqlParameter CreateOutSqlParameter(string paraName, DbType dbType, int size)
        {
            return CreateSqlParameter(paraName, dbType, size, null, ParameterDirection.Output);
        }

        /// <summary>
        /// 创造输出SqlParameter的实例
        /// </summary>        
        public static SqlParameter CreateOutSqlParameter(string paraName, DbType dbType)
        {
            return CreateSqlParameter(paraName, dbType, 0, null, ParameterDirection.Output);
        }

        /// <summary>
        /// 创造返回SqlParameter的实例
        /// </summary>        
        public static SqlParameter CreateReturnSqlParameter(string paraName, DbType dbType, int size)
        {
            return CreateSqlParameter(paraName, dbType, size, null, ParameterDirection.ReturnValue);
        }

        /// <summary>
        /// 创造返回SqlParameter的实例
        /// </summary>        
        public static SqlParameter CreateReturnSqlParameter(string paraName, DbType dbType)
        {
            return CreateSqlParameter(paraName, dbType, 0, null, ParameterDirection.ReturnValue);
        }

        /// <summary>
        /// 创造SqlParameter的实例
        /// </summary>
        public static SqlParameter CreateSqlParameter(string paraName, DbType dbType, int size, object value, ParameterDirection direction)
        {
            SqlParameter para;
            //switch (_databaseType)
            //{
            //    case DatabaseTypes.MySql:
            //        para = new MySqlParameter();
            //        break;
            //    case DatabaseTypes.Oracle:
            //        para = new OracleParameter();
            //        break;
            //    case DatabaseTypes.OleDb:
            //        para = new OleSqlParameter();
            //        break;
            //    case DatabaseTypes.Sql:
            //    default:
            para = new SqlParameter();
            //break;
            //}
            para.ParameterName = paraName;

            if (size != 0)
                para.Size = size;

            para.DbType = dbType;

            if (value != null)
                para.Value = value;

            para.Direction = direction;

            return para;
        }
        #region === 由Object取值 ===

        /// <summary>
        /// 取得Int16值
        /// </summary>
        public static Int16 GetInt16(object obj)
        {
            if (obj != DBNull.Value)
            {
                return Convert.ToInt16(obj);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 取得UInt16值
        /// </summary>
        public static UInt16 GetUInt16(object obj)
        {
            if (obj != DBNull.Value)
            {
                return Convert.ToUInt16(obj);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 取得Int值
        /// </summary>
        public static int GetInt(object obj)
        {
            if (obj != DBNull.Value)
            {
                return Convert.ToInt32(obj);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 取得UInt值
        /// </summary>
        public static uint GetUInt(object obj)
        {
            if (obj != DBNull.Value)
            {
                return Convert.ToUInt32(obj);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 取得UInt64值
        /// </summary>
        public static ulong GetULong(object obj)
        {
            if (obj != DBNull.Value)
            {
                return Convert.ToUInt64(obj);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 取得byte值
        /// </summary>
        public static Byte GetByte(object obj)
        {
            if (obj != DBNull.Value)
            {
                return Convert.ToByte(obj);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 取得sbyte值
        /// </summary>
        public static sbyte GetSByte(object obj)
        {
            if (obj != DBNull.Value)
            {
                return Convert.ToSByte(obj);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 获得Long值
        /// </summary>
        public static long GetLong(object obj)
        {
            if (obj != DBNull.Value)
            {
                return Convert.ToInt64(obj);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 取得Decimal值
        /// </summary>
        public static decimal GetDecimal(object obj)
        {
            if (obj != DBNull.Value)
            {
                return Convert.ToDecimal(obj);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 取得float值
        /// </summary>
        public static float GetFloat(object obj)
        {
            if (obj != DBNull.Value)
            {
                return Convert.ToSingle(obj);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 取得double值
        /// </summary>
        public static double GetDouble(object obj)
        {
            if (obj != DBNull.Value)
            {
                return Convert.ToDouble(obj);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 取得Guid值
        /// </summary>
        public static Guid GetGuid(object obj)
        {
            if (obj != DBNull.Value)
            {
                return new Guid(obj.ToString());
            }
            else
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// 取得DateTime值
        /// </summary>
        public static DateTime GetDateTime(object obj)
        {
            if (obj != DBNull.Value)
            {
                DateTime tmp;
                if (DateTime.TryParse(obj.ToString(), out tmp))
                {
                    return tmp;
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// 取得bool值
        /// </summary>
        public static bool GetBool(object obj)
        {
            if (obj != DBNull.Value)
            {
                return Convert.ToBoolean(obj);
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// 取得byte[]
        /// </summary>
        public static Byte[] GetBinary(object obj)
        {
            if (obj != DBNull.Value)
            {
                return (Byte[])obj;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 取得string值
        /// </summary>
        public static string GetString(object obj)
        {
            return obj.ToString();
        }
        ////////////////////////////////

        #endregion

        #region 转换为繁体中文
        ///   <summary> 
        ///   转换为繁体中文 
        ///   caiwensheng 2013-09-20
        ///   </summary> 
        public static string ToTChinese(string str)
        {
            return Strings.StrConv(str, VbStrConv.TraditionalChinese, 1033);
        }
        #endregion

        #region 转换为简体中文
        ///   <summary> 
        ///   转换为简体中文 
        ///   caiwensheng 2013-09-20
        ///   </summary> 
        public static string ToSChinese(string str)
        {
            return Strings.StrConv(str, VbStrConv.SimplifiedChinese, 1033);
            //String.StrConv 的說明：

            //第一个参数是待转换的字符串；

            //第二个参数是欲转换的字体的枚举值；

            //第三个参数是文字转换后,最后对应的编码格式（LocaleID）;

            //1028 繁体中文
            //1033 ASCII
            //2052 简体中文
            //為何不管繁体转简体还是简体转繁体我都用 1033 呢？
            //首先，要先知道，有些简体中文的编码值是在繁体中文中对应不到任何字的。此时，显示的字就会是 "?" 号。
            //因为 2052 的 「 国] 已经是简体字了，转换到繁体时，刚好其编码对应不到1028 的编码，因此会是问号。
            //最好的方式，是以 en-US(美国) 作为编码格式。当以 en-US 作为最后的编码格式后，又遇到亚洲字时，就会以 unicode 作为储存编码格式。此时刚好与 .net 的 string 储存格式相同。这样就解决了问题！
            //因此,当我们在做繁简转换时，请将第三个参数都设成 1033


        }
        #endregion




        /// <summary>
        /// 执行sql语句，返回SqlDataReader
        /// </summary>
        /// <param name="safeSql"></param>
        /// <returns></returns>
        public static SqlDataReader GetReader(string safeSql)
        {
            SqlConnection connection = new SqlConnection(ConnectionWorkFlow);
            connection.Open();
            SqlCommand cmd = new SqlCommand(safeSql, connection);
            SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            //connection.Close();
            //connection.Dispose();
            return reader;

        }

        public static SqlDataReader GetReader(string sql, params SqlParameter[] values)
        {
            SqlConnection connection = new SqlConnection(ConnectionWorkFlow);
            connection.Open();
            SqlCommand cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddRange(values);
            SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            //connection.Close();
            //connection.Dispose();


            cmd.Parameters.Clear();//modify by luoxm
            return reader;


        }


        public static int Update(string safeSql)
        {
            SqlConnection connection = new SqlConnection(ConnectionWorkFlow);
            connection.Open();
            SqlCommand cmd = new SqlCommand(safeSql, connection);
            int row = cmd.ExecuteNonQuery();
            connection.Close();
            connection.Dispose();
            return row;

        }

        public static int Update(string safeSql, params SqlParameter[] values)
        {
            SqlConnection connection = new SqlConnection(ConnectionWorkFlow);
            connection.Open();
            SqlCommand cmd = new SqlCommand(safeSql, connection);
            cmd.Parameters.AddRange(values);
            int row = cmd.ExecuteNonQuery();
            connection.Close();
            connection.Dispose();

            cmd.Parameters.Clear();//modify by luoxm

            return row;
        }

        /// <summary>
        /// 执行sql语句，填充Datatable
        /// </summary>
        /// <param name="safeSql"></param>
        /// <returns></returns>
        public static DataTable GetDataSet(string safeSql)
        {
            SqlConnection connection = new SqlConnection(ConnectionWorkFlow);
            DataSet ds = new DataSet();
            SqlCommand cmd = new SqlCommand(safeSql, connection);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(ds);
            connection.Close();
            connection.Dispose();
            return ds.Tables[0];
        }



        /// <summary>
        /// 执行sql语句，填充DataSet
        /// </summary>
        /// <param name="safeSql"></param>
        /// <returns></returns>
        public static DataSet GDataSet(string safeSql)
        {
            SqlConnection connection = new SqlConnection(ConnectionWorkFlow);
            DataSet ds = new DataSet();
            SqlCommand cmd = new SqlCommand(safeSql, connection);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(ds);
            connection.Close();
            connection.Dispose();
            return ds;
        }

        /// <summary>
        /// 执行sql语句，填充Datatable
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static DataTable GetDataSet(string sql, params SqlParameter[] values)
        {
            SqlConnection connection = new SqlConnection(ConnectionWorkFlow);
            DataSet ds = new DataSet();
            SqlCommand cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddRange(values);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(ds);
            connection.Close();
            connection.Dispose();

            cmd.Parameters.Clear();//modify by luoxm

            return ds.Tables[0];


        }

        /// <summary>
        /// 执行Transact-SQL语句，返回受影响行数
        /// </summary>
        /// <param name="sql">Transact-SQL</param>
        /// <param name="values">参数集合</param>
        /// <returns>受影响行数</returns>
        public static int ExecuteNonQuery(string sql, params SqlParameter[] values)
        {
            SqlConnection conn = new SqlConnection(ConnectionWorkFlow);
            SqlCommand comm = new SqlCommand(sql, conn);
            comm.Parameters.AddRange(values);
            conn.Open();
            int count = comm.ExecuteNonQuery();
            conn.Close();
            conn.Dispose();

            comm.Parameters.Clear(); //modify by luoxm         
            return count;
        }
        /// <summary>
        /// 执行sql，返回受影响的行数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static int Add(string sql)
        {
            SqlConnection con = new SqlConnection(ConnectionWorkFlow);
            con.Open();
            SqlDataAdapter da = new SqlDataAdapter(sql, con);
            SqlCommand cmd = new SqlCommand(sql, con);
            int a = cmd.ExecuteNonQuery();
            con.Close();
            return a;
        }

        /// <summary>
        /// 有事物的批处理方法
        /// </summary>
        /// <param name="sqlone"></param>
        /// <param name="sqltwo"></param>
        /// <returns></returns>
        public static bool HasTransactionCom(string sqlone, string[] sqltwo)
        {
            SqlConnection conn = new SqlConnection(ConnectionWorkFlow);
            conn.Open();
            SqlTransaction tran = conn.BeginTransaction();
            SqlCommand commDel = new SqlCommand(sqlone, conn, tran);
            try
            {
                commDel.ExecuteNonQuery();
                for (int i = 0; i < sqltwo.Length; i++)
                {
                    SqlCommand commIn = new SqlCommand(sqltwo[i], conn, tran);
                    commIn.ExecuteNonQuery();
                }

                tran.Commit();
                return true;
            }
            catch (Exception)
            {
                tran.Rollback();
                return false;
            }
            finally
            {
                conn.Close();
            }
        }
        public static DataSet FillData(string strSql, string tableName)
        {

            SqlConnection con = new SqlConnection(ConnectionWorkFlow);
            SqlCommand cmd = new SqlCommand(strSql, con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds, tableName);

            return ds;
        }
        /// <summary>
        /// 更新 数据源
        /// </summary>
        /// <param name="connString">连接字符串</param>
        /// <param name="sql">用于查询的SQL语句</param>
        /// <param name="ds">用于修改的数据集</param>
        /// <returns>更新结果</returns>
        public static bool UpdateData(string safeSql, DataTable dt)
        {
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionWorkFlow);
                SqlCommand cmd = new SqlCommand(safeSql, conn);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                SqlCommandBuilder sqlBuilder = new SqlCommandBuilder(sda);
                sda.Update(dt);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 获取第一行第一列的值
        /// </summary>
        /// <param name="safeSql"></param>
        /// <returns></returns>
        public static string GetValue(string safeSql)
        {
            SqlConnection connection = new SqlConnection(ConnectionWorkFlow);
            connection.Open();
            SqlCommand cmd = new SqlCommand(safeSql, connection);
            string row = cmd.ExecuteScalar().ToString();
            connection.Close();
            connection.Dispose();
            return row;

        }
        /// <summary>
        /// 获取第一行第一列的值
        /// </summary>
        /// <param name="safeSql"></param>
        /// <returns></returns>
        public static long GetVal(string safeSql)
        {
            SqlConnection connection = new SqlConnection(ConnectionWorkFlow);
            connection.Open();
            SqlCommand cmd = new SqlCommand(safeSql, connection);
            long row = Convert.ToInt64(cmd.ExecuteScalar().ToString());
            connection.Close();
            connection.Dispose();
            return row;

        }


        #region 向數據庫插入空值
        ///   <summary> 
        ///   向數據庫插入空值 
        ///   caiwensheng 2013-09-20
        ///   </summary> 
        public static object ToDBNull(Object obj)
        {
            if (obj == null)
            {
                return DBNull.Value;
            }
            else
            {
                return obj;
            }
        }
        #endregion

        #region 從數據庫取出空值
        ///   <summary> 
        ///   從數據庫取出空值 
        ///   caiwensheng 2013-09-20
        ///   </summary> 
        public static object ToNull(Object obj)
        {
            if (obj == DBNull.Value)
            {
                return null;
            }
            else
            {
                return obj;
            }
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strConn">连?接ó对?象ó</param>
        /// <param name="dataSet"></param>
        /// <param name="strSql">sql语?句?</param>
        /// <param name="strTableName">获?取?表括名?</param>
        //public static void UpdateData(DataSet dataSet, string strSql)
        //{
        //    try
        //    {
        //        DataSet theDataSet = dataSet.GetChanges();
        //        if (theDataSet == null)
        //        {
        //            return;
        //        }
        //        SqlConnection sqlConn = new SqlConnection();
        //        sqlConn.ConnectionWorkFlow = ConnectionWorkFlow;

        //        SqlCommand sqlcmd = new SqlCommand();
        //        sqlcmd.Connection = sqlConn;
        //        sqlcmd.CommandText = strSql;

        //        SqlDataAdapter sqlAdapter = new SqlDataAdapter();
        //        sqlAdapter.SelectCommand = sqlcmd;

        //        SqlCommandBuilder build = new SqlCommandBuilder(sqlAdapter);

        //        sqlAdapter.Update(theDataSet);
        //        dataSet.AcceptChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
    }
}