﻿using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Quill.Framework;
using Quill.Framework.Serialization.External;
using OpenSim.Framework.Console;
using OpenSim.Region.Framework.Scenes;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Services.Interfaces;
using OpenMetaverse;
using om = OpenMetaverse;
using OpenMetaverse.StructuredData;
using log4net;
using OpenSim.Framework;
using Mono.Addins;
using System.Linq;

[assembly: Addin("AvatarAppearanceArchiver", "0.1")]
[assembly: AddinDependency("OpenSim.Region.Framework", OpenSim.VersionInfo.VersionNumber)]

namespace OpenSim.Region.CoreModules.Avatar.AvatarArchiver
{
    /// <summary>
    /// This module loads/saves the avatar's appearance from/down into an "Avatar Archive", also known as an AA.
    /// </summary>
    [Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule")]
    public class AvatarAppearanceArchiver : ISharedRegionModule, IAvatarAppearanceArchiver
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Scene m_scene;
        private IInventoryService InventoryService;
        private IAssetService AssetService;
        private IUserAccountService UserAccountService;
        private IAvatarService AvatarService;

        public void Initialise(Nini.Config.IConfigSource source)
        {
        }

        public void AddRegion(Scene scene)
        {
            if (m_scene == null)
                m_scene = scene;
            m_scene.AddCommand(
                    "Avatar Archive", this, "Avatar Archive",
                    "\n\nsave avatar archive <First> <Last> <Filename> <FolderNameToSaveInto>  (--snapshot <UUID>) (--private) Saves appearance to an avatar archive file\n",
                    "\nload avatar archive <First> <Last> (Past archives are listed just type a name from the list) Loads appearance from an avatar archive file",
                    "\nload avatar archive <First> <Last> <url> (ex http://example.com/archive.aa) Loads appearance from an avatar archive from a web url",
                    HandleHelpAvatarArchive);


            MainConsole.Instance.Commands.AddCommand("region", false, "save avatar archive", "save avatar archive <First> <Last> <Filename> <FolderNameToSaveInto>", "Saves appearance to an avatar archive archive (Note: put \"\" around the FolderName if you need more than one word)", HandleSaveAvatarArchive);
            MainConsole.Instance.Commands.AddCommand("region", false, "load avatar archive", "load avatar archive <First> <Last> <Filename>", "Loads appearance from an avatar archive archive", HandleLoadAvatarArchive);
        }

        public void RemoveRegion(Scene scene)
        {

        }

        public void RegionLoaded(Scene scene)
        {
            InventoryService = m_scene.InventoryService;
            AssetService = m_scene.AssetService;
            UserAccountService = m_scene.UserAccountService;
            AvatarService = m_scene.AvatarService;
        }

        public Type ReplaceableInterface
        {
            get { return null; }
        }

        public void PostInitialise() { }

        public void Close()
        {
        }

        public string Name
        {
            get { return "AvatarAppearanceArchiver"; }
        }

        public bool IsSharedModule
        {
            get { return true; }
        }

        protected void HandleHelpAvatarArchive(string module, string[] cmdparams)
        {
            return;
        }
        protected void HandleLoadwebAvatarArchive(string module, string[] cmdparams)
        {
            var client = new WebClient();
            client.DownloadFile(cmdparams[5], "temp.aa");
            string filename = "temp";

            if (cmdparams.Length != 5)
            {
                m_log.Info("[AvatarArchive] Not enough parameters!");
                return;
            }
            LoadAvatarArchive(filename, cmdparams[3], cmdparams[4]);
        }
        protected void HandleLoadAvatarArchive(string module, string[] cmdparams)
        {
            string filename;
            string file = cmdparams[5];
            if (file.StartsWith("http://"))
            {
                var client = new WebClient();
                client.DownloadFile(cmdparams[5], "temp.aa");
                filename = "temp";
                LoadAvatarArchive(filename, cmdparams[3], cmdparams[4]);
                System.IO.File.Delete("temp.aa");
                return;
            }
            if (file.StartsWith("https://"))
            {
                m_log.Info("[AvatarArchive] https not supported!");
                return;
            }
            
            string dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string[] filePaths = Directory.GetFiles(dir, "*.aa").Select(Path.GetFileNameWithoutExtension).Select(p => p.Substring(0)).ToArray();
            m_log.Info("[AvatarArchive] Type Avatar Archive name From List Below\n");
            for (int i = 0; i < filePaths.Length; ++i)
            {
                string path = filePaths[i];
                Console.WriteLine(System.IO.Path.GetFileName(path));
            }
            filename = MainConsole.Instance.CmdPrompt("\nFilename?");

            

            if (cmdparams.Length != 5)
            {
                m_log.Info("[AvatarArchive] Not enough parameters!");
                return;
            }
            LoadAvatarArchive(filename, cmdparams[3], cmdparams[4]);
        }

        public void LoadAvatarArchive(string FileName, string First, string Last)
        {
            UserAccount account = UserAccountService.GetUserAccount(om.UUID.Zero, First, Last);
            m_log.Info("[AvatarArchive] Loading archive from " + FileName);
            if (account == null)
            {
                m_log.Error("[AvatarArchive] User not found!");
                return;
            }

            StreamReader reader = new StreamReader(FileName+".aa");
            string file = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();

            ScenePresence SP;
            m_scene.TryGetScenePresence(account.PrincipalID, out SP);
            if (SP == null)
                return; //Bad people!

            SP.ControllingClient.SendAlertMessage("Appearance loading in progress...");

            string FolderNameToLoadInto = "";

            OSDMap map = ((OSDMap)OSDParser.DeserializeLLSDXml(file));

            OSDMap assetsMap = ((OSDMap)map["Assets"]);
            OSDMap itemsMap = ((OSDMap)map["Items"]);
            OSDMap bodyMap = ((OSDMap)map["Body"]);

            AvatarAppearance appearance = ConvertXMLToAvatarAppearance(bodyMap, out FolderNameToLoadInto);

            appearance.Owner = account.PrincipalID;

            List<InventoryItemBase> items = new List<InventoryItemBase>();

            InventoryFolderBase AppearanceFolder = InventoryService.GetFolderForType(account.PrincipalID, AssetType.Clothing);

            InventoryFolderBase folderForAppearance
                = new InventoryFolderBase(
                    UUID.Random(), FolderNameToLoadInto, account.PrincipalID,
                    -1, AppearanceFolder.ID, 1);

            InventoryService.AddFolder(folderForAppearance);

            folderForAppearance = InventoryService.GetFolder(folderForAppearance);

            try
            {
                LoadAssets(assetsMap);
                LoadItems(itemsMap, account.PrincipalID, folderForAppearance, out items);
            }
            catch (Exception ex)
            {
                m_log.Warn("[AvatarArchiver]: Error loading assets and items, " + ex.ToString());
            }

            //Now update the client about the new items
            SP.ControllingClient.SendBulkUpdateInventory(folderForAppearance);
            foreach (InventoryItemBase itemCopy in items)
            {
                if (itemCopy == null)
                {
                    SP.ControllingClient.SendAgentAlertMessage("Can't find item to give. Nothing given.", false);
                    continue;
                }
                if (!SP.IsChildAgent)
                {
                    SP.ControllingClient.SendBulkUpdateInventory(itemCopy);
                }
            }
            m_log.Info("[AvatarArchive] Loaded archive from " + FileName+".aa");
        }

        private InventoryItemBase GiveInventoryItem(UUID senderId, UUID recipient, InventoryItemBase item, InventoryFolderBase parentFolder)
        {
            InventoryItemBase itemCopy = new InventoryItemBase();
            itemCopy.Owner = recipient;
            itemCopy.CreatorId = item.CreatorId;
            itemCopy.ID = UUID.Random();
            itemCopy.AssetID = item.AssetID;
            itemCopy.Description = item.Description;
            itemCopy.Name = item.Name;
            itemCopy.AssetType = item.AssetType;
            itemCopy.InvType = item.InvType;
            itemCopy.Folder = UUID.Zero;

            //Give full permissions for them
            itemCopy.NextPermissions = (uint)om.PermissionMask.All;
            itemCopy.GroupPermissions = (uint)om.PermissionMask.All;
            itemCopy.EveryOnePermissions = (uint)om.PermissionMask.All;
            itemCopy.CurrentPermissions = (uint)om.PermissionMask.All;

            if (parentFolder == null)
            {
                InventoryFolderBase folder = InventoryService.GetFolderForType(recipient, (AssetType)itemCopy.AssetType);

                if (folder != null)
                    itemCopy.Folder = folder.ID;
                else
                {
                    InventoryFolderBase root = InventoryService.GetRootFolder(recipient);

                    if (root != null)
                        itemCopy.Folder = root.ID;
                    else
                        return null; // No destination
                }
            }
            else
                itemCopy.Folder = parentFolder.ID; //We already have a folder to put it in

            itemCopy.GroupID = UUID.Zero;
            itemCopy.GroupOwned = false;
            itemCopy.Flags = item.Flags;
            itemCopy.SalePrice = item.SalePrice;
            itemCopy.SaleType = item.SaleType;

            InventoryService.AddItem(itemCopy);
            return itemCopy;
        }

        private AvatarAppearance ConvertXMLToAvatarAppearance(OSDMap map, out string FolderNameToPlaceAppearanceIn)
        {
            AvatarAppearance appearance = new AvatarAppearance();
            appearance.Unpack(map);
            FolderNameToPlaceAppearanceIn = map["FolderName"].AsString();
            return appearance;
        }

        protected void HandleSaveAvatarArchive(string module, string[] cmdparams)
        {
            
            
            if (cmdparams.Length != 7)
            {
                m_log.Info("[AvatarArchive] Not enough parameters!");
                return;
            }
            string ext = Path.GetExtension(cmdparams[5]);
            if (ext != "")
            {
                m_log.Error("[AvatarArchive] Do not Specify File Extention, It is automaticly Added");
                return;
            }
            UserAccount account = UserAccountService.GetUserAccount(UUID.Zero, cmdparams[3], cmdparams[4]);
            if (account == null)
            {
                m_log.Error("[AvatarArchive] User not found!");
                return;
            }

            ScenePresence SP;
            m_scene.TryGetScenePresence(account.PrincipalID, out SP);
            if (SP == null)
                return; //Bad people!
            SP.ControllingClient.SendAlertMessage("Appearance saving in progress...");

            AvatarAppearance appearance = AvatarService.GetAppearance(SP.UUID);
            if (appearance == null)
                appearance = SP.Appearance;
            StreamWriter writer = new StreamWriter(cmdparams[5]+".aa", false);
            OSDMap map = new OSDMap();
            OSDMap body = new OSDMap();
            OSDMap assets = new OSDMap();
            OSDMap items = new OSDMap();
            body = appearance.Pack();
            body.Add("FolderName", OSD.FromString(cmdparams[6]));

            foreach (AvatarWearable wear in appearance.Wearables)
            {
                for (int i = 0; i < wear.Count; i++)
                {
                    WearableItem w = wear[i];
                    if (w.AssetID != UUID.Zero)
                    {
                        SaveAsset(w.AssetID, assets);
                        SaveItem(w.ItemID, items);
                    }
                }
            }
            List<AvatarAttachment> attachments = appearance.GetAttachments();
            foreach (AvatarAttachment a in attachments)
            {
                SaveAsset(a.AssetID, assets);
                SaveItem(a.ItemID, items);
            }
            map.Add("Body", body);
            map.Add("Assets", assets);
            map.Add("Items", items);
            //Write the map
            writer.Write(OSDParser.SerializeLLSDXmlString(map));
            writer.Close();
            writer.Dispose();
            m_log.Info("[AvatarArchive] Saved archive to " + cmdparams[5] + ".aa");
        }

        private void SaveAsset(UUID AssetID, OSDMap assetMap)
        {
            AssetBase asset = AssetService.Get(AssetID.ToString());
            if (asset != null)
            {
                OSDMap assetData = new OSDMap();
                m_log.Info("[AvatarArchive]: Saving asset " + asset.ID);
                CreateMetaDataMap(asset.Metadata, assetData);
                assetData.Add("AssetData", OSD.FromBinary(asset.Data));
                assetMap.Add(asset.ID, assetData);
            }
            else
            {
                m_log.Warn("[AvatarArchive]: Could not find asset to save: " + AssetID.ToString());
                return;
            }
        }

        private void CreateMetaDataMap(AssetMetadata data, OSDMap map)
        {
            map["ContentType"] = OSD.FromString(data.ContentType);
            map["CreationDate"] = OSD.FromDate(data.CreationDate);
            map["CreatorID"] = OSD.FromString(data.CreatorID);
            map["Description"] = OSD.FromString(data.Description);
            map["ID"] = OSD.FromString(data.ID);
            map["Name"] = OSD.FromString(data.Name);
            map["Type"] = OSD.FromInteger(data.Type);
        }

        private AssetBase LoadAssetBase(OSDMap map)
        {
            AssetBase asset = new AssetBase();
            asset.Data = map["AssetData"].AsBinary();

            AssetMetadata md = new AssetMetadata();
            md.ContentType = map["ContentType"].AsString();
            md.CreationDate = map["CreationDate"].AsDate();
            md.CreatorID = map["CreatorID"].AsString();
            md.Description = map["Description"].AsString();
            md.ID = map["ID"].AsString();
            md.Name = map["Name"].AsString();
            md.Type = (sbyte)map["Type"].AsInteger();

            asset.Metadata = md;
            asset.ID = md.ID;
            asset.FullID = UUID.Parse(md.ID);
            asset.Name = md.Name;
            asset.Type = md.Type;

            return asset;
        }

        private void SaveItem(UUID ItemID, OSDMap itemMap)
        {
            InventoryItemBase saveItem = InventoryService.GetItem(new InventoryItemBase(ItemID));
            if (saveItem == null)
            {
                m_log.Warn("[AvatarArchive]: Could not find item to save: " + ItemID.ToString());
                return;
            }
            m_log.Info("[AvatarArchive]: Saving item " + ItemID.ToString());
            string serialization = UserInventoryItemSerializer1.Serialize1(saveItem);
            itemMap[ItemID.ToString()] = OSD.FromString(serialization);
        }

        private void LoadAssets(OSDMap assets)
        {
            foreach (KeyValuePair<string, OSD> kvp in assets)
            {
                UUID AssetID = UUID.Parse(kvp.Key);
                OSDMap assetMap = (OSDMap)kvp.Value;
                AssetBase asset = AssetService.Get(AssetID.ToString());
                m_log.Info("[AvatarArchive]: Loading asset " + AssetID.ToString());
                if (asset == null) //Don't overwrite
                {
                    asset = LoadAssetBase(assetMap);
                    AssetService.Store(asset);
                }
            }
        }

        private void LoadItems(OSDMap items, UUID OwnerID, InventoryFolderBase folderForAppearance, out List<InventoryItemBase> litems)
        {
            litems = new List<InventoryItemBase>();
            foreach (KeyValuePair<string, OSD> kvp in items)
            {
                string serialization = kvp.Value.AsString();
                InventoryItemBase item = OpenSim.Framework.Serialization.External.UserInventoryItemSerializer.Deserialize(serialization);
                m_log.Info("[AvatarArchive]: Loading item " + item.ID.ToString());
                item = GiveInventoryItem(item.CreatorIdAsUuid, OwnerID, item, folderForAppearance);
                litems.Add(item);
            }
        }
    }
}