﻿namespace FileDownload;

public class Program
{
    public static async Task Main(string[] args){
        var cancellationTokenSource = new CancellationTokenSource();
        var peer = new Peer();
        var task = peer.Start(cancellationTokenSource.Token);

        if (args.Length > 0 && args[0] == "download"){
            //                         ip                      port    fileName, savePath
            await peer.DownloadFile(args[1], Convert.ToInt32(args[2]), args[3], args[4], cancellationTokenSource.Token);
        } else {
            Console.WriteLine("Waiting for other peers to connect...");
        }

        await task;
    }
}