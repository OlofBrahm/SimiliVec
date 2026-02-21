import React, { useState, useEffect, useRef } from 'react'
import { BasicDepthPacking } from 'three';

export const PopoutMenu = ({ onSave }) => {
    const [isOpen, setIsOpen] = useState(false);
    const [id, setId] = useState("");
    const [content, setContent] = useState("");
    const menuRef = useRef(null);

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (menuRef.current && !menuRef.current.contains(event.target)) {
                setIsOpen(false);
            }
        };

        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside)
    }, []);

    const handleSubmit = () => {
        if(!id.trim() || !content.trim()){
            alert("Please fill out both fields");
            return;
        }

        const newDocument = {
            id: id,
            content: content
        };

        onSave(newDocument);

        //Reset state
        setId("");
        setContent("");
        setIsOpen(false);
    }

    return (
        <div ref={menuRef} className="menu-container" style={{ position: 'relative', display: 'inline-block' }}>
            <button onClick={() => setIsOpen(!isOpen)}>Add Document</button>
            {isOpen && (
                <nav className="popout" style={navStyle}>
                    <input
                        type="text"
                        placeholder="ID"
                        value={id}
                        onChange={(e) => setId(e.target.value)}
                        style={{ padding: '5px' }}
                    />
                    <input
                        type="text"
                        placeholder="Content"
                        value={content}
                        onChange={(e) => setContent(e.target.value)}
                        style={{ padding: '5px' }}
                    />
                    <button onClick={handleSubmit}>Save Document</button>
                </nav>
            )

            }
        </div>
    )
}
const navStyle = {
    position: 'absolute',
    background: '#222',
    padding: '5px',
    borderRadius: '8px',
    bottom: 50,
    color: 'white',
    display: 'flex',
    flexDirection: 'row',
    gap: '5px',
    zIndex: 100
}
