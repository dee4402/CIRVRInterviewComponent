using SimpleJSON;
namespace Cirvr.ConversationManager {
    public interface IConstructFromJson<T>
    {
        T ConstructFromNode(JSONNode node);
    }
}