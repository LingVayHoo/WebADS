namespace WebADS.Models
{
    public class HistoryFormated
    {
        public DateTime ChangeDate { get; set; }
        public string OldValues { get; set; } = string.Empty;
        public string NewValues { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty;
        public string ChangedBy {  get; set; } = string.Empty;

        public HistoryFormated(AddressHistoryDBModel history)
        {
            ChangeDate = history.ChangeDate;
            OldValues = history.OldValues;
            NewValues = history.NewValues;
            ChangeType = history.ChangeType;
            ChangedBy = history.ChangedBy;
        }

        // Добавляем новое свойство для форматирования OldValues
        public string OldValuesFormatted
        {
            get
            {
                return ExtractZoneAndPlace(OldValues);
            }
        }

        // Добавляем новое свойство для форматирования NewValues
        public string NewValuesFormatted
        {
            get
            {
                return ExtractZoneAndPlace(NewValues);
            }
        }

        private string ExtractZoneAndPlace(string jsonString)
        {
            var values = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
            if (values == null) return string.Empty;

            string zone = values.ContainsKey("Zone") ? values["Zone"] : "N/A";
            string place = values.ContainsKey("Place") ? values["Place"] : "N/A";
            string row = values.ContainsKey("Row") ? values["Row"] : "N/A";
            string level = values.ContainsKey("Level") ? values["Level"] : "N/A";

            return $"Zone - {zone}, Place - {row}-{place}-{level}";
        }
    }
}
