using CommonTools.Classes;
using CommonTools;

namespace WebHMI.Data
{
    public class TagDataService
    {
        // returns an array of Tag objects with the latest data from memory
        public List<Tag> GetTagData()
        {
            // Get the current tag configuration
            TagManager tagManager = new();
            var plcList = tagManager.LoadConfig();

            // return object and temporary Tag object
            List<Tag> allTags = new List<Tag>();

            // memory map service for getting current tag values
            MemoryMapService memMap = new();

            // create a Tag object for all tracked tags
            if (plcList != null && plcList.Count > 0)
            {
                foreach (PLCConfig plc in plcList)
                {
                    if (plc.Tags == null || plc.Tags.Count == 0) continue;
                    foreach (TagConfig tag in plc.Tags)
                    {
                        Tag currentTag = new();

                        currentTag.plc = plc.Name;
                        currentTag.ipAddress = plc.IPAddress;
                        currentTag.name = tag.Name;
                        currentTag.type = tag.Type.ToUpper();

                        // get the tag's current value
                        if (tag.Type == "bool")
                        {
                            currentTag.data = "" + memMap.GetTagValue<bool>(plc.Name, tag.Name);
                        }
                        else if (tag.Type == "dint")
                        {
                            currentTag.data = "" + memMap.GetTagValue<int>(plc.Name, tag.Name);
                        }
                        currentTag.lastUpdate = currentTag.lastUpdate = DateTime.UtcNow.ToShortDateString() + " at " + DateTime.UtcNow.ToShortTimeString();

                        // add the tag to the allTags array to be sent to the front end
                        allTags.Add(currentTag);
                    }
                }
            }
            return allTags;
        }

        public void AddTag(Tag tag)
        {
            // Get the current tag configuration
            TagManager tagManager = new();
            var plcList = tagManager.LoadConfig();

            tagManager.AddTagToConfig(tag.plc, tag.ipAddress,tag.name, tag.type);

        }

        public void RemoveTag(Tag tag)
        {
            // Get the current tag configuration
            TagManager tagManager = new();
            var plcList = tagManager.LoadConfig();

            tagManager.DeleteTagFromConfig(tag.plc, tag.name);

        }

        public void UpdateTag(Tag tag)
        {
            // Get the current tag configuration
            TagManager tagManager = new();
            var plcList = tagManager.LoadConfig();

            tagManager.UpdateTagFromConfig(tag.plc, tag.name, tag.type);

        }
    }
}
