import { useState } from 'react'

export function QueryNode({ position }) {
    const [hovered, setHover] = useState(false);

    return (
        <mesh
            position={position}
            onPointerOver={() => setHover(true)}
            onPointerOut={() => setHover(false)}>

            <sphereGeometry args={[0.1, 16, 16]}/>
            <meshStandardMaterial
            color={hovered ? 'darkgreen' : 'green'}
            //emissive={hovered ? 'red' : 'black'}
            emissiveIntensity={0.5}
            />
        </mesh>
    )
}