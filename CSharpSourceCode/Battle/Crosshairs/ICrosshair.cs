namespace TOW_Core.Battle.Crosshairs
{
    public interface ICrosshair
    {
        void Show();
        void Hide();
        void Tick();

        bool IsVisible { get; }
    }
}
