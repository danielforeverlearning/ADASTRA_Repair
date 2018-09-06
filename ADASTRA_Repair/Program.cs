using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace ADASTRA_Repair
{
    class Program
    {
        public class ROOMNUMBER_MAPPING_STRUCT
        {
            public string MAPPING_ID;
            public string OLD_BUILDING_CODE;
            public string OLD_ROOM_ID;
            public string NEW_ROOM_ID;
            public string NEW_BUILDING_CODE;

            public ROOMNUMBER_MAPPING_STRUCT(string mapping_id, string old_building_code, string old_room_id, string new_room_id, string new_building_code)
            {
                MAPPING_ID = mapping_id;
                OLD_BUILDING_CODE = old_building_code;
                OLD_ROOM_ID = old_room_id;
                NEW_ROOM_ID = new_room_id;
                NEW_BUILDING_CODE = new_building_code;
            }
        }


        public class UPDATEABLE_ROOMNUMBER_MAPPING_STRUCT
        {
            public bool UPDATED;
            public string MAPPING_ID;
            public string OLD_BUILDING_CODE;
            public string OLD_ROOM_ID;

            public UPDATEABLE_ROOMNUMBER_MAPPING_STRUCT(string mapping_id, string old_building_code, string old_room_id)
            {
                UPDATED = false;
                MAPPING_ID = mapping_id;
                OLD_BUILDING_CODE = old_building_code;
                OLD_ROOM_ID = old_room_id;
            }
        }

        static string Search_For_Existence(ref List<ROOMNUMBER_MAPPING_STRUCT> mylist, string old_building_code, string old_room_id, string new_room_id, string new_building_code)
        {
            for (int ii = 0; ii < mylist.Count; ii++)
            {
                if (mylist[ii].OLD_BUILDING_CODE.Equals(old_building_code) && mylist[ii].OLD_ROOM_ID.Equals(old_room_id) && mylist[ii].NEW_BUILDING_CODE.Equals(new_building_code) && mylist[ii].NEW_ROOM_ID.Equals(new_room_id))
                    return mylist[ii].MAPPING_ID;
            }
            return null;
        }

        static string Search_For_Updateable_Existence_And_Change_To_Updated(ref List<UPDATEABLE_ROOMNUMBER_MAPPING_STRUCT> myupdateablelist, string old_building_code, string old_room_id)
        {
            for (int ii = 0; ii < myupdateablelist.Count; ii++)
            {
                if (myupdateablelist[ii].UPDATED == false && myupdateablelist[ii].OLD_BUILDING_CODE.Equals(old_building_code) && myupdateablelist[ii].OLD_ROOM_ID.Equals(old_room_id))
                {
                    myupdateablelist[ii].UPDATED = true;
                    return myupdateablelist[ii].MAPPING_ID;
                }
            }
            return null;
        }

        static void Main(string[] args)
        {
            //*************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************
            //choiceA will delete existing data and just do all inserts ..... this WILL NOT SAVE ANYTHING Shi-Pu did in original dbo.ROOMNUMBER_MAPPING before Max was hired.... PLEASE NOTE THERE IS ROWS in dbo.ROOMNUMBER_MAPPING that Shi-Pu did that is not in DAVID LACHINA NEW MAPPING FILE
            //choiceB will search for exact same row exisitng in dbo.ROOMNUMBER_MAPPING and not insert duplicates and insert all non-duplicates .... this WILL SAVE EVERYTHING Shi-Pu did in dbo.ROOMNUMBER_MAPPING before Max was hired and there will be additional data
            //choiceC will search for very first match on OLD_BUILDING_CODE and OLD_ROOM_ID in dbo.ROOMNUMBER_MAPPING, if it does not find a match it will insert, if it does find a match it will update that row ..... this will MODIFY some of what Shi-Pu did in dbo.ROOMNUMBER_MAPPING  and add additional data and it will SAVE what Shi-Pu did if there exists an OLD_BUILDING_CODE/OLD_ROOM_ID in original dbo.ROOMNUMBER_MAPPING BUT NOT IN DAVID LACHINA NEW MAPPING FILE
            //IN ALL 3 CHOICES, THERE WILL BE A OLD_BUILDING_CODE/OLD_ROOM_ID MAPPED TO MANY NEW_BUILDING_CODE/NEW_ROOM_ID (1-to-MANY relationship) ..... before in Shi-Pu original dbo.ROOMNUMBER_MAPPING table there was only 1-to-1 relationship or MANY-to-1 relationship from old to new
            //
            //example of old MANY-to-1 relationships:
            //MAPPING_ID   OLD_BUILDING_CODE  OLD_ROOM_ID   NEW_ROOM_ID  NEW_BUILDING_CODE
            //----------------------------------------------------------------------------
            //    1             ASB              101           JS101          JS
            //    11           CABRIN            101           JS101          CH
            //
            //example of new 1-to-MANY relationships in David LaChina file:
            //             OLD_BUILDING_CODE  OLD_ROOM_ID   NEW_ROOM_ID  NEW_BUILDING_CODE
            //----------------------------------------------------------------------------
            //                  ARCH             A104           AB           AB100
            //                  ARCH             A104           AB           AB101
            //                  ARCH             A104           AB           AB102
            //                  ARCH             A104           AB           AB103
            //**************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************
            StreamWriter choiceA = new StreamWriter(".\\choiceA.sql");
            StreamWriter choiceB = new StreamWriter(".\\choiceB.sql");
            StreamWriter choiceC = new StreamWriter(".\\choiceC.sql");

            choiceA.WriteLine("USE [Campus8];");
            choiceB.WriteLine("USE [Campus8];");
            choiceC.WriteLine("USE [Campus8];");

            choiceA.WriteLine("BEGIN TRANSACTION Do_Update_ROOMNUMBER_MAPPING;");
            choiceB.WriteLine("BEGIN TRANSACTION Do_Update_ROOMNUMBER_MAPPING;");
            choiceC.WriteLine("BEGIN TRANSACTION Do_Update_ROOMNUMBER_MAPPING;");

            choiceA.WriteLine("DELETE FROM [dbo].[ROOMNUMBER_MAPPING]");




            //*******************************************
            //existing dbo.ROOMNUMBER_MAPPING table
            //*******************************************
            List<UPDATEABLE_ROOMNUMBER_MAPPING_STRUCT> list_updateable = new List<UPDATEABLE_ROOMNUMBER_MAPPING_STRUCT>();
            List<ROOMNUMBER_MAPPING_STRUCT> list_ROOMNUMBER_MAPPING = new List<ROOMNUMBER_MAPPING_STRUCT>();

            StreamReader tablereader = new StreamReader(".\\original_dbo_ROOMNUMBER_MAPPING_table.csv");
            string tableline = tablereader.ReadLine(); //header row
            tableline = tablereader.ReadLine(); //first line of data
            while (tableline != null)
            {
                string[] substrs = tableline.Split(',');

                string temp_mapping_id = substrs[0].Trim().ToUpper();
                string temp_old_building_code = substrs[1].Trim().ToUpper();
                string temp_old_room_id = substrs[2].Trim().ToUpper();
                string temp_new_building_code = substrs[3].Trim().ToUpper();
                string temp_new_room_id = substrs[4].Trim().ToUpper();

                ROOMNUMBER_MAPPING_STRUCT obj_roomnumber_mapping = new ROOMNUMBER_MAPPING_STRUCT(temp_mapping_id, temp_old_building_code, temp_old_room_id, temp_new_room_id, temp_new_building_code);
                list_ROOMNUMBER_MAPPING.Add(obj_roomnumber_mapping);

                UPDATEABLE_ROOMNUMBER_MAPPING_STRUCT obj_updateable = new UPDATEABLE_ROOMNUMBER_MAPPING_STRUCT(temp_mapping_id, temp_old_building_code, temp_old_room_id);
                list_updateable.Add(obj_updateable);

                tableline = tablereader.ReadLine();
            }


            //***********************************************
            //David file
            //***********************************************
            StreamReader reader = new StreamReader(".\\mapping_from_david.csv");
            string line = reader.ReadLine(); //header row
            line = reader.ReadLine(); //first line of data
            while (line != null)
            {
                string[] substrings = line.Split(',');

                string check_old_building_code = substrings[0].Trim().ToUpper();
                string check_new_building_code = substrings[1].Trim().ToUpper();
                string check_old_room_id = substrings[2].Trim().ToUpper();
                string check_new_room_id = substrings[3].Trim().ToUpper();

                choiceA.WriteLine("INSERT INTO [dbo].[ROOMNUMBER_MAPPING] ([OLD_BUILDING_CODE],[NEW_BUILDING_CODE],[OLD_ROOM_ID],[NEW_ROOM_ID])  VALUES  ('{0}','{1}','{2}','{3}')", check_old_building_code, check_new_building_code, check_old_room_id, check_new_room_id);

                string existing_mapping_id = Search_For_Existence(ref list_ROOMNUMBER_MAPPING, check_old_building_code, check_old_room_id, check_new_room_id, check_new_building_code);
                if (existing_mapping_id == null)
                    choiceB.WriteLine("INSERT INTO [dbo].[ROOMNUMBER_MAPPING] ([OLD_BUILDING_CODE],[NEW_BUILDING_CODE],[OLD_ROOM_ID],[NEW_ROOM_ID])  VALUES  ('{0}','{1}','{2}','{3}')", check_old_building_code, check_new_building_code, check_old_room_id, check_new_room_id);
                else
                    choiceB.WriteLine("-- EXISTING MAPPING_ID={0}     SAME AS     OLD_BUILDING_CODE={1}  NEW_BUILDING_CODE={2}  OLD_ROOM_ID={3}  NEW_ROOM_ID={4}", existing_mapping_id, check_old_building_code, check_new_building_code, check_old_room_id, check_new_room_id);

                string update_mapping_id = Search_For_Updateable_Existence_And_Change_To_Updated(ref list_updateable, check_old_building_code, check_old_room_id);
                if (update_mapping_id == null)
                    choiceC.WriteLine("INSERT INTO [dbo].[ROOMNUMBER_MAPPING] ([OLD_BUILDING_CODE],[NEW_BUILDING_CODE],[OLD_ROOM_ID],[NEW_ROOM_ID])  VALUES  ('{0}','{1}','{2}','{3}')", check_old_building_code, check_new_building_code, check_old_room_id, check_new_room_id);
                else
                    choiceC.WriteLine("UPDATE [dbo].[ROOMNUMBER_MAPPING] SET [OLD_BUILDING_CODE] = '{0}', [NEW_BUILDING_CODE] = '{1}', [OLD_ROOM_ID] = '{2}', [NEW_ROOM_ID] = '{3}'  WHERE  MAPPING_ID = '{4}' ", check_old_building_code, check_new_building_code, check_old_room_id, check_new_room_id, update_mapping_id);


                line = reader.ReadLine();
            }
            reader.Close();

            choiceA.WriteLine("-- ROLLBACK TRANSACTION Do_Update_ROOMNUMBER_MAPPING;");
            choiceB.WriteLine("-- ROLLBACK TRANSACTION Do_Update_ROOMNUMBER_MAPPING;");
            choiceC.WriteLine("-- ROLLBACK TRANSACTION Do_Update_ROOMNUMBER_MAPPING;");

            choiceA.WriteLine("-- COMMIT TRANSACTION Do_Update_ROOMNUMBER_MAPPING;");
            choiceB.WriteLine("-- COMMIT TRANSACTION Do_Update_ROOMNUMBER_MAPPING;");
            choiceC.WriteLine("-- COMMIT TRANSACTION Do_Update_ROOMNUMBER_MAPPING;");

            choiceA.Close();
            choiceB.Close();
            choiceC.Close();

            //*******************************************
            //existing dbo.ROOM
            //*******************************************
            StreamReader room_reader = new StreamReader(".\\dbo.ROOM.csv");
            string room_line = room_reader.ReadLine(); //header row
            room_line = room_reader.ReadLine(); //first line of data
            while (room_line != null)
            {
                string[] substrs = room_line.Split(',');

                string building_code = substrs[3].Trim().ToUpper();
                string room_id = substrs[4].Trim().ToUpper();

                room_line = room_reader.ReadLine();
            }

            //*******************************************
            //existing dbo.BUILDING
            //*******************************************
            StreamReader building_reader = new StreamReader(".\\dbo.BUILDING.csv");
            string building_line = building_reader.ReadLine(); //header row
            building_line = building_reader.ReadLine(); //first line of data
            while (building_line != null)
            {
                string[] substrs = building_line.Split(',');

                string building_code = substrs[3].Trim().ToUpper();

                building_line = building_reader.ReadLine();
            }

        }
    }
}
