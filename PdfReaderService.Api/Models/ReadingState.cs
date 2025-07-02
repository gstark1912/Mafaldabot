using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PdfReaderService.Api.Models
{
    public class ReadingState
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("runTimeOfDay")]
        public string RunTimeOfDay { get; set; }

        [BsonElement("lastRunDateTime")]
        public DateTime LastRunDateTime { get; set; }

        [BsonElement("filePath")]
        public string FilePath { get; set; }

        [BsonElement("startPage")]
        public int StartPage { get; set; }

        [BsonElement("currentPage")]
        public int CurrentPage { get; set; }

        [BsonElement("endPage")]
        public int EndPage { get; set; }
    }
}
