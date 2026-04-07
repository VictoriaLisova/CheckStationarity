using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Models
{
    public class MatrixOperations
    {
        public double[,] MatrixTranspose(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            var transpocedMatrix = new double[cols, rows];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    transpocedMatrix[j, i] = matrix[i, j];
                }
            }
            return transpocedMatrix;
        }
        public double[,] MultipleMatrix(double[,] matrix1, double[,] matrix2)
        {
            var rows1 = matrix1.GetLength(0);
            var cols1 = matrix1.GetLength(1);
            var rows2 = matrix2.GetLength(0);
            var cols2 = matrix2.GetLength(1);
            var multMatrix = new double[rows1, cols2];
            if (cols1 == rows2)
            {
                for (int i = 0; i < rows1; i++)
                {
                    for (int j = 0; j < cols2; j++)
                    {
                        double sum = 0;
                        for (int k = 0; k < cols1; k++)
                        {
                            sum += matrix1[i, k] * matrix2[k, j];
                        }
                        multMatrix[i, j] = sum;
                    }
                }
            }
            return multMatrix;
        }

        public double DeterminantLU(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            double[,] A = (double[,])matrix.Clone();
            int swapCount = 0;

            for (int k = 0; k < n; k++)
            {
                int maxRow = k;
                for (int i = k + 1; i < n; i++)
                {
                    if (Math.Abs(A[i, k]) > Math.Abs(A[maxRow, k]))
                        maxRow = i;
                }
                if (maxRow != k)
                {
                    for (int j = 0; j < n; j++)
                    {
                        double temp = A[k, j];
                        A[k, j] = A[maxRow, j];
                        A[maxRow, j] = temp;
                    }

                    swapCount++;
                }

                if (Math.Abs(A[k, k]) < 1e-12)
                    return 0;

                for (int i = k + 1; i < n; i++)
                {
                    double factor = A[i, k] / A[k, k];

                    A[i, k] = factor;

                    for (int j = k + 1; j < n; j++)
                    {
                        A[i, j] -= factor * A[k, j];
                    }
                }
            }

            double det = 1;

            for (int i = 0; i < n; i++)
            {
                det *= A[i, i];
            }
            if (swapCount % 2 != 0)
                det = -det;

            return det;
        }

        public double[,] GetInverseMatrix(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] tempMatrix = new double[rows, cols * 2];
            
            if (rows != cols || DeterminantLU(matrix) == 0)
                throw new Exception("Incorrect matrix");

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    tempMatrix[i, j] = matrix[i, j];
                    tempMatrix[i, j + cols] = i == j ? 1 : 0;
                }
            }

            for (int i = 0; i < tempMatrix.GetLength(0); i++)
            {
                double pivot = tempMatrix[i, i];
                for (int j = 0; j < tempMatrix.GetLength(1); j++)
                {
                    tempMatrix[i, j] /= pivot;
                }

                for (int k = i + 1; k < tempMatrix.GetLength(0); k++)
                {
                    double factor = tempMatrix[k, i];
                    for (int l = 0; l < tempMatrix.GetLength(1); l++)
                    {
                        tempMatrix[k, l] -= factor * tempMatrix[i, l];
                    }
                }
            }

            for (int i = tempMatrix.GetLength(0) - 1; i >= 0; i--)
            {
                for (int k = i - 1; k >= 0; k--)
                {
                    double factor = tempMatrix[k, i];
                    for (int j = tempMatrix.GetLength(1) - 1; j >= 0; j--)
                    {
                        tempMatrix[k, j] -= factor * tempMatrix[i, j];
                    }
                }
            }

            double[,] extendedMatrix = new double[rows, cols];
            for (int i = 0; i < tempMatrix.GetLength(0); i++)
            {
                for (int j = cols; j < tempMatrix.GetLength(1); j++)
                {
                    extendedMatrix[i, j - cols] = tempMatrix[i, j];
                }
            }

            return extendedMatrix;
        }

        public double[,] AddMatrix(double[,] matrix1, double[,] matrix2)
        {
            double[,] result = new double[matrix1.GetLength(0), matrix2.GetLength(1)];
            for(int i = 0; i < matrix1.GetLength(0); i++)
            {
                for(int j = 0; j < matrix1.GetLength(1); j++)
                {
                    result[i, j] = matrix1[i, j] + matrix2[i, j];
                }
            }
            return result;
        }

        public double[,] MultByNumber(double[,] matrix, double number)
        {
            double[,] updatedMatrix = new double[matrix.GetLength(0), matrix.GetLength(1)];
            for(int i = 0; i < updatedMatrix.GetLength(0); i++)
            {
                for(int j = 0;j < updatedMatrix.GetLength(1); j++)
                {
                    updatedMatrix[i, j] = number * matrix[i, j];
                }
            }
            return updatedMatrix;
        }
    }
}
