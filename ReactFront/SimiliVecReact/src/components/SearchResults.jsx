import { Line } from "three";

export function SearchResults({ results, onSelect, onClose }) {
    if (!results || results.Length === 0) return null;

    return (
        <div style={containerStyle}>
            <div style={headerStyle}>
                <span style={{ fontWeight: 'bold' }}>Search Results</span>
                <button onClick={onClose} style={closeButtonStyle}>×</button>
            </div>
            <div style={listStyle}>
                {results.map((hit) => (
                    <div
                        key={hit.nodeId}
                        style={itemStyle}
                        onClick={() => onSelect(hit.document)}
                        onMouseEnter={(e) => (e.currentTarget.style.background = '#333')}
                        onMouseLeave={(e) => (e.currentTarget.style.background = 'transparent')}
                    >
                        <div style={{ fontSize: '0.75rem', color: '#aaa' }}>
                            <span style={simBadge}>
                                {Math.round(hit.similarity * 100)}% Match
                            </span>
                            <span style={{ color: '#666' }}> Dist: {hit.distance.toFixed(3)}</span>
                        </div>
                        <div style={contentStyle}>{hit.document.content}</div>
                    </div>
                ))
                }

            </div>
        </div>
    );
}

const containerStyle = {
  position: 'absolute',
  top: '80px',
  left: '20px',
  width: '300px',
  maxHeight: '50vh',
  background: 'rgba(25, 25, 25, 0.9)',
  backdropFilter: 'blur(8px)',
  border: '1px solid #3c3c3c',
  borderRadius: '10px',
  color: 'white',
  zIndex: 10,
  display: 'flex',
  flexDirection: 'column',
  boxShadow: '0 8px 32px rgba(0,0,0,0.5)'
};

const statsRow = {
  display: 'flex',
  justifyContent: 'space-between',
  fontSize: '0.7rem',
  marginBottom: '4px'
};

const simBadge = {
  background: '#1a4a2e',
  color: '#4ade80',
  padding: '2px 6px',
  borderRadius: '4px',
  fontWeight: 'bold'
};

const itemStyle = {
  padding: '12px',
  cursor: 'pointer',
  borderBottom: '1px solid #333',
  transition: 'all 0.2s ease'
};

const contentStyle = {
  fontSize: '0.85rem',
  lineHeight: '1.4',
  display: '-webkit-box',
  WebkitLineClamp: '3',
  WebkitBoxOrient: 'vertical',
  overflow: 'hidden'
};

const headerStyle = { 
    padding: '12px', 
    borderBottom: '1px solid #333', 
    display: 'flex', 
    justifyContent: 'space-between' };

const listStyle = { overflowY: 'auto' };

const closeButtonStyle = { 
    position: 'absolute',
    top: '25px',
    right: '-5px',
    transform: 'translate(-50%, -50%)',

    height: '24px',
    width: '24px',
    padding: '0px',
    boxSizing: 'border-box',

    background: 'none', 
    border: 'none', 
    color: '#888', 
    cursor: 'pointer', 
    fontSize: '1.2rem',
    fontFamily: 'sans-serif',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    lineHeight: '1',
    textAlign: 'center'

 };