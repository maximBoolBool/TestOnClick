namespace Assets.Scripts.Managers
{
    public interface IEquipmentUiManager
    {
        void SetEquipmentPanelActive(bool isActive);
        bool IsEquipmentPanelActive { get; }
    }

    public class EquipmentUiManager : IEquipmentUiManager
    {
        public bool IsEquipmentPanelActive { get; private set; }

        public void SetEquipmentPanelActive(bool isActive)
        {
            IsEquipmentPanelActive = isActive;
        }
    }
}
