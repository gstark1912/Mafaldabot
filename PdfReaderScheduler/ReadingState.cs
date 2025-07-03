using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class ReadingState
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("runTimeOfDay")]
    public string RunTimeOfDay { get; set; } = string.Empty;

    [BsonElement("lastRunDateTime")]
    public DateTime LastRunDateTime { get; set; }

    [BsonElement("filePath")]
    public string FilePath { get; set; } = string.Empty;

    [BsonElement("startPage")]
    public int StartPage { get; set; }

    [BsonElement("currentPage")]
    public int CurrentPage { get; set; }

    [BsonElement("endPage")]
    public int EndPage { get; set; }
}