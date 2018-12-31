using UnityEngine;

namespace SkyImmerseEngine.Utils
{
    public class ByteColorConverter
    {
        private enum AnonymousEnum8
        {
            HsiSiValues = 7,
            HsiHSteps = 19
        }

        public static Color from8bit(int color) {
            if(color >= 216 || color <= 0)
                return new Color(0, 0, 0);

            int r = (int)(color / 36) % 6 * 51;
            int g = (int)(color / 6) % 6 * 51;
            int b = color % 6 * 51;
            return new Color(r, g, b);
        }
        
        public static Color GetColor(int color)
        {
            if (color >= (float) AnonymousEnum8.HsiHSteps * (float) AnonymousEnum8.HsiSiValues)
                color = 0;

            float loc1 = 0;
            float loc2 = 0;
            float loc3 = 0;
            if (color % (float) AnonymousEnum8.HsiHSteps != 0)
            {
                loc1 = color % (float) AnonymousEnum8.HsiHSteps * 1.0f / 18.0f;
                loc2 = 1;
                loc3 = 1;

                switch ((int) (color / (float) AnonymousEnum8.HsiHSteps))
                {
                    case 0:
                        loc2 = 0.25f;
                        loc3 = 1.00f;
                        break;
                    case 1:
                        loc2 = 0.25f;
                        loc3 = 0.75f;
                        break;
                    case 2:
                        loc2 = 0.50f;
                        loc3 = 0.75f;
                        break;
                    case 3:
                        loc2 = 0.667f;
                        loc3 = 0.75f;
                        break;
                    case 4:
                        loc2 = 1.00f;
                        loc3 = 1.00f;
                        break;
                    case 5:
                        loc2 = 1.00f;
                        loc3 = 0.75f;
                        break;
                    case 6:
                        loc2 = 1.00f;
                        loc3 = 0.50f;
                        break;
                }
            }
            else
            {
                loc1 = 0;
                loc2 = 0;
                loc3 = 1 - (float) color / (float) AnonymousEnum8.HsiHSteps / (float) AnonymousEnum8.HsiSiValues;
            }

            if (loc3 == 0)
                return new Color(0, 0, 0);

            if (loc2 == 0)
            {
                float loc7 = loc3 * 255;
                return new Color(loc7, loc7, loc7);
            }

            float red = 0;
            float green = 0;
            float blue = 0;

            if (loc1 < 1.0 / 6.0)
            {
                red = loc3;
                blue = loc3 * (1 - loc2);
                green = blue + (loc3 - blue) * 6 * loc1;
            }
            else if (loc1 < 2.0 / 6.0)
            {
                green = loc3;
                blue = loc3 * (1 - loc2);
                red = green - (loc3 - blue) * (6 * loc1 - 1);
            }
            else if (loc1 < 3.0 / 6.0)
            {
                green = loc3;
                red = loc3 * (1 - loc2);
                blue = red + (loc3 - red) * (6 * loc1 - 2);
            }
            else if (loc1 < 4.0 / 6.0)
            {
                blue = loc3;
                red = loc3 * (1 - loc2);
                green = blue - (loc3 - red) * (6 * loc1 - 3);
            }
            else if (loc1 < 5.0 / 6.0)
            {
                blue = loc3;
                green = loc3 * (1 - loc2);
                red = green + (loc3 - green) * (6 * loc1 - 4);
            }
            else
            {
                red = loc3;
                green = loc3 * (1 - loc2);
                blue = red - (loc3 - green) * (6 * loc1 - 5);
            }
            return new Color((int) (red * 255), (int) (green * 255), (int) (blue * 255));
        }
    }
}