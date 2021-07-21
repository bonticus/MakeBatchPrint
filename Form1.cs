using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;

namespace MakePrintBatch
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            string mediaType = ConfigurationManager.AppSettings["mediatype"];  //change this to your media type value you're checking against
            string idPadding = ConfigurationManager.AppSettings["idpadding"];    //id padding, we pad with a 0, use "" if you don't pad.
            string cardStock = ConfigurationManager.AppSettings["cardstock"];

            string lines = txtIDs.Text.ToString();

            string[] idList = lines.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            //Console.WriteLine(String.Join(",", idList));

            //get job id
            string sql = "SELECT max(JOBID)+1 FROM batchprintjobs";
            List<string> items = new List<string>();
            items = OraQuery(sql);
            if (items.Count > 0)
            {
                string jobID = items[0];
                //Console.WriteLine("Job ID:" + items[0].ToString());

                if (idList.Length > 0)
                {
                    //create new batch
                    String dateTimeStamp = DateTime.Now.ToString("MMddyyy");
                    sql = "INSERT INTO batchprintjobs (JOBID,JOBDESCRIPTION,JOBSTATUS,CARDSTOCKTYPE,CREATEDBY,PRINTTYPE) VALUES (" + jobID + ",'" + txtBatchName.Text.ToString() + "-" + dateTimeStamp + "','I','" + cardStock + "','mortonb',0)";
                    Console.WriteLine(sql);
                    OraQuery(sql);

                    int counter = 1;

                    foreach (string currID in idList)
                    {
                        //check that an image exists (checks db image blob size)
                        sql = @"select * from keymappinginfo 
                            where keyvalue = '" + idPadding + currID + @"' 
                            and mediatype = '-1' 
                            AND patronid in (select patronid from patronimages where dbms_lob.getlength(dbd_image) > 20)";
                        if (chkNew.Checked)
                        {
                            //New cards checked, make sure patron doesn't have smart media type and not in cardlog
                            sql += @"AND (patronid not in (select patronid from keymappinginfo where mediatype = '" + mediaType + @"') 
                            OR patronid not in (select patronid from patroncardlog))";
                        }

                        items = OraQuery(sql);

                        if (items.Count > 0)
                        {
                            Console.WriteLine("Patron ID:" + currID);
                            //Build SQL String
                            string addpatron = "INSERT INTO batchprintpatrons (PATRONID, JOBID, PRINTORDERNUM,STATUS) VALUES (getpatronid('0" + currID + "')," + jobID + "," + counter.ToString() + ",'I')";
                            Console.WriteLine(addpatron);
                            OraQuery(addpatron);
                            counter++;
                        }
                    }
                    status.Text = "Job created with " + (counter - 1) + " patrons.";
                }
                else
                {
                    status.Text = "No IDs in list.";
                }
            }
            else
            {
                status.Text = "Failed to connect to Oracle?";
            }
            
        }

        private static List<string> OraQuery(string sql)
        {
            string user = ConfigurationManager.AppSettings["goldUser"];
            string pass = ConfigurationManager.AppSettings["goldPass"];
            string db = ConfigurationManager.AppSettings["goldDB"];
            List<string> results = new List<string>();
            string oradb = "Data Source="+db+";User Id="+user+";Password="+pass+";";
            OracleConnection con = new OracleConnection(oradb);
            try
            {
                con.Open();

                OracleCommand cmd = new OracleCommand()
                {
                    Connection = con,
                    CommandText = sql,
                    CommandType = CommandType.Text
                };
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(reader[0].ToString());
                    }
                }
                con.Close();
                con.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                con.Dispose();
            }

            return results;
        }
    }
}
