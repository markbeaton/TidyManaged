using System.IO;
using System.Threading.Tasks;

namespace TidyManaged
{
    public partial class Document
    {
        /// <summary>
        /// Parses input markup, executes configured cleanup, and repair operations asynchronously.
        /// </summary>
        /// <returns></returns>
        public Task CleanAndRepairAsync()
        {
            return Task.Run(() => CleanAndRepair());
        }

        /// <summary>
		/// Saves the processed markup to a string asynchronously.
		/// </summary>
		/// <returns>A string containing the processed markup.</returns>
        public Task<string> SaveAsync()
        {
            return Task.FromResult(Save());
        }

        /// <summary>
		/// Saves the processed markup to a file asynchronously.
		/// </summary>
		/// <param name="filePath">The full filesystem path of the file to save the markup to.</param>
        public Task SaveAsync(string filePath)
        {
            return Task.Run(() => Save(filePath));
        }

        /// <summary>
        /// Saves the processed markup to the supplied stream asynchronously.
        /// </summary>
        /// <param name="stream">A <see cref="System.IO.Stream"/> to write the markup to.</param>
        public Task SaveAsync(Stream stream)
        {
            return Task.Run(() => Save(stream));
        }
    }
}
