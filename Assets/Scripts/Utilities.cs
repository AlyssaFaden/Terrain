using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    //public float[,] RotateArray(float[,] matrix)
    //{
    //    int rows = matrix.GetLength(0);
    //    int cols = matrix.GetLength(1);
    //    float temp = 0f;
    //    int row = rows - 1;
    //    int col = cols - 1;

    //    int row_size = (rows / 2);
    //    int col_size = cols;

    //    if (rows % 2 == 1)
    //    {
    //        //When in case number of rows are Odd size
    //        row_size = (rows / 2) + 1;
    //    }
    //    for (int i = 0; i < row_size; ++i)
    //    {
    //        if (rows / 2 == i)
    //        {
    //            //When in case number of rows are Odd size
    //            //In This case reverse middle row element
    //            col_size = cols / 2;
    //        }
    //        else
    //        {
    //            col_size = cols;
    //        }
    //        for (int j = 0; j < col_size; ++j)
    //        {
    //            //Swap element
    //            temp = matrix[i, j];
    //            matrix[i, j] = matrix[row - i, col - j];
    //            matrix[row - i, col - j] = temp;
    //        }
    //    }

    //    return matrix;
    //}

    public static float[,] RotateMatrix180(float[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        float[,] result = new float[rows, cols];
        int row = rows - 1;
        int col = cols - 1;

        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < cols; ++j)
            {
                result[i, j] = matrix[row - i, col - j];
            }
        }

        return result;
    }
}
