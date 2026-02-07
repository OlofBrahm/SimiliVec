import React,{useState, useEffect, useRef} from "react";
import { contain } from "three/src/extras/TextureUtils.js";

export const PopoutSearch = ({ onSearch }) => {
    const [isOpen, setIsOpen] = useState(false);
    const [query, setQuery] = useState("");
    const menuRef = useRef(null);

    useEffect(() => {
        const handleClickOutside = (event) => {
            if(menuRef.current && !menuRef.current.contains(event.target))
            {
                setIsOpen(false);
            }
        };

        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside)
        
    }, []);

    return (
        <div ref={menuRef} className="menu-container" style={{position: 'relative', display: 'inline-block'}}>
            <button onClick={() => setIsOpen(!isOpen)}>Search Vectors</button>
            {isOpen && (
                <nav className="popout" style={navStyle}>
                        <input
                        type="text"
                        placeholder="Search..."
                        value={query}
                        onChange={(e) => setQuery(e.target.value)}
                        style={{ padding: '5px' }}
                        />
                        <button onClick={() => onSearch(query)}>Submit Search</button>
                </nav>
            )}
        </div>
    );
};

const navStyle = {
    positsion: 'absolute',
    background: '#222',
    padding: '15px',
    borderRadius: '8px',
    top: '40px',
    color: 'white',
    display: 'flex',
    flexDirection: 'coloumn',
    gap: '10px',
    zIndex: 100
}