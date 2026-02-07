import { useEffect, useState } from 'react'
import { Canvas } from '@react-three/fiber'
import { OrbitControls, Grid } from '@react-three/drei'
import { vectorApi } from './services/api'
import { Enviroment } from './components/Scene'
import { VectorNode } from './components/VectorNode'
import {QueryNode} from './components/QueryNode'
import {PopoutSearch} from './components/PopoutSearch'
import { PopoutMenu } from './components/PopoutMenu'
import './App.css'


const API_URL = "http://localhost:5202/api/vector"

export default function App() {

  const [nodes, setNodes] = useState([]);
  const [mode, setMode] = useState('standard');
  const [selectedNode, setSelectedNode] = useState(null);

  //Fetch API
  useEffect(() => {
    const loadInitialData = async () => {
      try {
        const data = await vectorApi.getNodes();
        console.log("Response Type:", typeof data);
        console.log("Keys in Dictionary:", Object.keys(data).length);
        setNodes(Object.values(data));
      } catch (error) {
        console.error("The API is unreachable. Check the port");
      }
    };
    loadInitialData();
  }, []);


  const [queryPosition, setQueryPosition] = useState(null);
  const handleSearch = async (queryText) => {
    try {
      const results = await vectorApi.search(queryText);
      console.log("Search Results", results.queryPosition)
      
      const pos = results.queryPosition;
      if(pos) {
        console.log("Found position:", pos)
        setQueryPosition(pos);
      }else{
        console.error("the API returned data but 'queryPosition' was missing")
      }

      //CAN UPDATE NODES HERE BUT SHOULD WE?
    }catch(error) {
      console.error("Search failed:", error);
    }
  }

  const handleDocument = async (newDocument) => {
    try{
      await vectorApi.addDocument(newDocument);
      console.log("Added document:",  newDocument.id)
      const updatedNodes = await vectorApi.getNodes();
      const nodesArray = Object.values(updatedNodes);
      setNodes(nodesArray);
    }catch (error){
      console.log("ERROR OCCURD", error);
    }
  }


  return (
    <div style={{ width: '100vw', height: '100vh', position: 'relative' }}>

      {selectedNode && 
        <div className="node-info" style={infoBoxStyle}> 
        <button onClick={() => setSelectedNode(null)} style={{ float: 'right' }}>x</button>
        <h3>Node Details</h3>
        <p><strong>ID:</strong> {selectedNode.id}</p>
        <p><strong>Content:</strong> {selectedNode.content}</p>
        </div>
    
      }
      <div className="controls" style={{ 
        position: 'absolute', 
        zIndex: 1, 
        top: 20, 
        left: 20}}>
        <select onChange={(e) => setMode(e.target.value)}>
          <option value="standard">Standard</option>
          <option value="nebula">Nebula</option>
          <option value="blueprint">BluePrint</option>
        </select>
      </div>

      <div className="Search"style={{
        position: 'absolute',
        bottom: 20,
        left: 20,
        zIndex: 10
      }}>
        <PopoutSearch onSearch={handleSearch}/>
        <PopoutMenu onSave={handleDocument}/>
      </div>
      <Canvas camera={{ position: [10, 10, 10] }}>
        <Enviroment mode={mode} />

        {nodes.map((node) => (
          <VectorNode
            key={node.id}
            node={node}
            position={node.reducedVector}
            onSelect={setSelectedNode}
          />
        ))}

        {queryPosition && (
          <QueryNode position={queryPosition} color="yellow" />
        )}

        <OrbitControls makeDefault />
        
      </Canvas>
    </div>
  )
}

const infoBoxStyle = {
  position: 'absolute',
  top: '20px',
  right: '20px',
  background: '#222',
  color: 'white',
  padding: '15px',
  borderRadious: '8px',
  zIndex: 100,
  maxWidth: '300px'
}

