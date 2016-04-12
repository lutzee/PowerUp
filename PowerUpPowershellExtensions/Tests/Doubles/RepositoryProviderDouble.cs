using System.Collections.Generic;
using System.Xml;
using Umbraco.Courier.Core;
using Umbraco.Courier.Core.Interfaces;

namespace Tests.Doubles
{
    public class RepositoryProviderDouble : RepositoryProvider, IExtractionTarget
    {
        public int ResourcesSeen = 0;

        public override void LoadSettings(XmlNode settingsXml)
        {
        }

        public bool Exists(ItemIdentifier itemId)
        {
            return true;
        }

        public ItemStatus ExtractItem(Item item, bool overWrite)
        {
            ResourcesSeen++;
            return ItemStatus.Extracted;
        }

        public ItemStatus PostProcess(Item item, bool overWrite)
        {
            return ItemStatus.Extracted;
        }

        public bool TransferResource(ResourceTransfer resource)
        {
            return true;
        }

        public bool TransferResources(ResourceTransfer[] resources)
        {
            return true;
        }

        public void ExecuteEvent(string eventAlias, ItemIdentifier itemId, SerializableDictionary<string, string> parameters)
        {
        }

        public void ExecuteQueue(string queue)
        {
        }

        public void OpenSession()
        {
        }

        public void CloseSession()
        {
        }

        public void Commit()
        {
        }

        public void Rollback()
        {
        }

        public List<ItemConflict> Compare(Item item)
        {
            return new List<ItemConflict>();
        }
    }
}