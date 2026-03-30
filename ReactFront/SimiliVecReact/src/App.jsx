import { useEffect, useState } from 'react'
import { Canvas } from '@react-three/fiber'
import { OrbitControls, Grid } from '@react-three/drei'
import { vectorApi } from './services/api'
import { Enviroment } from './components/Scene'
import { VectorNode } from './components/VectorNode'
import { QueryNode } from './components/QueryNode'
import { PopoutSearch } from './components/PopoutSearch'
import { PopoutMenu } from './components/AddDocument'
import { SearchResults } from './components/SearchResults'
import { HomeScreen } from './components/HomeScreen'
import { Controls } from './components/Controls'
import { NodeInfo } from './components/NodeInfo'
import SimilivecLogo from './assets/similivec.svg?react'
import GithubIcon from './assets/github.svg?react'
import LinkedinIcon from './assets/linkedin.svg?react'
import './App.css'


const GITHUB_URL = 'https://github.com/OlofBrahm/SimiliVec'
const LINKEDIN_URL = 'https://www.linkedin.com/in/olofbrahm/'

export default function App() {
  const [showHome, setShowHome] = useState(true)
  const [nodes, setNodes] = useState([]);
  const [mode, setMode] = useState('standard');
  const [selectedNode, setSelectedNode] = useState(null);
  const [searchResults, setSearchResults] = useState([]);
  const [algo, setAlgo] = useState('pca');
  const [queryPosition, setQueryPosition] = useState(null);
  const [refreshKey, setRefreshKey] = useState(0);


  useEffect(() => {
    const loadNodes = async () => {
      try {
        let data;
        if (algo === 'umap') {
          data = await vectorApi.getUMAPNodes();
        } else {
          data = await vectorApi.getPCANodes();
        }

        const nodesArray = Array.isArray(data) ? data : Object.values(data);
        const normalizedNodes = nodesArray.map(n => ({
          ...n,
          displayPos: n.reducedVector || [n.x, n.y, n.z || 0]
        }));
        setNodes(normalizedNodes);
      } catch (error) {
        console.error("Failed to fetch nodes:", error);
      }
    };

    (async () => {
      await loadNodes();
    })();
  }, [algo, refreshKey]);

  const handleSearch = async (queryText) => {
    try {
      const data = algo === 'umap'
        ? await vectorApi.searchUmap(queryText)
        : await vectorApi.search(queryText);

      const pos = data.queryPosition;
      console.log("Query position received:", pos);

      if (pos) {
        setQueryPosition(Array.isArray(pos) ? pos : [pos[0] ?? 0, pos[1] ?? 0, pos[2] ?? 0]);
      } else {
        console.error("API returned data but 'queryPosition' was missing");
      }

      if (data.results) {
        setSearchResults(data.results);
      }
    } catch (error) {
      console.error("Search failed:", error);
    }
  }

  const handleDocument = async (newDocument) => {
    try {
      await vectorApi.addDocument(newDocument);
      console.log("Added document:", newDocument.id);
      setRefreshKey((current) => current + 1);
    } catch (error) {
      console.error("Failed to add document:", error);
    }
  }

  const handleContinue = () => {
    setShowHome(false);
  };

  const handleLogoClick = () => {
    setShowHome(true);
  };

  if (showHome) {
    return (
      <HomeScreen
        onContinue={handleContinue}
        githubUrl={GITHUB_URL}
        linkedinUrl={LINKEDIN_URL}
      />
    )
  }

  return (
    <>
      <header
        className='logo'
        onClick={handleLogoClick}
        onKeyDown={(event) => {
          if (event.key === 'Enter' || event.key === ' ') {
            event.preventDefault();
            handleLogoClick();
          }
        }}
        role="button"
        tabIndex={0}
      >
        <SimilivecLogo className="header-svg"/>
      </header>

      <footer className='socials'>
        <div className="logo-footer">
          <a href={GITHUB_URL} target="_blank" rel="noopener noreferrer" aria-label="GitHub">
            <GithubIcon className="social-icon"/>
          </a>
          <a href={LINKEDIN_URL} target="_blank" rel="noopener noreferrer" aria-label="LinkedIn">
            <LinkedinIcon className="social-icon"/>
          </a>
        </div>
      </footer>
      <SearchResults
        results={searchResults}
        onSelect={(hit) => {
          const node = nodes.find(n => n.id === hit.nodeId || n.documentId === hit.documentId);
          if (node) {
            setSelectedNode(node);
          } else {
            setSelectedNode({
              id: hit.documentId ?? `node:${hit.nodeId}`,
              content: hit.document?.content ?? ''
            });
          }
        }}
        onClose={() => setSearchResults([])}
      />

      <NodeInfo node={selectedNode} onClose={() => setSelectedNode(null)} />

      <Controls algo={algo} setAlgo={setAlgo} mode={mode} setMode={setMode} />

      <div className="Search" style={{
        position: 'absolute',
        bottom: 20,
        left: 20,
        zIndex: 10
      }}>
        <PopoutSearch onSearch={handleSearch} />
        <PopoutMenu onSave={handleDocument} />
      </div>
      <Canvas camera={{ position: [10, 10, 10] }}>
        <Enviroment mode={mode} />

        {nodes.map((node) => (
          <VectorNode
            key={`${algo}-${node.id}`}
            node={node}
            position={node.displayPos}
            onSelect={setSelectedNode}
          />
        ))}

        {queryPosition && <QueryNode position={queryPosition} />}

        <OrbitControls makeDefault />
      </Canvas>
    </>
  )
}

