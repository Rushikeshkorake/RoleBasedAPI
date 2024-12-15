namespace RoleBasedAPI.Model
{
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Tags { get; set; }
        public string FilePath { get; set; }
        public string UploadedBy { get; set; }
        public DateTime UploadedDate { get; set; }
    }
}
