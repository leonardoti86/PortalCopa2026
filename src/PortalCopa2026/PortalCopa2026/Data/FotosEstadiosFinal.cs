namespace PortalCopa2026.Data;

// URLs fixas de fotos reais e de licença livre dos estádios do Terceiro Lugar e da Final,
// verificadas no Wikimedia Commons durante o design desta change (2026-09-17) - sem coluna nova
// no banco, já que não há foto de estádio em ./fontes nem na API pública da FIFA - design.md
// (criar-jogo-teceiro-lugar-final) - Decisão 6.
public static class FotosEstadiosFinal
{
    // Hard Rock Stadium, Miami Gardens - Wikimedia Commons, CC BY-SA 4.0, foto de A.J. Lipp.
    public const string FotoTerceiroLugar = "https://upload.wikimedia.org/wikipedia/commons/9/94/Hard_Rock_Stadium.jpg";

    // MetLife Stadium, East Rutherford - Wikimedia Commons, CC BY 4.0.
    public const string FotoFinal = "https://upload.wikimedia.org/wikipedia/commons/2/2a/MetLife_Stadium_Exterior%2C_2026_FIFA_World_Cup_%28June_20%2C_2026%29.jpg";
}
