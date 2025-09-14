// using 절은 그대로 유지
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace halligalli_client
{
    // 데이터 구조 정의
    public class CardInfoPacket
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool turn { get; set; }

        [JsonPropertyName("card")]
        public List<cardInfo> cardsInfo { get; set; }

        [JsonPropertyName("user_status")]
        public int userStatus { get; set; }

        [JsonPropertyName("remaining_card_count")]
        public List<playersInfo> otherPlayers { get; set; }
    }

    public class cardInfo
    {
        public int ID { get; set; }

        [JsonPropertyName("num")]
        public int Number { get; set; }
    }

    public class playersInfo
    {
        public int ID { get; set; }

        [JsonPropertyName("card_count")]
        public int Count { get; set; }
    }

    public class ClientSendPacket
    {
        public int id { get; set; }
        public string name { get; set; }
        public int key { get; set; }
        public long? time_dif { get; set; }
        public bool penalty { get; set; }
    }

    public partial class MainWindow : Window
    {
        private Dictionary<int, string> idToName = new();
        private Dictionary<string, (Image image, TextBlock text)> playerControls = new();
        private const int MaxPlayers = 4;

        private NetworkStream stream;
        private StreamWriter writer;
        private StreamReader reader;
        private TcpClient client;

        private CardInfoPacket previousData = null;
        private int myId = -1;
        private string myName = "sanghoon";
        private bool isInitialized = false;
        private bool spaceUsed = false;

        private bool isDefeated = false; // 최종 패배 여부

        private DateTime lastCardReceivedTime;

        public MainWindow()
        {
            var nameWindow = new NameInputWindow();
            bool? result = nameWindow.ShowDialog();

            if (result != true || string.IsNullOrWhiteSpace(nameWindow.UserName))
            {
                Application.Current.Shutdown();
                return;
            }

            myName = nameWindow.UserName;

            client = new TcpClient();
            client.Connect("127.0.0.1", 1234);
            stream = client.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream) { AutoFlush = true };

            Thread receivedTh = new Thread(readMsg)
            {
                IsBackground = true
            };
            receivedTh.Start();

            SendInitialName();
            InitializeComponent();
        }

        private void SendInitialName()
        {
            var obj = new ClientSendPacket
            {
                id = -1,
                name = myName,
                key = -1,
                time_dif = null,
                penalty = false
            };

            string json = JsonSerializer.Serialize(obj);
            writer.WriteLine(json);
        }

        private void turn_Button_Click(object sender, RoutedEventArgs e)
        {
            if (isDefeated || !Turn_Alram_Button.IsEnabled) return;
            Turn_Alram_Button.Opacity = 0.8;
            Turn_Alram_Button.IsEnabled = false;
            SendClientAction(1);
        }


        private void GameStart_Click(object sender, RoutedEventArgs e)
        {
            if (isDefeated) return;

            SendClientAction(3);
            GameStart_Button.Visibility = Visibility.Collapsed;
        }


        private void SendClientAction(int key, long? timeDiff = null, bool penalty = false)
        {
            var packet = new ClientSendPacket
            {
                id = myId,
                name = myName,
                key = key,
                time_dif = timeDiff,
                penalty = penalty
            };

            string json = JsonSerializer.Serialize(packet);
            writer.WriteLine(json);
        }

        public void chooseBehavior(CardInfoPacket data)
        {
            if (!idToName.ContainsKey(data.id))
                idToName[data.id] = data.name;

            if (data.userStatus != 0)
                HandleUserState(data.userStatus, data.name, data.id);

            if (data.turn && data.id == myId)
                turn_Button_Activate();

            if (data.cardsInfo != null && data.cardsInfo.Count > 0)
                GameStart_Button.Visibility = Visibility.Collapsed; // 게임 시작 시 버튼 숨김

            if (data.cardsInfo != null)
            {
                foreach (var card in data.cardsInfo)
                {
                    if (idToName.TryGetValue(card.ID, out var name))
                        updateCardImage(name, card.Number);
                }
            }

            if (data.otherPlayers != null)
            {
                foreach (var player in data.otherPlayers)
                {
                    if (idToName.TryGetValue(player.ID, out var name))
                        updateLeftCard(name, player.Count);
                }
            }

            spaceUsed = false; // 새 카드 입력되면 스페이스 다시 허용
        }

        public void readMsg()
        {
            string str;
            while ((str = reader.ReadLine()) != null)
            {
                if (str.StartsWith("/stop")) break;

                try
                {
                    var data = JsonSerializer.Deserialize<CardInfoPacket>(str);
                    Debug.WriteLine(data);
                    if (data == null) continue;
                    Dispatcher.Invoke(() =>
                    {
                        if (!isInitialized && data.id >= 0 && data.name == myName)
                        {
                            myId = data.id;
                            isInitialized = true;
                        }
                        
                        chooseBehavior(data);

                        if (previousData == null)
                            previousData = new CardInfoPacket();

                        previousData.id = data.id;
                        previousData.name = data.name;
                        previousData.turn = data.turn;
                        previousData.userStatus = data.userStatus;

                        if (data.cardsInfo != null && data.cardsInfo.Count > 0)
                            previousData.cardsInfo = new List<cardInfo>(data.cardsInfo);

                        if (data.otherPlayers != null && data.otherPlayers.Count > 0)
                            previousData.otherPlayers = new List<playersInfo>(data.otherPlayers);
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("파싱 실패: " + ex.Message);
                }
            }

            client.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (isDefeated) return; // 입력 자체 무시
            e.Handled = true;

            if (e.Key == Key.Space)
            {
                if (spaceUsed) return;

                bellEffectAsync();
                spaceUsed = true;

                if (previousData?.cardsInfo != null && previousData.cardsInfo.Count > 0)
                {
                    bool success = IsValidBell(previousData.cardsInfo);
                    if (success)
                    {
                        long diff = (long)(DateTime.Now - lastCardReceivedTime).TotalMilliseconds;
                        SendClientAction(2, diff, false);
                    }
                    else
                    {
                        SendClientAction(2, null, true);
                    }
                }
                else
                {
                    SendClientAction(2, null, true);
                }
            }
            else if (e.Key == Key.LeftCtrl)
            {
                if (!Turn_Alram_Button.IsEnabled) return;
                turn_Button_Click(Turn_Alram_Button, new RoutedEventArgs());
            }
        }


        private async void bellEffectAsync()
        {
            BellImage.Opacity = 0.6;
        }

        private async void bellOn()
        {
            BellImage.Opacity = 1.0;
        }

        private void turn_Button_Activate()
        {
            Turn_Alram_Button.Opacity = 1.0;
            Turn_Alram_Button.IsEnabled = true;
        }

        private bool IsValidBell(List<cardInfo> cardsInfo)
        {
            int[] counts = new int[4]; // 카드 종류 4개

            foreach (var card in cardsInfo)
            {
                if (card == null)
                {
                    Debug.WriteLine("널 카드 발견됨");
                    continue;
                }

                if (card.Number < 1 || card.Number > 20)
                {
                    Debug.WriteLine($"범위 밖: {card.Number}");
                    continue; // 잘못된 카드 번호는 무시
                }

                int type = (card.Number - 1) / 5; // 0~3
                int count = (card.Number - 1) % 5 + 1; // 눈 개수 (1 ~ 5)
                counts[type] += count;
            }

            foreach (var c in counts)
            {
                if (c == 5) return true;
            }

            return false;
        }

        private void updateCardImage(string userId, int cardIndex)
        {
            bellOn();
            var imageControl = FindName($"Player_{userId}_Image") as Image;
            if (imageControl != null)
            {
                string imagePath = $"Assets/CardImage/{cardIndex}.png";
                imageControl.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
            }
            lastCardReceivedTime = DateTime.Now;  // 새 카드 받으면 시간 기록
        }

        private void updateLeftCard(string userId, int cardIndex)
        {
            var idTextControl = FindName($"Player_{userId}_Text") as TextBlock;
            if (idTextControl != null)
                idTextControl.Text = "카드 수 : " + cardIndex.ToString();
        }

        private void HandleUserState(int state, string userId, int playerIndex)
        {
            switch (state)
            {
                case 1:  // 라운드 승리
                    ClearAllCardUIs();
                    ShowResult("라운드 승리!");
                    break;
                case 2:  // 라운드 패배
                    ClearAllCardUIs();
                    ShowResult("라운드 패배!");
                    break;
                case 3:
                    ShowResult("!! Penalty !!");
                    break;
                case 4:
                    AddPlayerSlot(userId, playerIndex);
                    break;
                case 5:
                    RemovePlayerSlot(userId);
                    break;
                case 6:  // 이름 중복
                    MessageBox.Show("이름이 중복되었습니다.", "입장 실패", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
                case 8:  // 최종 승리
                    ShowResult($"{userId} Win");
                    break;
                case 9:  // 최종 패배
                    if (playerIndex == myId)
                    {
                        isDefeated = true;
                        Turn_Alram_Button.IsEnabled = false;
                        GameStart_Button.IsEnabled = false;
                        ShowResult("You Lose");
                    }

                    RemovePlayerSlot(userId); // 남이든 나든 UI에서 지움
                    break;

            }
        }

        private void ClearAllCardUIs()
        {
            Dispatcher.Invoke(() =>
            {
                Debug.WriteLine($"[ClearAllCardUIs 호출] 플레이어 수: {playerControls.Count}");
                foreach (var (image, text) in playerControls.Values)
                {
                    Debug.WriteLine($"[초기화 대상] {image.Name}");
                    image.Source = new BitmapImage(new Uri("Assets/CardImage/null.png", UriKind.Relative));
                }
            });
        }



        public void AddPlayerSlot(string userId, int playerIndex)
        {
            if (playerControls.ContainsKey(userId)) return;

            var cardImage = new Image
            {
                Name = $"Player_{userId}_Image",
                Width = (double)FindResource("CardSize_W"),
                Height = (double)FindResource("CardSize_H"),
                Stretch = Stretch.Fill
            };

            var border = new Border
            {
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2),
                Width = cardImage.Width,
                Height = cardImage.Height,
                Child = cardImage
            };

            var userIdText = new TextBlock
            {
                Text = userId,
                Foreground = Brushes.Yellow,
                FontSize = (double)FindResource("FontSizeSmall"),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var cardText = new TextBlock
            {
                Name = $"Player_{userId}_Text",
                Text = $"카드 수 : ",
                Foreground = Brushes.White,
                FontSize = (double)FindResource("FontSizeSmall"),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            RegisterName(cardImage.Name, cardImage);
            RegisterName(cardText.Name, cardText);
            playerControls[userId] = (cardImage, cardText);

            StackPanel? panel = playerIndex switch
            {
                0 => Player1StackPanel,
                1 => Player2StackPanel,
                3 => Player3StackPanel,
                2 => Player4StackPanel,
                _ => null
            };

            if (panel != null)
            {
                panel.Children.Clear();
                panel.Children.Add(userIdText);
                panel.Children.Add(cardText);
            }

            var (row, col) = GetPlayerPosition(playerIndex);
            Grid.SetRow(border, row);
            Grid.SetColumn(border, col);
            GameUIGrid.Children.Add(border);
        }

        public void RemovePlayerSlot(string userId)
        {
            if (!playerControls.ContainsKey(userId)) return;

            var (image, text) = playerControls[userId];

            Border? borderToRemove = null;
            foreach (var child in GameUIGrid.Children)
                if (child is Border border && border.Child == image)
                {
                    borderToRemove = border;
                    break;
                }

            if (borderToRemove != null)
                GameUIGrid.Children.Remove(borderToRemove);

            foreach (var panel in new[] { Player1StackPanel, Player2StackPanel, Player3StackPanel, Player4StackPanel })
                if (panel.Children.Contains(text))
                {
                    panel.Children.Clear();
                    break;
                }

            UnregisterName(image.Name);
            UnregisterName(text.Name);
            playerControls.Remove(userId);
        }

        private (int row, int col) GetPlayerPosition(int index)
        {
            return index switch
            {
                0 => (1, 2),
                1 => (2, 1),
                3 => (2, 3),
                2 => (3, 2),
                _ => (0, 0)
            };
        }

        private void ShowResult(string resultText)
        {
            var overlay = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(15),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, -500, 0, 0),
                Child = new TextBlock
                {
                    Text = resultText,
                    Foreground = Brushes.White,
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };

            OverlayGrid.Children.Add(overlay);

            Task.Delay(2000).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() => OverlayGrid.Children.Remove(overlay));
            });
        }
    }
}
