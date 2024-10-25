namespace IdCode.Models {
    public class ManufacturerModel {
        public ManufacturerModel(int id, string manufacturerName) {
            Id = id;
            ManufacturerName = manufacturerName;
        }

        public int Id { get; set; }
        public string ManufacturerName { get; set; }
    }
}