// https://gist.github.com/Flafla2/1a0b9ebef678bbce3215
// http://flafla2.github.io/2014/08/09/perlinnoise.html
// https://mrl.nyu.edu/~perlin/noise/

namespace MorphxLibs {
    public static class PerlinNoise {
        // Hash lookup table As defined by Ken Perlin.
        // This is a randomly arranged array of all numbers from 0-255 inclusive
        private static readonly int[] permutation = {151, 160, 137, 91, 90, 15,
                                                        131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23,
                                                        190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33,
                                                        88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166,
                                                        77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244,
                                                        102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196,
                                                        135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123,
                                                        5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42,
                                                        223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
                                                        129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228,
                                                        251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107,
                                                        49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254,
                                                        138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180
                                                };

        private static readonly int[] p = new int[512];

        public static void Init() {
            for(int x = 0; x < p.Length; x++)
                p[x] = permutation[x % 256];
        }

        public static double Perlin(double x, double y, double z) {
            int xi = (int)x & 255;
            int yi = (int)y & 255;
            int zi = (int)z & 255;
            double xf = x - (int)x;
            double yf = y - (int)y;
            double zf = z - (int)z;
            double u = Fade(xf);
            double v = Fade(yf);
            double w = Fade(zf);

            int a = p[xi] + yi;
            int aa = p[a] + zi;
            int ab = p[a + 1] + zi;
            int b = p[xi + 1] + yi;
            int ba = p[b] + zi;
            int bb = p[b + 1] + zi;

            double x1;
            double x2;
            double y1;
            double y2;

            x1 = Lerp(Grad(p[aa], xf, yf, zf), Grad(p[ba], xf - 1, yf, zf), u);
            x2 = Lerp(Grad(p[ab], xf, yf - 1, zf), Grad(p[bb], xf - 1, yf - 1, zf), u);
            y1 = Lerp(x1, x2, v);
            x1 = Lerp(Grad(p[aa + 1], xf, yf, zf - 1), Grad(p[ba + 1], xf - 1, yf, zf - 1), u);
            x2 = Lerp(Grad(p[ab + 1], xf, yf - 1, zf - 1), Grad(p[bb + 1], xf - 1, yf - 1, zf - 1), u);
            y2 = Lerp(x1, x2, v);

            return (Lerp(y1, y2, w) + 1) / 2;
        }

        private static double Grad(int hash, double x, double y, double z) {
            int h = hash & 15;
            double u = h < 8 ? x : y;
            double v;

            if(h < 4)
                v = y;
            else if(h == 12 || h == 14)
                v = x;
            else
                v = z;

            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        private static double Fade(double t) {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        private static double Lerp(double a, double b, double x) {
            return a + x * (b - a);
        }

        public static double OctavePerlin(double x, double y, double z, int octaves, double persistence) {
            double total = 0;
            double frequency = 1;
            double amplitude = 1;
            double maxValue = 0;

            for(int i = 0; i < octaves; i++) {
                total += Perlin(x * frequency, y * frequency, z * frequency) * amplitude;
                amplitude *= persistence;
                frequency *= 2;
                maxValue += amplitude;
            }
            return total / maxValue;
        }
    }
}