using System;
using OpenMetaverse;

namespace OpenSim.Framework
{
    /// <summary>
    /// Inventory Item - contains all the properties associated with an individual inventory piece.
    /// </summary>
    public class AAInventoryItemBase : InventoryNodeBase, ICloneable
    {
        /// <value>
        /// The inventory type of the item.  This is slightly different from the asset type in some situations.
        /// </value>
        public int InvType
        {
            get
            {
                return m_invType;
            }

            set
            {
                m_invType = value;
            }
        }
        protected int m_invType;

        /// <value>
        /// The folder this item is contained in
        /// </value>
        public UUID Folder
        {
            get
            {
                return m_folder;
            }

            set
            {
                m_folder = value;
            }
        }
        protected UUID m_folder;

        /// <value>
        /// The creator of this item
        /// </value>
        public string CreatorId
        {
            get
            {
                return m_creatorId;
            }

            set
            {
                m_creatorId = value;
            }
        }
        protected string m_creatorId;


        public UUID CreatorIdAsUuid
        {
            get
            {
                if (UUID.Zero == m_creatorIdAsUuid)
                {
                    UUID.TryParse(CreatorId, out m_creatorIdAsUuid);
                }

                return m_creatorIdAsUuid;
            }

            set
            {
                m_creatorIdAsUuid = value;
            }
        }
        protected UUID m_creatorIdAsUuid = UUID.Zero;

        /// <value>
        /// The description of the inventory item (must be less than 64 characters)
        /// </value>
        public string Description
        {
            get
            {
                return m_description;
            }

            set
            {
                m_description = value;
            }
        }
        protected string m_description = String.Empty;

        /// <value>
        ///
        /// </value>
        public uint NextPermissions
        {
            get
            {
                return m_nextPermissions;
            }

            set
            {
                m_nextPermissions = value;
            }
        }
        protected uint m_nextPermissions;

        /// <value>
        /// A mask containing permissions for the current owner (cannot be enforced)
        /// </value>
        public uint CurrentPermissions
        {
            get
            {
                return m_currentPermissions;
            }

            set
            {
                m_currentPermissions = value;
            }
        }
        protected uint m_currentPermissions;

        /// <value>
        ///
        /// </value>
        public uint BasePermissions
        {
            get
            {
                return m_basePermissions;
            }

            set
            {
                m_basePermissions = value;
            }
        }
        protected uint m_basePermissions;

        /// <value>
        ///
        /// </value>
        public uint EveryOnePermissions
        {
            get
            {
                return m_everyonePermissions;
            }

            set
            {
                m_everyonePermissions = value;
            }
        }
        protected uint m_everyonePermissions;

        /// <value>
        ///
        /// </value>
        public uint GroupPermissions
        {
            get
            {
                return m_groupPermissions;
            }

            set
            {
                m_groupPermissions = value;
            }
        }
        protected uint m_groupPermissions;

        /// <value>
        /// This is an enumerated value determining the type of asset (eg Notecard, Sound, Object, etc)
        /// </value>
        public int AssetType
        {
            get
            {
                return m_assetType;
            }

            set
            {
                m_assetType = value;
            }
        }
        protected int m_assetType;

        /// <value>
        /// The UUID of the associated asset on the asset server
        /// </value>
        public UUID AssetID
        {
            get
            {
                return m_assetID;
            }

            set
            {
                m_assetID = value;
            }
        }
        protected UUID m_assetID;

        /// <value>
        ///
        /// </value>
        public UUID GroupID
        {
            get
            {
                return m_groupID;
            }

            set
            {
                m_groupID = value;
            }
        }
        protected UUID m_groupID;

        /// <value>
        ///
        /// </value>
        public bool GroupOwned
        {
            get
            {
                return m_groupOwned;
            }

            set
            {
                m_groupOwned = value;
            }
        }
        protected bool m_groupOwned;

        /// <value>
        ///
        /// </value>
        public int SalePrice
        {
            get
            {
                return m_salePrice;
            }

            set
            {
                m_salePrice = value;
            }
        }
        protected int m_salePrice;

        /// <value>
        ///
        /// </value>
        public byte SaleType
        {
            get
            {
                return m_saleType;
            }

            set
            {
                m_saleType = value;
            }
        }
        protected byte m_saleType;

        /// <value>
        ///
        /// </value>
        public uint Flags
        {
            get
            {
                return m_flags;
            }

            set
            {
                m_flags = value;
            }
        }
        protected uint m_flags;

        /// <value>
        ///
        /// </value>
        public int CreationDate
        {
            get
            {
                return m_creationDate;
            }

            set
            {
                m_creationDate = value;
            }
        }
        protected int m_creationDate = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

        public AAInventoryItemBase()
        {
        }

        public AAInventoryItemBase(UUID id)
        {
            ID = id;
        }

        public AAInventoryItemBase(UUID id, UUID owner)
        {
            ID = id;
            Owner = owner;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}