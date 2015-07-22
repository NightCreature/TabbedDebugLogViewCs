using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System;
using System.Windows.Media;
using System.IO;

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

            m_filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\TabbedDebugLog\filters.txt";
            //Check if settings file exists if so load it otherwise create one.
            if (File.Exists(m_filePath))
            {
                StreamReader reader = new StreamReader(m_filePath);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    //We need to add a new filter to the filter list
                    ListBoxItem newFilterItem = new ListBoxItem();
                    newFilterItem.Content = line;
                    Filters.Items.Add(newFilterItem);

                    AddRegexFilter(line);

                    AddTabToToolwindow(line);
                }
            }
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
            textBox.Background = Brushes.LightGray;
            scrollViewer.Content = textBox;
            m_textBlocks.Add(textBox);
            m_scrollViews.Add(scrollViewer);

            TabItem tabItem = new TabItem();
            tabItem.Name = controlBaseName + "Tab";
            tabItem.Header = controlBaseName;
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

        private void SaveFilters(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(m_filePath))
            {
                string rootFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\TabbedDebugLog\";
                if (!Directory.Exists(rootFolderPath))
                {
                    DirectoryInfo dirInfo = Directory.CreateDirectory(rootFolderPath);
                }
            }
            //Check if settings file exists if so load it otherwise create one.
            FileStream file = new FileStream(m_filePath, FileMode.OpenOrCreate);
            StreamWriter writer = new StreamWriter(file);

            Debug.WriteLine("Saving to: {0}", m_filePath);

            foreach ( Regex regex in m_filters)
            {
                var str = regex.ToString();
                if (str[0] == '^')
                {
                    str = str.Remove(0, 1);
                }
                int index = str.LastIndexOf(".*");
                if (index < str.Length)
                {
                    str = str.Remove(index);
                }
                writer.WriteLine(str);
                    
            }

            writer.Flush();
            file.Close();
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
            foreach (TextBox tb in m_textBlocks)
            {
                tb.Text = "";
            }
        }

        private ArrayList m_filters;
        private ArrayList m_textBlocks;
        private ArrayList m_scrollViews;
        private string m_filePath;

        private void NewFilter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                AddNewFilterInternal();
            }
        }
    }
}