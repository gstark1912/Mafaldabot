using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PdfReaderService.Api.Models
{
    public class Fact
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("fact")]
        public string Text { get; set; }
    }
}