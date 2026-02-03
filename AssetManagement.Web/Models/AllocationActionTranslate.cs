namespace AssetManagement.Web.Models;

public static class AllocationActionTranslate
{
    public static string ToPtBr(string action) => action switch
    {
        "Allocated" => "Alocado",
        "Returned" => "Devolvido",
        _ => action
    };
}