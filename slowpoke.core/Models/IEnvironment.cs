namespace slowpoke.core.Models;


public interface IEnvironment
{
    string AppRootPath { get; }
    
    string ContentRootPath { get; }
}