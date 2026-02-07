import { useState } from 'react'

export function VectorNode({ position, color = 'orange', node, onSelect }) {
    const[hovered, setHover] = useState(false);

    return (
        <mesh
            position={position}
            onPointerOver={() => setHover(true)}
            onPointerOut={() => setHover(false)}
            onClick={(e) => {
                e.stopPropagation();
                onSelect(node);
            }}
        >
            <sphereGeometry args={[0.05, 16, 10]}/>
            <meshStandardMaterial
            color={hovered ? 'hotpink' : color}
            emissive={hovered ? 'hotpink' : 'black'}
            emissiveIntensity={0.5}
            />
        </mesh>
    );
}