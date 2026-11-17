namespace Codelex
{
    /// <summary>
    /// Elemento de lista desplegable: guarda el identificador, el texto que se ve y un tipo.
    /// </summary>
    class ItemLista
    {
        public int id;
        public string texto;
        public string tipo;

        /// <summary>
        /// Crea el objeto con sus valores iniciales.
        /// </summary>
        /// <param name="id">Identificador.</param>
        /// <param name="texto">Texto a usar.</param>
        /// <param name="tipo">Tipo de dato.</param>
        public ItemLista(int id, string texto, string tipo)
        {
            this.id = id;
            this.texto = texto;
            this.tipo = tipo;
        }

        /// <summary>
        /// Devuelve el texto que se muestra en las listas.
        /// </summary>
        /// <returns>Texto resultante.</returns>
        public override string ToString()
        {
            return texto;
        }
    }
}
