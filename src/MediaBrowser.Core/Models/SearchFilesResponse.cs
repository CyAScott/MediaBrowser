using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Models
{
    /// <summary>
    /// The search parameters for finding media files.
    /// </summary>
    public class SearchFilesResponse<TFile> : SearchFilesRequest
    {
        /// <inheritdoc/>
        public SearchFilesResponse(SearchFilesRequest request, int count, IEnumerable<TFile> files)
        {
            Ascending = request.Ascending;
            Count = count;
            Filter = request.Filter;
            Keywords = request.Keywords;
            Results = files.ToArray();
            Skip = request.Skip;
            Sort = request.Sort;
            Take = request.Take;
        }

        /// <summary>
        /// The found media files.
        /// </summary>
        public TFile[] Results { get; }

        /// <summary>
        /// The number of media files the query could return.
        /// </summary>
        public int Count { get; set; }
    }
}
