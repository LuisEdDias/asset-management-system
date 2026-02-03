namespace AssetManagement.Web.Models;

public static class AssetStatusTranslate
{
    public static string ToPtBr(string assetStatus) => assetStatus switch
    {
        "Available" => "Disponível",
        "InUse" => "Alocado",
        "Maintenance" => "Manutenção",
        _ => assetStatus
    };
}

