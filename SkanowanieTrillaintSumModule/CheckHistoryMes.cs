using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SkanowanieTrillaintSumModule
{
    public static class CheckHistoryMes
    {
        public static string AssemblyID { get; private set; }
        public static string VerifySetupSheetDSResult { get; private set; }


        const int M_NIENARODZONY = 1;
        const int M_BRAK_KROKU = 2;
        const int M_FAIL = 3;
        const int M_BRAK_POLACZENIA_Z_MES = 4;


        public static string CheckSerialNumberByCheckpointEPS(string SerialTxt, string client, string step)
        {
            using (MESwebservice.BoardsSoapClient wsMES = new MESwebservice.BoardsSoapClient(MESwebservice.BoardsSoapClient.EndpointConfiguration.BoardsSoap12))
            {
                try
                {
                    return wsMES.CheckSerialNumberByCheckpointEPS(@client, @step, SerialTxt);
                }
                catch
                {
                    return "Błąd połączenia";
                }
            }
        }
        public static void CheckSerialNumberByCheckpointEPSList(string step)
        {
            using (MESwebservice.BoardsSoapClient wsMES = new MESwebservice.BoardsSoapClient(MESwebservice.BoardsSoapClient.EndpointConfiguration.BoardsSoap12))
            {
                for (int i = 0; i < MainWindow.ListOfSerialNumbers.Count; i++)
                {
                    if (MainWindow.ListOfSerialNumbers[i].Contains("FAIL"))    
                        continue;

                    try
                    {                       
                        if (!wsMES.CheckSerialNumberByCheckpointEPS("TRILLIANT", @step, MainWindow.ListOfSerialNumbers[i]).ToUpper().Equals("TRUE"))
                            MainWindow.ListOfSerialNumbers[i] += "FAILHistory";
                    }
                    catch
                    {
                        MainWindow.ListOfSerialNumbers[i] += "FAILHistoryConnection";
                    }
                }

            }
        }
        public static async Task CheckSerialNumberByCheckpointEPSListAsync(string step)
        {
            using (MESwebservice.BoardsSoapClient wsMES = new MESwebservice.BoardsSoapClient(MESwebservice.BoardsSoapClient.EndpointConfiguration.BoardsSoap12))
            {
                for (int i = 0; i < MainWindow.ListOfSerialNumbers.Count; i++)
                {
                    if (MainWindow.ListOfSerialNumbers[i].Contains("FAIL"))
                        continue;

                    try
                    {
                        var a = await wsMES.CheckSerialNumberByCheckpointEPSAsync("TRILLIANT", @step, MainWindow.ListOfSerialNumbers[i]);
                        if (!a.Equals("TRUE"))
                            MainWindow.ListOfSerialNumbers[i] += "FAILHistory";
                    }
                    catch
                    {
                        MainWindow.ListOfSerialNumbers[i] += "FAILHistoryConnection";
                    }
                }

            }
        }

        private static string GetAssemblyNo(string SerialTxt)
        {

            using (MESwebservice.BoardsSoapClient wsMES = new MESwebservice.BoardsSoapClient(MESwebservice.BoardsSoapClient.EndpointConfiguration.BoardsSoap))
            {
                
                try
                {
                    var res = wsMES.GetAssemblyNo(@"TRILLIANT", SerialTxt);

                    if (res is null) 
                        return "";
                    else
                        return res;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                return "";
            }
        }
        private static string GetBoardData(string SerialTxt)
        {

            using (MESwebservice.BoardsSoapClient wsMES = new MESwebservice.BoardsSoapClient(MESwebservice.BoardsSoapClient.EndpointConfiguration.BoardsSoap))
            {

                try
                {
                    var res = wsMES.GetBoardData(@"TRILLIANT", SerialTxt);

                    if (res is null)
                        return "";
                    else if (res[0].Contains(";"))
                        return res[0].Split(';')[1];
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                return "";
            }
        }
        private static async Task<string> GetBoardDataAsync(string SerialTxt)
        {

            using (MESwebservice.BoardsSoapClient wsMES = new MESwebservice.BoardsSoapClient(MESwebservice.BoardsSoapClient.EndpointConfiguration.BoardsSoap))
            {

                try
                {
                    var res = await wsMES.GetBoardDataAsync(@"TRILLIANT", SerialTxt);

                    if (res is null)
                        return "";
                    else if (res[0].Contains(";"))
                        return res[0].Split(';')[1];
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                return "";
            }
        }
        public static string VerifySetupSheetDS(string AssemblyID, string RouteId)
        {

            using (MESwebservice.BoardsSoapClient wsMES = new MESwebservice.BoardsSoapClient(MESwebservice.BoardsSoapClient.EndpointConfiguration.BoardsSoap))
            {

                try
                {

                    var res = wsMES.VerifySetupSheetDS(@AssemblyID, @RouteId, "", "1");  //24671 top // 24670 bottom

                    if (res is null)
                        return "";
                    else
                        ;

                    
                    var tableName = res.Nodes.Last().Value.ToString().ToUpper();
                    //.Tables[0].TableName;
               //     var tableName2 = res.Nodes.Last().FirstNode.ToString();

                //    var fdg = res.Nodes[1].FirstAttribute.Parent.Name;//.Rows[0].Field<string>("Column1");
                    //if (tableName.Equals("Table1"))
                    //    return res.Tables[0].Rows[0].Field<string>("Column1");
                    //else
                    //    return tableName;
                    return tableName;
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
                return "";
            }
        }

        public static bool CheckSetupSheet(string SerialNumber, string RouteId)
        {
            AssemblyID = CheckHistoryMes.GetBoardData(SerialNumber);
            if (AssemblyID.Length >= 5)
                VerifySetupSheetDSResult = CheckHistoryMes.VerifySetupSheetDS(@AssemblyID, @RouteId);
            else
                return false;

            if (VerifySetupSheetDSResult.ToUpper().Equals("VERIFIED"))
                return true;
            else
                return false;   
        }

        public static void CheckSetupSheetList(string RouteId)
        {
            for (int i = 0; i < MainWindow.ListOfSerialNumbers.Count -15; i++)
            {
                var AssemblyID = CheckHistoryMes.GetBoardData(MainWindow.ListOfSerialNumbers[i]);
                if (AssemblyID.Length >= 5)
                    VerifySetupSheetDSResult = CheckHistoryMes.VerifySetupSheetDS(@AssemblyID, @RouteId);
                else
                    MainWindow.ListOfSerialNumbers[i] += "FAILAssemblyID";

                if (!VerifySetupSheetDSResult.ToUpper().Equals("VERIFIED"))
                    MainWindow.ListOfSerialNumbers[i] += "FAILAssemblyID";
            }

            

        }

        private static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }

        public static void GetPanelSnList(string serialNumber)
        {
            DateTime stop = DateTime.Now;
            using (wsTis.MES_TISSoapClient ws = new wsTis.MES_TISSoapClient(wsTis.MES_TISSoapClient.EndpointConfiguration.MES_TISSoap))
            {
                var res =  ws.GetPanelSerializeResult("TRILLIANT", "", @serialNumber);

                if (res is null)
                {
                    return;
                }
                    
                
                string[] words = res.Split("<Table>");

                //              List<string> words2 = new List<string>();
                MainWindow.ListOfSerialNumbers.Clear();

                foreach (var item in words)
                {
                    if (item.Contains("SerialNumberOri"))
                    {
                        var a = item.Split('\n');

                        foreach (var b in a)
                        {
                            if (b.Contains("<SerialNumberOri>") && b.Length > 40)
                                MainWindow.ListOfSerialNumbers.Add(RemoveSpecialCharacters(b.Replace("SerialNumberOri", "")));
                        }

                    }
                }
            }      
        }



            public static int CheckStepHistory(string sn)
        {
            int Result;
            string path = Directory.GetCurrentDirectory();

        //    Result = GetBoardHistoryDS(sn);

            switch (1)
            {
                case M_BRAK_POLACZENIA_Z_MES:

                 //   ChangeControl.UpdateControl(label, Color.Red, "Brak połączenia z MES.", true);
                    break;

                case M_NIENARODZONY:

         //           ChangeControl.UpdateControl(label, Color.Red, "Numer nienarodzony w MES", true);
                    break;

                case M_BRAK_KROKU:

         //           ChangeControl.UpdateControl(label, Color.Red, "Brak poprzedniego kroku", true);
                    break;

                case M_FAIL:


          //          ChangeControl.UpdateControl(label, Color.Red, "Poprzedni krok = FAIL", true);
                    break;

                default:
     //               ChangeControl.UpdateControl(label, Color.LawnGreen, $"Barkod: {sn} OK", true);
                    return 1;
            }
            return 0;
        }
    }
}
