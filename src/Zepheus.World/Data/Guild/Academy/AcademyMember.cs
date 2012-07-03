﻿using System;
using Zepheus.World.Managers;
using Zepheus.Database.DataStore;
using Zepheus.World.Networking;
using Zepheus.FiestaLib;
using System.Data;

namespace Zepheus.World.Data
{
   public class AcademyMember : GuildMember
    {
        #region Properties

        public GuildAcademyRank  Rank { get; set; }
        public Academy Academy { get; set; }

        #endregion

        public  static  new AcademyMember  LoadFromDatabase(DataRow row)
        {
            AcademyMember pMember = new AcademyMember
            {
                CharID = GetDataTypes.GetInt(row["CharID"]),
                Rank = (GuildAcademyRank)GetDataTypes.GetByte(row["Rank"]),
                GuildID = GetDataTypes.GetInt(row["OwnerGuildID"]),
            };
            return pMember;
        }
        public override void AddToDatabase()
        {
                Program.DatabaseManager.GetClient().ExecuteQuery("INSERT INTO academymembers (OwnerGuildID,CharID,Rank) VALUES ('" + this.GuildID + "','" + this.CharID + "','" + this.Rank + "')");
        }
    }
}
