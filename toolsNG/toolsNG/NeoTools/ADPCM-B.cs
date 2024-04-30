using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoTools
{
    class ADPCM_B_encoder
    {
        //ADPCM-B tables
        static int[] ym_deltat_decode_tableB1 = { 1, 3, 5, 7, 9, 11, 13, 15, -1, -3, -5, -7, -9, -11, -13, -15 }; //16 elems
        static short[] ym_deltat_decode_tableB2 = { 57, 57, 57, 57, 77, 102, 128, 153, 57, 57, 57, 57, 77, 102, 128, 153 }; //16 elems
        static long[] stepsizeTable = { 57, 57, 57, 57, 77, 102, 128, 153, 57, 57, 57, 57, 77, 102, 128, 153 };

        private int acc, adpcmd;

        private short YM2610_ADPCM_B_Decode(byte code)
        {
            acc += ym_deltat_decode_tableB1[code] * adpcmd / 8;

            if (acc > 32767) acc = 32767;
            else if (acc < -32768) acc = -32768;

            adpcmd = (adpcmd * ym_deltat_decode_tableB2[code]) / 64;

            if (adpcmd > 24576) adpcmd = 24576;
            else if (adpcmd < 127) adpcmd = 127;

            return (short)acc;
        }


        private byte[] YM2610_ADPCM_B_Encode(short[] data)
        {
            int lpc, flag;
            long i, dn, xn, stepSize;
            byte adpcm;
            byte adpcmPack = 0;
            short src;
            byte[] encoded = new byte[data.Length / 2];
            int idx = 0;

            xn = 0;
            stepSize = 127;
            flag = 0;

            for (lpc = 0; lpc < data.Length; lpc++)
            {
                //src = (short)((buffer[lpc * 2]) | (buffer[lpc * 2 + 1] << 8)); //16 bit samples, + fixing byte order
                src = data[lpc];

                dn = src - xn;
                i = (Math.Abs(dn) << 16) / (stepSize << 14);
                if (i > 7) i = 7;
                adpcm = (byte)i;
                i = (adpcm * 2 + 1) * stepSize / 8;
                if (dn < 0)
                {
                    adpcm |= 0x8;
                    xn -= i;
                }
                else
                {
                    xn += i;
                }
                stepSize = (stepsizeTable[adpcm] * stepSize) / 64;
                if (stepSize < 127)
                    stepSize = 127;
                else if (stepSize > 24576)
                    stepSize = 24576;
                if (flag == 0)
                {
                    adpcmPack = (byte)(adpcm << 4);
                    flag = 1;
                }
                else
                {
                    adpcmPack |= adpcm;
                    encoded[idx++] = adpcmPack;
                    flag = 0;
                }
            }
            return encoded;
        }


    }
}
