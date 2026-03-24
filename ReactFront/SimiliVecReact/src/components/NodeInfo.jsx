import React from 'react'
import '../css/NodeInfo.css'

export const NodeInfo = ({ node, onClose }) => {
  if (!node) return null;

  return (
    <div className="node-info">
      <button onClick={onClose} className="node-info-close">×</button>
      <h3>Node Details</h3>
      <p><strong>ID:</strong> {node.id}</p>
      <p><strong>Content:</strong> {node.content}</p>
    </div>
  );
};
