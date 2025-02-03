using Avalonia;
using Avalonia.Controls;

namespace Quantum.Service
{
    // Класс, реализующий присоединённое поведение для автоматической прокрутки
    public static class AutoScrollBehavior
    {
        // Объявляем присоединённое свойство EnableAutoScroll
        public static readonly AttachedProperty<bool> EnableAutoScrollProperty =
            // Регистрируем присоединённое свойство с владельцем ScrollViewer, типом bool и значением по умолчанию false
            AvaloniaProperty.RegisterAttached<ScrollViewer, bool>(
                "EnableAutoScroll", // Имя свойства
                typeof(AutoScrollBehavior), // Владелец свойства (класс, где оно определяется)
                false); // Значение по умолчанию

        // Геттер для получения значения свойства EnableAutoScroll
        public static bool GetEnableAutoScroll(AvaloniaObject element) =>
            element.GetValue(EnableAutoScrollProperty);

        // Сеттер для установки значения свойства EnableAutoScroll
        public static void SetEnableAutoScroll(AvaloniaObject element, bool value)
        {
            // Устанавливаем значение свойства
            element.SetValue(EnableAutoScrollProperty, value);

            // Проверяем, является ли элемент ScrollViewer
            if (element is ScrollViewer scrollViewer)
            {
                // Если свойство включено (true), подписываемся на событие ScrollChanged
                if (value)
                {
                    scrollViewer.ScrollChanged += ScrollViewerOnScrollChanged;
                }
                // Если свойство отключено (false), отписываемся от события
                else
                {
                    scrollViewer.ScrollChanged -= ScrollViewerOnScrollChanged;
                }
            }
        }

        // Обработчик события ScrollChanged, вызывается при изменении прокрутки
        private static void ScrollViewerOnScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            // Проверяем, что отправитель - это ScrollViewer
            if (sender is ScrollViewer scrollViewer && e.ExtentDelta.Y > 0)
            {
                // Если высота содержимого изменилась, выполняем прокрутку вниз
                scrollViewer.ScrollToEnd();
            }
        }
    }
}