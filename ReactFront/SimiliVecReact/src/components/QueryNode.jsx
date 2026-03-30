

export function QueryNode({ position }){

    return (
        <mesh position={position}>
            <sphereGeometry args={[0.1, 16, 4]}/>
            <meshStandardMaterial
            color={'#0011ff'}
            transparent={true}
            wireframe={true}
            opacity={50}
            emissiveIntensity={0.5}
            />
        </mesh>
    )
}