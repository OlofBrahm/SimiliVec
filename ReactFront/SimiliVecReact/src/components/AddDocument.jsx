import React, { useState, useEffect, useRef } from 'react'
import '../css/Popout.css'

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

    const handleSubmit = async () => {
        if(!id.trim() || !content.trim()){
            alert("Please fill out both fields");
            return;
        }

        const newDocument = {
            id: id,
            content: content
        };

        if (typeof onSave !== "function") {
            alert("Save handler is missing.");
            return;
        }

        try {
            await onSave(newDocument);

            //Reset state only after successful save
            setId("");
            setContent("");
            setIsOpen(false);
        } catch (error) {
            const message = error instanceof Error ? error.message : "Failed to save document.";
            alert(message);
        }
    }

    return (
        <div ref={menuRef} className="menu-container">
            <button onClick={() => setIsOpen(!isOpen)}>Add Document</button>
            {isOpen && (
                <nav className="popout popout-menu">
                    <input
                        type="text"
                        placeholder="ID"
                        value={id}
                        onChange={(e) => setId(e.target.value)}
                    />
                    <input
                        type="text"
                        placeholder="Content"
                        value={content}
                        onChange={(e) => setContent(e.target.value)}
                    />
                    <button onClick={handleSubmit}>Save Document</button>
                </nav>
            )

            }
        </div>
    )
}