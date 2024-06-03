namespace WebHMI.Data
{
    public class Press
    {
        // Left
        public string? L_DeliveryTime { get; set; }
        public string? L_CureTime { get; set; }
        public bool L_TireBeingDelivered { get; set; }
        public bool L_TireOnHold { get; set; }
        public bool L_TireInPan { get; set; }
        public bool L_TireInLoader { get; set; }
        public bool L_TireInCavity { get; set; }
        public bool L_InAutoDelivery { get; set; }
        public bool L_CavityAlarm { get; set; }
        public bool L_PanAlarm { get; set; }

        // Right
        public string? R_DeliveryTime { get; set; }
        public string? R_CureTime { get; set; }
        public bool R_TireBeingDelivered { get; set; }
        public bool R_TireOnHold { get; set; }
        public bool R_TireInPan { get; set; }
        public bool R_TireInLoader { get; set; }
        public bool R_TireInCavity { get; set; }
        public bool R_InAutoDelivery { get; set; }
        public bool R_CavityAlarm { get; set; }
        public bool R_PanAlarm { get; set; }


    }
}
