using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Toub.Collections;
using Microsoft.Win32;
using System.Threading.Tasks;

namespace ahmazedotnet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        [Flags]
        public enum Direction
        {
            UP=1, DOWN=1<<1, LEFT=1<<2, RIGHT=1<<3,
            UP_LEFT=1<<4, UP_RIGHT=1<<5, DOWN_LEFT=1<<6, DOWN_RIGHT=1<<7,

            HORIZONTAL_AND_VERTICAL_ONLY = UP | DOWN | LEFT | RIGHT,
            DIAGONAL_ONLY = UP_LEFT | UP_RIGHT | DOWN_LEFT | DOWN_RIGHT,

            ALL_DIRECTIONS = HORIZONTAL_AND_VERTICAL_ONLY | DIAGONAL_ONLY,
        }


        public enum CellNeighborOption
        {
            
        }

        public interface ICell
        {
            int X {get;}
            int Y {get;}

            ICell GetAdjacentCell(Direction dir);
            List<ICell> GetAdjacentCells(Direction dir);

            bool IsWalkable {get;}
        }

        public struct CellInfo : ICell
        {
            public enum BlockType
            {
                EMPTY,
                WALL,
                START,
                FINISH,
                BACKTRACK,
                PLAYER1
            }
            static Dictionary<BlockType, Brush> brushes = new Dictionary<BlockType, Brush>();

            public const double WIDTH = 24.0;
            public const double HEIGHT = 18.0;
            public static Brush EMPTY_BRUSH = new SolidColorBrush(Colors.Black);
            public static Brush WALL_BRUSH = new LinearGradientBrush(Colors.Green, Colors.Blue, 40);

            public static Brush START_BRUSH = new SolidColorBrush(Colors.LawnGreen);
            public static Brush FINISH_BRUSH = new SolidColorBrush(Colors.Red);
            public static Brush BACKTRACK_BRUSH = new SolidColorBrush(Colors.Gray);
            
            public static Brush PLAYER1_BRUSH = new RadialGradientBrush(Colors.HotPink, Colors.Yellow);

            public int x, y;

            private Grid grid;
            private Path rect;

            // a workaround to enforce only one copy of Brush
            private class WorkAround { public Brush brush; }
            private WorkAround wa;

            static CellInfo()
            {
                brushes[BlockType.EMPTY] = EMPTY_BRUSH;
                brushes[BlockType.WALL] = WALL_BRUSH;
                brushes[BlockType.START] =START_BRUSH ;
                brushes[BlockType.FINISH] = FINISH_BRUSH;
                brushes[BlockType.BACKTRACK] = BACKTRACK_BRUSH;
                brushes[BlockType.PLAYER1] = PLAYER1_BRUSH;
            }

            public CellInfo(Grid parent, int x, int y, Path rect)
            {
                wa = new WorkAround();
                wa.brush = rect.Fill;
                this.grid = parent;
                this.x = x;
                this.y = y;
                this.rect = rect;
            }

            public static Brush FromBrushEnum(BlockType type)
            {
                return brushes[type];
            }

            public Brush BrushType 
            { 
                get
                {
                    return wa.brush;
                }
                set
                {
                    wa.brush = value;
                    rect.Fill = wa.brush;
                }
            }

            public void SetBrush(Brush b)
            {
                BrushType = b;
            }

            public int X { get { return x; } }
            public int Y { get { return y; } }

            public bool IsWalkable 
            { 
                get
                {
                    return (BrushType == CellInfo.EMPTY_BRUSH);
                }
            }

            public ICell GetAdjacentCell(Direction dir)
            {
                    ICell ci = null;

                    if (dir.HasFlag(Direction.UP) && grid.CellExists(x, y - 1))
                        ci = grid.GetCell(x, y - 1);
                    else if (dir.HasFlag(Direction.DOWN) && grid.CellExists(x, y + 1))
                        ci = grid.GetCell(x, y + 1);
                    else if (dir.HasFlag(Direction.LEFT) && grid.CellExists(x - 1, y))
                        ci = grid.GetCell(x - 1, y);
                    else if (dir.HasFlag(Direction.RIGHT) && grid.CellExists(x + 1, y))
                        ci = grid.GetCell(x + 1, y);

                    else if (dir.HasFlag(Direction.UP_LEFT) && grid.CellExists(x - 1, y - 1))
                        ci = grid.GetCell(x - 1, y - 1);
                    else if (dir.HasFlag(Direction.UP_RIGHT) && grid.CellExists(x + 1, y - 1))
                        ci = grid.GetCell(x + 1, y - 1);
                    else if (dir.HasFlag(Direction.DOWN_LEFT) && grid.CellExists(x - 1, y + 1))
                        ci = grid.GetCell(x - 1, y + 1);
                    else if (dir.HasFlag(Direction.DOWN_RIGHT) && grid.CellExists(x + 1, y + 1))
                        ci = grid.GetCell(x + 1, y + 1);
                    
                    return ci;
            }

            public List<ICell> GetAdjacentCells(Direction dir = Direction.ALL_DIRECTIONS)
            {
                List<ICell> cells = new List<ICell>(8);

                if (dir.HasFlag(Direction.UP) && grid.CellExists(x, y - 1))
                    cells.Add(grid.GetCell(x, y - 1));
                if (dir.HasFlag(Direction.DOWN) && grid.CellExists(x, y + 1))
                    cells.Add(grid.GetCell(x, y + 1));
                if (dir.HasFlag(Direction.LEFT) && grid.CellExists(x - 1, y))
                    cells.Add(grid.GetCell(x - 1, y));
                if (dir.HasFlag(Direction.RIGHT) && grid.CellExists(x + 1, y))
                    cells.Add(grid.GetCell(x + 1, y));

                if (dir.HasFlag(Direction.UP_LEFT) && grid.CellExists(x - 1, y - 1))
                    cells.Add(grid.GetCell(x - 1, y - 1));
                if (dir.HasFlag(Direction.UP_RIGHT) && grid.CellExists(x + 1, y - 1))
                    cells.Add(grid.GetCell(x + 1, y - 1));
                if (dir.HasFlag(Direction.DOWN_LEFT) && grid.CellExists(x - 1, y + 1))
                    cells.Add(grid.GetCell(x - 1, y + 1));
                if (dir.HasFlag(Direction.DOWN_RIGHT) && grid.CellExists(x + 1, y + 1))
                    cells.Add(grid.GetCell(x + 1, y + 1));

                return cells;
            }

            public override bool Equals(System.Object obj)
            {
                CellInfo? p = obj as CellInfo?;
                if (!p.HasValue)
                {
                    return false;
                }

                // Return true if the fields match:
                return this == p;
            }

            public bool Equals(CellInfo p)
            {
                // Return true if the fields match:
                return this == p;
            }

            public override int GetHashCode()
            {
                return x.GetHashCode() ^ y.GetHashCode() ^ rect.GetHashCode();
            }

            public static bool operator ==(CellInfo a, CellInfo b)
            {
                return (a.x == b.x) && (a.y == b.y) && (a.rect == b.rect);
            }

            public static bool operator !=(CellInfo a, CellInfo b)
            {
                return !(a==b);
            }
        }

        public class Grid : System.Collections.IEnumerable
        {
            public const int ROWS = 60;
            public const int COLS = 60;

            private CellInfo[,] info = new CellInfo[COLS, ROWS];

            public CellInfo startCell, finishCell;

            public CellInfo GetCell(int x, int y)
            {
                return info[x, y];
            }
            public void AddCell(CellInfo gi)
            {
                info[gi.X,gi.y] = gi;
            }
            public bool CellExists(int x, int y)
            {
                if (x >= 0 && y >= 0 && x < COLS && y < ROWS)
                    return true;
                else
                    return false;
            }
            public void Add(object o)
            {
                //Console.WriteLine("IT IS ADDED!!!!!!!!!!!!!!!");
                CellInfo ci = (CellInfo)o;
                info[ci.x, ci.y].BrushType = ci.BrushType;
                info[ci.x, ci.y].x = ci.x;
                info[ci.x, ci.y].y = ci.y;
            }
            /*public IEnumerator<CellInfo> GetEnumerator()
            {
                return info.GetEnumerator();
            }*/
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return info.GetEnumerator();
            }
        }

        class Player
        {
            CellInfo cell;
            Grid grid;
            Brush playerBrush;

            public Player(Grid grid, Brush playerBrush, CellInfo initialCell)
            {
                this.grid = grid;
                this.playerBrush = playerBrush;
                cell = initialCell;

                cell.BrushType = playerBrush;
            }
            public void SetPosition(int x, int y)
            {
                cell.BrushType = CellInfo.EMPTY_BRUSH;
                cell = grid.GetCell(x, y);
                cell.BrushType = playerBrush;
            }
            public bool Move(Direction dir)
            {
                ICell nextCell = cell.GetAdjacentCell(dir);
                if (nextCell != null 
                    && ((CellInfo)nextCell).BrushType != CellInfo.WALL_BRUSH)
                {
                    cell.BrushType = CellInfo.EMPTY_BRUSH;
                    cell = (CellInfo)nextCell;
                    cell.BrushType = playerBrush;
                    return true;
                }
                return false;
            }
            public bool MoveUp()
            {
                return Move(Direction.UP);
            }
            public bool MoveDown()
            {
                return Move(Direction.DOWN);
            }
            public bool MoveLeft()
            {
                return Move(Direction.LEFT);
            }
            public bool MoveRight()
            {
                return Move(Direction.RIGHT);
            }

            public bool IsMoveLegal(Direction dir)
            {
                ICell inextCell = cell.GetAdjacentCell(dir);
                if (inextCell != null)
                {
                    CellInfo nextCell = (CellInfo)inextCell;
                    if(nextCell != grid.startCell
                        && nextCell != grid.finishCell
                        && nextCell.BrushType != CellInfo.WALL_BRUSH)
                    {
                        return true;
                    }
                }
                return false;
            }
            public bool IsMoveUpLegal()
            {
                return IsMoveLegal(Direction.UP);
            }
            public bool IsMoveDownLegal()
            {
                return IsMoveLegal(Direction.DOWN);
            }
            public bool IsMoveLeftLegal()
            {
                return IsMoveLegal(Direction.LEFT);
            }
            public bool IsMoveRightLegal()
            {
                return IsMoveLegal(Direction.RIGHT);
            }
        }

        interface IMazeSolver
        {
            /// <summary>
            /// attempt to find the shortest path
            /// </summary>
            /// <returns>an array of cells, the path</returns>
            void Solve(); 

            /// <summary>
            /// get the shortest path
            /// </summary>
            List<CellInfo> Path
            {
                get;
            }

            /// <summary>
            /// get the back tracked path (cells that have been visited but not added to the solution path)
            /// </summary>
            List<CellInfo> BackTrack
            {
                get;
            }
        }

        class MazeSolverBFS : IMazeSolver
        {
            class GraphNode
            {
                public GraphNode parent = null;
                public CellInfo cell;
            }


            Queue<CellInfo> opened = new Queue<CellInfo>();
            HashSet<CellInfo> visited = new HashSet<CellInfo>();
            Dictionary<CellInfo, GraphNode> cellNodes = new Dictionary<CellInfo, GraphNode>();
            GraphNode solutionGraph = null;
            List<CellInfo> solutionPath = null; 

            Grid grid;

            public MazeSolverBFS(Grid grid)
            {
                this.grid = grid;
            }

            public void Solve()
            {
                solutionPath = null;
                solutionGraph = null;
                opened.Clear();
                visited.Clear();

                CellInfo current;

                opened.Enqueue(grid.startCell);
                visited.Add(grid.startCell);

                GraphNode graph = new GraphNode();
                graph.cell = grid.startCell;
                graph.parent = null;
                cellNodes[grid.startCell] = graph;

                while (opened.Count > 0)
                {
                    current = opened.Dequeue();
                    graph = cellNodes[current];

                    if (current == grid.finishCell)
                    {
                        solutionGraph = graph;
                        break;
                    }
                    List<ICell> adjacents = current.GetAdjacentCells();

                    foreach (CellInfo c in adjacents)
                    {
                        if (!visited.Contains(c) && c.BrushType != CellInfo.WALL_BRUSH)
                        {
                            GraphNode childNode = new GraphNode();
                            childNode.cell = c;
                            childNode.parent = graph;
                            cellNodes[c] = childNode;

                            opened.Enqueue(c);
                            visited.Add(c);
                        }
                    }
                }
            }
            public List<CellInfo> Path
            {
                get
                {
                    if (solutionPath != null)
                    {
                        return solutionPath;
                    }

                    solutionPath = new List<CellInfo>();

                    GraphNode graph = solutionGraph;
                    while(graph != null)
                    {
                        solutionPath.Add(graph.cell);
                        graph = graph.parent;
                    }
                    solutionPath.Reverse();

                    return solutionPath;
                }
            }
            public List<CellInfo> BackTrack
            {
                get
                {
                    return visited.ToList();
                }
            }
        }

        class MazeSolverAStar : IMazeSolver
        {
            class GraphNode
            {
                public GraphNode parent = null;
                public CellInfo cell;

                public int F, G, H;
            }

            PriorityQueue<CellInfo> opened = new PriorityQueue<CellInfo>();
            HashSet<CellInfo> visited = new HashSet<CellInfo>();
            Dictionary<CellInfo, GraphNode> cellNodes = new Dictionary<CellInfo, GraphNode>();
            GraphNode solutionGraph = null;
            List<CellInfo> solutionPath = null;

            //delegate int HeuristicFunction(CellInfo current, CellInfo finish);

            //HeuristicFunction heuristicFunction;

            Grid grid;

            public MazeSolverAStar(Grid grid)
            {
                this.grid = grid;
                //heuristicFunction = ManhattenDistance;
            }

            int EuclideanDistance(CellInfo current, CellInfo finish)
            {
                int x = finish.x - current.x;
                int y = finish.y - current.y;
                return (int)((Math.Sqrt(x * x) + Math.Sqrt(y * y))*10);
            }
            int ManhattenDistance(CellInfo current, CellInfo finish)
            {
                return Math.Abs(finish.x - current.x) + Math.Abs(finish.y - current.y);
            }
            int NoHeuristic(CellInfo current, CellInfo finish)
            {
                return 0;
            }

            public void Solve()
            {
                solutionPath = null;
                solutionGraph = null;
                opened.Clear();
                visited.Clear();
                cellNodes.Clear();

                CellInfo current;

                GraphNode currentNode = new GraphNode();
                currentNode.cell = grid.startCell;
                currentNode.parent = null;
                currentNode.G = 0;
                currentNode.H = EuclideanDistance(grid.startCell, grid.finishCell);
                currentNode.F = currentNode.H;
                cellNodes[grid.startCell] = currentNode;

                opened.Enqueue(-currentNode.F, grid.startCell);
                visited.Add(grid.startCell);

                while (opened.Count > 0)
                {
                    current = opened.Dequeue();
                    currentNode = cellNodes[current];

                    if (current == grid.finishCell)
                    {
                        solutionGraph = currentNode;
                        break;
                    }
                    List<ICell> adjacents = current.GetAdjacentCells();

                    foreach (CellInfo c in adjacents)
                    {
                        if (!visited.Contains(c) && c.BrushType != CellInfo.WALL_BRUSH)
                        {
                            GraphNode childNode = new GraphNode();
                            childNode.cell = c;
                            childNode.parent = currentNode;
                            childNode.G = currentNode.G+1;
                            childNode.H = EuclideanDistance(c, grid.finishCell);
                            childNode.F = childNode.G + childNode.H;
                            cellNodes[c] = childNode;

                            opened.Enqueue(-childNode.F, c);
                            visited.Add(c);
                        }
                    }
                }
            }

            public List<CellInfo> Path
            {
                get
                {
                    if (solutionPath != null)
                    {
                        return solutionPath;
                    }

                    solutionPath = new List<CellInfo>();

                    GraphNode graph = solutionGraph;
                    while (graph != null)
                    {
                        solutionPath.Add(graph.cell);
                        graph = graph.parent;
                    }
                    solutionPath.Reverse();

                    return solutionPath;
                }
            }
            public List<CellInfo> BackTrack
            {
                get
                {
                    return visited.ToList();
                }
            }
        }

        interface IMazeGenerator
        {
            void Generate();
            List<CellInfo> Path
            {
                get;
            }
        }

        class MazeGeneratorDepthFirst : IMazeGenerator
        {
            class GraphNode
            {
                public GraphNode parent = null;
                public CellInfo cell;
            }

            Stack<CellInfo> opened = new Stack<CellInfo>();
            HashSet<CellInfo> visited = new HashSet<CellInfo>();
            List<CellInfo> solutionPath = null;

            CellInfo startCell;

            public MazeGeneratorDepthFirst(CellInfo startCell)
            {
                this.startCell = startCell;
            }

            public void Generate()
            {
                solutionPath = new List<CellInfo>();

                opened.Clear();
                visited.Clear();

                CellInfo current;

                opened.Push(startCell);
                visited.Add(startCell);

                bool[,] wallInfo = new bool[Grid.COLS, Grid.ROWS];
                wallInfo[startCell.x, startCell.y] = true;
                for (int i = 0; i < wallInfo.GetLength(0);i++ )
                {
                    for (int j = 0; j < wallInfo.GetLength(1); j++)
                    {
                        wallInfo[i, j] = false;
                    }
                }
                while (opened.Count > 0)
                {
                    current = opened.Pop();
                    solutionPath.Add(current);

                    ICell[] adjacents = current.GetAdjacentCells().ToArray();
                    adjacents.Shuffle();

                    foreach (CellInfo c in adjacents)
                    {
                        List<ICell> walls = c.GetAdjacentCells();
                        bool skip = false;
                        foreach(CellInfo w in walls)
                        {
                            if (wallInfo[w.x, w.y] && w != current)
                            {
                                skip = true;
                                break;
                            }
                        }
                        if(skip)
                            continue;
                        if (!visited.Contains(c))
                        {
                            opened.Push(c);
                            visited.Add(c);

                            wallInfo[c.x, c.y] = true;
                        }
                    }
                }

            }

            public List<CellInfo> Path
            {
                get
                {
                    return solutionPath;
                }
            }
        }

        public struct SaveCellInfo
        {
            public int x, y;
            public CellInfo.BlockType type;

            public SaveCellInfo(int x, int y, CellInfo.BlockType type)
            {
                this.x = x;
                this.y = y;
                this.type = type;
            }
            public SaveCellInfo(CellInfo ci)
            {
                this.x = 0;
                this.y = 0;
                this.type = CellInfo.BlockType.EMPTY;

                this.x = ci.x;
                this.y = ci.y;

                if (ci.BrushType == CellInfo.EMPTY_BRUSH) type = CellInfo.BlockType.EMPTY;
                else if (ci.BrushType == CellInfo.WALL_BRUSH) type = CellInfo.BlockType.WALL;
                else if (ci.BrushType == CellInfo.START_BRUSH) type = CellInfo.BlockType.START;
                else if (ci.BrushType == CellInfo.FINISH_BRUSH) type = CellInfo.BlockType.FINISH;
                else if (ci.BrushType == CellInfo.BACKTRACK_BRUSH) type = CellInfo.BlockType.BACKTRACK;
                else if (ci.BrushType == CellInfo.PLAYER1_BRUSH) type = CellInfo.BlockType.PLAYER1;
                else ci.BrushType = CellInfo.EMPTY_BRUSH;
            }
        }

        public struct SaveInfo
        {
            public List<SaveCellInfo> grid;
            public SaveCellInfo start;
            public SaveCellInfo finish;
        }

        /*
         * Any live cell with fewer than two live neighbours dies, as if caused by under-population.
         * Any live cell with two or three live neighbours lives on to the next generation.
         * Any live cell with more than three live neighbours dies, as if by overcrowding.
         * Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.
         * 
         */
        // "B3/S23" = born if a dead cell has exactly 3 alive neighbors, stay alive if a live cell has 2 or 3 live neighbors, otherwise die
        // < 2, death
        // == 3, born
        // else die
        class GameOfLife
        {
            int[] B = new int[]{ 3 };
            int[] S = new int[]{ 2, 3 };

            public const bool ALIVE = true;
            public const bool DEAD = false;

            bool[,] stateMap;

            public GameOfLife(int width, int height)
            {
                stateMap = new bool[width, height];
            }

            public bool[,] Step(IEnumerable<ICell> cells)
            {
                Parallel.ForEach(cells, (ICell c) => {
                    // !cell.IsWalkable == true == alive, cell.IsWalkable == false == dead
                    bool curIsAlive = !c.IsWalkable; //everything except for the empty blocks are considered alive, true = alive, false = dead

                    int nAlive=0;

                    var neighbors = c.GetAdjacentCells(Direction.ALL_DIRECTIONS);
                    foreach (ICell adj in neighbors)
                    {
                        bool isAlive = !adj.IsWalkable;

                        if (isAlive)
                            nAlive++;
                    }

                    if(!curIsAlive)
                    {
                        // attempt rebirth
                        foreach (int b in B)
                        {
                            if (b == nAlive)
                            {
                                // found the magical pill (population), now its alive again
                                curIsAlive = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        // attempt suicide
                        curIsAlive = false;
                        foreach (int s in S)
                        {
                            if (s == nAlive)
                            {
                                // found the magical pill (population), now its alive again
                                curIsAlive = true;
                                break;
                            }
                        }
                    }

                    stateMap[c.X, c.Y] = curIsAlive;
                });

                return stateMap;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        Random rand = new Random();
        string originalTitle;

        Grid grid = new Grid();
        Player player1;
        IMazeSolver solver;
        IMazeGenerator generator;

        GameOfLife gol = new GameOfLife(Grid.COLS, Grid.ROWS);

        private void Window_Initialized(object sender, EventArgs e)
        {
            originalTitle = Title;
            Title = originalTitle + " A*";

            CreateGrid();

            solver = new MazeSolverAStar(grid);
        }

        private void CreateGrid()
        {

            canvas.Background = new SolidColorBrush(Colors.Black);
            int i=0, j=0;

            for (i = 0; i < Grid.COLS; i += 1)
            {
                for (j = 0; j < Grid.ROWS; j += 1)
                {
                    RectangleGeometry r = new RectangleGeometry(new Rect(i * CellInfo.WIDTH, j * CellInfo.HEIGHT, CellInfo.WIDTH, CellInfo.HEIGHT));
                    Path p = new Path();
                    p.Fill = CellInfo.EMPTY_BRUSH;
                    p.Stroke = new SolidColorBrush(Colors.Red);
                    p.StrokeThickness = 0.5;
                    p.Data = r;
                    canvas.Children.Add(p);
                    grid.AddCell(new CellInfo(grid, i, j, p));
                }
            }
            canvas.Width = i * CellInfo.WIDTH;
            canvas.Height = j * CellInfo.HEIGHT;

            grid.startCell = grid.GetCell(0, 0);
            grid.finishCell = grid.GetCell(Grid.COLS - 1, Grid.ROWS - 1);

            grid.startCell.BrushType = CellInfo.START_BRUSH;
            grid.finishCell.BrushType = CellInfo.FINISH_BRUSH;

            player1 = new Player(grid, CellInfo.PLAYER1_BRUSH, grid.GetCell(Grid.COLS/2, Grid.ROWS/2));
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            canvas.CaptureMouse();
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            canvas.ReleaseMouseCapture();

            if (e.ChangedButton == MouseButton.Middle)
            {
                foreach(CellInfo c in grid)
                {
                    if(c.BrushType == CellInfo.WALL_BRUSH)
                        c.SetBrush(CellInfo.EMPTY_BRUSH);
                }
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(canvas);

            Rect canvasBound = new Rect(0,0,canvas.Width-1,canvas.Height-1);

            if (canvasBound.Contains(p))
            {
                int xcell = (int)(p.X / CellInfo.WIDTH);
                int ycell = (int)(p.Y / CellInfo.HEIGHT);
                CellInfo info = grid.GetCell(xcell, ycell);

                if (info != grid.startCell 
                    && (info.BrushType == CellInfo.WALL_BRUSH 
                    || info.BrushType == CellInfo.EMPTY_BRUSH 
                    || info.BrushType == CellInfo.START_BRUSH
                    || info.BrushType == CellInfo.BACKTRACK_BRUSH))
                {
                    if (e.RightButton.HasFlag(MouseButtonState.Pressed))
                    {
                        // right mouse = clear

                        info.BrushType = CellInfo.EMPTY_BRUSH;
                    }
                    else if (e.LeftButton.HasFlag(MouseButtonState.Pressed))
                    {
                        // left mouse = add
                        info.BrushType = CellInfo.WALL_BRUSH;
                    }
                }
            }

        }

        private void canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //up = start block
            //down = finish block

            Point p = e.GetPosition(canvas);
            Rect canvasBound = new Rect(0,0,canvas.Width-1,canvas.Height-1);

            if (canvasBound.Contains(p))
            {
                int xcell = (int)(p.X / CellInfo.WIDTH);
                int ycell = (int)(p.Y / CellInfo.HEIGHT);
                CellInfo ci = grid.GetCell(xcell, ycell);

                if (ci.BrushType != CellInfo.PLAYER1_BRUSH)
                {
                    if (e.Delta < 0 && ci != grid.startCell)
                    {
                        grid.finishCell.BrushType = CellInfo.EMPTY_BRUSH;
                        grid.finishCell = ci;
                        grid.finishCell.BrushType = CellInfo.FINISH_BRUSH;
                    }
                    else if (e.Delta > 0 && ci != grid.finishCell)
                    {
                        player1.SetPosition(xcell, ycell);
                        grid.startCell.BrushType = CellInfo.EMPTY_BRUSH;
                        grid.startCell = ci;
                        grid.startCell.BrushType = CellInfo.START_BRUSH;
                    }
                }

            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    player1.MoveUp();
                    break;
                case Key.Down:
                    player1.MoveDown();
                    break;
                case Key.Left:
                    player1.MoveLeft();
                    break;
                case Key.Right:
                    player1.MoveRight();
                    break;
                case Key.D1:
                    solver = new MazeSolverAStar(grid);
                    Title = originalTitle + " A*";
                    break;
                case Key.D2:
                    solver = new MazeSolverBFS(grid);
                    Title = originalTitle + " BFS";
                    break;
                case Key.Space:
                    foreach (CellInfo c in grid)
                    {
                        if ((c.BrushType == CellInfo.START_BRUSH 
                            || c.BrushType == CellInfo.BACKTRACK_BRUSH) 
                            && c != grid.startCell)
                            c.SetBrush(CellInfo.EMPTY_BRUSH);
                    }
                    solver.Solve();
                    //Console.WriteLine("Solved path length: " + solver.Path.Count);
                    foreach (CellInfo c in solver.BackTrack)
                    {
                        if (c != grid.startCell && c != grid.finishCell)
                            c.SetBrush(CellInfo.BACKTRACK_BRUSH);
                    }
                    foreach (CellInfo c in solver.Path)
                    {
                        if(c != grid.startCell && c != grid.finishCell)
                            c.SetBrush(CellInfo.START_BRUSH);
                    }
                    break;

                case Key.Escape:
                    foreach (CellInfo c in grid)
                    {
                        if ((c.BrushType == CellInfo.START_BRUSH 
                            || c.BrushType == CellInfo.BACKTRACK_BRUSH) 
                            && c != grid.startCell)
                            c.SetBrush(CellInfo.EMPTY_BRUSH);
                    }
                    break;

                case Key.Enter:
                    foreach (CellInfo c in grid)
                    {
                        if (c != grid.startCell)
                            c.SetBrush(CellInfo.WALL_BRUSH);
                    }
                    generator = new MazeGeneratorDepthFirst(grid.startCell);
                    generator.Generate();
                    //Console.WriteLine("length " + generator.Path.Count);
                    PriorityQueue<CellInfo> cellsByDistance = new PriorityQueue<CellInfo>();
                    foreach (CellInfo c in generator.Path)
                    {
                        cellsByDistance.Enqueue(Math.Abs(grid.startCell.x - c.x) + Math.Abs(grid.startCell.y - c.y), c);
                        if (c != grid.startCell)
                        {
                            c.SetBrush(CellInfo.EMPTY_BRUSH);
                        }
                    }
                    if(cellsByDistance.Count > 0)
                    {
                        grid.finishCell = cellsByDistance.Dequeue();
                        grid.finishCell.BrushType = CellInfo.FINISH_BRUSH;
                    }
                    player1.SetPosition(grid.startCell.x, grid.startCell.y);
                    break;
                case Key.G: //run a step of game of life
                    
                    bool[,] stateMap = gol.Step(grid.Cast<ICell>());
                    for(int i = stateMap.GetLowerBound(0);i <= stateMap.GetUpperBound(0);i++)
                    {
                        for(int j = stateMap.GetLowerBound(1);j <= stateMap.GetUpperBound(1);j++)
                        {
                            if(stateMap[i,j])
                            {
                                grid.GetCell(i, j).SetBrush(CellInfo.WALL_BRUSH);
                            }
                            else
                            {
                                grid.GetCell(i, j).SetBrush(CellInfo.EMPTY_BRUSH);
                            }
                        }
                    }
                    break;
            }

            grid.startCell.BrushType = CellInfo.START_BRUSH;
            grid.finishCell.BrushType = CellInfo.FINISH_BRUSH;
        }

        private void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void ShowHelp()
        {
            MessageBox.Show("Control:\n" +
                            "Left Mouse Button = Create a Wall\n" +
                            "Right Mouse Button = Erase a Wall\n" +
                            "Middle Mouse Button = Erase all Walls\n" +
                            "\n" +
                            "Mouse Wheel Up = Set Starting Point\n" +
                            "Mouse Wheel Down = Set Finishing Point\n" +
                            "\n" +
                            "1 = Use A*\n" + 
                            "2 = Use BFS\n" +
                            "\n" +
                            "Enter = Create a random maze\n" +
                            "Spacebar = Show Solution\n" +
                            "Esc = Erase Solution\n" +
                            "Arrow keys = move the player\n"
                            );
        }

        Thread helpThread;
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            helpThread = new Thread(ShowHelp);
            helpThread.Start();
        }

        private void MenuItem_Click_Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog sfd = new OpenFileDialog();
            sfd.FileName = "newmap";
            sfd.DefaultExt = ".xml";
            sfd.Filter = "Map File (.xml)|*.xml";

            bool? suc = sfd.ShowDialog();

            if (suc.HasValue && suc.Value)
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(SaveInfo));
                using (System.IO.Stream file = sfd.OpenFile())
                {
                    SaveInfo si = (SaveInfo)serializer.Deserialize(file);
                    grid.startCell.BrushType = CellInfo.EMPTY_BRUSH;
                    grid.startCell = grid.GetCell(si.start.x, si.start.y);
                    grid.startCell.BrushType = CellInfo.FromBrushEnum(si.start.type);

                    grid.finishCell.BrushType = CellInfo.EMPTY_BRUSH;
                    grid.finishCell = grid.GetCell(si.finish.x, si.finish.y);
                    grid.finishCell.BrushType = CellInfo.FromBrushEnum(si.finish.type);
                    player1.SetPosition(si.start.x, si.start.y);
                    foreach(SaveCellInfo s in si.grid)
                    {
                        CellInfo ci = grid.GetCell(s.x, s.y);

                        if(s.type != CellInfo.BlockType.PLAYER1)
                        {
                            ci.BrushType = CellInfo.FromBrushEnum(s.type);
                        }
                        else
                        {
                            ci.BrushType = CellInfo.EMPTY_BRUSH;
                        }
                    }
                }
            }
        }

        private void MenuItem_Click_Save(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.OverwritePrompt = true;
            sfd.FileName = "newmap";
            sfd.DefaultExt = ".xml";
            sfd.Filter = "Map File (.xml)|*.xml"; 
            sfd.AddExtension = true;

            bool? suc = sfd.ShowDialog();

            if (suc.HasValue && suc.Value)
            {
                SaveInfo si = new SaveInfo();
                si.grid  = new List<SaveCellInfo>();
                foreach (CellInfo ci in grid)
                {
                    si.grid .Add(new SaveCellInfo(ci));
                }
                si.start = new SaveCellInfo(grid.startCell);
                si.finish = new SaveCellInfo(grid.finishCell);

                var serializer = new System.Xml.Serialization.XmlSerializer(si.GetType());
                using (System.IO.Stream file = sfd.OpenFile())
                {
                    serializer.Serialize(file, si);
                }
            }

        }
    }
}
