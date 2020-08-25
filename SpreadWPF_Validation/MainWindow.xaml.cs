using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GrapeCity.Spreadsheet;
using GrapeCity.Wpf.SpreadSheet;

namespace SpreadWPF_Validation
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 検証の設定
            SetValidation(gcSpreadSheet1.Workbook.ActiveSheet, "Excel互換のエラー通知UI");
            SetValidation(gcSpreadSheet2.Workbook.ActiveSheet, "GcSpreadSheet独自のエラー通知UI");
            SetValidation(gcSpreadSheet3.Workbook.ActiveSheet, "カスタムのエラー通知UI");

            // 検証UIの指定
            cmbUiType.SelectedIndex = 0;
        }

        private void SetValidation(IWorksheet sheet, string comment)
        {
            // 注釈の表示
            sheet.Columns[0, 2].ColumnWidth = 80;
            sheet.Cells[0, 0, 0, 2].Merge();
            sheet.Cells[0, 0].Value = comment;

            // 整数値の入力規則
            IValidation validation = sheet.Cells[2, 1].Validation.Add(
                DataValidationType.WholeNumber,
                DataValidationErrorStyle.Stop,
                DataValidationOperator.GreaterThanOrEqual,
                "10");
            validation.ShowInput = true;
            validation.InputTitle = "入力メッセージ";
            validation.InputMessage = "10以上の整数を入力してください。";
            validation.ShowError = true;
            validation.ErrorTitle = "範囲外エラー";
            validation.ErrorMessage = "10以上の整数を入力してください。";
            validation.IgnoreBlank = true;

            // 値の設定
            sheet.Cells[2, 1].Value = 2;
        }

        private void btnMark_Click(object sender, RoutedEventArgs e)
        {
            // 検証マークの設定
            gcSpreadSheet1.Workbook.ActiveSheet.CircleInvalid();
            gcSpreadSheet2.Workbook.ActiveSheet.CircleInvalid();
            gcSpreadSheet3.Workbook.ActiveSheet.CircleInvalid();
        }

        private void btnUnmark_Click(object sender, RoutedEventArgs e)
        {
            // 検証マークの解除
            gcSpreadSheet1.Workbook.ActiveSheet.ClearCircles();
            gcSpreadSheet2.Workbook.ActiveSheet.ClearCircles();
            gcSpreadSheet3.Workbook.ActiveSheet.ClearCircles();
        }

        private void cmbUiType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 検証UIの指定
            switch (cmbUiType.SelectedIndex)
            {
                case 0: // Excel互換UI
                    gcSpreadSheet1.Visibility = Visibility.Visible;
                    gcSpreadSheet2.Visibility = Visibility.Collapsed;
                    gcSpreadSheet3.Visibility = Visibility.Collapsed;
                    gcSpreadSheet1.Workbook.ActiveSheet.CircleInvalid();
                    break;
                case 1: // SPREAD独自UI
                    gcSpreadSheet1.Visibility = Visibility.Collapsed;
                    gcSpreadSheet2.Visibility = Visibility.Visible;
                    gcSpreadSheet3.Visibility = Visibility.Collapsed;
                    gcSpreadSheet2.Workbook.ActiveSheet.CircleInvalid();
                    break;
                case 2: // カスタムUI
                    gcSpreadSheet1.Visibility = Visibility.Collapsed;
                    gcSpreadSheet2.Visibility = Visibility.Collapsed;
                    gcSpreadSheet3.Visibility = Visibility.Visible;
                    gcSpreadSheet3.Workbook.ActiveSheet.CircleInvalid();
                    break;
            }
        }
    }

    // カスタム検証エラーインジケーター用コントロール
    public class CustomValidationIndicatorControl : Control
    {
        public static readonly DependencyProperty IsActivedProperty = DependencyProperty.Register("IsActived", typeof(bool), typeof(CustomValidationIndicatorControl), new UIPropertyMetadata(UpdateTooltip));
        public static readonly DependencyProperty IsEditErrorProperty = DependencyProperty.Register("IsEditError", typeof(bool), typeof(CustomValidationIndicatorControl), new UIPropertyMetadata(UpdateTooltip));
        public static readonly DependencyProperty IsInvalidProperty = DependencyProperty.Register("IsInvalid", typeof(bool), typeof(CustomValidationIndicatorControl), new UIPropertyMetadata());
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(CustomValidationIndicatorControl), new UIPropertyMetadata());
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(CustomValidationIndicatorControl), new UIPropertyMetadata());

        public bool IsInvalid
        {
            get { return (bool)GetValue(IsInvalidProperty); }
            internal set { SetValue(IsInvalidProperty, value); }
        }

        public bool IsActived
        {
            get { return (bool)GetValue(IsActivedProperty); }
            internal set { SetValue(IsActivedProperty, value); }
        }

        public bool IsEditError
        {
            get { return (bool)GetValue(IsEditErrorProperty); }
            internal set { SetValue(IsEditErrorProperty, value); }
        }

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            internal set { SetValue(MessageProperty, value); }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            internal set { SetValue(TitleProperty, value); }
        }

        public CustomValidationIndicatorControl()
        {
            this.IsTabStop = false;
            Binding binding = new Binding("IsFocused");
            this.SetBinding(CustomValidationIndicatorControl.IsActivedProperty, binding);

            Binding binding2 = new Binding("IsEditError");
            this.SetBinding(CustomValidationIndicatorControl.IsEditErrorProperty, binding2);

            Binding binding3 = new Binding("IsInvalid");
            this.SetBinding(CustomValidationIndicatorControl.IsInvalidProperty, binding3);
        }

        private static void UpdateTooltip(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            CustomValidationIndicatorControl validationErrorIndicator = dependencyObject as CustomValidationIndicatorControl;
            DataValidationContext dataValidationContext = validationErrorIndicator.DataContext as DataValidationContext;

            if (dataValidationContext != null && validationErrorIndicator.IsActived)
            {
                if (!string.IsNullOrEmpty(dataValidationContext.EditingErrorException))
                {
                    validationErrorIndicator.Message = dataValidationContext.EditingErrorException;
                    validationErrorIndicator.Title = string.Empty;
                }
                else
                {
                    validationErrorIndicator.Message = (validationErrorIndicator.IsEditError) ? (dataValidationContext.ShowError ? dataValidationContext.ErrorMessage : string.Empty)
                      : (dataValidationContext.ShowInput ? dataValidationContext.InputMessage : string.Empty);
                    validationErrorIndicator.Title = (validationErrorIndicator.IsEditError) ? (dataValidationContext.ShowError ? dataValidationContext.ErrorTitle : string.Empty)
                      : (dataValidationContext.ShowInput ? dataValidationContext.InputTitle : string.Empty);
                }
            }
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            if (oldParent != null && !this.IsDescendantOf(oldParent))
            {
                this.IsActived = false;
            }
            base.OnVisualParentChanged(oldParent);
        }
    }
}
