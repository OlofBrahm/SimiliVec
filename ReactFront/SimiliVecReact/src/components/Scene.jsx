import {Grid, Stars, Sparkles, ContactShadows} from '@react-three/drei'

export function Enviroment({ mode })
{
    return(
        <>
        <ambientLight intensity={5.5}/>
        <pointLight position={[10,10,10]} intensity={1.5}/>

        {mode === 'standard' && <Grid infiniteGrid fadeDistance={50} sectionColor={"#444"}/>}
        {mode === 'nebula' && (
            <>
            <Stars radius={100} depth={50} count={5000} factor={4} saturation={0} fade speed={1}/>
            <Sparkles count={200} scale={10} size={2} speed={0.4}/>
            </>
        )}

        {mode === 'blueprint' && (
            <Grid infiniteGrid cellColor={"cyan"} sectionColor={'white'} sectionThickness={1.5} />
        )}

        <ContactShadows opacity={0.4} scale={20} blur={2.4} far={4.5}/>
        </>
    )
}