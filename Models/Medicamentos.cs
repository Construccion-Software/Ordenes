using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ordenes.Api.Models
{
    [BsonIgnoreExtraElements]
    public sealed class Medicamento
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

        [BsonElement("dosis")]
        public string Dosis { get; set; } = string.Empty;

        [BsonElement("duracionTratamiento")]
        public int DuracionTratamiento { get; set; }
    }
}
