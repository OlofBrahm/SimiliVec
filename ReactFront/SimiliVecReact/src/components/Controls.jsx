import '../css/Controls.css';

export function Controls({ algo, setAlgo, mode, setMode }) {
  return (
    <div className="controls">
      <label htmlFor="algo-select" className="visually-hidden">Algorithm</label>
      <select 
        id="algo-select"
        className="control-select"
        value={algo} 
        onChange={(e) => setAlgo(e.target.value)}
      >
        <option value="pca">PCA (Linear)</option>
        <option value="umap">UMAP (clusters)</option>
      </select>
      <label htmlFor="mode-select" className="visually-hidden">Display mode</label>
      <select 
        id="mode-select"
        className="control-select"
        value={mode} 
        onChange={(e) => setMode(e.target.value)}
      >
        <option value="standard">Standard</option>
        <option value="nebula">Nebula</option>
        <option value="blueprint">Blueprint</option>
      </select>
    </div>
  );
}
