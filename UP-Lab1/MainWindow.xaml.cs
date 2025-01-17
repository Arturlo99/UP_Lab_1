﻿using DocumentFormat.OpenXml.ExtendedProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AForge;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;
using System.Runtime.CompilerServices;
using UP_Lab1.Properties;
using System.Linq.Expressions;
using AForge.Video.FFMPEG;
using System.Threading;
using Microsoft.Win32;
using System.IO;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using AForge.Imaging.Filters;

namespace UP_Lab1
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private FilterInfoCollection cameraDevices;
        private VideoCaptureDevice videoSource; //aktywna kamerka
        private Boolean greyscaled;
        private Boolean thresholded;
        private Boolean isRecording;
        private VideoFileWriter writer;
        private DateTime? firstFrameTime;

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            cameraDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice); // lista wszystkihc kamerek
            foreach (FilterInfo Device in cameraDevices)
            {

                if (!cameraListComboBox.Items.Contains(Device.Name))
                {
                    cameraListComboBox.Items.Add(Device.Name);
                }

            }
            cameraListComboBox.SelectedIndex = 0;
            videoSource = new VideoCaptureDevice();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            StartCamera();
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {


            try
            {
                BitmapImage bi;
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    //obsługa filtrów obrazu
                    if (greyscaled)
                    {
                        using (var grayscaledBitmap = Grayscale.CommonAlgorithms.BT709.Apply(bitmap))
                        {
                            bi = grayscaledBitmap.ToBitmapImage();
                        }

                    }
                    else if (thresholded)
                    {
                        using (var grayscaledBitmap = Grayscale.CommonAlgorithms.BT709.Apply(bitmap))
                        using (var thresholdedbitmap = new Threshold(100).Apply(grayscaledBitmap))
                        {
                            bi = thresholdedbitmap.ToBitmapImage();
                        }
                    }

                    else
                    {
                        bi = bitmap.ToBitmapImage(); //konwersja bitmapy na BitmapImage
                    }
                }

                bi.Freeze(); // Aby zapobiec wyciekom pamięci
                Dispatcher.BeginInvoke(new ThreadStart(delegate { cameraImage.Source = bi; }));

            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show("Wystąpił błąd przy tworzeniu nowych klatek\n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void SaveSnapshot()
        {
            BitmapImage snapshot = (BitmapImage)cameraImage.Source;
            try
            {
                if (cameraImage != null)
                {
                    var dialog = new SaveFileDialog();
                    dialog.FileName = "Snapshot1";
                    dialog.DefaultExt = ".jpg";
                    var dialogresult = dialog.ShowDialog();
                    if (dialogresult != true)
                    {
                        return;
                    }
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(snapshot));
                    using (var filestream = new FileStream(dialog.FileName, FileMode.Create))
                    {
                        encoder.Save(filestream);
                    }
                }
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("Zapisywanie zdjęcia nie powiodło się");
            }

        }

        private void StartCamera()
        {
            //przypisanie wybranej kamerki jako ta z której będziemy korzystać
            videoSource = new VideoCaptureDevice(cameraDevices[cameraListComboBox.SelectedIndex].MonikerString);
            videoSource.NewFrame += video_NewFrame;
            videoSource.Start();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveSnapshot();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            StopCamera();
        }
        private void StopCamera()
        {
            videoSource.Stop();
            videoSource.NewFrame -= video_NewFrame;
        }

        private void btnThreshhold_Click(object sender, RoutedEventArgs e)
        {
            thresholded = !thresholded;
        }

        private void btnGrayscale_Click(object sender, RoutedEventArgs e)
        {
            greyscaled = !greyscaled;
        }

    }
}
