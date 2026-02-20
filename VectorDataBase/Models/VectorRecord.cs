using System;
using System.Collections.Generic;

namespace VectorDataBase.Models;

/// <summary>
/// Record representing a vector with metadata and original text
/// </summary>
public record VectorRecord(
    int id,
    Dictionary<string, string> Metadata,
    string OriginalText
);
