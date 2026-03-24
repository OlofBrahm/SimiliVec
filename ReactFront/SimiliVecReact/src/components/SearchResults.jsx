import '../css/SearchResults.css';

export function SearchResults({ results, onSelect, onClose }) {
    if (!results || results.length === 0) return null;



    return (
        <div className="search-results-container">
            <div className="search-results-header">
                <span className="search-results-title">Search Results</span>
                <button onClick={onClose} className="search-results-close">×</button>
            </div>
            <div className="search-results-list">
                {results.map((hit) => (
                    <div
                        key={hit.nodeId}
                        className="search-results-item"
                        onClick={() => onSelect(hit)}
                    >
                        <div className="search-results-meta">
                            <span className="search-results-sim-badge">
                                {percentFromSimilarity(hit.similarity)}% Match
                            </span>
                            <span className="search-results-distance"> Dist: {hit.distance.toFixed(3)}</span>
                        </div>
                        <div className="search-results-content">{hit.document?.content}</div>
                    </div>
                ))
                }

            </div>
        </div>
    );
}
function percentFromSimilarity(sim) {
  const min = 0.7, max = 1.0
  const clamped = Math.max(min, Math.min(max, Number(sim) || min))
  return Math.round(((clamped - min) / (max - min)) * 100)
}
