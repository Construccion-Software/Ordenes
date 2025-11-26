namespace Ordenes.Api.Settings
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string OrdenesCollectionName { get; set; } = "Ordenes";
        public string MedicamentosCollectionName { get; set; } = "Medicamentos";
        public string ProcedimientosCollectionName { get; set; } = "Procedimientos";
        public string AyudasDiagnosticasCollectionName { get; set; } = "AyudasDiagnosticas";
    }
}