using VectorDataBase.Datahandling;
using System.Collections.Generic;
namespace VectorDataBase.Interfaces;

public interface IDataLoader
{
    IEnumerable<DocumentModel> LoadDataFromFile();
    void SaveDataToFile<T>(T data);
    IEnumerable<DocumentModel> LoadAllDocuments();
}