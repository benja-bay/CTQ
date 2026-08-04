using Platforms.Data;

namespace Platforms.Modules
{
    public interface IPlatformModule
    {
        void Initialize(PlatformController controller, PlatformModuleData data);
    
        void OnUpdate();
    
        // Eventos de interacción con el jugador
        void OnPlayerEnter(PlayerMovement player);
        void OnPlayerStay(PlayerMovement player);
        void OnPlayerExit(PlayerMovement player);
    }
}