import React,{useState, useEffect, useRef} from "react";
import '../css/Popout.css';

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
        <div ref={menuRef} className="menu-container">
            <button onClick={() => setIsOpen(!isOpen)}>Search Vectors</button>
            {isOpen && (
                <nav className="popout popout-search">
                        <input
                        type="text"
                        placeholder="Search..."
                        value={query}
                        onChange={(e) => setQuery(e.target.value)}
                        />
                        <button onClick={() => onSearch(query)}>Submit Search</button>
                </nav>
            )}
        </div>
    );
};
