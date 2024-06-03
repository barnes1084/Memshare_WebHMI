using CommonTools;

namespace WebHMI.Data
{
    public class MemoryMapService
    {
        private readonly TagManager tagManager;

        public MemoryMapService()
        {
            this.tagManager = new TagManager();
            this.tagManager.LoadConfig();
        }

        private long ResolvePosition(string plcName, string tagName)
        {
            // Find the PLC by its name
            var plcConfigList = tagManager.LoadConfig();
            var plc = plcConfigList.FirstOrDefault(p => p.Name == plcName);
            if (plc == null)
            {
                return -1;  // or throw an exception if the PLC is not found
            }

            // Find the tag within the PLC's tags
            var tagConfig = plc.Tags.FirstOrDefault(t => t.Name == tagName);
            if (tagConfig != null)
            {
                return tagConfig.MemoryPosition;
            }

            return -1;  // or throw an exception if the tag is not found
        }


        // This method retrieves the value of a given tag from the memory-mapped file.
        public T GetTagValue<T>(string plcName, string tagName) where T : struct
        {
            return MemoryMappedFileHelper.Retrieve<T>(ResolvePosition(plcName, tagName));
        }
    }
}
