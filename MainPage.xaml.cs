using System;
using System.Collections.Generic;

namespace StudyMateProject
{
    public partial class MainPage : ContentPage
    {
        private readonly List<string> _tipsOfDay;
        private readonly Random _random;

        public MainPage()
        {
            InitializeComponent();

            _random = new Random();
            _tipsOfDay = new List<string>
            {
                "Используйте калькулятор для быстрых вычислений во время учебы!",
                "Создавайте заметки с картинками для лучшего запоминания материала.",
                "Настройте напоминания для важных дедлайнов и экзаменов.",
                "Организуйте заметки по папкам для быстрого поиска.",
                "Регулярно синхронизируйте данные между устройствами.",
                "Используйте графические заметки для схем и диаграмм.",
                "Делайте короткие перерывы каждые 25 минут (метод Помодоро).",
                "Повторяйте материал через определенные интервалы времени."
            };

            LoadMainPageData();
        }

        //protected override void OnAppearing()
        //{
        //    base.OnAppearing();
        //    RefreshStatistics();
        //    ShowRandomTip();
        //}

        private void LoadMainPageData()
        {
            // Инициализация данных при первой загрузке
            RefreshStatistics();
            ShowRandomTip();
            LoadRecentNotes();
        }

        private void RefreshStatistics()
        {
            // Здесь будет логика получения реальной статистики
            // Пока используем заглушки

            //NotesCountLabel.Text = GetNotesCount().ToString();
            //DrawingsCountLabel.Text = GetDrawingsCount().ToString();
            //RemindersCountLabel.Text = GetRemindersCount().ToString();

            //// Обновляем прогресс-бар
            //UpdateWeekProgress();
        }

        private void ShowRandomTip()
        {
            int randomIndex = _random.Next(_tipsOfDay.Count);
            //TipOfDayLabel.Text = _tipsOfDay[randomIndex];
        }

        private void LoadRecentNotes()
        {
            // Здесь будет логика загрузки последних заметок
            // Пока заметок нет, показываем заглушку

            //var recentNotes = GetRecentNotes();

            //if (recentNotes.Count == 0)
            //{
            //    // Заглушка уже есть в XAML
            //    return;
            //}

            //// Если есть заметки, очищаем контейнер и добавляем их
            //RecentNotesContainer.Children.Clear();

            //foreach (var note in recentNotes)
            //{
            //    var noteFrame = CreateNotePreview(note);
            //    RecentNotesContainer.Children.Add(noteFrame);
            //}
        }

        //private Frame CreateNotePreview(string noteTitle)
        //{
        //    var frame = new Frame
        //    {
        //        BackgroundColor = Colors.White,
        //        CornerRadius = 8,
        //        Padding = new Thickness(10),
        //        Margin = new Thickness(0, 5),
        //        HasShadow = false
        //    };

        //    var stackLayout = new StackLayout
        //    {
        //        Orientation = StackOrientation.Horizontal
        //    };

        //    var titleLabel = new Label
        //    {
        //        Text = noteTitle,
        //        FontSize = 16,
        //        VerticalOptions = LayoutOptions.Center,
        //        HorizontalOptions = LayoutOptions.FillAndExpand
        //    };

        //    var dateLabel = new Label
        //    {
        //        Text = DateTime.Now.ToString("dd.MM"),
        //        FontSize = 12,
        //        TextColor = Colors.Gray,
        //        VerticalOptions = LayoutOptions.Center
        //    };

        //    stackLayout.Children.Add(titleLabel);
        //    stackLayout.Children.Add(dateLabel);
        //    frame.Content = stackLayout;

        //    // Добавляем обработчик нажатия
        //    var tapGesture = new TapGestureRecognizer();
        //    tapGesture.Tapped += (sender, e) => OnNotePreviewTapped(noteTitle);
        //    frame.GestureRecognizers.Add(tapGesture);

        //    return frame;
        //}

        ////private void UpdateWeekProgress()
        ////{
        ////    // Здесь будет логика расчета прогресса за неделю
        ////    // Пока используем случайное значение
        ////    double progress = _random.NextDouble() * 0.8; // 0-80%
        ////    WeekProgressBar.Progress = progress;
        ////}

        //#region Event Handlers

        //private async void OnCreateNoteClicked(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        await Shell.Current.GoToAsync("//notes");
        //    }
        //    catch (Exception ex)
        //    {
        //        await DisplayAlert("Ошибка", $"Не удалось перейти к заметкам: {ex.Message}", "ОК");
        //    }
        //}

        //private async void OnCalculatorClicked(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        await Shell.Current.GoToAsync("//calculator");
        //    }
        //    catch (Exception ex)
        //    {
        //        await DisplayAlert("Ошибка", $"Не удалось открыть калькулятор: {ex.Message}", "ОК");
        //    }
        //}

        //private async void OnDrawingClicked(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        await Shell.Current.GoToAsync("//drawing");
        //    }
        //    catch (Exception ex)
        //    {
        //        await DisplayAlert("Ошибка", $"Не удалось перейти к рисованию: {ex.Message}", "ОК");
        //    }
        //}

        //private async void OnRemindersClicked(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        await Shell.Current.GoToAsync("//reminders");
        //    }
        //    catch (Exception ex)
        //    {
        //        await DisplayAlert("Ошибка", $"Не удалось перейти к напоминаниям: {ex.Message}", "ОК");
        //    }
        //}

        //private async void OnNotePreviewTapped(string noteTitle)
        //{
        //    try
        //    {
        //        // Переход к детальному просмотру заметки
        //        await Shell.Current.GoToAsync($"notedetail?title={noteTitle}");
        //    }
        //    catch (Exception ex)
        //    {
        //        await DisplayAlert("Ошибка", $"Не удалось открыть заметку: {ex.Message}", "ОК");
        //    }
        //}

        //#endregion

        //#region Data Methods (заглушки)

        //private int GetNotesCount()
        //{
        //    // Здесь будет обращение к сервису данных
        //    return _random.Next(0, 15);
        //}

        //private int GetDrawingsCount()
        //{
        //    // Здесь будет обращение к сервису данных
        //    return _random.Next(0, 8);
        //}

        //private int GetRemindersCount()
        //{
        //    // Здесь будет обращение к сервису данных
        //    return _random.Next(0, 5);
        //}

        //private List<string> GetRecentNotes()
        //{
        //    // Здесь будет обращение к сервису данных
        //    // Пока возвращаем пустой список
        //    return new List<string>();

        //    // Пример данных для тестирования:
        //    /*
        //    return new List<string>
        //    {
        //        "Лекция по математике",
        //        "Конспект по физике",
        //        "Домашнее задание",
        //        "Подготовка к экзамену"
        //    };
        //    */
        //}

//#endregion
    }
}