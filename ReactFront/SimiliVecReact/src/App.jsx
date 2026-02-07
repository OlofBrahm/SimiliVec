import { useEffect, useState } from 'react'
import { Canvas } from '@react-three/fiber'
import { OrbitControls, Grid } from '@react-three/drei'
import { vectorApi } from './services/api'
import { Enviroment } from './components/Scene'
import { VectorNode } from './components/VectorNode'
import {QueryNode} from './components/QueryNode'
import {PopoutSearch} from './components/PopoutSearch'
import './App.css'


const API_URL = "http://localhost:5202/api/vector"

export default function App() {

  const [nodes, setNodes] = useState([]);
  const [mode, setMode] = useState('standard');

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


  return (
    <div style={{ width: '100vw', height: '100vh', position: 'relative' }}>
      <div className="controls" style={{ 
        position: 'absolute', 
        zIndex: 1, 
        bottom: 20, 
        right: 20}}>
        <select onChange={(e) => setMode(e.target.value)}>
          <option value="standard">Standard</option>
          <option value="nebula">Nebula</option>
          <option value="blueprint">BluePrint</option>
        </select>
      </div>

      <div className="Search"style={{
        position: 'absolute',
        top: 20,
        left: 20,
        zIndex: 10
      }}>
        <PopoutSearch onSearch={handleSearch}/>
      </div>
      <Canvas camera={{ position: [10, 10, 10] }}>
        <Enviroment mode={mode} />

        {nodes.map((node) => (
          <VectorNode
            key={node.id}
            position={node.reducedVector}
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

