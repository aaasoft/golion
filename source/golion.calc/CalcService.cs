using System;
using System.Collections.Generic;
using System.Text;

namespace golion.calc
{
    public interface CalcService
    {
        /// <summary>
        /// 加法
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        int Add(int x, int y);
        /// <summary>
        /// 乘法
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        long Mul(int x, int y);
    }
}
