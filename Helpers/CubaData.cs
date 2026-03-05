namespace GestionApp.Helpers
{
    /// <summary>
    /// Datos estáticos de Cuba: provincias y municipios.
    /// Fuente: División político-administrativa de Cuba (15 provincias + 1 municipio especial).
    /// </summary>
    public static class CubaData
    {
        public static readonly string[] Provincias =
        {
            "Pinar del Río",
            "Artemisa",
            "La Habana",
            "Mayabeque",
            "Matanzas",
            "Villa Clara",
            "Cienfuegos",
            "Sancti Spíritus",
            "Ciego de Ávila",
            "Camagüey",
            "Las Tunas",
            "Holguín",
            "Granma",
            "Santiago de Cuba",
            "Guantánamo",
            "Isla de la Juventud"
        };

        public static readonly Dictionary<string, string[]> Municipios = new()
        {
            ["Pinar del Río"] = new[]
            {
                "Sandino", "Mantua", "Minas de Matahambre", "Viñales", "La Palma",
                "Los Palacios", "Consolación del Sur", "Pinar del Río", "San Luis",
                "San Juan y Martínez", "Guane"
            },
            ["Artemisa"] = new[]
            {
                "Bahía Honda", "Mariel", "Guanajay", "Caimito", "Bauta",
                "San Antonio de los Baños", "Güira de Melena", "Alquízar",
                "Artemisa", "Candelaria", "San Cristóbal"
            },
            ["La Habana"] = new[]
            {
                "Playa", "Plaza de la Revolución", "Centro Habana", "La Habana Vieja",
                "Regla", "La Habana del Este", "Guanabacoa", "San Miguel del Padrón",
                "Diez de Octubre", "Cerro", "Marianao", "La Lisa", "Boyeros",
                "Arroyo Naranjo", "Cotorro"
            },
            ["Mayabeque"] = new[]
            {
                "Bejucal", "San José de las Lajas", "Jaruco", "Santa Cruz del Norte",
                "Madruga", "Nueva Paz", "San Nicolás", "Güines", "Melena del Sur",
                "Batabanó", "Quivicán"
            },
            ["Matanzas"] = new[]
            {
                "Matanzas", "Cárdenas", "Martí", "Colón", "Perico",
                "Jovellanos", "Pedro Betancourt", "Limonar", "Unión de Reyes",
                "Ciénaga de Zapata", "Jagüey Grande", "Calimete", "Los Arabos"
            },
            ["Villa Clara"] = new[]
            {
                "Corralillo", "Quemado de Güines", "Sagua la Grande", "Encrucijada",
                "Camajuaní", "Caibarién", "Remedios", "Placetas", "Santa Clara",
                "Cifuentes", "Santo Domingo", "Ranchuelo", "Manicaragua"
            },
            ["Cienfuegos"] = new[]
            {
                "Aguada de Pasajeros", "Rodas", "Palmira", "Lajas",
                "Cruces", "Cumanayagua", "Cienfuegos", "Abreus"
            },
            ["Sancti Spíritus"] = new[]
            {
                "Yaguajay", "Jatibonico", "Taguasco", "Cabaiguán",
                "Fomento", "Trinidad", "Sancti Spíritus", "La Sierpe"
            },
            ["Ciego de Ávila"] = new[]
            {
                "Chambas", "Morón", "Bolivia", "Primero de Enero",
                "Ciro Redondo", "Florencia", "Majagua", "Ciego de Ávila",
                "Venezuela", "Baraguá"
            },
            ["Camagüey"] = new[]
            {
                "Carlos Manuel de Céspedes", "Esmeralda", "Sierra de Cubitas",
                "Minas", "Nuevitas", "Guáimaro", "Sibanicú", "Camagüey",
                "Florida", "Vertientes", "Jimaguayú", "Najasa",
                "Santa Cruz del Sur"
            },
            ["Las Tunas"] = new[]
            {
                "Manatí", "Puerto Padre", "Jesús Menéndez", "Majibacoa",
                "Las Tunas", "Jobabo", "Colombia", "Amancio"
            },
            ["Holguín"] = new[]
            {
                "Gibara", "Rafael Freyre", "Banes", "Antilla", "Báguanos",
                "Holguín", "Calixto García", "Cacocum", "Urbano Noris",
                "Cueto", "Mayarí", "Frank País", "Sagua de Tánamo", "Moa"
            },
            ["Granma"] = new[]
            {
                "Río Cauto", "Cauto Cristo", "Jiguaní", "Bayamo",
                "Yara", "Manzanillo", "Campechuela", "Media Luna",
                "Niquero", "Pilón", "Bartolomé Masó", "Buey Arriba",
                "Guisa"
            },
            ["Santiago de Cuba"] = new[]
            {
                "Contramaestre", "Mella", "San Luis", "Segundo Frente",
                "Songo-La Maya", "Santiago de Cuba", "Palma Soriano",
                "Tercer Frente", "Guamá"
            },
            ["Guantánamo"] = new[]
            {
                "El Salvador", "Manuel Tames", "Yateras", "Baracoa",
                "Maisí", "Imías", "San Antonio del Sur", "Caimanera",
                "Guantánamo", "Niceto Pérez"
            },
            ["Isla de la Juventud"] = new[]
            {
                "Isla de la Juventud"
            }
        };

        /// <summary>
        /// Obtiene los municipios de una provincia.
        /// </summary>
        public static string[] ObtenerMunicipios(string provincia)
        {
            if (string.IsNullOrWhiteSpace(provincia)) return Array.Empty<string>();
            return Municipios.TryGetValue(provincia, out var municipios) ? municipios : Array.Empty<string>();
        }
    }
}
