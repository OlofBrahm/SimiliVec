import { useState } from 'react'

export function QueryNode({ position }) {
    const [hovered, setHover] = useState(false);

    return (
        <mesh
            position={position}
            onPointerOver={() => setHover(true)}
            onPointerOut={() => setHover(false)}>

            <sphereGeometry args={[0.1, 16, 4]}/>
            <meshStandardMaterial
            color={'#0011ff'}
            //emissive={hovered ? 'red' : 'black'}
            transparent={true}
            wireframe={true}
            opacity={50}
            emissiveIntensity={0.5}
            />
        </mesh>
    )
}