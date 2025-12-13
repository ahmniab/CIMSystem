namespace CIMSystemGUI.Services

open System
open System.IO
open CIMSystemGUI.Models
open CIMSystemGUI.Helpers
open QRCoder

module HtmlTicketGenerator =

    let private generateQrCodeBase64 (text: string) =
        use qrGenerator = new QRCodeGenerator()
        let qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q)
        use qrCode = new PngByteQRCode(qrCodeData)
        let qrCodeBytes = qrCode.GetGraphic(20) 
        let base64 = Convert.ToBase64String(qrCodeBytes)
        $"data:image/png;base64,{base64}"

    let generateTicketHtml (ticketInfo: TicketInfo) =
        let bookingDate = ticketInfo.BookingDate.ToString("yyyy-MM-dd")
        let bookingTime = ticketInfo.BookingDate.ToString("HH:mm")
        let generatedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        let seatNumber = sprintf "R%02d-S%02d" ticketInfo.SeatRow ticketInfo.SeatColumn
        
        let qrImageSrc = generateQrCodeBase64 ticketInfo.TicketId

        sprintf
            """<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Ticket - %s</title>
    <style>
        body { font-family: 'Segoe UI', Tahoma, sans-serif; background-color: #e0e5ec; margin: 0; padding: 20px; display: flex; justify-content: center; align-items: center; min-height: 100vh; }
        .ticket { background: white; border-radius: 16px; box-shadow: 0 10px 30px rgba(0,0,0,0.2); width: 100%%; max-width: 380px; overflow: hidden; position: relative; color: #333; }
        .header { background: linear-gradient(135deg, #2b5876 0%%, #4e4376 100%%); padding: 25px; text-align: center; color: white; }
        .cinema-label { font-size: 11px; text-transform: uppercase; letter-spacing: 2px; opacity: 0.8; margin-bottom: 5px; }
        .movie-title { font-size: 26px; font-weight: 800; margin: 5px 0; color: #ffd700; text-shadow: 0 2px 4px rgba(0,0,0,0.3); line-height: 1.2; }
        .content { padding: 25px; }
        .info-row { display: flex; justify-content: space-between; margin-bottom: 15px; font-size: 15px; border-bottom: 1px solid #eee; padding-bottom: 5px; }
        .label { color: #666; font-weight: 500; }
        .value { font-weight: 800; color: #000; text-align: right; }
        .seat-box { background: #f8f9fa; border-radius: 12px; padding: 15px; text-align: center; margin-top: 25px; border: 2px solid #e9ecef; }
        .seat-number { font-size: 36px; font-weight: 900; color: #4e4376; letter-spacing: 1px; }
        .seat-label { font-size: 12px; text-transform: uppercase; color: #888; margin-top: 5px; }
        
        .qr-section { text-align: center; margin: 20px 0; padding: 10px; background: white; }
        .qr-image { width: 150px; height: 150px; }

        .ticket-id { background: #f1f3f5; text-align: center; padding: 10px; font-family: monospace; font-size: 12px; color: #555; border-top: 1px dashed #ccc; }
        .footer { text-align: center; font-size: 10px; color: #999; padding: 10px; background: #fff; }
    </style>
</head>
<body>
    <div class="ticket">
        <div class="header">
            <div class="cinema-label">CIM Cinema Ticket</div>
            <div class="movie-title">%s</div>
        </div>

        <div class="content">
            <div class="info-row">
                <span class="label">Hall / Screen</span>
                <span class="value">%s</span>
            </div>
            <div class="info-row">
                <span class="label">Customer</span>
                <span class="value">%s</span>
            </div>
            <div class="info-row">
                <span class="label">Date</span>
                <span class="value">%s</span>
            </div>
            <div class="info-row">
                <span class="label">Time</span>
                <span class="value">%s</span>
            </div>

            <div class="seat-box">
                <div class="seat-number">%s</div>
                <div class="seat-label">Assigned Seat</div>
            </div>

            <div class="qr-section">
                <img src="%s" class="qr-image" alt="Ticket QR Code" />
            </div>
        </div>

        <div class="ticket-id">
            ID: %s
        </div>

        <div class="footer">
            Printed: %s
        </div>
    </div>
</body>
</html>"""
            ticketInfo.MovieTitle    
            ticketInfo.MovieTitle    
            ticketInfo.HallName      
            ticketInfo.CustomerName  
            bookingDate              
            bookingTime              
            seatNumber               
            qrImageSrc               
            ticketInfo.TicketId      
            generatedTime            

    let saveTicketAsHtml (ticketInfo: TicketInfo) =
        try
            FileStorage.initializeStorage()
            let filename = FileStorage.getTicketFilePath $"ticket_{ticketInfo.TicketId}.html"
            let htmlContent = generateTicketHtml ticketInfo
            
            match FileStorage.writeFile filename htmlContent with
            | Result.Ok() -> Result.Ok filename
            | Result.Error msg -> Result.Error msg
        with ex ->
            Result.Error $"Failed to save ticket as HTML: {ex.Message}"