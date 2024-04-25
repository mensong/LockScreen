using MaterialDesignThemes.Wpf;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LockScreen
{
    /// <summary>
    /// EditQuestionBank.xaml 的交互逻辑
    /// </summary>
    public partial class EditQuestionBank : Window
    {
        DataBase<tbl_QuestionBank> m_tblQuestionBank;
        List<tbl_QuestionBank> m_edittingQuestions;


        public EditQuestionBank()
        {
            try
            {
                InitializeComponent();

                initQuestionBank();

                m_edittingQuestions = m_tblQuestionBank.SelectAll().ToList();
                ListQuestions.ItemsSource = m_edittingQuestions;
            }
            catch { }
        }

        void initQuestionBank()
        {
            m_tblQuestionBank = new DataBase<tbl_QuestionBank>();

            if (m_tblQuestionBank.Count() == 0)
            {
                tbl_QuestionBank question = new tbl_QuestionBank()
                {
                    level = 1,
                    caseSensitive = false,
                };

                int id = 1;
                //加法题
                for (int i = 0; i <= 20; i++)
                {
                    for (int j = 0; j <= 20; j++)
                    {
                        question.id = id++;
                        question.question = i.ToString() + "+" + j.ToString() + "=";
                        question.answer = (i + j).ToString();
                        m_tblQuestionBank.Insert(question);
                    }
                }

                //减法题
                for (int i = 1; i <= 20; i++)
                {
                    for (int j = 0; j <= 20; j++)
                    {
                        if (i < j)
                            continue;

                        question.id = id++;
                        question.question = i.ToString() + "-" + j.ToString() + "=";
                        question.answer = (i - j).ToString();
                        m_tblQuestionBank.Insert(question);
                    }
                }
            }
        }

        private void ListQuestions_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Column.DisplayIndex == 0 || e.Column.DisplayIndex == 5 || e.Column.DisplayIndex == 6)
                e.Cancel = true;
        }

        private void ListQuestions_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var addQuestions = m_edittingQuestions.Where(a => a.id == 0);
            bool bAdd = m_tblQuestionBank.Insert(addQuestions);
            if (!bAdd)
            {
                MessageBox.Show(this, "保存错误", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var editQuestions = m_edittingQuestions.Where(a => a.id != 0);
            bool bEdit = m_tblQuestionBank.Update(editQuestions);
            if (!bAdd)
            {
                MessageBox.Show(this, "保存错误", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            IEnumerable<int> existIds = editQuestions.Select(a => a.id);
            bool bDel = m_tblQuestionBank.Delete(a => !existIds.Contains(a.id));
            if (!bDel)
            {
                MessageBox.Show(this, "保存错误", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
