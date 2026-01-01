using System;
using System.Drawing;
using System.Windows.Forms;
using KRPC.Client;
using KRPC.Client.Services.KRPC;
using KRPC.Client.Services.SpaceCenter;
using Timer = System.Windows.Forms.Timer;

namespace KSPControlPanel
{
    /// <summary>
    /// Главная форма приложения - панель управления KSP через kRPC
    /// Первая альфа для обучения
    /// </summary>
    public class Form1 : Form
    {
        #pragma warning disable CS8618 // Подавление предупреждения о ненулевых полях

        // ============ ПОЛЯ КЛАССА ============

        // Таймер для обновления телеметрии
        private Timer telemetryTimer;
        private int telemetryUpdateCount = 0; // Счетчик обновлений телеметрии

        // Кнопки управления
        private Button btnConnect;      // Подключение к KSP
        private Button btnDisconnect;   // Отключение от KSP
        private Button btnStage;        // Активация этапа
        private Button btnSAS;          // Включение/выключение SAS
        private Button btnGear;         // Выпуск/уборка шасси
        private Button btnThrottle;     // Управление тягой
        
        // Метки и текстовые поля
        private Label lblStatus;        // Статус подключения
        private Label lblVerticalSpeed; // Отображение вертикальной скорости
        private Label lblSpeed;         // Отображение общей скорости
        private TextBox txtLog;         // Лог событий

        // Подключение к kRPC серверу
        private Connection? krpcConnection;

        #pragma warning restore CS8618

        // ============ КОНСТАНТЫ ДЛЯ ЦВЕТОВ СКОРОСТИ ============

        // Пороги и цвета для вертикальной скорости
        private static readonly double[] VERTICAL_SPEED_THRESHOLDS = { 1, 10, 100 };
        private static readonly Color[] VERTICAL_SPEED_COLORS = 
        { 
            Color.Cyan,     // До 1 м/с
            Color.Yellow,   // 1-10 м/с
            Color.Orange,   // 10-100 м/с
            Color.Red       // Свыше 100 м/с
        };

        // Пороги и цвета для общей скорости
        private static readonly double[] SPEED_THRESHOLDS = { 10, 100, 1000 };
        private static readonly Color[] SPEED_COLORS = 
        { 
            Color.LightGreen, // До 10 м/с
            Color.Yellow,     // 10-100 м/с
            Color.Orange,     // 100-1000 м/с
            Color.Red         // Свыше 1000 м/с
        };

        // ============ КОНСТРУКТОР ФОРМЫ ============

        /// <summary>
        /// Конструктор формы - инициализация всех компонентов
        /// </summary>
        public Form1()
        {
            InitializeWindow();
            CreateControls();
            SetInitialState();
            InitializeTelemetryTimer();
        }

        // ============ МЕТОДЫ ИНИЦИАЛИЗАЦИИ ============

        /// <summary>
        /// Настройка параметров главного окна
        /// </summary>
        private void InitializeWindow()
        {
            Text = "KSP Control Panel";
            Size = new Size(700, 500);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(30, 30, 40);
            ForeColor = Color.White;
            TopMost = true; // Окно поверх всех окон
        }

        /// <summary>
        /// Инициализация таймера для обновления телеметрии
        /// </summary>
        private void InitializeTelemetryTimer()
        {
            telemetryTimer = new Timer();
            telemetryTimer.Interval = 500; // Обновление 2 раза в секунду (500 мс)
            telemetryTimer.Tick += TelemetryTimer_Tick;
        }

        /// <summary>
        /// Создание всех элементов управления на форме
        /// </summary>
        private void CreateControls()
        {
            CreateConnectionPanel();     // Панель подключения
            CreateControlPanel();        // Панель управления кораблем
            CreateLogPanel();           // Панель лога событий
            CreateTelemetryPanel();     // Панель телеметрии
            CreateDisconnectButton();   // Кнопка отключения
        }

        /// <summary>
        /// Установка начального состояния элементов управления
        /// </summary>
        private void SetInitialState()
        {
            // Все кнопки управления неактивны до подключения
            btnStage.Enabled = false;
            btnSAS.Enabled = false;
            btnGear.Enabled = false;
            btnThrottle.Enabled = false;
            btnDisconnect.Enabled = false;
            
            // Начальное сообщение в лог
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] KSP Control Panel запущен\r\n");
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Ожидание подключения...\r\n");
        }

        // ============ СОЗДАНИЕ ЭЛЕМЕНТОВ УПРАВЛЕНИЯ ============

        /// <summary>
        /// Создание панели подключения к KSP
        /// </summary>
        private void CreateConnectionPanel()
        {
            var panelConnect = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(660, 80),
                BackColor = Color.FromArgb(50, 50, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // Кнопка подключения
            btnConnect = new Button
            {
                Text = "🚀 ПОДКЛЮЧИТЬСЯ К KSP",
                Location = new Point(20, 20),
                Size = new Size(200, 40),
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnConnect.FlatAppearance.BorderSize = 0;
            btnConnect.Click += BtnConnect_Click;
            
            // Метка статуса подключения
            lblStatus = new Label
            {
                Text = "Статус: ОТКЛЮЧЕНО",
                Location = new Point(240, 30),
                Size = new Size(400, 20),
                Font = new Font("Arial", 10),
                ForeColor = Color.Gray
            };
            
            panelConnect.Controls.Add(btnConnect);
            panelConnect.Controls.Add(lblStatus);
            Controls.Add(panelConnect);
        }

        /// <summary>
        /// Создание кнопки отключения от KSP
        /// </summary>
        private void CreateDisconnectButton()
        {
            btnDisconnect = new Button
            {
                Text = "❌ ОТКЛЮЧИТЬСЯ",
                Location = new Point(430, 40),
                Size = new Size(200, 40),
                Font = new Font("Arial", 9),
                BackColor = Color.FromArgb(120, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnDisconnect.FlatAppearance.BorderSize = 0;
            btnDisconnect.Click += BtnDisconnect_Click;
            
            Controls.Add(btnDisconnect);
            btnDisconnect.BringToFront();
        }

        /// <summary>
        /// Создание панели управления кораблем
        /// </summary>
        private void CreateControlPanel()
        {
            var panelControl = new Panel
            {
                Location = new Point(20, 120),
                Size = new Size(320, 180),
                BackColor = Color.FromArgb(50, 50, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // Кнопка активации этапа
            btnStage = CreateControlButton("🚀 АКТИВИРОВАТЬ ЭТАП", new Point(20, 20), 
                Color.FromArgb(220, 80, 60), BtnStage_Click);
            
            // Кнопка системы стабилизации (SAS)
            btnSAS = CreateControlButton("🎯 SAS: ВЫКЛ", new Point(20, 65), 
                Color.FromArgb(70, 70, 80), BtnSAS_Click, new Size(135, 35));
            
            // Кнопка управления шасси
            btnGear = CreateControlButton("🛬 ШАССИ: УБРАНО", new Point(165, 65), 
                Color.FromArgb(70, 70, 80), BtnGear_Click, new Size(135, 35));
            
            // Кнопка управления тягой двигателя
            btnThrottle = CreateControlButton("⚡ ГАЗ: 0%", new Point(20, 110), 
                Color.FromArgb(70, 70, 80), BtnThrottle_Click);
            
            panelControl.Controls.Add(btnStage);
            panelControl.Controls.Add(btnSAS);
            panelControl.Controls.Add(btnGear);
            panelControl.Controls.Add(btnThrottle);
            
            Controls.Add(panelControl);
        }

        /// <summary>
        /// Вспомогательный метод для создания кнопок управления
        /// </summary>
        private Button CreateControlButton(string text, Point location, Color backColor, 
                                          EventHandler clickHandler, Size? size = null)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                Size = size ?? new Size(280, 35),
                Font = new Font("Arial", 9, text.Contains("АКТИВИРОВАТЬ") ? FontStyle.Bold : FontStyle.Regular),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += clickHandler;
            
            return button;
        }

        /// <summary>
        /// Создание панели лога событий
        /// </summary>
        private void CreateLogPanel()
        {
            var panelLog = new Panel
            {
                Location = new Point(360, 120),
                Size = new Size(320, 320),
                BackColor = Color.FromArgb(50, 50, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            var lblLog = new Label
            {
                Text = "ЖУРНАЛ СОБЫТИЙ:",
                Location = new Point(10, 10),
                Size = new Size(300, 20),
                Font = new Font("Arial", 9, FontStyle.Bold),
                ForeColor = Color.LightGray
            };
            
            txtLog = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(10, 35),
                Size = new Size(300, 275),
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(20, 20, 25),
                ForeColor = Color.LimeGreen,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true
            };
            
            panelLog.Controls.Add(lblLog);
            panelLog.Controls.Add(txtLog);
            Controls.Add(panelLog);
        }

        /// <summary>
        /// Создание панели телеметрии (скорости)
        /// </summary>
        private void CreateTelemetryPanel()
        {
            var panelTelemetry = new Panel
            {
                Location = new Point(20, 320),
                Size = new Size(320, 85),
                BackColor = Color.FromArgb(50, 50, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // Заголовок панели телеметрии
            var lblTelemetryTitle = new Label
            {
                Text = "ТЕЛЕМЕТРИЯ:",
                Location = new Point(10, 10),
                Size = new Size(300, 20),
                Font = new Font("Arial", 9, FontStyle.Bold),
                ForeColor = Color.LightGray
            };
            
            // Метка вертикальной скорости
            lblVerticalSpeed = new Label
            {
                Text = "▼ Вертикально: -- м/с",
                Location = new Point(20, 35),
                Size = new Size(280, 20),
                Font = new Font("Arial", 9),
                ForeColor = Color.Cyan
            };
            
            // Метка общей скорости
            lblSpeed = new Label
            {
                Text = "➤ Общая скорость: -- м/с",
                Location = new Point(20, 60),
                Size = new Size(280, 20),
                Font = new Font("Arial", 9),
                ForeColor = Color.LightGreen
            };
            
            panelTelemetry.Controls.Add(lblTelemetryTitle);
            panelTelemetry.Controls.Add(lblVerticalSpeed);
            panelTelemetry.Controls.Add(lblSpeed);
            
            Controls.Add(panelTelemetry);
        }

        // ============ УПРАВЛЕНИЕ ПОДКЛЮЧЕНИЕМ KSP ============

        /// <summary>
        /// Обработчик нажатия кнопки подключения к KSP
        /// </summary>
        private void BtnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Попытка подключения...\r\n");
                
                // Создание подключения к kRPC серверу
                krpcConnection = new Connection("KSP Control Panel");
                
                // Проверка версии kRPC
                var krpc = krpcConnection.KRPC();
                var version = krpc.GetStatus().Version;
                
                // Получение информации о текущем корабле
                var spaceCenter = krpcConnection.SpaceCenter();
                var vessel = spaceCenter.ActiveVessel;
                
                // Обновление интерфейса при успешном подключении
                UpdateConnectionUI(vessel, version);
                
                // Запуск таймера для обновления телеметрии
                StartTelemetryTimer(vessel);
                
                // Активация кнопок управления
                EnableControlButtons();
            }
            catch (Exception ex)
            {
                // Обработка ошибки подключения
                HandleConnectionError(ex);
            }
        }

        /// <summary>
        /// Обновление интерфейса после успешного подключения
        /// </summary>
        private void UpdateConnectionUI(Vessel vessel, string version)
        {
            lblStatus.Text = $"✅ ПОДКЛЮЧЕНО: {vessel.Name}";
            lblStatus.ForeColor = Color.LimeGreen;
            
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] ✅ Успешное подключение\r\n");
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] kRPC версия: {version}\r\n");
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Корабль: {vessel.Name}\r\n");
        }

        /// <summary>
        /// Запуск таймера обновления телеметрии
        /// </summary>
        private void StartTelemetryTimer(Vessel vessel)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] ЗАПУСКАЕМ ТАЙМЕР...\r\n");
            telemetryTimer.Start();
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Таймер запущен: Enabled={telemetryTimer.Enabled}\r\n");
            
            // Тестовое обновление телеметрии сразу после подключения
            try
            {
                UpdateVerticalSpeed(vessel);
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Телеметрия протестирована\r\n");
            }
            catch (Exception ex)
            {
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Ошибка теста телеметрии: {ex.Message}\r\n");
            }
        }

        /// <summary>
        /// Активация кнопок управления после подключения
        /// </summary>
        private void EnableControlButtons()
        {
            btnConnect.Enabled = false;
            btnConnect.BackColor = Color.FromArgb(40, 40, 50);
            btnConnect.Text = "✅ ПОДКЛЮЧЕНО";
            
            btnStage.Enabled = true;
            btnSAS.Enabled = true;
            btnGear.Enabled = true;
            btnThrottle.Enabled = true;
            btnDisconnect.Enabled = true;
            
            // Обновление начальных состояний кнопок
            UpdateButtonStates();
        }

        /// <summary>
        /// Обработка ошибки подключения
        /// </summary>
        private void HandleConnectionError(Exception ex)
        {
            lblStatus.Text = "❌ ОШИБКА ПОДКЛЮЧЕНИЯ";
            lblStatus.ForeColor = Color.Red;
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] ❌ Ошибка: {ex.Message}\r\n");
        }

        /// <summary>
        /// Обработчик нажатия кнопки отключения
        /// </summary>
        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            CloseConnection();
            UpdateDisconnectionUI();
        }

        /// <summary>
        /// Обновление интерфейса после отключения
        /// </summary>
        private void UpdateDisconnectionUI()
        {
            lblStatus.Text = "Статус: ОТКЛЮЧЕНО";
            lblStatus.ForeColor = Color.Gray;
            
            btnConnect.Enabled = true;
            btnConnect.BackColor = Color.FromArgb(0, 120, 215);
            btnConnect.Text = "🚀 ПОДКЛЮЧИТЬСЯ К KSP";
            
            btnDisconnect.Enabled = false;
            btnStage.Enabled = false;
            btnSAS.Enabled = false;
            btnGear.Enabled = false;
            btnThrottle.Enabled = false;
            
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 🔌 Отключено от KSP\r\n");
        }

        /// <summary>
        /// Закрытие подключения к kRPC серверу
        /// </summary>
        private void CloseConnection()
        {
            // Остановка таймера телеметрии
            if (telemetryTimer != null)
            {
                telemetryTimer.Stop();
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Таймер остановлен\r\n");
            }
            
            // Закрытие подключения kRPC
            try
            {
                if (krpcConnection != null)
                {
                    krpcConnection.Dispose();
                    krpcConnection = null;
                }
            }
            catch { }
            
            // Сброс отображения телеметрии
            ResetTelemetryDisplay();
            
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 🔌 Подключение закрыто\r\n");
        }

        /// <summary>
        /// Сброс отображения телеметрии при отключении
        /// </summary>
        private void ResetTelemetryDisplay()
        {
            if (lblVerticalSpeed != null && !lblVerticalSpeed.IsDisposed)
            {
                lblVerticalSpeed.Text = "▼ Вертикально: -- м/с";
                lblVerticalSpeed.ForeColor = Color.Gray;
            }
            
            if (lblSpeed != null && !lblSpeed.IsDisposed)
            {
                lblSpeed.Text = "➤ Общая скорость: -- м/с";
                lblSpeed.ForeColor = Color.Gray;
            }
        }

        // ============ УПРАВЛЕНИЕ КОРАБЛЕМ ============

        /// <summary>
        /// Обновление состояний кнопок управления на основе текущего состояния корабля
        /// </summary>
        private void UpdateButtonStates()
        {
            if (krpcConnection == null) return;
            
            try
            {
                var vessel = krpcConnection.SpaceCenter().ActiveVessel;
                var control = vessel.Control;
                
                // Обновление кнопки SAS
                UpdateSASButton(control.SAS);
                
                // Обновление кнопки шасси
                UpdateGearButton(control.Gear);
                
                // Обновление кнопки тяги
                UpdateThrottleButton(control.Throttle);
            }
            catch
            {
                // Игнорируем ошибки при обновлении
            }
        }

        /// <summary>
        /// Обновление кнопки системы стабилизации (SAS)
        /// </summary>
        private void UpdateSASButton(bool sasEnabled)
        {
            btnSAS.Text = sasEnabled ? "🎯 SAS: ВКЛ" : "🎯 SAS: ВЫКЛ";
            btnSAS.BackColor = sasEnabled ? 
                Color.FromArgb(0, 150, 100) : 
                Color.FromArgb(70, 70, 80);
        }

        /// <summary>
        /// Обновление кнопки управления шасси
        /// </summary>
        private void UpdateGearButton(bool gearDown)
        {
            btnGear.Text = gearDown ? "🛬 ШАССИ: ВЫП." : "🛬 ШАССИ: УБРАНО";
            btnGear.BackColor = gearDown ? 
                Color.FromArgb(0, 150, 100) : 
                Color.FromArgb(70, 70, 80);
        }

        /// <summary>
        /// Обновление кнопки управления тягой двигателя
        /// </summary>
        private void UpdateThrottleButton(float throttle)
        {
            int throttlePercent = (int)(throttle * 100);
            btnThrottle.Text = $"⚡ ГАЗ: {throttlePercent}%";
            btnThrottle.BackColor = throttlePercent > 0 ? 
                Color.FromArgb(220, 120, 0) : 
                Color.FromArgb(70, 70, 80);
        }

        /// <summary>
        /// Обработчик активации следующего этапа
        /// </summary>
        private void BtnStage_Click(object sender, EventArgs e)
        {
            if (krpcConnection == null) return;
            
            try
            {
                var vessel = krpcConnection.SpaceCenter().ActiveVessel;
                vessel.Control.ActivateNextStage();
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 🚀 ЭТАП АКТИВИРОВАН!\r\n");
            }
            catch (Exception ex)
            {
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] ❌ Ошибка: {ex.Message}\r\n");
            }
        }

        /// <summary>
        /// Обработчик включения/выключения системы стабилизации (SAS)
        /// </summary>
        private void BtnSAS_Click(object sender, EventArgs e)
        {
            if (krpcConnection == null) return;
            
            try
            {
                var vessel = krpcConnection.SpaceCenter().ActiveVessel;
                bool newState = !vessel.Control.SAS;
                vessel.Control.SAS = newState;
                
                // Обновление кнопки SAS
                UpdateSASButton(newState);
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] SAS: {(newState ? "ВКЛЮЧЕН" : "ВЫКЛЮЧЕН")}\r\n");
            }
            catch (Exception ex)
            {
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] ❌ Ошибка SAS: {ex.Message}\r\n");
            }
        }

        /// <summary>
        /// Обработчик выпуска/уборки шасси
        /// </summary>
        private void BtnGear_Click(object sender, EventArgs e)
        {
            if (krpcConnection == null) return;
            
            try
            {
                var vessel = krpcConnection.SpaceCenter().ActiveVessel;
                bool newState = !vessel.Control.Gear;
                vessel.Control.Gear = newState;
                
                // Обновление кнопки шасси
                UpdateGearButton(newState);
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Шасси: {(newState ? "ВЫПУЩЕНО" : "УБРАНО")}\r\n");
            }
            catch (Exception ex)
            {
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] ❌ Ошибка шасси: {ex.Message}\r\n");
            }
        }

        /// <summary>
        /// Обработчик управления тягой двигателя (циклическое переключение)
        /// </summary>
        private void BtnThrottle_Click(object sender, EventArgs e)
        {
            if (krpcConnection == null) return;
            
            try
            {
                var vessel = krpcConnection.SpaceCenter().ActiveVessel;
                float currentThrottle = vessel.Control.Throttle;
                
                // Циклическое переключение: 0% → 25% → 50% → 75% → 100% → 0%
                float newThrottle = currentThrottle switch
                {
                    < 0.1f => 0.25f,
                    < 0.35f => 0.5f,
                    < 0.6f => 0.75f,
                    < 0.85f => 1.0f,
                    _ => 0.0f
                };
                
                vessel.Control.Throttle = newThrottle;
                int percent = (int)(newThrottle * 100);
                
                // Обновление кнопки тяги
                UpdateThrottleButton(newThrottle);
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Газ установлен: {percent}%\r\n");
            }
            catch (Exception ex)
            {
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] ❌ Ошибка газа: {ex.Message}\r\n");
            }
        }

        // ============ ТЕЛЕМЕТРИЯ И МОНИТОРИНГ ============

        /// <summary>
        /// Обработчик таймера для обновления телеметрии
        /// </summary>
        private void TelemetryTimer_Tick(object sender, EventArgs e)
        {
            telemetryUpdateCount++;
            
            // Диагностика: запись каждого тика таймера
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss.fff}] Таймер тик #{telemetryUpdateCount}\r\n");
            
            // Проверка подключения
            if (krpcConnection == null)
            {
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Нет подключения, останавливаем таймер\r\n");
                telemetryTimer.Stop();
                return;
            }
            
            try
            {
                var vessel = krpcConnection.SpaceCenter().ActiveVessel;
                
                if (vessel != null)
                {
                    // Обновление отображения скоростей
                    UpdateVerticalSpeed(vessel);
                }
                else
                {
                    // Нет активного корабля (например, в ангаре VAB)
                    ResetTelemetryDisplay();
                }
            }
            catch (Exception ex)
            {
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Ошибка в таймере: {ex.GetType().Name}: {ex.Message}\r\n");
            }
        }

        /// <summary>
        /// Основной метод обновления отображения скоростей корабля
        /// </summary>
        private void UpdateVerticalSpeed(Vessel vessel)
        {
            try
            {
                var flight = vessel.Flight(vessel.Orbit.Body.ReferenceFrame);
                var verticalSpeed = flight.VerticalSpeed;
                var speed = flight.Speed;
                
                // Обновление отображения вертикальной скорости
                UpdateVerticalSpeedDisplay(verticalSpeed);
                
                // Обновление отображения общей скорости
                UpdateSpeedDisplay(speed);
                
                // Периодическое логирование для диагностики (каждые 20 обновлений)
                if (telemetryUpdateCount % 20 == 0)
                {
                    txtLog.AppendText($"[{DateTime.Now:HH:mm:ss.fff}] V={verticalSpeed:F1} S={speed:F1} м/с\r\n");
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок получения телеметрии
                HandleTelemetryError();
            }
        }

        /// <summary>
        /// Обновление отображения вертикальной скорости
        /// </summary>
        private void UpdateVerticalSpeedDisplay(double verticalSpeed)
        {
            if (lblVerticalSpeed != null && !lblVerticalSpeed.IsDisposed)
            {
                string direction = verticalSpeed < 0 ? "▼" : "▲";
                lblVerticalSpeed.Text = $"{direction} Вертикально: {Math.Abs(verticalSpeed):F1} м/с";
                
                // Определение цвета на основе скорости
                lblVerticalSpeed.ForeColor = GetSpeedColor(
                    Math.Abs(verticalSpeed), 
                    VERTICAL_SPEED_THRESHOLDS, 
                    VERTICAL_SPEED_COLORS
                );
            }
        }

        /// <summary>
        /// Обновление отображения общей скорости
        /// </summary>
        private void UpdateSpeedDisplay(double speed)
        {
            if (lblSpeed != null && !lblSpeed.IsDisposed)
            {
                lblSpeed.Text = $"➤ Общая скорость: {speed:F1} м/с";
                
                // Определение цвета на основе скорости
                lblSpeed.ForeColor = GetSpeedColor(
                    speed, 
                    SPEED_THRESHOLDS, 
                    SPEED_COLORS
                );
            }
        }

        /// <summary>
        /// Определение цвета для отображения скорости на основе порогов
        /// </summary>
        private Color GetSpeedColor(double speed, double[] thresholds, Color[] colors)
        {
            // Проверяем каждый порог
            for (int i = 0; i < thresholds.Length; i++)
            {
                if (speed <= thresholds[i])
                    return colors[i];
            }
            
            // Если скорость превышает все пороги - возвращаем последний цвет
            return colors[colors.Length - 1];
        }

        /// <summary>
        /// Обработка ошибок получения телеметрии
        /// </summary>
        private void HandleTelemetryError()
        {
            if (lblVerticalSpeed != null && !lblVerticalSpeed.IsDisposed)
            {
                lblVerticalSpeed.Text = "▼ Ошибка телеметрии";
                lblVerticalSpeed.ForeColor = Color.Red;
            }
            
            if (lblSpeed != null && !lblSpeed.IsDisposed)
            {
                lblSpeed.Text = "➤ Ошибка телеметрии";
                lblSpeed.ForeColor = Color.Red;
            }
        }

        // ============ ОБРАБОТЧИКИ СОБЫТИЙ ФОРМЫ ============

        /// <summary>
        /// Обработчик закрытия формы
        /// </summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseConnection();
        }

        /// <summary>
        /// Обработчик закрытия формы
        /// </summary>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseConnection();
        }
    }
}