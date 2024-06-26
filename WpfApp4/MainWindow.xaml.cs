using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    class Block()
    {
        public double x { get; set; }
        public double y { get; set; }
        int width = 190;
        int height = 50;
        public List<Block> blockList = new List<Block>();

        public void MakeBlock(int index, int canvasWidth)
        {
            for(int i=0; i<index; i++)
            {
                Block block = new()
                {
                    x = (width * index) + i*10,
                    y = 0,
                };
                if (block.x >= canvasWidth)
                {
                    block.x = (i % 4) * block.width + (i % 4) * 10;
                    block.y = (i / 4) * block.height;
                }
                blockList.Add(block);
            }
        }

        public void MakeBlockControls(Canvas canvas)
        {
            int i = 0;
            foreach(Block block in blockList)
            {
                Rectangle rect = new()
                {
                    Width = block.width,
                    Height = block.height,
                    Fill = new SolidColorBrush(Colors.Blue),
                };
                rect.DataContext = block;
                canvas.Children.Add(rect);

                Canvas.SetTop(rect, block.y + 10*(i/4));
                Canvas.SetLeft(rect, block.x);
                i++;
            }
        }
    }

    public partial class MainWindow : Window
    {
        List<Block> _blockList;
        DispatcherTimer _timer;
        Rectangle _rectangle;
        Rectangle _ball;
        int _y;
        int _random;
        double _ballX;
        double _ballY;

        public MainWindow()
        {
            InitializeComponent();

            Init();
        }

        void Init()
        {
            canvas.Children.Clear();

            int canvasWidth = 800;
            int canvasHeight = 450;
            canvas.Width = canvasWidth;
            canvas.Height = canvasHeight;

            Block block = new();
            block.MakeBlock(16, canvasWidth);
            block.MakeBlockControls(canvas);
            _blockList = block.blockList;

            Rectangle rectangle = new()
            {
                Width = 80,
                Height = 10,
                Fill = new SolidColorBrush(Colors.Blue),
            };
            _rectangle = rectangle;
            canvas.Children.Add(rectangle);

            Canvas.SetLeft(rectangle, (canvasWidth / 2) - (rectangle.Width / 2));
            Canvas.SetTop(rectangle, 380);

            Rectangle ball = new()
            {
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(Colors.White),
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 1,
            };
            _ball = ball;
            canvas.Children.Add(ball);

            _ballX = (canvasWidth / 2) - (ball.Width / 2);
            _ballY = 280;
            Canvas.SetLeft(ball, (canvasWidth / 2) - (ball.Width / 2));
            Canvas.SetTop(ball, _ballY);
            Random random = new Random();
            _random = random.Next(-5, 5);

            while(_random == 0)
                _random = random.Next(-5, 5);

            _y = 5;

            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;
            _timer.Interval = TimeSpan.FromMilliseconds(30);
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _ballX += _random;
            _ballY += _y;

            foreach (Block block in _blockList)
            {
                Rectangle rect = FindBlockRectangle(block);

                if (rect != null && IsCollision(rect))
                {
                    _y = -_y;
                    canvas.Children.Remove(rect);
                    _blockList.Remove(block);
                    break;
                }
            }

            if (_ballX <= 0 || _ballX >= canvas.Width - (_ball.Width+15))
                _random = -_random;
            if (_ballY <= 0)
                _y = -_y;
            if ((_ballY >= 370 && _ballY <= 380) && (_ballX >= Canvas.GetLeft(_rectangle) && _ballX <= Canvas.GetLeft(_rectangle) + 80))
                _y = -_y;
            if (_ballY >= 430)
            {
                _timer.Stop();
                if (MessageBox.Show("Retry?", "Game Over.", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    Init();
                else
                    Process.GetCurrentProcess().Kill();
            }

            Canvas.SetLeft(_ball, _ballX);
            Canvas.SetTop(_ball, _ballY);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Move(e.GetPosition(this));
        }

        void Move(Point point)
        {
            double pointX = point.X;
            Canvas.SetLeft(_rectangle, pointX - (_rectangle.Width / 2));
        }

        private Rectangle FindBlockRectangle(Block block)
        {
            foreach (UIElement element in canvas.Children)
            {
                if (element is Rectangle rect && rect.DataContext == block)
                {
                    return rect;
                }
            }
            return null;
        }

        private bool IsCollision(Rectangle rect)
        {
            double ballLeft = Canvas.GetLeft(_ball);
            double ballTop = Canvas.GetTop(_ball);
            double ballRight = ballLeft + _ball.Width;
            double ballBottom = ballTop + _ball.Height;

            double rectLeft = Canvas.GetLeft(rect);
            double rectTop = Canvas.GetTop(rect);
            double rectRight = rectLeft + rect.Width;
            double rectBottom = rectTop + rect.Height;

            return !(ballRight < rectLeft || ballLeft > rectRight || ballBottom < rectTop || ballTop > rectBottom);
        }
    }
}