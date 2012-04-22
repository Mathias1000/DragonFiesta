﻿using System.Collections.Generic;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Data;
using Zepheus.InterLib.Networking;
using Zepheus.Util;
using Zepheus.World.Handlers;
using Zepheus.World.Networking;
using Zepheus.World.Data;

namespace Zepheus.World.InterServer
{
	public sealed class InterHandler
	{
		[InterPacketHandler(InterHeader.Worldmsg)]
		public static void HandleWorldMessage(ZoneConnection zc, InterPacket packet)
		{
			string msg;
			bool wut;
			byte type;
			if (!packet.TryReadString(out msg) || !packet.TryReadByte(out type) || !packet.TryReadBool(out wut))
			{
				return;
			}
			if (wut)
			{
				string to;
				if (!packet.TryReadString(out to))
				{
					return;
				}
				WorldClient client;
				if ((client = ClientManager.Instance.GetClientByCharname(to)) == null)
				{
					Log.WriteLine(LogLevel.Warn, "Tried to send a WorldMessage to a character that is unknown. Charname: {0}", to);
				}
				else
				{
					using (var p = Handler25.CreateWorldMessage((WorldMessageTypes)type, msg))
					{
						client.SendPacket(p);
					}
				}
			}
			else
			{
				using (var p = Handler25.CreateWorldMessage((WorldMessageTypes)type, msg))
				{
					ClientManager.Instance.SendPacketToAll(p);
				}
			}
		}

		[InterPacketHandler(InterHeader.BanAccount)]
		public static void BanAccount(ZoneConnection zc, InterPacket packet)
		{
			  string playername;
			  if (packet.TryReadString(out playername, 16))
			  {
				  WorldClient bannclient = ClientManager.Instance.GetClientByCharname(playername);
				  if (bannclient != null)
				  {
					  using (var p = new InterPacket(InterHeader.BanAccount))
					  {
						  p.WriteInt(bannclient.AccountID);
						  LoginConnector.Instance.SendPacket(p);
					  }
					  bannclient.Disconnect();
				  }
			  }
		}
		[InterPacketHandler(InterHeader.ChangeZone)]
		public static void ChangeZoneBeginn(ZoneConnection zc, InterPacket packet)
		{
		   // ClientManager.Instance.AddZoneTrans(
		}
		[InterPacketHandler(InterHeader.Assigned)]
		public static void HandleAssigned(LoginConnector lc, InterPacket packet)
		{
			Log.WriteLine(LogLevel.Info, "<3 LoginServer.");
		}

		[InterPacketHandler(InterHeader.Assign)]
		public static void HandleAssigning(ZoneConnection lc, InterPacket packet)
		{
			string ip;
			if (!packet.TryReadString(out ip))
			{
				return;
			}

			lc.IP = ip;

			// make idlist
			InterHandler.SendZoneStarted(lc.ID, lc.IP, lc.Port, lc.Maps);
			InterHandler.SendZoneList(lc);
			Log.WriteLine(LogLevel.Info, "Zone {0} listens @ {1}:{2}", lc.ID, lc.IP, lc.Port);
		}
		 [InterPacketHandler(InterHeader.ClientDisconect)]
		public static void DisconnectFromzoneserver(ZoneConnection zc, InterPacket packet)
		{
			string charname;
			if (packet.TryReadString(out charname,16))
			{
			  WorldClient client =  ClientManager.Instance.GetClientByCharname(charname);
			  client.Character.Loggeout(client);
			  ClientManager.Instance.RemoveClient(client);
			}
		}
		[InterPacketHandler(InterHeader.Clienttransfer)]
		public static void HandleTransfer(LoginConnector lc, InterPacket packet)
		{
			byte v;
			if (!packet.TryReadByte(out v))
			{
				return;
			}

			if (v == 0)
			{
				byte admin;
				int accountid;
				string username, hash, hostip;
				if (!packet.TryReadInt(out accountid) || !packet.TryReadString(out username) || !packet.TryReadString(out hash) || !packet.TryReadByte(out admin) || !packet.TryReadString(out hostip)) {
					return;
				}
				ClientTransfer ct = new ClientTransfer(accountid, username, admin, hostip, hash);
				ClientManager.Instance.AddTransfer(ct);
			}
			else if (v == 1)
			{
				byte admin;
				int accountid;
				string username, charname, hostip;
				ushort randid;
				if (!packet.TryReadInt(out accountid) || !packet.TryReadString(out username) || !packet.TryReadString(out charname) || 
					!packet.TryReadUShort(out randid) || !packet.TryReadByte(out admin) || !packet.TryReadString(out hostip))
				{
					return;
				}
				ClientTransfer ct = new ClientTransfer(accountid, username, charname, randid, admin, hostip);
				ClientManager.Instance.AddTransfer(ct);
			}
		}

		[InterPacketHandler(InterHeader.Clienttransferzone)]
		public static void HandleClientTransferZone(ZoneConnection zc, InterPacket packet)
		{
			byte admin, zoneid;
			int accountid;
			string username, charname, hostip;
			ushort randid, mapid;
			if (!packet.TryReadByte(out zoneid) || !packet.TryReadInt(out accountid) || !packet.TryReadUShort(out mapid) || !packet.TryReadString(out username) ||
				!packet.TryReadString(out charname) || !packet.TryReadUShort(out randid) || !packet.TryReadByte(out admin) ||
				!packet.TryReadString(out hostip))
			{
				return;
			}
			if (Program.Zones.ContainsKey(zoneid))
			{
				ZoneConnection z;
				if (Program.Zones.TryGetValue(zoneid, out z))
				{
					z.SendTransferClientFromZone(accountid, username, charname, randid, admin, hostip);
					WorldClient client = ClientManager.Instance.GetClientByCharname(charname);
					client.Character.ChangeMap(client.Character.GetMapname(mapid));
				}
			}
			else
			{
				Log.WriteLine(LogLevel.Warn, "Uh oh, Zone {0} tried to transfer {1} to zone {1} D:", zc.ID, charname, zoneid);
			}
		}

		[InterPacketHandler(InterHeader.FunctionCharIsOnline)]
		public static void FunctionGetCharacterOnline(ZoneConnection pConnection, InterPacket pPacket)
		{
			long id;
			string charName;

			if(!pPacket.TryReadLong(out id))
				throw new InvalidPacketException();
			if(!pPacket.TryReadString(out charName, 16))
				throw new InvalidPacketException();

			bool isOnline = ClientManager.Instance.IsOnline(charName);
			using (InterPacket packet = new InterPacket(InterHeader.FunctionAnswer))
			{
				packet.WriteLong(id);
				packet.WriteBool(isOnline);
				pConnection.SendPacket(packet);
			}
		}

		public static void TryAssiging(LoginConnector lc)
		{
			using (var p = new InterPacket(InterHeader.Assign))
			{
				p.WriteByte(Settings.Instance.ID);
				p.WriteStringLen(Settings.Instance.WorldName);
				p.WriteStringLen(Settings.Instance.IP);
				p.WriteUShort(Settings.Instance.Port);
				lc.SendPacket(p);
			}
		}
		public static void SendZoneStarted(byte zoneid, string ip, ushort port, List<MapInfo> maps)
		{
			using (var packet = new InterPacket(InterHeader.Zoneopened))
			{
				packet.WriteByte(zoneid);
				packet.WriteStringLen(ip);
				packet.WriteUShort(port);
				packet.WriteInt(maps.Count);
				foreach (var m in maps)
				{
					packet.WriteUShort(m.ID);
					packet.WriteStringLen(m.ShortName);
					packet.WriteStringLen(m.FullName);
					packet.WriteInt(m.RegenX);
					packet.WriteInt(m.RegenY);
					packet.WriteByte(m.Kingdom);
					packet.WriteUShort(m.ViewRange);
				}
				foreach (var c in Program.Zones.Values)
				{
					if (c.ID != zoneid)
						c.SendPacket(packet);
				}
			}
		}

		public static void SendZoneList(ZoneConnection zc)
		{
			using (var packet = new InterPacket(InterHeader.Zonelist))
			{
				packet.Write(Program.Zones.Values.Count);
				foreach (var z in Program.Zones.Values)
				{
					packet.Write(z.ID);
					packet.Write(z.IP);
					packet.Write(z.Port);
					packet.WriteInt(z.Maps.Count);
					foreach (var m in z.Maps)
					{
						packet.WriteUShort(m.ID);
						packet.WriteStringLen(m.ShortName);
						packet.WriteStringLen(m.FullName);
						packet.WriteInt(m.RegenX);
						packet.WriteInt(m.RegenY);
						packet.WriteByte(m.Kingdom);
						packet.WriteUShort(m.ViewRange);
					}
				}
				zc.SendPacket(packet);
			}
		}
		public static void SendZoneStopped(byte zoneid)
		{
			using (var packet = new InterPacket(InterHeader.Zoneclosed))
			{
				packet.Write(zoneid);
				foreach (var c in Program.Zones.Values)
					c.SendPacket(packet);
			}
		}
	}
}
