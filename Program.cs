using System;
using System.Collections.Generic;
using System.Threading;

class Tetris
{
    private static readonly int Width = 10; // Ширина поля
    private static readonly int Height = 20; // Высота поля
    private static char[,] Field = new char[Height, Width];
    private static List<char[,]> Shapes = new List<char[,]>();
    private static Random Rand = new Random();

    static Tetris()
    {
        Shapes.Add(new char[,] { { '█', '█' }, { '█', '█' } }); // Квадрат
        Shapes.Add(new char[,] { { ' ', '█', ' ' }, { '█', '█', '█' } }); // Z-фигура
        Shapes.Add(new char[,] { { '█', ' ', ' ' }, { '█', '█', '█' } }); // L-фигура
        Shapes.Add(new char[,] { { ' ', '█', ' ' }, { '█', '█', '█' } }); // S-фигура
        Shapes.Add(new char[,] { { '█', '█', '█', '█' } }); // Линия
    }

    static void Main(string[] args)
    {
        InitializeField();
        GameLoop();
    }

    private static void InitializeField()
    {
        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                Field[i, j] = ' ';
            }
        }
    }

    private static void GameLoop()
    {
        char[,] currentShape = GetRandomShape();
        int shapeX = Width / 2 - currentShape.GetLength(1) / 2;
        int shapeY = 0;

        while (true)
        {
            DrawFieldWithBorders(currentShape, shapeX, shapeY);

            ConsoleKeyInfo key = WaitForKeyOrTimeout(500);
            if (key.Key == ConsoleKey.LeftArrow)
            {
                if (CanMove(currentShape, shapeX - 1, shapeY))
                    shapeX--;
            }
            else if (key.Key == ConsoleKey.RightArrow)
            {
                if (CanMove(currentShape, shapeX + 1, shapeY))
                    shapeX++;
            }
            else if (key.Key == ConsoleKey.DownArrow)
            {
                if (CanMove(currentShape, shapeX, shapeY + 1))
                    shapeY++;
            }
            else if (key.Key == ConsoleKey.UpArrow)
            {
                RotateShape(ref currentShape);
            }

            if (!CanMove(currentShape, shapeX, shapeY + 1))
            {
                MergeShapeToField(currentShape, shapeX, shapeY);
                ClearCompletedLines();
                currentShape = GetRandomShape();
                shapeX = Width / 2 - currentShape.GetLength(1) / 2;
                shapeY = 0;

                if (!CanMove(currentShape, shapeX, shapeY))
                {
                    DrawFieldWithBorders(null, 0, 0); // Показать финальное состояние
                    Console.SetCursorPosition(Width * 2 + 4, Height + 2);
                    Console.WriteLine("Игра окончена!");
                    break;
                }
            }
            else
            {
                shapeY++;
            }
        }
    }

    private static bool CanMove(char[,] shape, int x, int y)
    {
        int shapeHeight = shape.GetLength(0);
        int shapeWidth = shape.GetLength(1);

        for (int row = 0; row < shapeHeight; row++)
        {
            for (int col = 0; col < shapeWidth; col++)
            {
                if (shape[row, col] != ' ')
                {
                    int newX = x + col;
                    int newY = y + row;

                    if (newX < 0 || newX >= Width || newY >= Height || (newY >= 0 && Field[newY, newX] != ' '))
                        return false;
                }
            }
        }
        return true;
    }

    private static void MergeShapeToField(char[,] shape, int x, int y)
    {
        int shapeHeight = shape.GetLength(0);
        int shapeWidth = shape.GetLength(1);

        for (int row = 0; row < shapeHeight; row++)
        {
            for (int col = 0; col < shapeWidth; col++)
            {
                if (shape[row, col] != ' ')
                {
                    Field[y + row, x + col] = shape[row, col];
                }
            }
        }
    }

    private static void ClearCompletedLines()
    {
        for (int row = Height - 1; row >= 0; row--)
        {
            bool isFull = true;
            for (int col = 0; col < Width; col++)
            {
                if (Field[row, col] == ' ')
                {
                    isFull = false;
                    break;
                }
            }

            if (isFull)
            {
                for (int r = row; r > 0; r--)
                {
                    for (int c = 0; c < Width; c++)
                    {
                        Field[r, c] = Field[r - 1, c];
                    }
                }

                for (int c = 0; c < Width; c++)
                {
                    Field[0, c] = ' ';
                }

                row++; // Проверяем ту же строку снова
            }
        }
    }

    private static void DrawFieldWithBorders(char[,] shape, int shapeX, int shapeY)
    {
        Console.Clear();

        char[,] fieldCopy = (char[,])Field.Clone();
        if (shape != null)
        {
            int shapeHeight = shape.GetLength(0);
            int shapeWidth = shape.GetLength(1);

            for (int row = 0; row < shapeHeight; row++)
            {
                for (int col = 0; col < shapeWidth; col++)
                {
                    if (shape[row, col] != ' ')
                    {
                        int newX = shapeX + col;
                        int newY = shapeY + row;

                        if (newX >= 0 && newX < Width && newY >= 0 && newY < Height)
                        {
                            fieldCopy[newY, newX] = shape[row, col];
                        }
                    }
                }
            }
        }

        // Рамка сверху
        Console.Write("┌");
        for (int i = 0; i < Width; i++) Console.Write("──");
        Console.WriteLine("┐");

        // Игровое поле
        for (int row = 0; row < Height; row++)
        {
            Console.Write("│"); // Левая граница
            for (int col = 0; col < Width; col++)
            {
                Console.Write(fieldCopy[row, col] == ' ' ? "  " : $" {fieldCopy[row, col]}");
            }
            Console.WriteLine("│"); // Правая граница
        }

        // Рамка снизу
        Console.Write("└");
        for (int i = 0; i < Width; i++) Console.Write("──");
        Console.WriteLine("┘");
    }

    private static char[,] GetRandomShape()
    {
        return (char[,])Shapes[Rand.Next(Shapes.Count)].Clone();
    }

    private static void RotateShape(ref char[,] shape)
    {
        int oldRows = shape.GetLength(0);
        int oldCols = shape.GetLength(1);

        char[,] newShape = new char[oldCols, oldRows];

        for (int row = 0; row < oldRows; row++)
        {
            for (int col = 0; col < oldCols; col++)
            {
                newShape[col, oldRows - 1 - row] = shape[row, col];
            }
        }

        shape = newShape;
    }

    private static ConsoleKeyInfo WaitForKeyOrTimeout(int timeoutMilliseconds)
    {
        DateTime start = DateTime.Now;
        while (DateTime.Now - start < TimeSpan.FromMilliseconds(timeoutMilliseconds))
        {
            if (Console.KeyAvailable)
            {
                return Console.ReadKey(intercept: true);
            }
            Thread.Sleep(10);
        }
        return new ConsoleKeyInfo((char)0, ConsoleKey.NoName, false, false, false);
    }
}