using HeroesONE_R.Structures;
using HeroesONE_R.Structures.Common;
using HeroesONE_R.Structures.Subsctructures;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ONEBatch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        String target;
        String replacement;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void selectONEButton_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaOpenFileDialog dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
            {
                Filter = "Shadow The Hedgehog ONE files (*.one)|*.one|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == false)
            {
                target = "Target: Not Picked";
                targetFileStatusLabel.Text = target;
                return;
            }
            target = dialog.FileName;
            targetFileStatusLabel.Text = target;
        }

        private void selectReplacementButton_Click(object sender, RoutedEventArgs e) {
            Ookii.Dialogs.Wpf.VistaOpenFileDialog dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            if (dialog.ShowDialog() == false)
            {
                replacement = "Replacement: Not Picked";
                replacementFileStatusLabel.Text = replacement;
                return;
            }
            replacement = dialog.FileName;
            replacementFileStatusLabel.Text = replacement;
        }

        private void batchReplaceButton_Click(object sender, RoutedEventArgs e) {
            if (target.Equals("Target: Not Picked") || replacement.Equals("Replacement: Not Picked"))
            {
                MessageBox.Show("Target or Replacement Not Picked", "Aborted");
                return;
            }

            byte[] oneContent = File.ReadAllBytes(target);
            bool isShadow60Archive;
            switch (ONEArchiveTester.GetArchiveType(ref oneContent))
            {
                case ONEArchiveType.Shadow050:
                    isShadow60Archive = false;
                    break;
                case ONEArchiveType.Shadow060:
                    isShadow60Archive = true;
                    break;
                default:
                    MessageBox.Show("Unsupported .ONE File", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }
            HeroesONE_R.Structures.ShadowTheHedgehog.ONEShadowArchive parsedOne = HeroesONE_R.Structures.ShadowTheHedgehog.ONEShadowArchive.ParseONEFile(ref oneContent);

            byte[] replacementBytes = File.ReadAllBytes(replacement);
            byte[] replacementCompressed = Prs.Compress(ref replacementBytes);

            string operatedFiles = "";
            Archive newArchive = parsedOne.GetArchive();
            for (int i = 0; i < newArchive.Files.Count; i++)
            {
                if (newArchive.Files[i].Name.EndsWith(extensionTextBox.Text))
                {
                    operatedFiles += newArchive.Files[i].Name + "\n";
                    newArchive.Files[i].CompressedData = replacementCompressed;
                }
            }

            MessageBox.Show(operatedFiles, "Archive content to replace");
            
            Ookii.Dialogs.Wpf.VistaSaveFileDialog saveDialog = new Ookii.Dialogs.Wpf.VistaSaveFileDialog()
            {
                Filter = "Shadow The Hedgehog ONE files (*.one)|*.one|All files (*.*)|*.*"
            };

            if (saveDialog.ShowDialog() == false)
            {
                return;
            }

            File.WriteAllBytes(saveDialog.FileName, newArchive.BuildShadowONEArchive(isShadow60Archive).ToArray());
        }
    }
}
