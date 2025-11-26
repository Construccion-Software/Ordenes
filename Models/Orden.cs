using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Ordenes.Api.Models.Enums;
using System;

namespace Ordenes.Api.Models
{
    
    public class Orden
    {
        
        [BsonId]
        public ObjectId Id {get;  set;}
        public int NumeroOrden {get; set;}
        public string ? CedulaPaciente  {get; set;}
        public string ? CedulaMedico {get; set;}
        public DateTime FechaCreacion {get; set;}
        public EstadoOrden Estado {get; set;}

    }
}