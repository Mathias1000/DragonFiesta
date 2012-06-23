﻿using System;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Networking;
using Zepheus.Util;
using Zepheus.Zone.Game;
using Zepheus.Zone.Networking;
using Zepheus.Zone.Managers;
using System.Collections.Generic;

namespace Zepheus.Zone.Data
{
    public sealed class Commercial
    {
        #region .ctor
        public Commercial(ZoneCharacter pFrom,ZoneCharacter pTo)
        {
            this.pCharFrom = pFrom;
            this.pCharTo = pTo;
            this.pCharFrom.Commercial = this;
            this.pCharTo.Commercial = this;
            SendCommercialBeginn();
        }
        #endregion
        #region Properties

        public ZoneCharacter pCharTo { get; private set; }
        public List<CommercialItem> pToHandelItemList = new List<CommercialItem>();

        private long pToHandelMoney { get;  set; }
        private bool pToLocket { get;  set; }
        public byte pToItemCounter { get; private set; }

        private long pFromHandelMoney { get;  set; }
        private bool pFromLocket { get; set; }

        public List<CommercialItem> pFromHandelItemList = new List<CommercialItem>();
        public ZoneCharacter pCharFrom { get; private set; }
        public byte pFromItemCounter { get; private set; }
        
        #endregion
        #region Methods
        #region public
        public void ChangeMoneyToCommercial(ZoneCharacter pChar, long money)
        {
            if (this.pCharFrom == pChar)
            {
                this.pFromHandelMoney = money;
                SendChangeMoney(this.pCharTo.Client,money);
            }
            else if (this.pCharTo == pCharTo)
            {
                this.pToHandelMoney = money;
                SendChangeMoney(this.pCharFrom.Client, money);
            }

        }
        public void RemoveItemToHandel(ZoneCharacter pChar,byte pSlot)
        {
            if (this.pCharFrom == pChar)
            {
                CommercialItem item = pFromHandelItemList.Find(d => d.CommercialSlot == pSlot);
               
                this.pFromHandelItemList.Remove(item);
                SendItemRemovFromHandel(this.pCharTo.Client, pSlot);
                SendItemRemoveMe(this.pCharFrom.Client, pSlot);
                pFromItemCounter--;
            }
            else if (this.pCharTo == pCharTo)
            {
                CommercialItem item = pToHandelItemList.Find(d => d.CommercialSlot == pSlot);
                this.pToHandelItemList.Remove(item);
                SendItemRemovFromHandel(this.pCharFrom.Client, pSlot);
                SendItemRemoveMe(this.pCharTo.Client, pSlot);
                pToItemCounter--;
            }
        }
        public void AddItemToHandel(ZoneCharacter pChar,byte pSlot)
        {
            Item pItem;
            if (!pChar.Inventory.InventoryItems.TryGetValue(pSlot, out pItem))
                return;
            if (this.pCharFrom == pChar)
            {
    
                CommercialItem Item = new CommercialItem(pChar, pSlot, pFromItemCounter);
                this.pFromHandelItemList.Add(Item);
                this.SendComercialAddItemTo(this.pCharTo.Client, pItem,pFromItemCounter);
  
                this.SendComercialAddItemMe(this.pCharFrom.Client, pSlot, pFromItemCounter);
                pFromItemCounter++;

            }
            else if(this.pCharTo == pChar)
            {
                CommercialItem Item = new CommercialItem(pChar, pSlot, pToItemCounter);
                this.pFromHandelItemList.Add(Item);
                this.SendComercialAddItemTo(this.pCharFrom.Client, pItem, pToItemCounter);
                this.SendComercialAddItemMe(this.pCharTo.Client, pSlot, pToItemCounter);
                pToItemCounter++;
     
            }
               
        }
        public void CommercialLock(ZoneCharacter pChar)
        {
            if (this.pCharFrom == pChar)
            {
                this.pFromLocket = true;
                SendCommercialLock(this.pCharTo.Client);
            }
            else if (this.pCharTo == pCharTo)
            {
                this.pToLocket = true;
                SendCommercialLock(this.pCharFrom.Client);
            }

        }
        public void CommercialBreak(ZoneCharacter pChar)
        {

            if (this.pCharFrom == pChar)
            {
                this.SendCommercialBreak(this.pCharTo.Client);
                this.pCharTo.Commercial = null;

            }
            else if (this.pCharTo == pCharTo)
            {
                this.SendCommercialBreak(this.pCharFrom.Client);
                this.pCharFrom = null;
            }
        }
        public void AcceptCommercial(ZoneCharacter pChar)
        {
        }
        #endregion
        #region privat
        private void SendPacketToAllCommercialVendors(Packet packet)
        {
            pCharFrom.Client.SendPacket(packet);
            pCharTo.Client.SendPacket(packet);
        }
        #endregion 
        #region Packets
        private void SendCommercialLock(ZoneClient pClient)
        {
            using (var packet = new Packet(SH19Type.SendCommercialLock))
            {
                pClient.SendPacket(packet);
            }

        }
        private void SendComercialAddItemMe(ZoneClient pClient,byte pSlot,byte CommercialSlot)
        {
            using (var packet = new Packet(SH19Type.SendAddItemSuccefull))
            {
                packet.WriteByte(pSlot);
                packet.WriteByte(CommercialSlot);
                pClient.SendPacket(packet);
            }

        }
        private void SendComercialAddItemTo(ZoneClient pClient,Item pItem,byte ComercialpSlot)
        {
            using (var packet = new Packet(SH19Type.SendAddItem))
            {
                packet.WriteByte(ComercialpSlot);
              if(pItem is Equip)
              {
                  Equip eq = pItem as Equip;
                  eq.WritEquipInfo(packet);
              }
              else
              {
                  Item item = pItem as Item;
                
                  item.WriteItemStats(packet);
              }
              pClient.SendPacket(packet);
            }

        }
        private void SendCommercialBeginn()
        {
            using (var packet = new Packet(SH19Type.SendCommecialAccept))
            {
                packet.WriteUShort(pCharFrom.MapObjectID);
                this.pCharTo.Client.SendPacket(packet);
            }
            using (var packet = new Packet(SH19Type.SendCommecialAccept))
            {
                packet.WriteUShort(pCharTo.MapObjectID);
                this.pCharFrom.Client.SendPacket(packet);
            }
        }
        private void SendChangeMoney(ZoneClient pClient, long money)
        {
            using (var packet = new Packet(SH19Type.SendChangeMoney))
            {
                packet.WriteLong(money);
                pClient.SendPacket(packet);
            }
        }
        private void SendCommercialBreak(ZoneClient pClient)
        {
            using (var packet = new Packet(SH19Type.SendCommercialBreak))
            {
                pClient.SendPacket(packet);
            }
        }
        private void SendItemRemovFromHandel(ZoneClient pClient, byte Slot)
        {
            using (var packet = new Packet(SH19Type.SendRemoveItemFromHandel))
            {
                packet.WriteByte(Slot);
                pClient.SendPacket(packet);
            }
        }
        private void SendItemRemoveMe(ZoneClient pClient,byte pCommercialSlot)
        {
            using (var packet = new Packet(SH19Type.SendItemRemove))
            {
                packet.WriteByte(pCommercialSlot);
                pClient.SendPacket(packet);
            }
        }
        #endregion
        #endregion
    }
}
