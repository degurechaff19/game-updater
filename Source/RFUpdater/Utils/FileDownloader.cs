﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace RFUpdater.Utils
{
    class FileDownloader
    {
        private static int      curFile;
        private static long     lastBytes;
        private static long     currentBytes;

        private static Stopwatch stopWatch = new Stopwatch();
		private static int NetworkAction;

        public static void DownloadFile(int Action)
        {
			NetworkAction = Action;

			var basePath = String.Empty;
			if (Globals.ACTION_DOWNLOAD_DEFINITIONS == NetworkAction)
			{
				basePath = Globals.LocalModuleDefinitionFolder;
			}
			else {
				basePath = Globals.GameBasePath;
			}

            if(Globals.OldFiles.Count <= 0)
            {
                Common.ChangeStatus(Texts.Keys.CHECKCOMPLETE);
                return;
            }

            if (curFile >= Globals.OldFiles.Count)
            {
                Common.ChangeStatus(Texts.Keys.DOWNLOADCOMPLETE);
                return;
            }

            if (Globals.OldFiles[curFile].Contains("/"))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(basePath + Globals.OldFiles[curFile]));
            }

            WebClient webClient = new WebClient();

            webClient.DownloadProgressChanged   += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);

            webClient.DownloadFileCompleted     += new AsyncCompletedEventHandler(webClient_DownloadFileCompleted);

            stopWatch.Start();

			webClient.DownloadFileAsync(new Uri(Globals.ServerURL + Globals.RemoteModuleFolder + Globals.OldFiles[curFile]), basePath + Globals.OldFiles[curFile]);
        }

        private static void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            currentBytes = lastBytes + e.BytesReceived;

            Common.ChangeStatus(Texts.Keys.DOWNLOADFILE, Globals.OldFiles[curFile], Computer.ComputeDownloadSize(e.BytesReceived).ToString("0.00") + " MB ", Computer.ComputeDownloadSize(e.TotalBytesToReceive).ToString("0.00") + " MB");

            Common.UpdateCompleteProgress(Computer.Compute(Globals.CompleteSize + currentBytes));

            Common.UpdateCurrentProgress(e.ProgressPercentage, Computer.ComputeDownloadSpeed(e.BytesReceived, stopWatch));
        }

        private static void webClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            lastBytes = currentBytes;

            Common.UpdateCurrentProgress(100, 0);

            curFile++;

            stopWatch.Reset();

            DownloadFile(NetworkAction);
        }
    }
}
