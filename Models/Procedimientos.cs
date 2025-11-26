using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ordenes.Api.Models
{
    [BsonIgnoreExtraElements]
    public sealed class Procedimiento
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("numeroOrden")]
        public int NumeroOrden { get; set; }

        [BsonElement("numeroItem")]
        public int NumeroItem { get; set; }

        [BsonElement("catalogoId")]
        [BsonRequired]
        public string CatalogoId { get; set; } = string.Empty;

        [BsonElement("numeroVecesRepite")]
        public int NumeroVecesRepite { get; set; }

        [BsonElement("frecuencia")]
        public string Frecuencia { get; set; } = string.Empty;
    }
}