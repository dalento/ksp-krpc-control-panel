using System;
using System.Drawing;
using System.Windows.Forms;
using KRPC.Client;
using KRPC.Client.Services.KRPC;
using KRPC.Client.Services.SpaceCenter;
using Timer = System.Windows.Forms.Timer;
namespace KSPControlPanel
{
    public class Form1 : Form
    {

    #pragma warning disable CS8618
        private Timer telemetryTimer;
        // Поля для элементов управления
        private Button btnConnect;
        private Button btnStage;
        private Button btnSAS;
        private Button btnGear;
        private Button btnThrottle;
        private Label lblStatus;
        private TextBox txtLog;
        private Label lblSpeed;

        private Label lblVerticalSpeed; 
        private Button btnDisconnect;
        
        // Подключение к KSP
        private Connection? krpcConnection;
        
        public Form1()
{
    // Настройка главного окна
    Text = "KSP Control Panel";
    Size = new Size(700, 500);
    StartPosition = FormStartPosition.CenterScreen;
    BackColor = Color.FromArgb(30, 30, 40);
    ForeColor = Color.White;
    
    // Делаем окно поверх всех окон
    this.TopMost = true;
    
    // Создаем элементы управления
    CreateControls();
    
    // Начальное состояние
    SetInitialState();
    
    // Создаём и НАСТРАИВАЕМ таймер (но не запускаем)
    telemetryTimer = new System.Windows.Forms.Timer();
    telemetryTimer.Interval = 500; // 500 мс = 2 раза в секунду
    telemetryTimer.Tick += TelemetryTimer_Tick;
    
    // Запись в лог
    txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Таймер создан (Interval={telemetryTimer.Interval}ms)\r\n");
    txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] lblVerticalSpeed создан: {(lblVerticalSpeed != null ? "ДА" : "НЕТ")}\r\n");
}

        // Эти методы тоже нужно объявить:
// Общий метод обновления телеметрии
private void UpdateAllTelemetry()
{
    if (krpcConnection == null) return;
    
    try
    {
        var vessel = krpcConnection.SpaceCenter().ActiveVessel;
        if (vessel != null)
        {
            UpdateVerticalSpeed(vessel);
        }
    }
    catch { }
}


private void Form1_FormClosing(object sender, FormClosingEventArgs e)
{
    CloseConnection();  // Теперь метод существует
}

private void Form1_FormClosed(object sender, FormClosedEventArgs e)
{
    CloseConnection();  // Теперь метод существует
}

// Добавьте этот метод в ваш класс Form1 (после других методов)

private int telemetryUpdateCount = 0;

private void TelemetryTimer_Tick(object sender, EventArgs e)
{
    telemetryUpdateCount++;
    
    // Пишем в лог каждый тик (для отладки)
    txtLog.AppendText($"[{DateTime.Now:HH:mm:ss.fff}] Таймер тик #{telemetryUpdateCount}\r\n");
    
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
            UpdateVerticalSpeed(vessel);
        }
        else
        {
            // Нет активного корабля (может быть в VAB)
            if (lblVerticalSpeed != null && !lblVerticalSpeed.IsDisposed)
            {
                lblVerticalSpeed.Text = "▼ Верт. скорость: -- м/с";
                lblVerticalSpeed.ForeColor = Color.Gray;
            }
        }
    }
    catch (Exception ex)
    {
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Ошибка в таймере: {ex.GetType().Name}: {ex.Message}\r\n");
    }
}



private void CloseConnection()
{
    try
    {
        if (krpcConnection != null)
        {
            // Правильное закрытие подключения
            krpcConnection.Dispose();  // ← Вместо Close()
            krpcConnection = null;
            
            // Логируем
            if (txtLog != null && !txtLog.IsDisposed)
            {
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 🔌 Подключение закрыто\r\n");
            }
        }
    }
    catch (Exception ex)
    {
        // Тихий игнор или отладка
        System.Diagnostics.Debug.WriteLine($"Ошибка при закрытии: {ex.Message}");
    }


// Останавливаем таймер
    if (telemetryTimer != null)
    {
        telemetryTimer.Stop();
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Таймер остановлен\r\n");
    }
    
    // Закрываем подключение
    try
    {
        if (krpcConnection != null)
        {
            krpcConnection.Dispose();
            krpcConnection = null;
        }
    }
    catch { }
    
    // Сбрасываем телеметрию
    if (lblVerticalSpeed != null && !lblVerticalSpeed.IsDisposed)
    {
        lblVerticalSpeed.Text = "▼ Верт. скорость: -- м/с";
        lblVerticalSpeed.ForeColor = Color.Gray;
    }
    
    txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] 🔌 Подключение закрыто\r\n");

}
        
        private void CreateControls()
        {
            

var btnTest = new Button
{
    Text = "Тест телеметрии",
    Location = new Point(20, 400),
    Size = new Size(150, 30)
};

btnTest.Click += (s, e) => 
{
    if (krpcConnection == null) return;
    
    try
    {
        var vessel = krpcConnection.SpaceCenter().ActiveVessel;
        if (vessel != null)
        {
            UpdateVerticalSpeed(vessel);
        }
    }
    catch { }
};


Controls.Add(btnTest);

            // ============ ПАНЕЛЬ ПОДКЛЮЧЕНИЯ ============
            var panelConnect = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(660, 80),
                BackColor = Color.FromArgb(50, 50, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            btnConnect = new Button
            {
                Text = "ПОДКЛЮЧИТЬСЯ K KSP",
                Location = new Point(20, 20),
                Size = new Size(200, 40),
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnConnect.FlatAppearance.BorderSize = 0;
            btnConnect.Click += BtnConnect_Click;
            
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
            
            // ============ ПАНЕЛЬ УПРАВЛЕНИЯ ============
            var panelControl = new Panel
            {
                Location = new Point(20, 120),
                Size = new Size(320, 180),
                BackColor = Color.FromArgb(50, 50, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // Кнопка Stage
            btnStage = new Button
            {
                Text = "🚀 АКТИВИРОВАТЬ ЭТАП",
                Location = new Point(20, 20),
                Size = new Size(280, 35),
                Font = new Font("Arial", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(220, 80, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnStage.FlatAppearance.BorderSize = 0;
            btnStage.Click += BtnStage_Click;
            
            // Кнопка SAS
            btnSAS = new Button
            {
                Text = "🎯 SAS: ВЫКЛ",
                Location = new Point(20, 65),
                Size = new Size(135, 35),
                Font = new Font("Arial", 9),
                BackColor = Color.FromArgb(70, 70, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnSAS.FlatAppearance.BorderSize = 0;
            btnSAS.Click += BtnSAS_Click;
            
            // Кнопка шасси
            btnGear = new Button
            {
                Text = "🛬 ШАССИ: УБРАНО",
                Location = new Point(165, 65),
                Size = new Size(135, 35),
                Font = new Font("Arial", 9),
                BackColor = Color.FromArgb(70, 70, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnGear.FlatAppearance.BorderSize = 0;
            btnGear.Click += BtnGear_Click;
            
            // Кнопка газа
            btnThrottle = new Button
            {
                Text = "⚡ ГАЗ: 0%",
                Location = new Point(20, 110),
                Size = new Size(280, 35),
                Font = new Font("Arial", 9),
                BackColor = Color.FromArgb(70, 70, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnThrottle.FlatAppearance.BorderSize = 0;
            btnThrottle.Click += BtnThrottle_Click;
            
            
            panelControl.Controls.Add(btnStage);
            panelControl.Controls.Add(btnSAS);
            panelControl.Controls.Add(btnGear);
            panelControl.Controls.Add(btnThrottle);
            
            // ============ ПАНЕЛЬ ЛОГА ============
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


            // В CreateControls():
btnDisconnect = new Button
{
    Text = "❌ ОТКЛЮЧИТЬСЯ",
    Location = new Point(430, 40), // Рядом с кнопкой подключения
    Size = new Size(200, 40),
    Font = new Font("Arial", 9),
    BackColor = Color.FromArgb(120, 60, 60),
    ForeColor = Color.White,
    FlatStyle = FlatStyle.Flat,
    Enabled = false
};
btnDisconnect.FlatAppearance.BorderSize = 0;
btnDisconnect.Click += BtnDisconnect_Click;
            // Добавляем на форму
Controls.Add(btnDisconnect);
btnDisconnect.BringToFront();  // ← На передний план
    // ============ ОТОБРАЖЕНИЕ ВЕРТИКАЛЬНОЙ СКОРОСТИ ============
    
    // Панель для телеметрии (если ещё нет)
    var panelTelemetry = new Panel
    {
        Location = new Point(20, 320),  // Под панелью управления
        Size = new Size(320, 60),
        BackColor = Color.FromArgb(50, 50, 60),
        BorderStyle = BorderStyle.FixedSingle
    };
    
    // Заголовок
    var lblTelemetryTitle = new Label
    {
        Text = "ТЕЛЕМЕТРИЯ:",
        Location = new Point(10, 10),
        Size = new Size(300, 20),
        Font = new Font("Arial", 9, FontStyle.Bold),
        ForeColor = Color.LightGray
    };
    
    // Вертикальная скорость
    lblVerticalSpeed = new Label
    {
        Text = "▼ Верт. скорость: -- м/с",
        Location = new Point(20, 35),
        Size = new Size(280, 20),
        Font = new Font("Arial", 9),
        ForeColor = Color.Cyan
    };
        // После создания lblVerticalSpeed:
    if (lblVerticalSpeed == null)
    {
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] ОШИБКА: lblVerticalSpeed не создан!\r\n");
    }
    else
    {
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] lblVerticalSpeed создан, текст: '{lblVerticalSpeed.Text}'\r\n");
    }
    // Под lblVerticalSpeed добавьте:
lblSpeed = new Label
{
    Text = "➤ Общая скорость: -- м/с",
    Location = new Point(20, 60), // Под вертикальной скоростью
    Size = new Size(280, 20),
    Font = new Font("Arial", 9),
    ForeColor = Color.LightGreen
};

// И добавьте на ту же панель:
panelTelemetry.Controls.Add(lblSpeed);
    
    panelTelemetry.Controls.Add(lblTelemetryTitle);
    panelTelemetry.Controls.Add(lblVerticalSpeed);
    
    // Добавляем на форму
    Controls.Add(panelTelemetry);


            // ============ ДОБАВЛЯЕМ ВСЕ НА ФОРМУ ============
            Controls.Add(panelConnect);
            Controls.Add(panelControl);
            Controls.Add(panelLog);
        }
        
        private void SetInitialState()
        {
            // Все кнопки управления неактивны до подключения
            btnStage.Enabled = false;
            btnSAS.Enabled = false;
            btnGear.Enabled = false;
            btnThrottle.Enabled = false;
            
            // Начальный текст лога
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] KSP Control Panel запущен\r\n");
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Ожидание подключения...\r\n");
        }
        
        // ============ ОБРАБОТЧИКИ СОБЫТИЙ ============
        
        private void BtnConnect_Click(object sender, EventArgs e)
{
    try
    {
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Попытка подключения...\r\n");
        
        krpcConnection = new Connection("KSP Control Panel");
        
        // Проверяем подключение
        var krpc = krpcConnection.KRPC();
        var version = krpc.GetStatus().Version;
        
        // Получаем информацию о корабле
        var spaceCenter = krpcConnection.SpaceCenter();
        var vessel = spaceCenter.ActiveVessel;
        
        // Обновляем UI
        lblStatus.Text = $"✅ ПОДКЛЮЧЕНО: {vessel.Name}";
        lblStatus.ForeColor = Color.LimeGreen;
        
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] ✅ Успешное подключение\r\n");
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] kRPC версия: {version}\r\n");
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Корабль: {vessel.Name}\r\n");
        
        // ============ ЗАПУСК ТАЙМЕРА ============
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] ЗАПУСКАЕМ ТАЙМЕР...\r\n");
        
        telemetryTimer.Start();
        
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Таймер запущен: Enabled={telemetryTimer.Enabled}\r\n");
        
        // Сразу тестируем обновление телеметрии
        try
        {
            UpdateVerticalSpeed(vessel);
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Телеметрия протестирована\r\n");
        }
        catch (Exception ex)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Ошибка теста телеметрии: {ex.Message}\r\n");
        }
        // ========================================
        
        // Активируем все кнопки управления
        btnConnect.Enabled = false;
        btnConnect.BackColor = Color.FromArgb(40, 40, 50);
        btnConnect.Text = "✅ ПОДКЛЮЧЕНО";
        
        btnStage.Enabled = true;
        btnSAS.Enabled = true;
        btnGear.Enabled = true;
        btnThrottle.Enabled = true;
        btnDisconnect.Enabled = true;
        
        // Устанавливаем начальные состояния кнопок
        UpdateButtonStates();
    }
    catch (Exception ex)
    {
        lblStatus.Text = "❌ ОШИБКА ПОДКЛЮЧЕНИЯ";
        lblStatus.ForeColor = Color.Red;
        
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] ❌ Ошибка: {ex.Message}\r\n");
    }
}
        
        private void UpdateButtonStates()
        {
            if (krpcConnection == null) return;
            
            try
            {
                var vessel = krpcConnection.SpaceCenter().ActiveVessel;
                var control = vessel.Control;
                
                // Обновляем SAS
                btnSAS.Text = control.SAS ? "🎯 SAS: ВКЛ" : "🎯 SAS: ВЫКЛ";
                btnSAS.BackColor = control.SAS ? 
                    Color.FromArgb(0, 150, 100) : 
                    Color.FromArgb(70, 70, 80);
                
                // Обновляем шасси
                btnGear.Text = control.Gear ? "🛬 ШАССИ: ВЫП." : "🛬 ШАССИ: УБРАНО";
                btnGear.BackColor = control.Gear ? 
                    Color.FromArgb(0, 150, 100) : 
                    Color.FromArgb(70, 70, 80);
                
                // Обновляем газ
                int throttlePercent = (int)(control.Throttle * 100);
                btnThrottle.Text = $"⚡ ГАЗ: {throttlePercent}%";
                btnThrottle.BackColor = throttlePercent > 0 ? 
                    Color.FromArgb(220, 120, 0) : 
                    Color.FromArgb(70, 70, 80);
            }
            catch
            {
                // Игнорируем ошибки при обновлении
            }
        }
        
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
        
        private void BtnSAS_Click(object sender, EventArgs e)
        {
            if (krpcConnection == null) return;
            
            try
            {
                var vessel = krpcConnection.SpaceCenter().ActiveVessel;
                bool newState = !vessel.Control.SAS;
                vessel.Control.SAS = newState;
                
                // Обновляем кнопку
                btnSAS.Text = newState ? "🎯 SAS: ВКЛ" : "🎯 SAS: ВЫКЛ";
                btnSAS.BackColor = newState ? 
                    Color.FromArgb(0, 150, 100) : 
                    Color.FromArgb(70, 70, 80);
                
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] SAS: {(newState ? "ВКЛЮЧЕН" : "ВЫКЛЮЧЕН")}\r\n");
            }
            catch (Exception ex)
            {
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] ❌ Ошибка SAS: {ex.Message}\r\n");
            }

            UpdateAllTelemetry();  // ← Добавить эту строку
        }
        
        private void BtnGear_Click(object sender, EventArgs e)
        {
            if (krpcConnection == null) return;
            
            try
            {
                var vessel = krpcConnection.SpaceCenter().ActiveVessel;
                bool newState = !vessel.Control.Gear;
                vessel.Control.Gear = newState;
                
                // Обновляем кнопку
                btnGear.Text = newState ? "🛬 ШАССИ: ВЫП." : "🛬 ШАССИ: УБРАНО";
                btnGear.BackColor = newState ? 
                    Color.FromArgb(0, 150, 100) : 
                    Color.FromArgb(70, 70, 80);
                
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Шасси: {(newState ? "ВЫПУЩЕНО" : "УБРАНО")}\r\n");
            }
            catch (Exception ex)
            {
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] ❌ Ошибка шасси: {ex.Message}\r\n");
            }
        }
        
        private void BtnThrottle_Click(object sender, EventArgs e)
        {
            if (krpcConnection == null) return;
            
            try
            {
                var vessel = krpcConnection.SpaceCenter().ActiveVessel;
                float current = vessel.Control.Throttle;
                
                // Цикл: 0% → 25% → 50% → 75% → 100% → 0%
                float newThrottle = current switch
                {
                    < 0.1f => 0.25f,
                    < 0.35f => 0.5f,
                    < 0.6f => 0.75f,
                    < 0.85f => 1.0f,
                    _ => 0.0f
                };
                
                vessel.Control.Throttle = newThrottle;
                int percent = (int)(newThrottle * 100);
                
                // Обновляем кнопку
                btnThrottle.Text = $"⚡ ГАЗ: {percent}%";
                btnThrottle.BackColor = percent > 0 ? 
                    Color.FromArgb(220, 120, 0) : 
                    Color.FromArgb(70, 70, 80);
                
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Газ установлен: {percent}%\r\n");
            }
            catch (Exception ex)
            {
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] ❌ Ошибка газа: {ex.Message}\r\n");
            }
        }

    // Обработчик:
private void BtnDisconnect_Click(object sender, EventArgs e)
{
    CloseConnection();
    
    // Обновляем UI
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
    
    #pragma warning restore CS8618
// Метод обновления вертикальной скорости
private void UpdateVerticalSpeed(Vessel vessel)
{
    try
    {
        var flight = vessel.Flight(vessel.Orbit.Body.ReferenceFrame);
        var verticalSpeed = flight.VerticalSpeed;
        var speed = flight.Speed;
        
        // Обновляем вертикальную скорость
        if (lblVerticalSpeed != null && !lblVerticalSpeed.IsDisposed)
        {
            string direction = verticalSpeed < 0 ? "▼" : "▲";
            lblVerticalSpeed.Text = $"{direction} Вертикально: {Math.Abs(verticalSpeed):F1} м/с";
            
            if (Math.Abs(verticalSpeed) > 100) lblVerticalSpeed.ForeColor = Color.Red;
            else if (Math.Abs(verticalSpeed) > 10) lblVerticalSpeed.ForeColor = Color.Orange;
            else if (Math.Abs(verticalSpeed) > 1) lblVerticalSpeed.ForeColor = Color.Yellow;
            else lblVerticalSpeed.ForeColor = Color.Cyan;
        }
        
        // Обновляем общую скорость
        if (lblSpeed != null && !lblSpeed.IsDisposed)
        {
            lblSpeed.Text = $"➤ Общая скорость: {speed:F1} м/с";
            
            if (speed > 1000) lblSpeed.ForeColor = Color.Red;
            else if (speed > 100) lblSpeed.ForeColor = Color.Orange;
            else if (speed > 10) lblSpeed.ForeColor = Color.Yellow;
            else lblSpeed.ForeColor = Color.LightGreen;
        }
        
        // Логируем для диагностики
        if (telemetryUpdateCount % 20 == 0)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss.fff}] V={verticalSpeed:F1} S={speed:F1} м/с\r\n");
        }
    }
    catch (Exception ex)
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
}

    
    }

    
    
}