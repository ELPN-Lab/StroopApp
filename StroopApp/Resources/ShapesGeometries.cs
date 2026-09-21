using System.Windows;
using System.Windows.Media;

namespace StroopApp.Resources
{
    public static class ShapesGeometries
    {
        public static Geometry Circle { get; } = Frozen(new EllipseGeometry(new Point(50, 50), 50, 50));
        public static Geometry Square { get; } = Frozen(new RectangleGeometry(new Rect(0, 0, 100, 100)));
        public static Geometry Triangle { get; } = Frozen("M43.3013 0L86.6025 75H0L43.3013 0Z");  //M50,0 L100,100 L0,100 Z
        public static Geometry LeftArrow { get; } = Frozen("M9.2945 18.9112C9.72155 18.7306 10 18.3052 10 17.8333V15H21C21.5523 15 22 14.5523 22 14V10C22 9.44772 21.5523 9 21 9H10V6.1667C10 5.69483 9.72155 5.26942 9.2945 5.08884C8.86744 4.90826 8.37588 5.00808 8.04902 5.34174L2.33474 11.175C1.88842 11.6307 1.88842 12.3693 2.33474 12.825L8.04902 18.6583C8.37588 18.9919 8.86744 19.0917 9.2945 18.9112Z");
        public static Geometry RightArrow { get; } = Frozen("M14.7055 18.9112C14.2784 18.7306 14 18.3052 14 17.8333V15H3C2.44772 15 2 14.5523 2 14V10C2 9.44772 2.44772 9 3 9H14V6.1667C14 5.69483 14.2784 5.26942 14.7055 5.08884C15.1326 4.90826 15.6241 5.00808 15.951 5.34174L21.6653 11.175C22.1116 11.6307 22.1116 12.3693 21.6653 12.825L15.951 18.6583C15.6241 18.9919 15.1326 19.0917 14.7055 18.9112Z");

        private static Geometry Frozen(Geometry geometry)
        {
            geometry.Freeze();
            return geometry;
        }

        private static Geometry Frozen(string pathData) => Frozen(Geometry.Parse(pathData));

    }
}
