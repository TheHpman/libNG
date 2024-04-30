using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoTools
{
    public class ADPCM_A_encoder
    {
        static short[] step_size = {   16, 17, 19, 21, 23, 25, 28, 31, 34, 37,
                                       41, 45, 50, 55, 60, 66, 73, 80, 88, 97,
                                       107, 118, 130, 143, 157, 173, 190, 209, 230, 253,
                                       279, 307, 337, 371, 408, 449, 494, 544, 598, 658,
                                       724, 796, 876, 963, 1060, 1166, 1282, 1411, 1552
                                   }; //49elems
        static int[] step_adj = { -1, -1, -1, -1, 2, 5, 7, 9, -1, -1, -1, -1, 2, 5, 7, 9 };
        private int[] jedi_table;

        //decode
        int acc, decstep;
        //encode
        int index, diff, step;
        //encoder
        int prevsample, predsample, previndex;

        private short YM2610_ADPCM_A_Decode(byte code)
        {
            acc += jedi_table[decstep + code];
            if ((acc & ~0x7ff) != 0) // > 2047
                acc |= ~0xfff;
            else acc &= 0xfff;
            decstep += step_adj[code & 7] * 16;
            if (decstep < 0) decstep = 0;
            if (decstep > 48 * 16) decstep = 48 * 16;
            return (short)acc;
        }

        private byte YM2610_ADPCM_A_Encode(short sample, short clampValue = 0)
        {
            int tempstep;
            byte code;

            if (clampValue > 0)
            {
                if (sample > clampValue) sample = clampValue;
                if (sample < -clampValue) sample = (short)-clampValue;
            }

            predsample = prevsample;
            index = previndex;
            step = step_size[index];

            diff = sample - predsample;
            if (diff >= 0)
                code = 0;
            else
            {
                code = 8;
                diff = -diff;
            }

            tempstep = step;
            if (diff >= tempstep)
            {
                code |= 4;
                diff -= tempstep;
            }
            tempstep >>= 1;
            if (diff >= tempstep)
            {
                code |= 2;
                diff -= tempstep;
            }
            tempstep >>= 1;
            if (diff >= tempstep) code |= 1;

            predsample = YM2610_ADPCM_A_Decode(code);

            index += step_adj[code];
            if (index < 0) index = 0;
            if (index > 48) index = 48;

            prevsample = predsample;
            previndex = index;

            return code;
        }

        public ADPCM_A_encoder()
        {
            int step, nib;

            jedi_table = new int[16 * 49];
            for (step = 0; step < 49; step++)
            {
                /* loop over all nibbles and compute the difference */
                for (nib = 0; nib < 16; nib++)
                {
                    int value = (2 * (nib & 0x07) + 1) * step_size[step] / 8;
                    jedi_table[step * 16 + nib] = ((nib & 0x08) != 0) ? -value : value;
                }
            }
        }

        public byte[] encode(short[] data, short clampValue = 0)
        {
            int i;
            byte[] encoded;
            acc = 0;
            decstep = 0;

            prevsample = 0;
            predsample = 0;
            previndex = 0;

            encoded = new byte[data.Length % 512 == 0 ? (data.Length / 2) : ((data.Length & 0xfffffe00) / 2 + 0x100)];

            //downsampling 16b->12b + encoding
            for (i = 0; i < data.Length; i += 2)
            {
                encoded[i / 2] = (byte)((YM2610_ADPCM_A_Encode((short)(data[i] >> 4), clampValue) << 4) | YM2610_ADPCM_A_Encode(i + 1 < data.Length ? (short)(data[i + 1] >> 4) : (short)0, clampValue));
            }
            //padding
            while (i % 512 != 0)
            {
                encoded[i / 2] = (byte)((YM2610_ADPCM_A_Encode(0) << 4) | YM2610_ADPCM_A_Encode(0));
                i += 2;
            }

            return encoded;
        }


    }
}
