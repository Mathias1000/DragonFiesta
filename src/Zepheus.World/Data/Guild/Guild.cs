﻿using System;
using Zepheus.Database.DataStore;
using System.Collections.Generic;
using System.Data;
using Zepheus.Util;
using Zepheus.Database;

namespace Zepheus.World.Data
{
    public class Guild
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<GuildMember> GuildMembers { get; set; }
        public string GuildPassword { get; set; }
        public string GuildMaster { get; set; }

        public static Guild LoadFromDatabase(DataRow row)
        {
            Guild g = new Guild
            {
               ID = GetDataTypes.GetInt(row["ID"]),
               Name = row["Name"].ToString(),
               GuildPassword = row["Password"].ToString(),
               GuildMaster = row["GuildMaster"].ToString(),
            };
            g.LoadMembers();
            return g;
        }
       private void LoadMembers()
       {
           DataTable MemberData = null;
           DataTable GuildExtraData = null;
           using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
           {
               MemberData = dbClient.ReadDataTable("SELECT* FROM GuildMembers WHERE GuildID='"+this.ID+"'");
               GuildExtraData = dbClient.ReadDataTable("SELECT* FROM Characters WHERE GuildID='" + this.ID + "'");
           }
           if (MemberData != null)
           {
               foreach (DataRow row in MemberData.Rows)
               {
                   GuildMember pMember = GuildMember.LoadFromDatabase(row);
                   this.GuildMembers.Add(pMember);
               }
           }
           if (GuildExtraData != null)
           {
               foreach (DataRow row in GuildExtraData.Rows)
               {
                   int CharID = GetDataTypes.GetInt(row["CharID"]);
                   GuildMember pMember = this.GuildMembers.Find(m => m.CharID == CharID);
                   if(pMember != null)
                   {
                       pMember.LoadMemberExtraData(row);
                   }
                   else
                   {
                       Log.WriteLine(LogLevel.Warn, "Failed Load Guild ExtraData By Character {0}", CharID);
                   }
               }
           }
       }
    }
}
