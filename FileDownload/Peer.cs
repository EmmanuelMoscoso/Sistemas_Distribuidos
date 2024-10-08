using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FileDownload;

public class Peer
{
    private readonly TcpListener _listener;
    private TcpClient _client;
    private const int Port = 8080;

    public Peer() {
        _listener = new TcpListener(IPAddress.Any, Port);
    }

    public async Task DownloadFile(string peerIP, int peerPort, string fileName, string savePath, CancellationToken cancellationToken){
        _client = new TcpClient(peerIP, peerPort);
        await using var stream = _client.GetStream();
        var request = Encoding.UTF8.GetBytes(fileName);
        await stream.WriteAsync(request, 0, request.Length, cancellationToken);

        await using var fs = new FileStream(savePath, FileMode.Create, FileAccess.Write);
        var buffer = new byte[1024];
        int bytesRead;
        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0){
            await fs.WriteAsync(buffer, 0, bytesRead, cancellationToken);
        }
        Console.WriteLine($"El archivo {fileName} se ha descargado en la ruta {savePath}");
    }

    public async Task Start(CancellationToken cancellationToken){
        _listener.Start();
        Console.WriteLine("Listening for incoming connections...");
        while (!cancellationToken.IsCancellationRequested){
            _client = await _listener.AcceptTcpClientAsync(cancellationToken);
            await HandleClient(cancellationToken);
        }
    }

    private async Task HandleClient(CancellationToken cancellationToken){
        await using var stream = _client.GetStream();
        var buffer = new byte[1024];
        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
        var fileName = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

        if (File.Exists(fileName)){
            var fileData = await File.ReadAllBytesAsync(fileName, cancellationToken);
            await stream.WriteAsync(fileData, 0, fileData.Length, cancellationToken);
            Console.WriteLine($"File {fileName} sent to client.");
        } else {
            var errorMessage = Encoding.UTF8.GetBytes("File not found.");
            await stream.WriteAsync(errorMessage, 0, errorMessage.Length, cancellationToken);
            Console.WriteLine($"File {fileName} not found.");
        }
    }
}
