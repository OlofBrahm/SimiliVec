import React, { useState, useEffect, useRef } from 'react'
import '../css/Popout.css'

export const PopoutMenu = ({ onSave }) => {
    const [isOpen, setIsOpen] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
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
        if (isLoading) {
            return;
        }

        if(!id.trim() || !content.trim()){
            alert("Please fill out both fields");
            return;
        }

        const newDocument = {
            id: id.trim(),
            content: content.trim()
        };
        if (typeof onSave !== "function") {
            alert("Save handler is missing.");
            return;
        }

        try {
            setIsLoading(true);
            await onSave(newDocument);

            //Reset state only after successful save
            setId("");
            setContent("");
            setIsOpen(false);
        } catch (error) {
            const message = error instanceof Error ? error.message : "Failed to save document.";
            alert(message);
        } finally {
            setIsLoading(false);
        }
    }

    return (
        <div ref={menuRef} className="menu-container">
            <button 
                onClick={() => setIsOpen(!isOpen)}
                aria-expanded={isOpen}
                aria-controls="add-document-popout"
            >
                Add Document
            </button>
            {isOpen && (
                <form id="add-document-popout" className="popout popout-menu" onSubmit={(e) => { e.preventDefault(); handleSubmit(); }}>
                    <label htmlFor="doc-id" className="visually-hidden">Document ID</label>
                    <input
                        id="doc-id"
                        type="text"
                        placeholder="ID"
                        value={id}
                        onChange={(e) => setId(e.target.value)}
                    />
                    <label htmlFor="doc-content" className="visually-hidden">Document Content</label>
                    <input
                        id="doc-content"
                        type="text"
                        placeholder="Content"
                        value={content}
                        onChange={(e) => setContent(e.target.value)}
                    />
                    <button type="submit" disabled={isLoading}>{isLoading ? 'Saving...' : 'Save Document'}</button>
                </form>
            )
            }
        </div>
    ) 
}