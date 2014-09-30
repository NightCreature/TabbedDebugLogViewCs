using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System;

namespace DarknessvsLightness.TabbedDebugLogView
{
    /// <summary>
    /// Interaction logic for TabbedLogControl.xaml
    /// </summary>
    public partial class TabbedLogControl : UserControl
    {
        public TabbedLogControl()
        {
            InitializeComponent();
            m_filters = new ArrayList();
            m_textBlocks = new ArrayList();
            m_scrollViews = new ArrayList();
        }

        private void AddFilter(object sender, RoutedEventArgs e)
        {
            AddNewFilterInternal();
        }

        private void AddNewFilterInternal()
        {
            if (NewFilter.Text != "")
            {
                //We need to add a new filter to the filter list
                ListBoxItem newFilterItem = new ListBoxItem();
                newFilterItem.Content = NewFilter.Text;
                Filters.Items.Add(newFilterItem);

                AddRegexFilter(NewFilter.Text);

                AddTabToToolwindow(NewFilter.Text);
            }
        }

        private void AddRegexFilter(string filterTtext)
        {
            //We need to escape certain characters from this filter, .,$,^,{,[,|,(,),*,+,\ need to be escaped to be able to use them
            //string[] escapeSequences = { "\\", ".", "$", "^", "{", "[", "|", "(", ")", "*", "+" };
            //string[] escapedVersions = { "\\\\", "\\.", "\\$", "\\^", "\\{", "\\[", "\\|", "\\(", "\\)", "\\*", "\\+" };
            //for (int counter = 0; counter < escapeSequences.Length; ++counter)
            //{
            //    filterTtext = filterTtext.Replace(escapeSequences[counter], escapedVersions[counter]);
            //}

            RegexOptions regexOptions = RegexOptions.IgnoreCase;
            Regex regex = new Regex("^" + filterTtext + ".*", regexOptions);
            
            m_filters.Add(regex);
        }

        private void AddTabToToolwindow(string filterText)
        {
            string controlBaseName = filterText;
            if (controlBaseName.Contains("["))
            {
                controlBaseName = controlBaseName.Replace("[", "");
            }
            if (controlBaseName.Contains("]"))
            {
                controlBaseName = controlBaseName.Replace("]", "");
            }

            string[] charsToRemove = { " ", "\\", ".", "$", "^", "{", "[", "|", "(", ")", "*", "+" };
            foreach (var str in charsToRemove)
            {
                controlBaseName = controlBaseName.Replace(str, "");
            }
            

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            Grid.SetRow(scrollViewer, 0);
            grid.Children.Add(scrollViewer);
            TextBox textBox = new TextBox();
            textBox.Name = controlBaseName + "TextBox";
            textBox.Text = "";
            textBox.IsReadOnly = true;
            scrollViewer.Content = textBox;
            m_textBlocks.Add(textBox);
            m_scrollViews.Add(scrollViewer);

            TabItem tabItem = new TabItem();
            tabItem.Name = controlBaseName + "Tab";
            tabItem.Header = NewFilter.Text.Replace("\\", "");
            tabItem.Content = grid;
            Tabs.Items.Add(tabItem);
        }

        private void RemoveFilter(object sender, RoutedEventArgs e)
        {
            if (Filters.SelectedIndex > -1)
            {
                Tabs.Items.Remove(Tabs.Items[Filters.SelectedIndex + 1]);
                m_filters.RemoveAt(Filters.SelectedIndex);
                m_textBlocks.RemoveAt(Filters.SelectedIndex);
                m_scrollViews.RemoveAt(Filters.SelectedIndex);

                //Always do this last it will invalidate Filters.SelectedItem otherwise
                Filters.Items.Remove(Filters.SelectedItem);
            }
        }

        public void ReceivedString(string newDebugString)
        {
            DateTime now = DateTime.Now;
            for (int counter = 0; counter < m_filters.Count; ++counter )
            {
                var regex = m_filters[counter] as Regex;
                if (regex.IsMatch(newDebugString))
                {
                    var textBlock = m_textBlocks[counter] as TextBox;
                    textBlock.Text += now.ToLongTimeString() + " " + newDebugString + Environment.NewLine;
                    var scroller = m_scrollViews[counter] as ScrollViewer;
                    scroller.ScrollToBottom();
                }
            }
        }

        public void ResetOutputTabs()
        {
            foreach (TextBlock tb in m_textBlocks)
            {
                tb.Text = "";
            }
        }

        private ArrayList m_filters;
        private ArrayList m_textBlocks;
        private ArrayList m_scrollViews;

        private void NewFilter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                AddNewFilterInternal();
            }
        }
    }
}