using System.Globalization;
using System.Text;

namespace SistemaCotizaciones.Web.Services;

public class ChileGeoService
{
    private static readonly List<RegionChile> Regiones =
    [
        new("15", "XV", "Arica y Parinacota", "Arica", ["Arica", "Camarones", "Putre", "General Lagos"]),
        new("01", "I", "Tarapaca", "Iquique", ["Iquique", "Alto Hospicio", "Pozo Almonte", "Camina", "Colchane", "Huara", "Pica"]),
        new("02", "II", "Antofagasta", "Antofagasta", ["Antofagasta", "Mejillones", "Sierra Gorda", "Taltal", "Calama", "Ollague", "San Pedro de Atacama", "Tocopilla", "Maria Elena"]),
        new("03", "III", "Atacama", "Copiapo", ["Copiapo", "Caldera", "Tierra Amarilla", "Chanaral", "Diego de Almagro", "Vallenar", "Alto del Carmen", "Freirina", "Huasco"]),
        new("04", "IV", "Coquimbo", "La Serena", ["La Serena", "Coquimbo", "Andacollo", "La Higuera", "Vicuña", "Illapel", "Canela", "Los Vilos", "Salamanca", "Ovalle", "Combarbala", "Monte Patria", "Punitaqui", "Rio Hurtado", "Paiguano"]),
        new("05", "V", "Valparaiso", "Valparaiso", ["Valparaiso", "Casablanca", "Concon", "Juan Fernandez", "Puchuncavi", "Quintero", "Viña del Mar", "Isla de Pascua", "Los Andes", "Calle Larga", "Rinconada", "San Esteban", "La Ligua", "Cabildo", "Papudo", "Petorca", "Zapallar", "Quillota", "Calera", "Hijuelas", "La Cruz", "Nogales", "San Antonio", "Algarrobo", "Cartagena", "El Quisco", "El Tabo", "Santo Domingo", "San Felipe", "Catemu", "Llaillay", "Panquehue", "Putaendo", "Santa Maria", "Quilpue", "Limache", "Olmue", "Villa Alemana"]),
        new("13", "RM", "Metropolitana de Santiago", "Santiago", ["Santiago", "Cerrillos", "Cerro Navia", "Conchali", "El Bosque", "Estacion Central", "Huechuraba", "Independencia", "La Cisterna", "La Florida", "La Granja", "La Pintana", "La Reina", "Las Condes", "Lo Barnechea", "Lo Espejo", "Lo Prado", "Macul", "Maipu", "Nunoa", "Pedro Aguirre Cerda", "Penalolen", "Providencia", "Pudahuel", "Quilicura", "Quinta Normal", "Recoleta", "Renca", "San Joaquin", "San Miguel", "San Ramon", "Vitacura", "Puente Alto", "Pirque", "San Jose de Maipo", "Colina", "Lampa", "Tiltil", "San Bernardo", "Buin", "Calera de Tango", "Paine", "Melipilla", "Alhue", "Curacavi", "Maria Pinto", "San Pedro", "Talagante", "El Monte", "Isla de Maipo", "Padre Hurtado", "Penaflor"]),
        new("06", "VI", "O'Higgins", "Rancagua", ["Rancagua", "Codegua", "Coinco", "Coltauco", "Donihue", "Graneros", "Las Cabras", "Machali", "Malloa", "Mostazal", "Olivar", "Peumo", "Pichidegua", "Quinta de Tilcoco", "Rengo", "Requinoa", "San Vicente", "Pichilemu", "La Estrella", "Litueche", "Marchigue", "Navidad", "Paredones", "San Fernando", "Chepica", "Chimbarongo", "Lolol", "Nancagua", "Palmilla", "Peralillo", "Placilla", "Pumanque", "Santa Cruz"]),
        new("07", "VII", "Maule", "Talca", ["Talca", "Constitucion", "Curepto", "Empedrado", "Maule", "Pelarco", "Pencahue", "Rio Claro", "San Clemente", "San Rafael", "Cauquenes", "Chanco", "Pelluhue", "Curico", "Hualane", "Licanten", "Molina", "Rauco", "Romeral", "Sagrada Familia", "Teno", "Vichuquen", "Linares", "Colbun", "Longavi", "Parral", "Retiro", "San Javier", "Villa Alegre", "Yerbas Buenas"]),
        new("16", "XVI", "Ñuble", "Chillan", ["Chillan", "Bulnes", "Chillan Viejo", "El Carmen", "Pemuco", "Pinto", "Quillon", "San Ignacio", "Yungay", "Quirihue", "Cobquecura", "Coelemu", "Ninhue", "Portezuelo", "Ranquil", "Treguaco", "San Carlos", "Coihueco", "Niquen", "San Fabian", "San Nicolas"]),
        new("08", "VIII", "Biobio", "Concepcion", ["Concepcion", "Coronel", "Chiguayante", "Florida", "Hualqui", "Lota", "Penco", "San Pedro de la Paz", "Santa Juana", "Talcahuano", "Tome", "Hualpen", "Lebu", "Arauco", "Canete", "Contulmo", "Curanilahue", "Los Alamos", "Tirua", "Los Angeles", "Antuco", "Cabrero", "Laja", "Mulchen", "Nacimiento", "Negrete", "Quilaco", "Quilleco", "San Rosendo", "Santa Barbara", "Tucapel", "Yumbel", "Alto Biobio"]),
        new("09", "IX", "La Araucania", "Temuco", ["Temuco", "Carahue", "Cunco", "Curarrehue", "Freire", "Galvarino", "Gorbea", "Lautaro", "Loncoche", "Melipeuco", "Nueva Imperial", "Padre Las Casas", "Perquenco", "Pitrufquen", "Pucon", "Saavedra", "Teodoro Schmidt", "Tolten", "Vilcun", "Villarrica", "Cholchol", "Angol", "Collipulli", "Curacautin", "Ercilla", "Lonquimay", "Los Sauces", "Lumaco", "Puren", "Renaico", "Traiguen", "Victoria"]),
        new("14", "XIV", "Los Rios", "Valdivia", ["Valdivia", "Corral", "Lanco", "Los Lagos", "Mafil", "Mariquina", "Paillaco", "Panguipulli", "La Union", "Futrono", "Lago Ranco", "Rio Bueno"]),
        new("10", "X", "Los Lagos", "Puerto Montt", ["Puerto Montt", "Calbuco", "Cochamo", "Fresia", "Frutillar", "Los Muermos", "Llanquihue", "Maullin", "Puerto Varas", "Castro", "Ancud", "Chonchi", "Curaco de Velez", "Dalcahue", "Puqueldon", "Queilen", "Quellon", "Quemchi", "Quinchao", "Osorno", "Puerto Octay", "Purranque", "Puyehue", "Rio Negro", "San Juan de la Costa", "San Pablo", "Chaiten", "Futaleufu", "Hualaihue", "Palena"]),
        new("11", "XI", "Aysen", "Coyhaique", ["Coyhaique", "Lago Verde", "Aysen", "Cisnes", "Guaitecas", "Cochrane", "O'Higgins", "Tortel", "Chile Chico", "Rio Ibanez"]),
        new("12", "XII", "Magallanes y la Antartica Chilena", "Punta Arenas", ["Punta Arenas", "Laguna Blanca", "Rio Verde", "San Gregorio", "Cabo de Hornos", "Antartica", "Porvenir", "Primavera", "Timaukel", "Natales", "Torres del Paine"])
    ];

    public IEnumerable<string> BuscarRegiones(string? texto)
    {
        return Filtrar(Regiones.Select(FormatoRegion), texto);
    }

    public IEnumerable<string> BuscarComunas(string? region, string? texto)
    {
        return Filtrar(ObtenerRegion(region)?.Comunas ?? Regiones.SelectMany(x => x.Comunas), texto);
    }

    public IEnumerable<string> BuscarCiudades(string? region, string? comuna, string? texto)
    {
        var items = ObtenerRegion(region)?.Comunas ?? Regiones.SelectMany(x => x.Comunas);
        if (!string.IsNullOrWhiteSpace(comuna))
        {
            items = items.Where(x => Normalizar(x).Contains(Normalizar(comuna)) || Normalizar(comuna).Contains(Normalizar(x)));
        }

        return Filtrar(items, texto);
    }

    public bool ComunaPerteneceARegion(string? region, string? comuna)
    {
        return ObtenerRegion(region)?.Comunas.Any(x => string.Equals(x, comuna, StringComparison.OrdinalIgnoreCase)) == true;
    }

    private static RegionChile? ObtenerRegion(string? region)
    {
        if (string.IsNullOrWhiteSpace(region))
        {
            return null;
        }

        var normalizada = Normalizar(region);
        return Regiones.FirstOrDefault(x =>
            Normalizar(FormatoRegion(x)) == normalizada ||
            Normalizar(x.Nombre) == normalizada ||
            Normalizar(x.Codigo) == normalizada ||
            Normalizar(x.Romano) == normalizada);
    }

    private static IEnumerable<string> Filtrar(IEnumerable<string> items, string? texto)
    {
        var normalizado = Normalizar(texto);
        return items
            .Where(x => string.IsNullOrWhiteSpace(normalizado) || Normalizar(x).Contains(normalizado))
            .Distinct()
            .OrderBy(x => x)
            .Take(80);
    }

    private static string FormatoRegion(RegionChile region)
    {
        return $"{region.Codigo} - {region.Nombre}";
    }

    private static string Normalizar(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private sealed record RegionChile(string Codigo, string Romano, string Nombre, string Capital, IReadOnlyList<string> Comunas);
}
